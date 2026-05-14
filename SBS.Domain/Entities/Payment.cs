using SBS.Domain.Enums;
using System;

namespace SBS.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string TransactionId { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public PaymentStatus Status { get; private set; }

    // Navigation properties
    public virtual Booking Booking { get; private set; }

    protected Payment() { }

    public Payment(Guid bookingId, decimal amount, PaymentMethod paymentMethod)
    {
        BookingId = bookingId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
    }

    public void ProcessPayment(PaymentStatus status, string transactionId = null)
    {
        Status = status;
        if (status == PaymentStatus.Success)
        {
            PaymentDate = DateTime.UtcNow;
            TransactionId = transactionId;
        }
        UpdateTimestamp();
    }
}
