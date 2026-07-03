using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.GenerateSlots;

public record GenerateSlotsCommand(
    int PoolId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DurationMinutes,
    int BreakMinutes
) : IRequest<SuccessResponse>;

public class GenerateSlotsCommandHandler : IRequestHandler<GenerateSlotsCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public GenerateSlotsCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(GenerateSlotsCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        // Lấy tất cả Slot hiện có của Bể bơi trong khoảng thời gian này (In-Memory Check để chống N+1 Query)
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
                
                // Check overlap In-Memory
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
        }

        if (slotsToInsert.Count == 0)
        {
            throw new BadRequestException("Không có ca bơi nào được tạo. Toàn bộ các khung giờ trong khoảng thời gian này đã bị trùng lịch với các ca bơi hiện có.");
        }

        await _uow.Repository<PoolSlot>().AddRangeAsync(slotsToInsert, ct);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã tạo tự động thành công {slotsToInsert.Count} ca bơi." };
    }
}

public class GenerateSlotsCommandValidator : AbstractValidator<GenerateSlotsCommand>
{
    public GenerateSlotsCommandValidator()
    {
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Thời lượng ca bơi phải lớn hơn 0.");
            
        RuleFor(x => x.BreakMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian nghỉ không được âm.");
            
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày bắt đầu không được trong quá khứ.");
    }
}
