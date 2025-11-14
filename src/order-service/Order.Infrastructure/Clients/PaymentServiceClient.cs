using System.Net.Http;
using System.Threading.Tasks;
using Order.Application.Contracts;
using System.Runtime.CompilerServices;

namespace Order.Infrastructure.Clients
{
    public class PaymentServiceClient : IPaymentServiceClient
    {
        private readonly HttpClient _httpClient;

        public PaymentServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Phuơng thức gọi Payment API để cập nhật trạng thái Order (Sau khi Payment thành công)
        public async Task<bool> UpdateTransactionStatusAsync(int transactionId, int status)
        {
            // Gọi qua đường dẫn nội bộ: /payment-internal
            var url = $"/payment-internal/api/payment/transaction-status?transactionId={transactionId}&status={status}";

            var response = await _httpClient.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }

        // Phương thức gọi Payment API để yêu cầu hoàn tiền
        public async Task<bool> RequestRefundAsync(int transactionId)
        {
            // Gọi qua đường dẫn nội bộ: /payment-internal
            var url = $"/payment-internal/api/payment/refund/{transactionId}";

            var response = await _httpClient.PostAsync(url, null);

            // Chấp nhận 200 OK hoặc 202 Accepted (yêu cầu đang được xử lý)
            return response.IsSuccessStatusCode;
        }
    }
}
