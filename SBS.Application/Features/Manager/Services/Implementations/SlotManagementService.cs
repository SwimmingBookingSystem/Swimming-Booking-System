using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Application.Features.Manager.Slots.Commands.CreateSlot;
using SBS.Application.Features.Manager.Slots.Commands.GenerateSlots;
using SBS.Application.Features.Manager.Slots.Commands.UpdateSlot;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Implementations;

public class SlotManagementService : ISlotManagementService
{
    private readonly IUnitOfWork _uow;

    public SlotManagementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SuccessResponse> CloseSlotAsync(int slotId, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == slotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), slotId);

        if (slot.Status == "Closed")
            throw new BadRequestException("Slot đã ở trạng thái Closed.");

        var hasActiveBookings = await _uow.AnyAsync(
            _uow.Repository<Booking>().Query()
                .Where(b => b.PoolSlotId == slotId && (b.Status == "Paid" || b.Status == "PendingPayment")), ct);
                
        if (hasActiveBookings)
            throw new BadRequestException("Không thể đóng slot vì đã có người booking.");

        slot.Status = "Closed";
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã đóng slot thành công." };
    }

    public async Task<PoolSlotDto> CreateSlotAsync(CreateSlotCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        if (request.StartTime < pool.OpeningTime || request.EndTime > pool.ClosingTime)
            throw new BadRequestException(
                $"Slot phải nằm trong giờ mở cửa của bể bơi ({pool.OpeningTime:hh\\:mm} – {pool.ClosingTime:hh\\:mm}).");

        if (request.Capacity < 1 || request.Capacity > pool.StandardCapacity)
            throw new BadRequestException($"Sức chứa ca bơi phải lớn hơn 0 và không vượt quá giới hạn an toàn của bể bơi ({pool.StandardCapacity} người).");

        bool hasOverlap = await _uow.AnyAsync(
            _uow.Repository<PoolSlot>().Query()
                .Where(s => s.PoolId    == request.PoolId
                         && s.SlotDate  == request.SlotDate
                         && s.StartTime <  request.EndTime
                         && s.EndTime   >  request.StartTime), ct);

        if (hasOverlap)
            throw new BadRequestException("Đã tồn tại slot trùng khung giờ trong bể bơi này.");

        var slot = new PoolSlot
        {
            PoolId    = request.PoolId,
            SlotName  = request.SlotName,
            StartTime = request.StartTime,
            EndTime   = request.EndTime,
            SlotDate  = request.SlotDate,
            Capacity  = request.Capacity,
            Status    = "Open",
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Repository<PoolSlot>().AddAsync(slot, ct);
        await _uow.SaveChangesAsync(ct);

        string displayName = slot.SlotName
            ?? $"{request.StartTime:hh\\:mm} - {request.EndTime:hh\\:mm}";

        return new PoolSlotDto
        {
            PoolSlotId = slot.PoolSlotId,
            PoolId     = slot.PoolId,
            SlotName   = displayName,
            StartTime  = slot.StartTime.ToString(@"hh\:mm"),
            EndTime    = slot.EndTime.ToString(@"hh\:mm"),
            SlotDate   = slot.SlotDate.ToString("yyyy-MM-dd"),
            Capacity   = slot.Capacity,
            Status     = slot.Status,
            CreatedAt  = slot.CreatedAt
        };
    }

    public async Task<SuccessResponse> GenerateSlotsAsync(GenerateSlotsCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        var existingSlots = await _uow.ToListAsync(
            _uow.Repository<PoolSlot>().Query()
            .Where(s => s.PoolId == request.PoolId 
                     && s.SlotDate >= request.StartDate 
                     && s.SlotDate <= request.EndDate), ct);

        var slotsToInsert = new List<PoolSlot>();
        
        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var currentTime = pool.OpeningTime;
            
            while (currentTime.Add(TimeSpan.FromMinutes(request.DurationMinutes)) <= pool.ClosingTime)
            {
                var endTime = currentTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));
                
                bool overlap = existingSlots.Any(s => 
                       s.SlotDate == date 
                    && s.StartTime < endTime 
                    && s.EndTime > currentTime);

                if (!overlap)
                {
                    slotsToInsert.Add(new PoolSlot
                    {
                        PoolId = request.PoolId,
                        SlotName = $"{currentTime:hh\\:mm} - {endTime:hh\\:mm}",
                        StartTime = currentTime,
                        EndTime = endTime,
                        SlotDate = date,
                        Capacity = pool.StandardCapacity > 0 ? pool.StandardCapacity : 50,
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                currentTime = endTime.Add(TimeSpan.FromMinutes(request.BreakMinutes));
            }

            if (currentTime < pool.ClosingTime)
            {
                var remainingMinutes = (pool.ClosingTime - currentTime).TotalMinutes;
                
                if (remainingMinutes >= 30) 
                {
                    var endTime = pool.ClosingTime;
                    
                    bool overlap = existingSlots.Any(s => 
                           s.SlotDate == date 
                        && s.StartTime < endTime 
                        && s.EndTime > currentTime);

                    if (!overlap)
                    {
                        slotsToInsert.Add(new PoolSlot
                        {
                            PoolId = request.PoolId,
                            SlotName = $"{currentTime:hh\\:mm} - {endTime:hh\\:mm}",
                            StartTime = currentTime,
                            EndTime = endTime,
                            SlotDate = date,
                            Capacity = pool.StandardCapacity > 0 ? pool.StandardCapacity : 50,
                            Status = "Open",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        if (slotsToInsert.Count == 0)
        {
            throw new BadRequestException("Không có ca bơi nào được tạo. Toàn bộ các khung giờ trong khoảng thời gian này đã bị trùng lịch với các ca bơi hiện có.");
        }

        await _uow.Repository<PoolSlot>().AddRangeAsync(slotsToInsert, ct);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã tạo tự động thành công {slotsToInsert.Count} ca bơi." };
    }

    public async Task<SuccessResponse> OpenSlotAsync(int slotId, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == slotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), slotId);

        if (slot.Status == "Open")
            throw new BadRequestException("Slot đã ở trạng thái Open.");

        slot.Status = "Open";
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã mở slot thành công." };
    }

    public async Task<PoolSlotDto> UpdateSlotAsync(UpdateSlotCommand request, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

        bool hasBooking = await _uow.AnyAsync(
            _uow.Repository<Booking>().Query()
                .Where(b => b.PoolSlotId == request.SlotId && (b.Status == "Paid" || b.Status == "PendingPayment")), ct);

        bool timeChanged = slot.StartTime != request.StartTime 
                        || slot.EndTime   != request.EndTime
                        || slot.SlotDate  != request.SlotDate;

        if (hasBooking && timeChanged)
            throw new BadRequestException("Không thể thay đổi giờ hoặc ngày của slot đã có booking.");

        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == slot.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), slot.PoolId);

        if (request.Capacity < 1 || request.Capacity > pool.StandardCapacity)
            throw new BadRequestException($"Sức chứa ca bơi phải lớn hơn 0 và không vượt quá giới hạn an toàn của bể bơi ({pool.StandardCapacity} người).");

        if (timeChanged)
        {
            if (request.StartTime < pool.OpeningTime || request.EndTime > pool.ClosingTime)
                throw new BadRequestException($"Slot phải nằm trong giờ mở cửa của bể bơi ({pool.OpeningTime:hh\\:mm} – {pool.ClosingTime:hh\\:mm}).");

            bool hasOverlap = await _uow.AnyAsync(
                _uow.Repository<PoolSlot>().Query()
                    .Where(s => s.PoolId      == slot.PoolId
                             && s.PoolSlotId  != request.SlotId
                             && s.SlotDate    == request.SlotDate
                             && s.StartTime   <  request.EndTime
                             && s.EndTime     >  request.StartTime), ct);

            if (hasOverlap)
                throw new BadRequestException("Đã tồn tại slot trùng khung giờ trong bể bơi này.");
        }

        slot.SlotName  = request.SlotName;
        slot.StartTime = request.StartTime;
        slot.EndTime   = request.EndTime;
        slot.SlotDate  = request.SlotDate;

        int currentBooked = await _uow.Repository<BookingDetail>().Query()
            .Where(bd => bd.Booking.PoolSlotId == request.SlotId && 
                         bd.Booking.Status != "Cancelled" && 
                         bd.Booking.Status != "Failed" &&
                         bd.Booking.Status != "Refunded" &&
                         bd.Booking.Status != "Completed")
            .SumAsync(bd => bd.Quantity, ct);

        if (request.Capacity < currentBooked)
        {
            throw new BadRequestException($"Không thể giảm tổng sức chứa xuống {request.Capacity} vì hiện tại đã có {currentBooked} vé được đặt cho ca bơi này. Sức chứa tối thiểu cho phép là {currentBooked}.");
        }

        slot.Capacity  = request.Capacity;

        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new PoolSlotDto
        {
            PoolSlotId = slot.PoolSlotId,
            PoolId     = slot.PoolId,
            SlotName   = slot.SlotName,
            StartTime  = slot.StartTime.ToString(@"hh\:mm"),
            EndTime    = slot.EndTime.ToString(@"hh\:mm"),
            SlotDate   = slot.SlotDate.ToString("yyyy-MM-dd"),
            Capacity   = slot.Capacity,
            Status     = slot.Status,
            CreatedAt  = slot.CreatedAt
        };
    }

    public async Task<SuccessResponse> UpdateSlotCapacityAsync(int slotId, int capacity, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == slotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), slotId);

        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == slot.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), slot.PoolId);

        if (capacity < 1 || capacity > pool.StandardCapacity)
            throw new BadRequestException($"Sức chứa ca bơi phải lớn hơn 0 và không vượt quá giới hạn an toàn của bể bơi ({pool.StandardCapacity} người).");

        int currentBooked = await _uow.Repository<BookingDetail>().Query()
            .Where(bd => bd.Booking.PoolSlotId == slotId && 
                         bd.Booking.Status != "Cancelled" && 
                         bd.Booking.Status != "Failed" &&
                         bd.Booking.Status != "Refunded" &&
                         bd.Booking.Status != "Completed")
            .SumAsync(bd => bd.Quantity, ct);

        if (capacity < currentBooked)
        {
            throw new BadRequestException($"Không thể giảm tổng sức chứa xuống {capacity} vì hiện tại đã có {currentBooked} vé được đặt. Sức chứa tối thiểu cho phép là {currentBooked}.");
        }

        slot.Capacity = capacity;
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã cập nhật sức chứa thành {capacity}." };
    }
}
