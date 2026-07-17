using System;

namespace SBS.Application.Common.Dtos.Admin;

public class ChangeRoleDto
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
}
