using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SBS.Application.Features.Customer.CustomerViewPoolList.Queries;

public class GetCustomerPoolsQueryHandler : IRequestHandler<GetCustomerPoolsQuery, PagedResponse<CustomerPoolDto>>
{
    private readonly IReadOnlyUnitOfWork _uow;
    private readonly IDistributedCache _cache;

    public GetCustomerPoolsQueryHandler(IReadOnlyUnitOfWork uow, IDistributedCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<PagedResponse<CustomerPoolDto>> Handle(GetCustomerPoolsQuery request, CancellationToken ct)
    {
        // 1. Tạo cache key độc nhất dựa trên các tham số lọc của query (Không dùng version để key luôn sạch)
        string cacheKey = $"customer_pools_page_{request.Page}_size_{request.PageSize}_" +
                          $"search_{request.SearchName ?? ""}_address_{request.Address ?? ""}_" +
                          $"open_{request.OpeningTime?.ToString() ?? ""}_close_{request.ClosingTime?.ToString() ?? ""}_" +
                          $"min_{request.MinCapacity?.ToString() ?? ""}_max_{request.MaxCapacity?.ToString() ?? ""}";

        try
        {
            // 2. Thử lấy dữ liệu từ cache
            var cachedData = await _cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerPoolDto>>(cachedData);
                if (cachedResponse != null)
                {
                    // Trả về dữ liệu từ cache ngay lập tức (không cần truy vấn Database)
                    return cachedResponse;
                }
            }
        }
        catch (Exception)
        {
            // Bỏ qua lỗi kết nối Redis để hệ thống vẫn chạy tiếp qua Database
        }

        // 3. Nếu không có cache, truy vấn từ database
        var query = _uow.Repository<Pool>().Query();

        // Chỉ lấy những bể bơi đang hoạt động
        query = query.Where(p => p.Status == "Active");

        // Lọc theo tên hoặc địa chỉ bể bơi (không phân biệt hoa thường)
        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = request.SearchName.ToLower();
            query = query.Where(p => p.PoolName.ToLower().Contains(search) || 
                                    p.Address.ToLower().Contains(search));
        }

        // Lọc theo địa chỉ (Tỉnh/Thành phố/Quận/Huyện...)
        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var addr = request.Address.ToLower();
            query = query.Where(p => p.Address.ToLower().Contains(addr));
        }

        // Lọc theo giờ mở/đóng cửa
        if (request.OpeningTime.HasValue)
        {
            query = query.Where(p => p.OpeningTime <= request.OpeningTime.Value);
        }
        if (request.ClosingTime.HasValue)
        {
            query = query.Where(p => p.ClosingTime >= request.ClosingTime.Value);
        }

        // Lọc theo sức chứa
        if (request.MinCapacity.HasValue)
        {
            query = query.Where(p => p.StandardCapacity >= request.MinCapacity.Value);
        }
        if (request.MaxCapacity.HasValue)
        {
            query = query.Where(p => p.StandardCapacity <= request.MaxCapacity.Value);
        }

        // Đếm tổng số lượng bản ghi thỏa mãn bộ lọc
        var total = await _uow.CountAsync(query, ct);

        // Lấy danh sách phân trang và map sang Dto
        var items = await _uow.ToListAsync(
            query.OrderBy(p => p.PoolName)
                 .Skip((request.Page - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .Select(p => new CustomerPoolDto
                 {
                      PoolId = p.PoolId,
                      PoolName = p.PoolName,
                      Address = p.Address,
                      Description = p.Description,
                      CoverImageUrl = p.PoolImages.Where(img => img.IsCover).Select(img => img.ImageUrl).FirstOrDefault() 
                                      ?? p.PoolImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault(),
                      OpeningTime = p.OpeningTime.ToString(@"hh\:mm"),
                      ClosingTime = p.ClosingTime.ToString(@"hh\:mm"),
                      StandardCapacity = p.StandardCapacity
                 }), ct);

        var response = new PagedResponse<CustomerPoolDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        try
        {
            // 4. Lưu kết quả mới lấy được vào cache Redis với thời gian hết hạn là 40 phút
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions, ct);

            // 5. Lưu lại Cache Key này vào danh sách quản lý để xóa đi khi thay đổi dữ liệu (Tránh tốn bộ nhớ)
            var activeKeysJson = await _cache.GetStringAsync("customer_pools_active_keys", ct);
            var activeKeys = !string.IsNullOrEmpty(activeKeysJson)
                ? JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(activeKeysJson)
                : new System.Collections.Generic.List<string>();

            if (activeKeys != null && !activeKeys.Contains(cacheKey))
            {
                activeKeys.Add(cacheKey);
                await _cache.SetStringAsync("customer_pools_active_keys", JsonSerializer.Serialize(activeKeys), ct);
            }
        }
        catch (Exception)
        {
            // Bỏ qua lỗi kết nối Redis để không làm ảnh hưởng luồng chính
        }

        return response;
    }
}
