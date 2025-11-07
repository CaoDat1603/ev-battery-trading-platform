using Identity.Domain.Abtractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Services
{
    public class SystemJwtProvider : ISystemJwtProvider
    {
        private readonly IConfiguration _config;

        public SystemJwtProvider(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateToken(string serviceName)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, serviceName),
                new(ClaimTypes.Role, "System"),
                new(JwtRegisteredClaimNames.Iss, "IdentityService")
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSystem:SigningKey"]!));
            var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSystem:Issuer"],
                audience: _config["JwtSystem:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
