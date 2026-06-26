using FluentValidation;

namespace SBS.Application.Features.Customer_Bookings.Commands.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(v => v.PoolSlotId)
            .GreaterThan(0).WithMessage("PoolSlotId is required.");

        RuleFor(v => v.Tickets)
            .NotEmpty().WithMessage("At least one ticket must be selected.");

        RuleForEach(v => v.Tickets).ChildRules(tickets =>
        {
            tickets.RuleFor(t => t.PoolTicketTypeId).GreaterThan(0).WithMessage("PoolTicketTypeId must be valid.");
            tickets.RuleFor(t => t.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                   .LessThanOrEqualTo(20).WithMessage("Cannot book more than 20 tickets per type in a single transaction.");
        });
    }
}
