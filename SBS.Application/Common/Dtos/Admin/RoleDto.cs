using System;

namespace SBS.Application.Common.Dtos.Admin;

public class RoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
