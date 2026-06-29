using FluentValidation;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Manager.TicketTypes.Commands.CreateTicketType;
using SBS.Application.Features.Manager.TicketTypes.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Commands.UpdateTicketType;

// ── Command 
public record UpdateTicketTypeCommand(
    int TicketTypeId,
    string TicketName,
    decimal BasePrice,
    decimal DiscountPercent,
    string? Description,
    List<ComboDetailRequest>? ComboDetails
) : IRequest<CreateTicketTypeResponse>;

// ── Handler 
public class UpdateTicketTypeCommandHandler
    : IRequestHandler<UpdateTicketTypeCommand, CreateTicketTypeResponse>
{
    private readonly IUnitOfWork _uow;

    public UpdateTicketTypeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CreateTicketTypeResponse> Handle(
        UpdateTicketTypeCommand request, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), request.TicketTypeId);

        // Kiểm tra trùng tên (trừ chính nó)
        bool nameExists = await _uow.AnyAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketName == request.TicketName
                         && t.TicketTypeId != request.TicketTypeId), ct);
        if (nameExists)
            throw new BadRequestException($"Tên vé '{request.TicketName}' đã được dùng bởi vé khác.");

        decimal finalBasePrice = request.BasePrice;

        // Xử lý Combo: tính lại giá
        if (ticket.Category == "Combo" && request.ComboDetails != null)
        {
            var details    = request.ComboDetails;
            int totalQty   = details.Sum(d => d.Quantity);
            if (totalQty > 10)
                throw new BadRequestException("Tổng số vé đơn trong combo không được vượt quá 10.");

            var singleIds = details.Select(d => d.SingleTicketTypeId).Distinct().ToList();
            var singles   = await _uow.ToListAsync(
                _uow.Repository<TicketType>().Query()
                    .Where(t => singleIds.Contains(t.TicketTypeId)), ct);

            if (singles.Count != singleIds.Count)
                throw new BadRequestException("Một hoặc nhiều vé đơn không tồn tại.");

            if (singles.Any(s => s.Category != "Single"))
                throw new BadRequestException("Combo chỉ được chứa vé đơn (Single).");

            decimal sumPrice = 0m;
            foreach (var d in details)
            {
                var single = singles.First(s => s.TicketTypeId == d.SingleTicketTypeId);
                decimal singleActual = single.BasePrice * (1 - single.DiscountPercent / 100m);
                sumPrice += singleActual * d.Quantity;
            }
            finalBasePrice = Math.Round(sumPrice * (1 - request.DiscountPercent / 100m), 0);

            // Xóa ComboDetails cũ và thêm lại
            var oldDetails = await _uow.ToListAsync(
                _uow.Repository<ComboDetail>().Query()
                    .Where(cd => cd.ComboTicketTypeId == ticket.TicketTypeId), ct);
            _uow.Repository<ComboDetail>().DeleteRange(oldDetails);

            foreach (var d in details)
            {
                await _uow.Repository<ComboDetail>().AddAsync(new ComboDetail
                {
                    ComboTicketTypeId  = ticket.TicketTypeId,
                    SingleTicketTypeId = d.SingleTicketTypeId,
                    Quantity           = d.Quantity
                }, ct);
            }
        }

        ticket.TicketName      = request.TicketName;
        ticket.BasePrice       = finalBasePrice;
        ticket.DiscountPercent = request.DiscountPercent;
        ticket.Description     = request.Description;

        _uow.Repository<TicketType>().Update(ticket);
        await _uow.SaveChangesAsync(ct);

        return new CreateTicketTypeResponse
        {
            TicketTypeId    = ticket.TicketTypeId,
            TicketCode      = ticket.TicketCode,
            TicketName      = ticket.TicketName,
            Category        = ticket.Category,
            BasePrice       = ticket.BasePrice,
            DiscountPercent = ticket.DiscountPercent,
            Status          = ticket.Status
        };
    }
}

// ── Validator
public class UpdateTicketTypeCommandValidator : AbstractValidator<UpdateTicketTypeCommand>
{
    public UpdateTicketTypeCommandValidator()
    {
        RuleFor(x => x.TicketName)
            .NotEmpty().WithMessage("Tên vé không được để trống.")
            .MaximumLength(200).WithMessage("Tên vé không được vượt quá 200 ký tự.");

        RuleFor(x => x.DiscountPercent)
            .Must(d => d == 0 || (d >= 5 && d <= 100 && d % 5 == 0))
            .WithMessage("Giảm giá vé đơn phải là 0% hoặc từ 5% trở lên (bội số của 5).")
            .When(x => x.ComboDetails == null);

        RuleFor(x => x.DiscountPercent)
            .Must(d => d >= 5 && d <= 100 && d % 5 == 0)
            .WithMessage("Ưu đãi combo phải từ 5% trở lên và là bội số của 5 (5%, 10%...).")
            .When(x => x.ComboDetails != null);
    }
}
