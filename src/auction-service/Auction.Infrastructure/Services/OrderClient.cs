using Auction.Application.Abstractions;
using Auction.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;


namespace Auction.Infrastructure.Services
{
    public class OrderClient : IOrderClient
    {
        private readonly HttpClient _http;
        private readonly IInternalTokenService _tokenService;

        public OrderClient(HttpClient http, IInternalTokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        public async Task<bool> IsTransactionCompleted(int transactionId, decimal Amount, CancellationToken ct)
        {
            try
            {
                // 1. Lấy Token
                var token = await _tokenService.GetSystemTokenAsync(ct);
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // 3. Lấy transaction
                var transUrl = $"http://kong:8000/api/internaltransaction/{transactionId}";
                Console.WriteLine($"[OrderClient] Calling Transaction URL: {transUrl}");

                var transResponse = await _http.GetAsync(transUrl, ct);
                if (!transResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[OrderClient] Transaction API returned {transResponse.StatusCode}");
                    return false;
                }

                var transaction = await transResponse.Content.ReadFromJsonAsync<TransactionDto>(cancellationToken: ct);
                if (transaction == null)
                {
                    Console.WriteLine("[OrderClient] Transaction response is null");
                    return false;
                }

                if (transaction.basePrice != Amount)
                {
                    Console.WriteLine($"[OrderClient] Transaction Amount mismatch. Expected: {Amount}, Actual: {transaction.basePrice}");
                    return false;
                }

                Console.WriteLine("[OrderClient] Transaction verified successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderClient] Exception: {ex.Message}");
                return false;
            }
        }
    }
}
