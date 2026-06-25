using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class BookingNotFoundException : Exception
{
    public BookingNotFoundException(int bookingId) 
        : base($"Booking with ID {bookingId} was not found.")
    {
    }
}
