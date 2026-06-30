using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Services;

public class PayOSService : IPayOSService
{
    private readonly Net.payOS.PayOS _payOS;

    public PayOSService(Net.payOS.PayOS payOS)
    {
        _payOS = payOS;
    }

    public async Task<string> CreatePaymentLinkAsync(long bookingId, decimal amount, string bookingCode, DateTime expireAt)
    {
        var item = new ItemData(bookingCode, 1, (int)amount);
        var items = new List<ItemData> { item };
        
        int unixTimestamp = (int)expireAt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var paymentData = new PaymentData(
            orderCode: bookingId,
            amount: (int)amount,
            description: $"Ve {bookingCode}",
            items: items,
            cancelUrl: $"https://localhost:7000/Customer/Bookings/PaymentResult",
            returnUrl: $"https://localhost:7000/Customer/Bookings/PaymentResult",
            expiredAt: unixTimestamp
        );

        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
        return createPayment.checkoutUrl;
    }

    public Task<string> VerifyPaymentWebhookDataAsync(string webhookBody)
    {
        try
        {
            WebhookType webhookData = JsonConvert.DeserializeObject<WebhookType>(webhookBody) 
                ?? throw new InvalidPaymentWebhookException("Invalid Webhook Payload");
                
            WebhookData verifiedData = _payOS.verifyPaymentWebhookData(webhookData);

            // Return the TransactionId from verified data (we can also return orderCode depending on what we need to save)
            return Task.FromResult(verifiedData.orderCode.ToString());
        }
        catch (Exception ex)
        {
            throw new InvalidPaymentWebhookException(ex.Message);
        }
    }
}
