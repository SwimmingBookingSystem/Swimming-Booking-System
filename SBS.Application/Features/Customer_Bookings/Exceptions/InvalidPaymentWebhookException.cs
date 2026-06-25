using System;

namespace SBS.Application.Features.Customer_Bookings.Exceptions;

public class InvalidPaymentWebhookException : Exception
{
    public InvalidPaymentWebhookException(string message) 
        : base(message)
    {
    }
}
