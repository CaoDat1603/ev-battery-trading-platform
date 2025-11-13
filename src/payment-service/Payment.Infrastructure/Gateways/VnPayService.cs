using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Abstraction;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using static Payment.Domain.Entities.Payment;

namespace Payment.Infrastructure.Gateways
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        // Triển khai VnPayLibrary dựa trên tài liệu VNPay
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly HttpClient _httpClient; // HttpClient để gọi API Refund

        public VnPayService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _vnPayLibrary = new VnPayLibrary();
            _httpClient = httpClient;
        }

        // Overload dùng trong nghiệp vụ nội bộ: đã có PaymentId và Amount, chỉ cần ipAddress
        public string CreatePaymentUrl(int paymentId, decimal amount, string ipAddress)
        {
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var returnUrl = _configuration["VnPay:ReturnUrl"];
            var vnpayUrl = _configuration["VnPay:VnPayUrl"];

            if (string.IsNullOrWhiteSpace(tmnCode)) throw new InvalidOperationException("VnPay TmnCode is not configured.");
            if (string.IsNullOrWhiteSpace(hashSecret)) throw new InvalidOperationException("VnPay HashSecret is not configured.");
            if (string.IsNullOrWhiteSpace(returnUrl)) throw new InvalidOperationException("VnPay ReturnUrl is not configured.");
            if (string.IsNullOrWhiteSpace(vnpayUrl)) throw new InvalidOperationException("VnPay VnPayUrl is not configured.");

            _vnPayLibrary.ResetRequestData();

            var tzId = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                ? "SE Asia Standard Time"
                : "Asia/Ho_Chi_Minh";
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var nowVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);

            _vnPayLibrary.AddRequestData("vnp_Version", "2.1.0");
            _vnPayLibrary.AddRequestData("vnp_Command", "pay");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", tmnCode);
            _vnPayLibrary.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            _vnPayLibrary.AddRequestData("vnp_CreateDate", nowVN.ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
            _vnPayLibrary.AddRequestData("vnp_IpAddr", ipAddress);
            _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
            _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"Thanh toan giao dich {paymentId}");
            _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
            _vnPayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
            _vnPayLibrary.AddRequestData("vnp_ExpireDate", nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_TxnRef", paymentId.ToString());

            return _vnPayLibrary.CreateSignedUrl(vnpayUrl, hashSecret);
        }

        public bool ValidateSignature(string queryString)
        {
            var hashSecret = _configuration["VnPay:HashSecret"];

            // Phân tích query string
            var data = HttpUtility.ParseQueryString(queryString);
            foreach (var key in data.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                _vnPayLibrary.AddResponseData(key, data[key]);
            }

            // Validate Hash (thư viện sẽ lấy vnp_SecureHash từ _responseData)
            return _vnPayLibrary.ValidateSecureHash(hashSecret!);
        }

        public Dictionary<string, string> GetResponseData(string queryString)
        {
            var data = HttpUtility.ParseQueryString(queryString);
            var result = new Dictionary<string, string>();

            foreach (var key in data.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                var value = data[key] ?? string.Empty;
                result.Add(key, value);
            }
            return result;
        }

        public async Task<string> RequestVnPayRefundAsync(int paymentId, decimal amount)
        {
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var refundApiUrl = _configuration["VnPay:RefundApiUrl"];

            if (string.IsNullOrWhiteSpace(tmnCode)) throw new InvalidOperationException("VnPay TmnCode is not configured.");
            if (string.IsNullOrWhiteSpace(hashSecret)) throw new InvalidOperationException("VnPay HashSecret is not configured.");
            if (string.IsNullOrWhiteSpace(refundApiUrl)) throw new InvalidOperationException("VnPay RefundApiUrl is not configured.");

            //1. Thêm các tham số bắt buộc khi dùng VNPAY Refund
            _vnPayLibrary.AddRequestData("vnp_RequestId", Guid.NewGuid().ToString().Replace("-", ""));
            _vnPayLibrary.AddRequestData("vnp_Version", "2.1.0");
            _vnPayLibrary.AddRequestData("vnp_Command", "refund");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", tmnCode);
            _vnPayLibrary.AddRequestData("vnp_TransactionType", "02"); // 02: Hoàn tiền toàn phần
            _vnPayLibrary.AddRequestData("vnp_TxnRef", paymentId.ToString());
            _vnPayLibrary.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"Hoan tien giao dich {paymentId}");
            // Use current time as transaction date
            _vnPayLibrary.AddRequestData("vnp_TransactionDate", DateTimeOffset.Now.ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_CreateBy", "admin");
            _vnPayLibrary.AddRequestData("vnp_CreateDate", DateTimeOffset.Now.ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_IpAddr", "127.0.0.1");

            // Tạo chuỗi Request và Hash
            string requestString = _vnPayLibrary.CreateRefundRequestUrl(hashSecret);

            // Gửi yêu cầu POST tới API của VNPay
            var content = new StringContent(requestString, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(refundApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var respDto = await response.Content.ReadFromJsonAsync<Payment.Infrastructure.Gateways.VnPayRefundResponseDto>();

                if (respDto != null)
                {
                    // --- BƯỚC KIỂM TRA CHỮ KÝ PHẢN HỒI ---

                    // 1. Reset thư viện và chuẩn bị kiểm tra Hash
                    _vnPayLibrary.ResetResponseData();

                    // 2. Lấy tất cả các thuộc tính của DTO để kiểm tra Hash
                    var properties = typeof(Payment.Infrastructure.Gateways.VnPayRefundResponseDto).GetProperties();

                    foreach (var prop in properties)
                    {
                        var key = "";
                        var value = prop.GetValue(respDto)?.ToString();

                        // Lấy tên gốc của tham số VNPay từ JsonPropertyName
                        var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                        key = jsonPropertyAttribute != null ? jsonPropertyAttribute.Name : prop.Name;

                        // Thêm toàn bộ tham số (bao gồm cả vnp_SecureHash) để ValidateSecureHash tự lấy và so sánh
                        if (!string.IsNullOrEmpty(key))
                        {
                            _vnPayLibrary.AddResponseData(key, value);
                        }
                    }

                    // 4. Kiểm tra Hash dựa trên secret
                    bool isValidHash = _vnPayLibrary.ValidateSecureHash(hashSecret!);

                    if (!isValidHash)
                    {
                        Console.WriteLine($"SECURITY ALERT: Invalid SecureHash received for Refund Transaction {respDto.VnpTxnRef}");
                        return "97";
                    }

                    // Trả về mã phản hồi từ VNPay (vnp_ResponseCode) nếu Hash hợp lệ
                    return respDto?.VnpResponseCode ?? "99";

                }

                return "99";
            }

            return "99"; // lỗi hệ thống nội bộ
        }
    }
}
