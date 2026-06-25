using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class SlotFullException : Exception
{
    public SlotFullException(int slotId, DateOnly date) 
        : base($"Slot with ID {slotId} on {date:yyyy-MM-dd} is full and cannot accept more bookings.")
    {
    }
}
