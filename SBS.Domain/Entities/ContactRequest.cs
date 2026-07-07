using System;

namespace SBS.Domain.Entities;

public class ContactRequest
{
    public int ContactRequestId { get; set; }
    public Guid? UserId { get; set; } // FK → AppUser, nullable for guest
    
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Category { get; set; } = "General"; // Vấn đề cần liên hệ
    public string Message { get; set; } = null!; // Nội dung chi tiết
    
    public string Status { get; set; } = "Pending";
    public Guid? HandledByUserId { get; set; } // FK → AppUser (Staff)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? HandledAt { get; set; }
}
