using MediatR;

namespace SBS.Application.Features.Customer_Bookings.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(int BookingId) : IRequest<bool>;
