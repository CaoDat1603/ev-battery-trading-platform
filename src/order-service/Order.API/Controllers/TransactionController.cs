using Order.Application.Contracts;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // --- Phương thức Helper để lấy UserId từ Token ---
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value; // Fallback to JWT 'sub'

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId;
        }

        // Endpoint tạo giao dịch
        // POST /api/transaction/create
        [HttpPost("create")]
        // Chỉ cho phép Member tạo giao dịch
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            var buyerId = GetCurrentUserId();
            var transactionId = await _transactionService.CreateNewTransaction(request, buyerId);
            // Có thể trả về URL để redirect đến trang thanh toán của PaymentService
            return Ok(new { TransactionId = transactionId });
        }

        // GET /api/transaction/my-purchases
        [HttpGet("my-purchases")]
        // Chỉ cho phép Member xem giao dịch của mình
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMyPurchases()
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetTransactionsByBuyerAsync(userId);
            return Ok(transactions);
        }

        // GET /api/transaction/my-sales
        [HttpGet("my-sales")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMySales() {
            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetTransactionsBySellerAsync(userId);
            return Ok(transactions);
        }

        // GET /api/transaction/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            var currentUserId = GetCurrentUserId();

            var transaction = await _transactionService.GetTransactionByIdAsync(id, currentUserId);
            if (transaction == null) return NotFound("Transaction not found or you do not have permission.");
            return Ok(transaction);
        }

        // Endpoint hủy giao dịch
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CancelTransaction(int id)
        {
            var success = await _transactionService.CancelTransaction(id);
            if (!success) return BadRequest("Unable to cancel transaction. Check if it's already completed.");
            return Ok("Transaction cancellation request processed.");
        }
        
        // GET /api/transaction/ (Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            return Ok(transactions);
        }

        //[Authorize]
        [HttpGet("debug/me")]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new
            {
                name = User.Identity?.Name,
                roles = User.Claims.Where(x => x.Type == "role" || x.Type.EndsWith("/role")).Select(x => x.Value),
                all = claims
            });
        }
    }
}