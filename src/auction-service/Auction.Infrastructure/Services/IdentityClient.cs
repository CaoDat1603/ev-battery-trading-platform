using Auction.Application.Abstractions;
using System.Net;
using System.Net.Http.Headers;


namespace Auction.Infrastructure.Services
{
    public class IdentityClient : IIdentityClient
    {
        private readonly HttpClient _http;
        private readonly IInternalTokenService _tokenService;

        public IdentityClient(HttpClient http, IInternalTokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        public async Task<bool> UserExistsAsync(int userId, CancellationToken ct)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"http://kong:8000/api/admin?userId={userId}", ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return false;

            res.EnsureSuccessStatusCode(); // nếu lỗi khác 404 thì throw

            return true;
        }
    }
}
