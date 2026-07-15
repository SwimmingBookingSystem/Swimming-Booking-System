using FluentValidation;

namespace SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;

public class JoinWaitlistCommandValidator : AbstractValidator<JoinWaitlistCommand>
{
    public JoinWaitlistCommandValidator()
    {
        RuleFor(v => v.PoolSlotId)
            .GreaterThan(0).WithMessage("PoolSlotId không hợp lệ.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Số lượng vé phải lớn hơn 0.")
            .LessThanOrEqualTo(10).WithMessage("Không thể đặt quá 10 vé trong hàng đợi.");
    }
}
