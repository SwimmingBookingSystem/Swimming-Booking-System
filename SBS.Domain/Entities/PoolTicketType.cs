namespace SBS.Domain.Entities;

public class PoolTicketType
{
    public int PoolId { get; set; }
    public int TicketTypeId { get; set; }
    public string Status { get; set; } = null!;

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual TicketType TicketType { get; set; } = null!;
}
