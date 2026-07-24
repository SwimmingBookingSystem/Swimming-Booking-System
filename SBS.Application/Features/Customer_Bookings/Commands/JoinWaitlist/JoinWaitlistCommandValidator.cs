using FluentValidation;

namespace SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;

public class JoinWaitlistCommandValidator : AbstractValidator<JoinWaitlistCommand>
{
    public JoinWaitlistCommandValidator()
    {
        RuleFor(v => v.PoolSlotId)
            .GreaterThan(0).WithMessage("Ca bơi được chọn không hợp lệ.");
    }
}
