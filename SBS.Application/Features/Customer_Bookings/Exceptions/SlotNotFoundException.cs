using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class SlotNotFoundException : Exception
{
    public SlotNotFoundException(int slotId, DateOnly date) 
        : base($"Slot with ID {slotId} on {date:yyyy-MM-dd} was not found.")
    {
    }
}
