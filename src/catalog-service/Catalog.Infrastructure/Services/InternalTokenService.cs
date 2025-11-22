using Catalog.Application.Abstractions;
using System.Text.Json;

namespace Catalog.Infrastructure.Services
{
    public class InternalTokenService : IInternalTokenService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public InternalTokenService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> GetSystemTokenAsync(CancellationToken ct)
        {
            var key = _config["Identity:InternalKey"];
            var req = new HttpRequestMessage(HttpMethod.Post, "http://identity-api:8080/api/systemtoken");
            req.Headers.Add("X-Internal-Key", key);

            var res = await _http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            var body = await res.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("token").GetString()!;
        }
    }
}
