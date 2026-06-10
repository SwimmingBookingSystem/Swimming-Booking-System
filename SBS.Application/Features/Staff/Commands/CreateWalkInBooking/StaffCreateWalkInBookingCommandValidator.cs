using FluentValidation;
using System;

namespace SBS.Application.Features.Staff.Commands.CreateWalkInBooking;

public class StaffCreateWalkInBookingCommandValidator : AbstractValidator<StaffCreateWalkInBookingCommand>
{
    public StaffCreateWalkInBookingCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Tên khách hàng không được để trống.")
            .MaximumLength(100).WithMessage("Tên khách hàng không được vượt quá 100 ký tự.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$")
            .WithMessage("Số điện thoại không hợp lệ (phải là số di động Việt Nam 10 chữ số).");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.PoolSlotId)
            .GreaterThan(0).WithMessage("Vui lòng chọn slot bơi.");

        RuleFor(x => x.BookingDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Ngày bơi không được là ngày trong quá khứ.");

        RuleFor(x => x.Tickets)
            .NotEmpty().WithMessage("Phải chọn ít nhất 1 loại vé.");

        RuleForEach(x => x.Tickets).ChildRules(ticket =>
        {
            ticket.RuleFor(t => t.PoolTicketTypeId)
                .GreaterThan(0).WithMessage("Loại vé không hợp lệ.");
            ticket.RuleFor(t => t.Quantity)
                .GreaterThan(0).WithMessage("Số lượng vé phải lớn hơn 0.")
                .LessThanOrEqualTo(10).WithMessage("Số lượng mỗi loại vé không được vượt quá 10.");
        });

        RuleFor(x => x.CashPaymentConfirmed)
            .Equal(true).WithMessage("Phải xác nhận đã thu tiền mặt trước khi tạo walk-in booking.");
    }
}
