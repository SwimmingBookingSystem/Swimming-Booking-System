using System;

namespace SBS.Application.Features.Contacts.Dtos;

public record ContactHistoryDto
{
    public int ContactRequestId { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? HandledAt { get; init; }
}
