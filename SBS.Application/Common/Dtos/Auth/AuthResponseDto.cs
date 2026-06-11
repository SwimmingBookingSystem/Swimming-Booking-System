using System;

namespace SBS.Application.Common.Dtos.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
}
