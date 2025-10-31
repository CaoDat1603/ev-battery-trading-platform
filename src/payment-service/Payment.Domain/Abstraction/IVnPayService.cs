namespace Payment.Domain.Abstraction
{
    public interface IVnPayService
    {
        // Tạo URL thanh toán
        string CreatePaymentUrl(int paymentId, decimal amount, string ipAddress);

        // Kiểm tra chữ ký bảo mật từ VNPAY gửi về
        bool ValidateSignature(string queryString);

        // Trích xuất dữ liệu trả về từ VNPAY
        Dictionary<string, string> GetResponseData(string queryString);
    }
}
