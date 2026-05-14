using Microsoft.AspNetCore.Identity;
using System;

namespace SBS.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    // Required by EF Core
    protected AppRole() { }

    public AppRole(string roleName)
    {
        Id = Guid.NewGuid();
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
    }
}
