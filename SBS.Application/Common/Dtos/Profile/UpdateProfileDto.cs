using System;

namespace SBS.Application.Common.Dtos.Profile;

public class UpdateProfileDto
{
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
}
