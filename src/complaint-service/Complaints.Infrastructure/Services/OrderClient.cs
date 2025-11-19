using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Complaints.Infrastructure.Services
{
    public class OrderClient : IOrderClient
    {
        private HttpClient _httpClient;
        private IInternalTokenService _tokenService;
        public OrderClient(HttpClient httpClient, IInternalTokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<TransactionInfoDto> GetTransactionInfoAsync(int tranId, CancellationToken ct = default)
        {
            try
            {
                var token = await _tokenService.GetSystemTokenAsync(ct);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"http://kong:8000/api/internaltransaction/{tranId}";

                var res = await _httpClient.GetAsync(url);

                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync(ct);

                    var transaction = JsonSerializer.Deserialize<TransactionInfoDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    });

                    if (transaction == null)
                        throw new InvalidOperationException("Cannot parse user info response.");

                    return transaction;
                }
                Console.WriteLine($"Ko gọi được {url.ToString()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Order API (GetTransaction): {ex.Message}");
                return null;
            }
        }
    }
}
