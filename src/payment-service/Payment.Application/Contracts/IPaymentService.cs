using Payment.Application.DTOs;

namespace Payment.Application.Contracts
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentUrl(CreatePaymentRequest request, string ipAddress);
        Task<bool> HandleVnPayReturn(string queryString);
        Task<bool> InitiateRefund(int transactionId, string ipAddress);
        Task<IEnumerable<PaymentDto>> GetPaymentsByTransactionIdAsync(int transactionId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(); // Cho Admin
        Task<VnPayIpnResponse> HandleVnPayIpnAsync(string rawQuery);
    }
}
