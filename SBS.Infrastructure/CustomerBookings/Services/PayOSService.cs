using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Services;

public class PayOSService : IPayOSService
{
    private readonly Net.payOS.PayOS _payOS;
    private readonly IConfiguration _configuration;

    public PayOSService(Net.payOS.PayOS payOS, IConfiguration configuration)
    {
        _payOS = payOS;
        _configuration = configuration;
    }

    public async Task<string> CreatePaymentLinkAsync(long bookingId, decimal amount, string bookingCode, DateTime expireAt)
    {
        var item = new ItemData(bookingCode, 1, (int)amount);
        var items = new List<ItemData> { item };

        int unixTimestamp = (int)expireAt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var baseUrl = _configuration["WebAppBaseUrl"]
                     ?? _configuration["PayOS:ReturnUrlBase"]
                     ?? _configuration["DOMAIN_OR_IP"];

        if (string.IsNullOrEmpty(baseUrl) || baseUrl.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://127.0.0.1:8082";
        }
        else if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
        {
            baseUrl = $"http://{baseUrl}";
        }

        var redirectUrl = $"{baseUrl.TrimEnd('/')}/Customer/Bookings/PaymentResult";

        var paymentData = new PaymentData(
            orderCode: bookingId,
            amount: (int)amount,
            description: $"Ve {bookingCode}",
            items: items,
            cancelUrl: redirectUrl,
            returnUrl: redirectUrl,
            expiredAt: unixTimestamp
        );

        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
        return createPayment.checkoutUrl;
    }

    public async Task<PayOSPaymentInformation> GetPaymentInformationAsync(long orderCode)
    {
        var payment = await _payOS.getPaymentLinkInformation(orderCode);
        var transactionReference = payment.transactions?
            .LastOrDefault(transaction => transaction.amount > 0)?
            .reference;

        return new PayOSPaymentInformation(
            payment.orderCode,
            payment.amount,
            payment.amountPaid,
            payment.status,
            transactionReference);
    }

    public Task<string> VerifyPaymentWebhookDataAsync(string webhookBody)
    {
        try
        {
            WebhookType webhookData = JsonConvert.DeserializeObject<WebhookType>(webhookBody)
                ?? throw new InvalidPaymentWebhookException("Dữ liệu Webhook thanh toán không hợp lệ.");

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
