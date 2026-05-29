using Microsoft.AspNetCore.Identity;
using System;

namespace SBS.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public string? Images { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public bool Status { get; set; } = true;
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
