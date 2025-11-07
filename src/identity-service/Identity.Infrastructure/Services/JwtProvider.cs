using Identity.Domain.Abtractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using Identity.Infrastructure.Settings;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtSettings _settings;

        public int ExpireMinutes => _settings.ExpireMinutes;
        public JwtProvider(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }
        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // chọn giá trị định danh phù hợp
            var uniqueName = !string.IsNullOrEmpty(user.UserEmail)
                ? user.UserEmail
                : user.UserPhone ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, uniqueName),
                new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), user.Role) ?? "User"),
                new Claim("login_type", !string.IsNullOrEmpty(user.UserEmail) ? "email" : "phone")
            };
       
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                ValidateLifetime = false, // cho phép token hết hạn vẫn đọc được claims
                NameClaimType = JwtRegisteredClaimNames.UniqueName,
                RoleClaimType = ClaimTypes.Role
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, tokenValidationParams, out var securityToken);
                if (securityToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return principal;
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
