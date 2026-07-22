using System;

namespace SBS.Application.Common.Dtos.Profile;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Images { get; set; }
    public DateTime CreatedAt { get; set; }
}
