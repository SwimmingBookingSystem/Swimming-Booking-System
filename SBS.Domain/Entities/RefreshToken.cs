using System;

namespace SBS.Domain.Entities;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
