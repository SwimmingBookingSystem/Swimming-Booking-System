using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class SlotNotFoundException : Exception
{
    public SlotNotFoundException(int slotId, DateOnly date) 
        : base($"Không tìm thấy ca bơi mã #{slotId} ngày {date:dd/MM/yyyy}.")
    {
    }
}
