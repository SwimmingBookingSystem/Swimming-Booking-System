using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SBS.Application.Common.Interfaces;

public interface ITokenService
{
    (string AccessToken, string JwtId) GenerateAccessToken(Guid userId, string userName, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
