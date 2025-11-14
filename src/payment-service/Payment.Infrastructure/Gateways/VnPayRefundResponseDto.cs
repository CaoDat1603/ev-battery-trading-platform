using System.Text.Json.Serialization;

namespace Payment.Infrastructure.Gateways
{
    // DTO để ánh xạ phản hồi JSON từ API Hoàn tiền/Truy vấn (QueryDR & Refund) của VNPay
    public class VnPayRefundResponseDto
    {
        // Mã hệ thống VNPay phân biệt các yêu cầu
        [JsonPropertyName("vnp_ResponseId")]
        public string VnpResponseId { get; set; } = string.Empty;

        // Mã giao dịch truyền đi (vnp_TxnRef), được VNPay trả về
        [JsonPropertyName("vnp_TxnRef")]
        public string VnpTxnRef{ get; set; } = string.Empty;

        // Số tiền giao dịch (đã nhân 100)
        [JsonPropertyName("vnp_Amount")]
        public string VnpAmount { get; set; } = string.Empty;

        // Nội dung của yêu cầu hoàn tiền
        [JsonPropertyName("vnp_OrderInfo")]
        public string VnpOrderInfo { get; set; } = string.Empty;

        // Mã phản hồi từ VNPay (00: Thành công, khác 00: Thất bại hoặc lỗi)
        [JsonPropertyName("vnp_ResponseCode")]
        public string VnpResponseCode { get; set; } = string.Empty;

        // Thông tin lỗi chi tiết (nếu có lỗi)
        [JsonPropertyName("vnp_Message")]
        public string VnpMessage { get; set; } = string.Empty;

        // Mã ngân hàng hoặc mã Ví điện tử thanh toán
        [JsonPropertyName("vnp_BankCode")]
        public string VnpBankCode { get; set; } = string.Empty;

        // Mã giao dịch VNPay (để tham chiếu nội bộ VNPay)
        [JsonPropertyName("vnp_TransactionNo")]
        public string VnpTransactionNo { get; set; } = string.Empty;

        // Mã phân loại giao dịch tại hệ thống
        [JsonPropertyName("vnp_TransactionType")]
        public string VnpTransactionType { get; set; } = string.Empty;

        // Tình trạng của giao dịch tại cổng thanh toán VNPAY
        // (00: Thành công, 01: Chưa hoàn tất, 02: Giao dịch bị lỗi, ...)
        [JsonPropertyName("vnp_TransactionStatus")]
        public string VnpTransactionStatus { get; set; } = string.Empty;

        // Mã bảo mật (Hash) để kiểm tra tính toàn vẹn của dữ liệu nhận về
        [JsonPropertyName("vnp_SecureHash")]
        public string VnpSecureHash { get; set; } = string.Empty;

    }
}
