using System;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface IPayOSService
{
    Task<string> CreatePaymentLinkAsync(long bookingId, decimal amount, string bookingCode, DateTime expireAt);

    Task<PayOSPaymentInformation> GetPaymentInformationAsync(long orderCode);
    
    // Returns the Transaction ID if valid, or throws exception if invalid
    Task<string> VerifyPaymentWebhookDataAsync(string webhookBody);
}

public sealed record PayOSPaymentInformation(
    long OrderCode,
    decimal Amount,
    decimal AmountPaid,
    string Status,
    string? TransactionReference);
