using Identity.Application.DTOs;

namespace Identity.Application.Contracts
{
    public interface ISystemTokenService
    {
        SystemTokenResponseDto GenerateSystemToken(string internalKey);
    }
}
