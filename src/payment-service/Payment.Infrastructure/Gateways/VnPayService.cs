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

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var returnUrl = _configuration["VnPay:ReturnUrl"];
            var vnpayUrl = _configuration["VnPay:VnPayUrl"];

            if (string.IsNullOrWhiteSpace(tmnCode)) throw new InvalidOperationException("VnPay TmnCode is not configured.");
            if (string.IsNullOrWhiteSpace(hashSecret)) throw new InvalidOperationException("VnPay HashSecret is not configured.");
            if (string.IsNullOrWhiteSpace(returnUrl)) throw new InvalidOperationException("VnPay ReturnUrl is not configured.");
            if (string.IsNullOrWhiteSpace(vnpayUrl)) throw new InvalidOperationException("VnPay VnPayUrl is not configured.");

            //0. Reset data nếu thư viện tái sử dụng 1 instance
            _vnPayLibrary.ResetRequestData();

            //1. Giờ Việt Nam (GMT+7)
            var tzId = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                ? "SE Asia Standard Time" // Windows
                : "Asia/Ho_Chi_Minh"; // Linux
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var nowVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);

            // Dùng để phân biệt unique cho TxnRef
            var tick = DateTime.Now.Ticks.ToString();

            //3. Thêm các tham số bắt buộc khi dùng VNPAY
            _vnPayLibrary.AddRequestData("vnp_Version", "2.1.0");
            _vnPayLibrary.AddRequestData("vnp_Command", "pay");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", tmnCode);
            _vnPayLibrary.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            _vnPayLibrary.AddRequestData("vnp_CreateDate", nowVN.ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
            _vnPayLibrary.AddRequestData("vnp_IpAddr", _vnPayLibrary.GetIpAddress(context));
            _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
            _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
            _vnPayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
            // Set expire date to 15 minutes from now in VNPAY format
            _vnPayLibrary.AddRequestData("vnp_ExpireDate", nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_TxnRef", tick);

            //4. Sinh URL đã sắp xếp + ký chuẩn (thêm vnp_SecureHashType cho chắc)
            var paymentUrl = _vnPayLibrary.CreateRequestUrl(vnpayUrl, hashSecret);

            //5. Kết quả cuối
            return paymentUrl;
        }

        // Overload dùng trong nghiệp vụ nội bộ: đã có PaymentId và Amount, chỉ cần ipAddress
        public string CreatePaymentUrl(int paymentId, decimal amount, string ipAddress, out string vnPayCreateDate)
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

            vnPayCreateDate = nowVN.ToString("yyyyMMddHHmmss");

            _vnPayLibrary.AddRequestData("vnp_Version", "2.1.0");
            _vnPayLibrary.AddRequestData("vnp_Command", "pay");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", tmnCode);
            _vnPayLibrary.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            _vnPayLibrary.AddRequestData("vnp_CreateDate", vnPayCreateDate);
            _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
            _vnPayLibrary.AddRequestData("vnp_IpAddr", ipAddress);
            _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
            _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"Thanh toan giao dich {paymentId}");
            _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
            _vnPayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
            _vnPayLibrary.AddRequestData("vnp_ExpireDate", nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            //_vnPayLibrary.AddRequestData("vnp_TxnRef", paymentId.ToString());

            // vnp_TxnRef must be the payment id so return/IPN can map it back
            var txnRef = paymentId.ToString();
            _vnPayLibrary.AddRequestData("vnp_TxnRef", txnRef);

            return _vnPayLibrary.CreateRequestUrl(vnpayUrl, hashSecret);
        }

        public bool ValidateSignature(string queryString)
        {
            var hashSecret = _configuration["VnPay:HashSecret"];
            if (string.IsNullOrEmpty(hashSecret)) { return false; }

            // Mỗi request mới thì reset response data
            _vnPayLibrary.ResetResponseData();

            // Phân tích query string
            var data = HttpUtility.ParseQueryString(queryString);

            // Lấy secure hash VNPay gửi về
            var secureHash = data["vnp_SecureHash"];
            if (string.IsNullOrEmpty(secureHash)) { return false; }

            // Thêm tất cả tham số vào thư viện (trừ vnp_SecureHash)
            foreach (var key in data.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (!key.StartsWith("vnp_")) continue;
                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)) continue;

                _vnPayLibrary.AddResponseData(key, data[key]);
            }

            // Validate Hash (thư viện sẽ lấy vnp_SecureHash từ _responseData)
            return _vnPayLibrary.ValidateSignature(secureHash, hashSecret);
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

            // Reset request data trước khi tạo mới
            _vnPayLibrary.ResetRequestData();

            //1. Thêm các tham số bắt buộc khi dùng VNPAY Refund
            _vnPayLibrary.AddRequestData("vnp_RequestId", Guid.NewGuid().ToString().Replace("-", ""));
            _vnPayLibrary.AddRequestData("vnp_Version", "2.1.0");
            _vnPayLibrary.AddRequestData("vnp_Command", "refund");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", tmnCode);
            _vnPayLibrary.AddRequestData("vnp_TransactionType", "02"); // 02: Hoàn tiền toàn phần
            _vnPayLibrary.AddRequestData("vnp_TxnRef", paymentId.ToString());
            _vnPayLibrary.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"Hoan tien giao dich {paymentId}");
            _vnPayLibrary.AddRequestData("vnp_TransactionNo", ""); // Số giao dịch gốc, để trống nếu không có
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

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var hashSecret = _configuration["VnPay:HashSecret"] ?? string.Empty;
            return _vnPayLibrary.GetFullResponseData(collections, hashSecret);
        }
    }
}
