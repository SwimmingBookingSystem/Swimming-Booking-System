using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class BookingNotFoundException : Exception
{
    public BookingNotFoundException(int bookingId) 
        : base($"Không tìm thấy thông tin đơn đặt vé có mã #{bookingId}.")
    {
    }
}
