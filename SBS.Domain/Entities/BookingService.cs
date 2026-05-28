namespace SBS.Domain.Entities;

public class BookingService
{
    public int BookingServiceId { get; set; }
    public int BookingId { get; set; }
    public int PoolServiceId { get; set; }
    public int? Quantity { get; set; }
    public decimal TotalServicePrice { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual PoolService PoolService { get; set; } = null!;
}
