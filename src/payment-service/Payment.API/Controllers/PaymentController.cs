using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Contracts;
using Payment.Application.DTOs;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Tất cả API đều cần xác thực (ngoại trừ callback)
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        // POST /api/payment/create
        [HttpPost("create")]
        //[Authorize(Policy = AuthorizationPolicies.MemberOnly)] // Chỉ Member
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var paymentUrl = await _paymentService.CreatePaymentUrl(request, ipAddress);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PAYMENT][CREATE] " + ex);
                return Problem(title: "Cannot create payment", detail: ex.Message, statusCode: 500);
            }
        }

        // GET /api/payment/by-transaction/{transactionId}
        [HttpGet("by-transaction/{transactionId}")]
        //[Authorize(Policy = AuthorizationPolicies.MemberOrAdmin)] // Member owns or Admin
        public async Task<IActionResult> GetPaymentsByTransaction(int transactionId)
        {
            var payments = await _paymentService.GetPaymentsByTransactionIdAsync(transactionId);
            return Ok(payments);
        }

        // GET /api/payment/ (Admin)
        [HttpGet]
        //[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        // --- ENDPOINT CÔNG KHAI (Cho VNPay) ---
        // GET /api/payment/vnpay-return
        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturnUrl()
        {
            // Query gốc từ VNPay (chứa vnp_* params)
            var queryString = HttpContext.Request.QueryString.ToString(); // ví dụ: ?vnp_Amount=...

            // Xử lý & cập nhật trạng thái Payment + Transaction
            var (success, transactionId) = await _paymentService.HandleVnPayReturn(queryString);

            // Build URL frontend /payment-result
            var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var baseUrl = frontendBase.TrimEnd('/');
            const string path = "/payment-result";

            // Giữ nguyên tất cả vnp_* params để FE đọc, bổ sung transactionId
            var qs = queryString.TrimStart('?'); // bỏ dấu ?
            string redirectUrl;

            if (transactionId.HasValue)
            {
                redirectUrl = string.IsNullOrEmpty(qs)
                    ? $"{baseUrl}{path}?transactionId={transactionId.Value}"
                    : $"{baseUrl}{path}?transactionId={transactionId.Value}&{qs}";
            }
            else
            {
                // fallback: vẫn redirect, FE sẽ tự báo lỗi
                redirectUrl = string.IsNullOrEmpty(qs)
                    ? $"{baseUrl}{path}"
                    : $"{baseUrl}{path}?{qs}";
            }

            return Redirect(redirectUrl);
        }

        [HttpGet("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn()
        {
            var queryString = HttpContext.Request.QueryString.Value ?? string.Empty;

            var result = await _paymentService.HandleVnPayIpnAsync(queryString);

            // VNPay yêu cầu JSON
            return new JsonResult(result);
        }

        // --- ENDPOINT NỘI BỘ (Cho Order Service dùng Key-Auth) ---
        // POST /api/payment/refund/{transactionId}
        [HttpPost("refund/{transactionId}")]
        //[Authorize(Policy = AuthorizationPolicies.InternalOnly)]
        public async Task<IActionResult> RequestRefund(int transactionId)
        {
            var success = await _paymentService.InitiateRefund(transactionId);
            if (success)
            {
                return Accepted(new { message = "Refund request accepted and is being processed." });
            }
            return BadRequest(new { message = "Refund not possible or transaction not found." });
        }
    }
}
