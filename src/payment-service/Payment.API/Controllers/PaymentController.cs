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

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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
            var queryString = HttpContext.Request.QueryString.ToString();

            // Kiểm tra chữ ký
            var success = await _paymentService.HandleVnPayReturn(queryString);
            if (success)
            {
                return Ok("Thanh toán thành công. Đang xử lý đơn hàng.");
            }
            return BadRequest("Thanh toán thất bại hoặc chữ ký không hợp lệ.");
        }

        /*[HttpGet("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn()
        {
            var queryString = HttpContext.Request.QueryString.ToString();
            var (ok, rspCode, message) = await _paymentService.HandleVnPayIpnAsync(queryString);

            // VNPay yêu cầu JSON
            return Ok(new { RspCode = rspCode, Message = message });
        }*/

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
