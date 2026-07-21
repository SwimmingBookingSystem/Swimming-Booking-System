using System;

namespace SBS.Application.Common.Dtos.Admin;

public class ContactRequestListDto
{
    public int ContactRequestId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Category { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
