using FluentValidation;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Manager.TicketTypes.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Commands.CreateTicketType;

// ── Shared DTO 
public class ComboDetailRequest
{
    public int SingleTicketTypeId { get; set; }
    public int Quantity { get; set; }
}

// ── Command 
public record CreateTicketTypeCommand(
    string TicketCode,
    string TicketName,
    string Category,                        // "Single" | "Combo"
    decimal BasePrice,                      // Nhập thẳng cho Single; Combo sẽ tự tính
    decimal DiscountPercent,
    string? Description,
    List<ComboDetailRequest>? ComboDetails
) : IRequest<CreateTicketTypeResponse>;

// ── Handler 
public class CreateTicketTypeCommandHandler
    : IRequestHandler<CreateTicketTypeCommand, CreateTicketTypeResponse>
{
    private readonly IUnitOfWork _uow;

    public CreateTicketTypeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CreateTicketTypeResponse> Handle(
        CreateTicketTypeCommand request, CancellationToken ct)
    {
        // ── 1. Kiểm tra trùng TicketCode 
        bool codeExists = await _uow.AnyAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketCode == request.TicketCode), ct);
        if (codeExists)
            throw new BadRequestException($"Mã vé '{request.TicketCode}' đã tồn tại.");

        // ── 2. Kiểm tra trùng TicketName 
        bool nameExists = await _uow.AnyAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketName == request.TicketName), ct);
        if (nameExists)
            throw new BadRequestException($"Tên vé '{request.TicketName}' đã tồn tại.");

        decimal finalBasePrice = request.BasePrice;

        // ── 3. Xử lý riêng cho Combo 
        if (request.Category == "Combo")
        {
            var details = request.ComboDetails!;

            // Tổng quantity ≤ 10
            int totalQty = details.Sum(d => d.Quantity);
            if (totalQty > 10)
                throw new BadRequestException("Tổng số vé đơn trong một combo không được vượt quá 10.");

            // Từng dòng quantity > 0
            if (details.Any(d => d.Quantity <= 0))
                throw new BadRequestException("Số lượng từng loại vé đơn trong combo phải lớn hơn 0.");

            // Load tất cả single tickets được tham chiếu
            var singleIds = details.Select(d => d.SingleTicketTypeId).Distinct().ToList();
            var singles   = await _uow.ToListAsync(
                _uow.Repository<TicketType>().Query()
                    .Where(t => singleIds.Contains(t.TicketTypeId)), ct);

            // Tất cả phải tồn tại
            if (singles.Count != singleIds.Count)
                throw new BadRequestException("Một hoặc nhiều vé đơn không tồn tại.");

            // Chỉ được chứa vé Single, không chứa Combo
            var nonSingle = singles.Where(s => s.Category != "Single").ToList();
            if (nonSingle.Any())
                throw new BadRequestException(
                    $"Combo chỉ được chứa vé đơn (Single). Các vé không hợp lệ: " +
                    string.Join(", ", nonSingle.Select(s => s.TicketCode)));

            // Tính BasePrice combo = Σ(giá thực từng Single × qty) × (1 − discountCombo%)
            decimal sumPrice = 0m;
            foreach (var d in details)
            {
                var single    = singles.First(s => s.TicketTypeId == d.SingleTicketTypeId);
                decimal singleActual = single.BasePrice * (1 - single.DiscountPercent / 100m);
                sumPrice += singleActual * d.Quantity;
            }
            finalBasePrice = sumPrice;
        }

        // ── 4. Tạo TicketType ─
        var ticket = new TicketType
        {
            TicketCode      = request.TicketCode,
            TicketName      = request.TicketName,
            Category        = request.Category,
            BasePrice       = finalBasePrice,
            DiscountPercent = request.DiscountPercent,
            Description     = request.Description,
            Status          = "Active",
            CreatedAt       = DateTime.UtcNow
        };

        await _uow.Repository<TicketType>().AddAsync(ticket, ct);
        await _uow.SaveChangesAsync(ct); // flush → có TicketTypeId

        // ── 5. Tạo ComboDetails (chỉ khi Combo) 
        if (request.Category == "Combo")
        {
            foreach (var d in request.ComboDetails!)
            {
                await _uow.Repository<ComboDetail>().AddAsync(new ComboDetail
                {
                    ComboTicketTypeId  = ticket.TicketTypeId,
                    SingleTicketTypeId = d.SingleTicketTypeId,
                    Quantity           = d.Quantity
                }, ct);
            }
            await _uow.SaveChangesAsync(ct);
        }

        // Nghiệp vụ Phân bổ Bể bơi đã được chuyển sang giao diện Pricing (Manager/PoolTicketType)

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
public class CreateTicketTypeCommandValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeCommandValidator()
    {
        RuleFor(x => x.TicketCode)
            .NotEmpty().WithMessage("Mã vé không được để trống.")
            .MaximumLength(50).WithMessage("Mã vé không được vượt quá 50 ký tự.");

        RuleFor(x => x.TicketName)
            .NotEmpty().WithMessage("Tên vé không được để trống.")
            .MaximumLength(200).WithMessage("Tên vé không được vượt quá 200 ký tự.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => c == "Single" || c == "Combo")
            .WithMessage("Category phải là 'Single' hoặc 'Combo'.");

        // Single: BasePrice bắt buộc > 0; DiscountPercent 0 hoặc bội số của 5
        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("BasePrice phải lớn hơn 0.")
            .When(x => x.Category == "Single");

        RuleFor(x => x.DiscountPercent)
            .Must(d => d == 0 || (d >= 5 && d <= 100 && d % 5 == 0))
            .WithMessage("Giảm giá vé đơn phải là 0% hoặc từ 5% trở lên (bội số của 5).")
            .When(x => x.Category == "Single");

        // Combo: discount từ 5-100 và là bội số của 5
        RuleFor(x => x.DiscountPercent)
            .Must(d => d >= 5 && d <= 100 && d % 5 == 0)
            .WithMessage("Ưu đãi combo phải từ 5% trở lên và là bội số của 5 (5%, 10%...).")
            .When(x => x.Category == "Combo");

        RuleFor(x => x.ComboDetails)
            .NotNull().WithMessage("Vé combo phải có danh sách ComboDetails.")
            .Must(d => d != null && d.Count >= 2)
            .WithMessage("Combo phải có ít nhất 2 dòng vé đơn.")
            .When(x => x.Category == "Combo");

        // Single không được có ComboDetails
        RuleFor(x => x.ComboDetails)
            .Null().WithMessage("Vé đơn không được có ComboDetails.")
            .When(x => x.Category == "Single");
    }
}
