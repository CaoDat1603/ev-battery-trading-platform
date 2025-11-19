using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace Complaints.Infrastructure.Services
{
    public class CatalogClient : ICatalogClient
    {
        private HttpClient _http;
        private readonly IInternalTokenService _tokenService;
        public CatalogClient (HttpClient httpClient, IInternalTokenService tokenService)
        {
            _http = httpClient;
            _tokenService = tokenService;
        }
        public async Task<bool> ProductExistsAsync(int productId, CancellationToken ct)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"http://kong:8000/api/products?productId={productId}", ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return false;

            res.EnsureSuccessStatusCode(); // nếu lỗi khác 404 thì throw

            return true;
        }
        public async Task<ProductInfoDto> GetProductInfoAsync(int productId, CancellationToken ct = default)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"http://kong:8000/api/products?productId={productId}", ct);

            if (res.StatusCode == HttpStatusCode.NotFound)
                throw new ArgumentException($"User with ID {productId} not found.");

            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync(ct);

            var product = JsonSerializer.Deserialize<ProductInfoDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });

            if (product == null)
                throw new InvalidOperationException("Cannot parse user info response.");

            return product;
        }
    }
}
