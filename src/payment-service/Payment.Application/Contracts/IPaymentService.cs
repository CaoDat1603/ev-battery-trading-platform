using Payment.Application.DTOs;

namespace Payment.Application.Contracts
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentUrl(CreatePaymentRequest request, string ipAddress);
        Task<bool> HandleVnPayReturn(string queryString);
        Task<bool> InitiateRefund(int transactionId);
        Task<IEnumerable<PaymentDto>> GetPaymentsByTransactionIdAsync(int transactionId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(); // Cho Admin
        //Task<(bool ok, string rspCode, string message)> HandleVnPayIpnAsync(string queryString);
    }
}
