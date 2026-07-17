using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.Events;

public class PoolCacheExpirationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PoolCacheExpirationWorker> _logger;

    // Trạng thái cục bộ lưu trữ đếm số lượng bể bơi và thời gian cập nhật lớn nhất
    private int _lastPoolCount = -1;
    private DateTime? _lastMaxUpdatedAt = null;

    public PoolCacheExpirationWorker(
        IServiceProvider serviceProvider,
        IDistributedCache cache,
        ILogger<PoolCacheExpirationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PoolCacheExpirationWorker started. Periodically checking pool database for cache invalidation...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPoolsChangeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in PoolCacheExpirationWorker while checking database.");
            }

            // Chạy kiểm tra định kỳ mỗi 5 giây
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task CheckPoolsChangeAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var query = uow.Repository<Pool>().Query();

        // 1. Đếm tổng số lượng bể bơi hiện tại
        int currentCount = await uow.CountAsync(query, stoppingToken);

        // 2. Lấy thời gian cập nhật lớn nhất của toàn bộ bể bơi (Dùng OrderByDescending & FirstOrDefault để tránh lỗi khi bảng trống)
        var maxUpdatedAt = query
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Select(p => (DateTime?)(p.UpdatedAt ?? p.CreatedAt))
            .FirstOrDefault();

        // 3. Khởi tạo trạng thái ban đầu khi bắt đầu khởi chạy Service
        if (_lastPoolCount == -1)
        {
            _lastPoolCount = currentCount;
            _lastMaxUpdatedAt = maxUpdatedAt;
            return;
        }

        // 4. So sánh để phát hiện thay đổi (thêm mới, xóa đi hoặc thay đổi thông tin/trạng thái)
        if (currentCount != _lastPoolCount || maxUpdatedAt != _lastMaxUpdatedAt)
        {
            _logger.LogInformation("Detect database change in pools (Count: {LastCount} -> {CurrentCount}, MaxUpdate: {LastMaxUpdate} -> {CurrentMaxUpdate}). Invalidating Customer Pool List Cache by removing active keys...", 
                _lastPoolCount, currentCount, _lastMaxUpdatedAt, maxUpdatedAt);

            try
            {
                // Lấy danh sách các Cache Key đang hoạt động trong Redis
                var activeKeysJson = await _cache.GetStringAsync("customer_pools_active_keys", stoppingToken);
                if (!string.IsNullOrEmpty(activeKeysJson))
                {
                    var activeKeys = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(activeKeysJson);
                    if (activeKeys != null)
                    {
                        foreach (var key in activeKeys)
                        {
                            try
                            {
                                // Xóa trực tiếp từng Key cũ khỏi bộ nhớ Redis để giải phóng bộ nhớ ngay lập tức
                                await _cache.RemoveAsync(key, stoppingToken);
                                _logger.LogInformation("Removed cache key: {Key} from Redis", key);
                            }
                            catch (Exception)
                            {
                                // Bỏ qua lỗi khi xóa 1 key đơn lẻ
                            }
                        }
                    }
                    
                    // Xóa luôn key danh sách quản lý để tránh dư thừa
                    await _cache.RemoveAsync("customer_pools_active_keys", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clearing active cache keys in Redis.");
            }

            // Đồng bộ trạng thái cục bộ mới
            _lastPoolCount = currentCount;
            _lastMaxUpdatedAt = maxUpdatedAt;
        }
    }
}
