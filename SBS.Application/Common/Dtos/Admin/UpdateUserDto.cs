namespace SBS.Application.Common.Dtos.Admin;

public class UpdateUserDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public int? PoolId { get; set; }
    public string? Password { get; set; }
}
