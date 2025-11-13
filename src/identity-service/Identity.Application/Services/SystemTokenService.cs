using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Abtractions;

namespace Identity.Application.Services
{
    public class SystemTokenService : ISystemTokenService
    {
        private readonly IConfiguration _config;
        private readonly ISystemJwtProvider _jwtProvider;

        public SystemTokenService(IConfiguration config, ISystemJwtProvider jwtProvider)
        {
            _config = config;
            _jwtProvider = jwtProvider;
        }

        public SystemTokenResponseDto GenerateSystemToken(string internalKey)
        {
            var expectedKey = _config["InternalApiKey"];
            if (string.IsNullOrEmpty(internalKey) || internalKey != expectedKey)
                return SystemTokenResponseDto.Fail("Invalid internal key");

            var token = _jwtProvider.GenerateToken("rating-api");
            return SystemTokenResponseDto.Success(token);
        }
    }
}
