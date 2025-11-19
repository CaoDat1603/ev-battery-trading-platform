using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static Payment.Domain.Entities.Payment;

namespace Payment.Infrastructure
{
    // Lớp xử lý các thuật toán và logic tạo URL của VNPAY
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }
            var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
            var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash =
                collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value; //hash của dữ liệu trả về
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            var checkSignature =
                vnPay.ValidateSignature(vnpSecureHash, hashSecret); //check Signature
            if (!checkSignature)
                return new PaymentResponseModel()
                {
                    Success = false
                };
            return new PaymentResponseModel()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId.ToString(),
                PaymentId = vnPayTranId.ToString(),
                TransactionId = vnPayTranId.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }
        // Sử dụng để thêm dữ liệu vào request
        public void AddRequestData(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        // Dùng để thêm dữ liệu phản hồi (return url)
        public void AddResponseData(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        // Lấy giá trị data phản hồi
        public string GetResponseData(string key)
        {
            return _responseData.ContainsKey(key) ? _responseData[key] : string.Empty;
        }

        public void ResetResponseData()
        {
            _responseData.Clear();
        }

        // Reset request data
        public void ResetRequestData()
        {
            _requestData.Clear();
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2")); // Lowercase hex format
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }

        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }


        // Verify return/ipn
        public bool ValidateSecureHash(string hashSecret)
        {
            if (!_responseData.TryGetValue("vnp_SecureHash", out var received) || string.IsNullOrEmpty(received))
                return false;

            var signData = BuildSignData(_responseData);
            var expected = HmacSha512(hashSecret, signData);
            return expected.Equals(received, StringComparison.OrdinalIgnoreCase);
        }

        public string CreateRefundRequestUrl(string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var key in _requestData.Keys)
            {
                // Các tham số không cần mã hóa URL cho API này (chỉ cần nối chuỗi)
                data.Append(key + "=" + _requestData[key] + "&");
            }

            var checkSum = HmacSha512(hashSecret, data.ToString().Remove(data.Length - 1, 1));
            data.Append("vnp_SecureHash=").Append(checkSum);

            return data.ToString();
        }

        //1) Chuỗi dùng để KÝ: KHÔNG encode, bỏ vnp_SecureHash & vnp_SecureHashType
        private string BuildSignData(IDictionary<string, string> data)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                    continue;

                sb.Append(kv.Key).Append('=').Append(kv.Value).Append('&');
            }
            if (sb.Length > 0) sb.Length--;
            return sb.ToString();
        }

        //2) Chuỗi đưa lên URL: phải URL-encode UTF8
        private static string Encode(string s) => HttpUtility.UrlEncode(s, Encoding.UTF8);

        // Tạo URL đầy đủ: ? + query + &vnp_SecureHash=... (đã encode)
        public string CreateSignedUrl(string baseUrl, string hashSecret, bool includeHashType = true)
        {
            var signData = BuildSignData(_requestData);
            var signature = HmacSha512(hashSecret, signData);

            // DEBUG để đối chiếu với portal/tool ngoài
            Console.WriteLine("[VNPay][SIGNDATA] " + signData);
            Console.WriteLine("[VNPay][HASH]     " + signature);

            var q = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                    continue;

                q.Append(Encode(kv.Key)).Append('=').Append(Encode(kv.Value)).Append('&');
            }
            q.Append("vnp_SecureHash=").Append(signature);

            return $"{baseUrl}?{q}";
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
