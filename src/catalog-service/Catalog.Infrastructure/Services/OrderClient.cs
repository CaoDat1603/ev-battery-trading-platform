using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;


namespace Catalog.Infrastructure.Services
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

        public async Task<bool> IsTransactionCompleted(int transactionId, Product product, ProductStatus productType, CancellationToken ct)
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

                if (transaction.ProductId != product.ProductId)
                {
                    Console.WriteLine("[OrderClient] Transaction ProductId mismatch");
                    return false;
                }

                if (!string.Equals(transaction.transactionStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[OrderClient] Transaction status is not Success (3)");
                    return false;
                }

                if (transaction.basePrice != product.Price)
                {
                    Console.WriteLine($"[OrderClient] Transaction Amount mismatch. Expected: {product.Price}, Actual: {transaction.basePrice}");
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
