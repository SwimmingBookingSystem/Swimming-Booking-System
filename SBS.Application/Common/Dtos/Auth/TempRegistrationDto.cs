using System;

namespace SBS.Application.Common.Dtos.Auth;

public class TempRegistrationDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Address { get; set; }
    public string Otp { get; set; } = null!;
}

