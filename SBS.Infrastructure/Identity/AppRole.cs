using Microsoft.AspNetCore.Identity;
using System;

namespace SBS.Infrastructure.Identity;

public class AppRole : IdentityRole<Guid>
{
    public AppRole() { }

    public AppRole(string roleName) : base(roleName)
    {
        NormalizedName = roleName.ToUpperInvariant();
    }
}
