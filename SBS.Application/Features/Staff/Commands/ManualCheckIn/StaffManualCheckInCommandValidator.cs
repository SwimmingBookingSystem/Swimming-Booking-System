using FluentValidation;

namespace SBS.Application.Features.Staff.Commands.ManualCheckIn;

public class StaffManualCheckInCommandValidator : AbstractValidator<StaffManualCheckInCommand>
{
    public StaffManualCheckInCommandValidator()
    {
        // Yêu cầu ít nhất một trong hai: BookingId hợp lệ hoặc BookingCode không rỗng
        RuleFor(x => x)
            .Must(x => (x.BookingId.HasValue && x.BookingId.Value > 0) || !string.IsNullOrWhiteSpace(x.BookingCode))
            .WithMessage("Vui lòng cung cấp BookingId hợp lệ hoặc mã BookingCode.");
    }
}
