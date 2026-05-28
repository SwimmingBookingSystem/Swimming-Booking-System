using System;

namespace SBS.Domain.Entities;

public class AccountBanLog
{
    public int BanId { get; set; }
    public int UserId { get; set; }
    public int BannedBy { get; set; }
    public string Reason { get; set; } = null!;
    public bool? IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
