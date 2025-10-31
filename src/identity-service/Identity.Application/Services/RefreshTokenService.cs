using Identity.Application.Common;
using Microsoft.AspNetCore.DataProtection;
using Identity.Application.Contracts;
namespace Identity.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDataProtector _dataProtector;
        private const string CookieName = "refreshToken";

        public RefreshTokenService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("RefreshTokenProtector");
        }
        public string GenerateToken()
        {
            return GenerateSecureToken._GenerateSecureToken();

        }
        public void SetTokenCookie(HttpResponse response, string token)
        {
            var encrypted= _dataProtector.Protect(token);
            response.Cookies.Append(CookieName, encrypted, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite=SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)

            });
        }
        public string? GetTokenFromCookie(HttpRequest httpRequest)
        {
            if(!httpRequest.Cookies.TryGetValue(CookieName, out var encrypted))
                return null;
            try
            {
                return _dataProtector.Unprotect(encrypted);
            }
            catch { 
                return null;
            }
        }

        public void ClearTokenCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieName);
        }
    }
}
