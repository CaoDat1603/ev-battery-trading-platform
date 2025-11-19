using System.Net.Http;
using static Payment.Domain.Entities.Payment;
using Microsoft.AspNetCore.Http;

namespace Payment.Domain.Abstraction
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        string CreatePaymentUrl(int paymentId, decimal amount, string ipAddress, out string vnPayCreateDate);

        // Kiểm tra chữ ký bảo mật từ VNPAY gửi về
        bool ValidateSignature(string queryString);

        // Trích xuất dữ liệu trả về từ VNPAY
        Dictionary<string, string> GetResponseData(string queryString);

        // Gửi yêu cầu hoàn tiền đến VNPAY
        Task<string> RequestVnPayRefundAsync(int paymentId, decimal amount);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
