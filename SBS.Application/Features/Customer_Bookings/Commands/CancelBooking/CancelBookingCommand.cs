using MassTransit;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CancelBooking;

public record CancelBookingCommand(int BookingId) : IRequest<bool>;

