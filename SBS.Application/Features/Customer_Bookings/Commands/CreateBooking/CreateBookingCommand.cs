using MediatR;
using SBS.Application.Features.Customer_Bookings.Dtos;
using System.Collections.Generic;

namespace SBS.Application.Features.Customer_Bookings.Commands.CreateBooking;

public class CreateBookingCommand : IRequest<CreateBookingResponseDto>
{
    public int PoolSlotId { get; set; }
    public List<BookingTicketDto> Tickets { get; set; } = new();
}

public class BookingTicketDto
{
    public int PoolTicketTypeId { get; set; }
    public int Quantity { get; set; }
}
