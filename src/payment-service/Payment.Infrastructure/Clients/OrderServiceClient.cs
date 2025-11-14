using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Payment.Application.Contracts;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Payment.Infrastructure.Clients
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;

        // HttpClient được truyền vào qua DI (cấu hình trong Program.cs)
        public OrderServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> UpdateTransactionStatusAsync(int transactionId, int status)
        {
            try
            {
                // Khớp với Order.API: POST /api/transaction/{id}/status?newStatus={status}
                var url = $"/api/transaction/{transactionId}/status?newStatus={status}";

                var response = await _httpClient.PostAsync(url, null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Order API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NotifyRefundCompletionAsync(int transactionId, bool isSuccess)
        {
            // Khớp với Order.API: POST /api/transaction/{id}/refund-status?success={isSuccess}
            var url = $"/api/transaction/{transactionId}/refund-status?success={isSuccess}";

            var response = await _httpClient.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }

        public async Task<OrderTransactionDto?> GetTransactionByIdAsync(int transactionId)
        {
            try
            {
                var url = $"/api/transaction/{transactionId}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OrderTransactionDto>();
                }

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
