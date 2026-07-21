using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Manager.Pools.Commands.CreatePool;
using SBS.Application.Features.Manager.Pools.Commands.UpdatePool;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Implementations;

public class PoolManagementService : IPoolManagementService
{
    private readonly IUnitOfWork _uow;

    public PoolManagementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SuccessResponse> ClosePoolAsync(int poolId, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == poolId), ct)
            ?? throw new NotFoundException(nameof(Pool), poolId);

        if (pool.Status == "Closed")
            throw new BadRequestException("Bể bơi đã ở trạng thái Closed, không thể đóng lại.");

        var hasActiveBookings = await _uow.AnyAsync(
            _uow.Repository<Booking>().Query()
                .Where(b => b.PoolSlot.PoolId == poolId && (b.Status == "Paid" || b.Status == "PendingPayment")), ct);
                
        if (hasActiveBookings)
            throw new BadRequestException("Không thể đóng bể bơi vì đang có slot có người booking.");

        pool.Status = "Closed";
        pool.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã tạm đóng bể bơi thành công." };
    }

    public async Task<CreatePoolResponse> CreatePoolAsync(CreatePoolCommand request, CancellationToken ct)
    {
        var pool = new Pool
        {
            PoolName    = request.PoolName,
            Address     = request.Address,
            Description = request.Description,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            Area        = request.Area,
            StandardCapacity = (int)(request.Area / 2.5),
            Status      = "Active",
            CreatedAt   = DateTime.UtcNow
        };

        if (request.Images != null && request.Images.Any())
        {
            var coverCount = request.Images.Count(i => i.IsCover);
            if (coverCount == 0) request.Images[0].IsCover = true;

            int index = 1;
            foreach (var img in request.Images)
            {
                pool.PoolImages.Add(new PoolImage
                {
                    ImageUrl  = img.ImageUrl,
                    IsCover   = img.IsCover,
                    SortOrder = img.SortOrder == 0 ? index : img.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
                index++;
            }
        }

        await _uow.Repository<Pool>().AddAsync(pool, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreatePoolResponse
        {
            PoolId   = pool.PoolId,
            PoolName = pool.PoolName,
            Status   = pool.Status
        };
    }

    public async Task<SuccessResponse> ReopenPoolAsync(int poolId, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == poolId), ct)
            ?? throw new NotFoundException(nameof(Pool), poolId);

        if (pool.Status == "Active")
            throw new BadRequestException("Bể bơi đã ở trạng thái Active.");

        pool.Status = "Active";
        pool.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã mở lại hoạt động bể bơi." };
    }

    public async Task<PoolDto> UpdatePoolAsync(UpdatePoolCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        if (pool.OpeningTime != request.OpeningTime || pool.ClosingTime != request.ClosingTime)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            
            var affectedSlots = await _uow.ToListAsync(
                _uow.Repository<PoolSlot>().Query()
                    .Include(s => s.Bookings)
                    .Where(s => s.PoolId == request.PoolId && s.SlotDate >= today && s.Status != "Cancelled"
                                && (s.StartTime < request.OpeningTime || s.EndTime > request.ClosingTime)),
                ct);

            if (affectedSlots.Any())
            {
                var slotsWithActiveBookings = affectedSlots
                    .Where(s => s.Bookings.Any(b => b.Status == "Paid" || b.Status == "PendingPayment" || b.Status == "Confirmed" || b.Status == "Success"))
                    .ToList();

                if (slotsWithActiveBookings.Any())
                {
                    throw new BadRequestException("Không thể thu hẹp giờ hoạt động vì đang có khách đặt vé trong ca bơi nằm ngoài khung giờ mới. Vui lòng xử lý (hoàn tiền/huỷ vé) cho khách hàng trước.");
                }
                else
                {
                    foreach(var slot in affectedSlots)
                    {
                        slot.Status = "Cancelled";
                        _uow.Repository<PoolSlot>().Update(slot);
                    }
                }
            }
        }


        pool.PoolName    = request.PoolName;
        pool.Address     = request.Address;
        pool.Description = request.Description;
        pool.OpeningTime = request.OpeningTime;
        pool.ClosingTime = request.ClosingTime;
        pool.Area        = request.Area;
        pool.StandardCapacity = (int)(request.Area / 2.5);
        pool.UpdatedAt   = DateTime.UtcNow;

        if (request.Images != null)
        {
            var oldImages = await _uow.ToListAsync(
                _uow.Repository<PoolImage>().Query().Where(img => img.PoolId == request.PoolId), ct);
            
            _uow.Repository<PoolImage>().DeleteRange(oldImages);

            if (request.Images.Any())
            {
                var coverCount = request.Images.Count(i => i.IsCover);
                if (coverCount == 0) request.Images[0].IsCover = true;

                int index = 1;
                foreach (var img in request.Images)
                {
                    pool.PoolImages.Add(new PoolImage
                    {
                        ImageUrl  = img.ImageUrl,
                        IsCover   = img.IsCover,
                        SortOrder = img.SortOrder == 0 ? index : img.SortOrder,
                        CreatedAt = DateTime.UtcNow
                    });
                    index++;
                }
            }
        }

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new PoolDto
        {
            PoolId      = pool.PoolId,
            PoolName    = pool.PoolName,
            Address     = pool.Address,
            Description = pool.Description,
            Images      = pool.PoolImages.Select(img => new PoolImageDto 
            {
                PoolImageId = img.PoolImageId,
                ImageUrl    = img.ImageUrl,
                IsCover     = img.IsCover,
                SortOrder   = img.SortOrder,
                CreatedAt   = img.CreatedAt
            }).ToList(),
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
            Area        = pool.Area,
            StandardCapacity = pool.StandardCapacity
        };
    }
}
