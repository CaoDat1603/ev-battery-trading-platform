using Payment.Application.Contracts;
using Payment.Domain.Abstraction;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Payment.Infrastructure.Clients
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IInternalTokenService _tokenService;

        // HttpClient được truyền vào qua DI (cấu hình trong Program.cs)
        public OrderServiceClient(HttpClient httpClient, IInternalTokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<bool> UpdateTransactionStatusAsync(int transactionId, int status, CancellationToken ct = default)
        {
            try
            {
                var token = await _tokenService.GetSystemTokenAsync(ct);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"http://kong:8000/api/internaltransaction/{transactionId}/status?newStatus={status}";

                var response = await _httpClient.PostAsync(url, null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Order API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NotifyRefundCompletionAsync(int transactionId, bool isSuccess, CancellationToken ct = default)
        {
            var token = await _tokenService.GetSystemTokenAsync(ct);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = $"http://kong:8000/api/internaltransaction/{transactionId}/refund-status?success={isSuccess}";

            var response = await _httpClient.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }

        public async Task<OrderTransactionDto?> GetTransactionByIdAsync(int transactionId, CancellationToken ct = default)
        {
            try
            {
                var token = await _tokenService.GetSystemTokenAsync(ct);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"http://kong:8000/api/internaltransaction/{transactionId}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OrderTransactionDto>();
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
