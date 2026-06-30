using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.CustomerViewPoolList;

public record GetCustomerPoolsQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchName = null,
    string? Address = null,
    TimeSpan? OpeningTime = null,
    TimeSpan? ClosingTime = null,
    int? MinCapacity = null,
    int? MaxCapacity = null
) : IRequest<PagedResponse<CustomerPoolDto>>;

public class GetCustomerPoolsQueryHandler : IRequestHandler<GetCustomerPoolsQuery, PagedResponse<CustomerPoolDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCustomerPoolsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResponse<CustomerPoolDto>> Handle(GetCustomerPoolsQuery request, CancellationToken ct)
    {
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

        return new PagedResponse<CustomerPoolDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
