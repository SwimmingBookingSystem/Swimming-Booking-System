using Microsoft.AspNetCore.Identity;

namespace SBS.Infrastructure.Identity;

public class AppRole : IdentityRole<int>
{
    public AppRole() { }

    public AppRole(string roleName) : base(roleName)
    {
        NormalizedName = roleName.ToUpperInvariant();
    }
}
