using Microsoft.AspNetCore.Identity;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;

namespace SBS.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    // Custom properties
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties to Domain entities (Infrastructure → Domain is valid dependency direction)
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public virtual ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
    public virtual ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();
    public virtual ICollection<ContactRequest> HandledContacts { get; set; } = new List<ContactRequest>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
