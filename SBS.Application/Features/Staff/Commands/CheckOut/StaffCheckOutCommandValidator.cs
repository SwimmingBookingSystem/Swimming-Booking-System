using FluentValidation;

namespace SBS.Application.Features.Staff.Commands.CheckOut;

public class StaffCheckOutCommandValidator : AbstractValidator<StaffCheckOutCommand>
{
    public StaffCheckOutCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.BookingId.HasValue && x.BookingId.Value > 0) || !string.IsNullOrWhiteSpace(x.BookingCode))
            .WithMessage("Vui lòng cung cấp BookingId hợp lệ hoặc mã BookingCode.");
    }
}
