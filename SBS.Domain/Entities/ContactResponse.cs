using System;

namespace SBS.Domain.Entities;

public class ContactResponse
{
    public int ResponseId { get; set; }
    public int ContactId { get; set; }
    public Guid ResponderId { get; set; }
    public string ResponseContent { get; set; } = null!;
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Contact Contact { get; set; } = null!;
}
