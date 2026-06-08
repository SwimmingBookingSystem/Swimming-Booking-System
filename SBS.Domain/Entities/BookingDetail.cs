namespace SBS.Domain.Entities;

public class BookingDetail
{
    public int BookingDetailId { get; set; }
    public int BookingId { get; set; }
    public int PoolTicketTypeId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual PoolTicketType PoolTicketType { get; set; } = null!;
}
