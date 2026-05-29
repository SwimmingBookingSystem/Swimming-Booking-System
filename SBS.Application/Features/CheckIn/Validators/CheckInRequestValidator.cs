using FluentValidation;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Validators;

public class CheckInRequestValidator : AbstractValidator<CheckInRequestDto>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.TicketCode)
            .NotEmpty().WithMessage("Mã vé không được để trống.")
            .MaximumLength(50).WithMessage("Mã vé không được vượt quá 50 ký tự.");

        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId không hợp lệ.");

        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("StaffId không hợp lệ.");
    }
}
