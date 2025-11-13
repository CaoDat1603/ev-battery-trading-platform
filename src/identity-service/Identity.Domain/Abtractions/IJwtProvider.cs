using Identity.Domain.Entities;
using System.Security.Claims;

namespace Identity.Domain.Abtractions
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
        int ExpireMinutes { get; }
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
