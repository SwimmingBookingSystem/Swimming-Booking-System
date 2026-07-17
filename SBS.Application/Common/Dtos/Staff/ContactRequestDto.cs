using System;

namespace SBS.Application.Common.Dtos.Staff;

public record ContactRequestDto
{
    public int ContactRequestId { get; init; }
    public string Email { get; init; } = null!;
    public string Reason { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? HandledAt { get; init; }
}
