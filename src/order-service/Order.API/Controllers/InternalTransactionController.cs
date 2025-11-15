using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Contracts;
using Order.Domain.Enums;

namespace Order.API.Controllers
{
    // --- API NỘI BỘ ---
    [Route("api/[controller]")]
    [ApiController]
    public class InternalTransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public InternalTransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // Endpoint lấy chi tiết giao dịch theo ID
        // GET /api/internaltransaction/{id}
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer,SystemBearer", Roles = "Admin,System")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdForInternalAsync(id);
            if (transaction == null) return NotFound("Transaction not found.");
            return Ok(transaction);
        }

        // Endpoint cập nhật trạng thái giao dịch
        [HttpPost("{id}/status")]
        [Authorize(AuthenticationSchemes = "Bearer,SystemBearer", Roles = "Admin,System")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] TransactionStatus newStatus)
        {
            var success = await _transactionService.UpdateTransactionStatus(id, newStatus);
            if (!success) return NotFound();
            return Ok();
        }

        // Endpoint nhận thông báo HOÀN TIỀN từ Payment Service
        // POST /api/internaltransaction/{id}/refund-status
        [HttpPost("{id}/refund-status")]
        [Authorize(AuthenticationSchemes = "Bearer,SystemBearer", Roles = "Admin,System")]
        public async Task<IActionResult> ReceiveRefundStatus(int id, [FromQuery] bool success)
        {
            if (success)
            {
                var updateSuccess = await _transactionService.UpdateTransactionStatus(id, TransactionStatus.Cancelled);
                if (!updateSuccess) return BadRequest("Order status update failed after refund.");

                return Ok("Transaction status updated to Cancelled.");
            }
            else
            {
                Console.WriteLine($"WARNING: Refund failed for Transaction {id} in Payment Service.");
                return BadRequest("Refund failed in Payment Service.");
            }
        }
    }
}
