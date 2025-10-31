using Identity.Application.DTOs;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = Identity.Application.DTOs.LoginRequest;
using RegisterRequest = Identity.Application.DTOs.RegisterRequest;

namespace Identity.Application.Contracts
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<TokenResponse?> RefreshTokenAsync(string accessToken,  CancellationToken cancellationToken = default);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<RegisterResponse> VerifyOtpAndCreateUserAsync(VerifyOtpRequest request);
        Task<bool> ResendOtpAsync(string emailOrPhone);
        Task<bool> RequestResetPasswordAsync(string emailOrPhone, string baseUrl);
        Task<bool> ResetPasswordAsync(DTOs.ResetPasswordRequest request);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
