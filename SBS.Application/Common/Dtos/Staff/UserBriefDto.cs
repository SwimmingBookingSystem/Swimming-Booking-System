namespace SBS.Application.Common.Dtos.Staff;

public record UserBriefDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
}
