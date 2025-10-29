namespace Identity.Application.Contracts
{
    public interface IRefreshTokenService
    {
        string GenerateToken();
        void SetTokenCookie(HttpResponse response, string token);
        string? GetTokenFromCookie(HttpRequest request);
        void ClearTokenCookie(HttpResponse response);
    }
}
