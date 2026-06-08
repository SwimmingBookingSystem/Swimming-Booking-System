using System;

namespace SBS.Domain.Entities;

public class ContactRequest
{
    public int ContactRequestId { get; set; }
    public Guid? UserId { get; set; } // FK → AppUser, nullable for guest
    public string Email { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public Guid? HandledByUserId { get; set; } // FK → AppUser (Staff)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? HandledAt { get; set; }
}
