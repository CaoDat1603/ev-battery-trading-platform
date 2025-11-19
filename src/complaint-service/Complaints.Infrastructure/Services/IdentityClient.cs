using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Complaints.Infrastructure.Services
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
        public async Task<UserInfoDto> GetUserInfoAsync(int userId, CancellationToken ct = default)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"http://kong:8000/api/admin?userId={userId}", ct);

            if (res.StatusCode == HttpStatusCode.NotFound)
                throw new ArgumentException($"User with ID {userId} not found.");

            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync(ct);

            var user = JsonSerializer.Deserialize<UserInfoDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });

            if (user == null)
                throw new InvalidOperationException("Cannot parse user info response.");

            return user;
        }
        public async Task<List<UserInfoDto>> GetUsersInfoAsync(List<int> ids, CancellationToken ct)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.PostAsJsonAsync("http://kong:8000/api/admin/batch", ids, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<UserInfoDto>>(cancellationToken: ct);
        }
    }
}
