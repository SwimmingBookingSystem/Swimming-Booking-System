using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class SlotFullException : Exception
{
    public SlotFullException(int slotId, DateOnly date) 
        : base($"Rất tiếc, ca bơi mã #{slotId} ngày {date:dd/MM/yyyy} đã hết suất bơi trống.")
    {
    }
}
