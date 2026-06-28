using FluentValidation;

namespace SBS.Application.Features.Staff.Commands.QrCheckIn;

public class StaffQrCheckInCommandValidator : AbstractValidator<StaffQrCheckInCommand>
{
    public StaffQrCheckInCommandValidator()
    {
        RuleFor(x => x.BookingCode)
            .NotEmpty().WithMessage("Mã QR không được để trống.")
            .MaximumLength(50).WithMessage("Mã QR không hợp lệ.");
    }
}
