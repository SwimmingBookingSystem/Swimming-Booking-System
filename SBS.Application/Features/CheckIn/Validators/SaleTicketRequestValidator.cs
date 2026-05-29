using FluentValidation;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Validators;

public class SaleTicketRequestValidator : AbstractValidator<SaleTicketRequestDto>
{
    private static readonly string[] AllowedPaymentMethods = ["cash", "transfer", "pos"];

    public SaleTicketRequestValidator()
    {
        RuleFor(x => x.PoolId)
            .GreaterThan(0).WithMessage("PoolId không hợp lệ.");

        RuleFor(x => x.TicketTypeId)
            .GreaterThan(0).WithMessage("TicketTypeId không hợp lệ.");

        RuleFor(x => x.SlotCount)
            .GreaterThan(0).WithMessage("Số lượng slot phải lớn hơn 0.")
            .LessThanOrEqualTo(10).WithMessage("Số lượng slot tối đa là 10.");

        RuleFor(x => x.BookingDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Ngày đặt không được là ngày trong quá khứ.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("Giờ bắt đầu phải trước giờ kết thúc.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("Giờ kết thúc phải sau giờ bắt đầu.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Phương thức thanh toán không được để trống.")
            .Must(m => AllowedPaymentMethods.Contains(m.ToLower()))
            .WithMessage("Phương thức thanh toán phải là: cash, transfer, hoặc pos.");

        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("StaffId không hợp lệ.");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Email khách hàng không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^[0-9]{9,11}$").WithMessage("Số điện thoại không hợp lệ (9-11 chữ số).")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerPhone));
    }
}
