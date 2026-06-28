using FluentValidation;

namespace SBS.Application.Features.Staff.Commands.ManualCheckIn;

public class StaffManualCheckInCommandValidator : AbstractValidator<StaffManualCheckInCommand>
{
    public StaffManualCheckInCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId không hợp lệ.");
    }
}
