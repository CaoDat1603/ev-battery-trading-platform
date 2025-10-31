using Order.Application.Contracts;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("create")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            var transactionId = await _transactionService.CreateNewTransaction(request);
            // Có thể trả về URL để redirect đến trang thanh toán của PaymentService
            return Ok(new { TransactionId = transactionId });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] TransactionStatus newStatus)
        {
            var success = await _transactionService.UpdateTransactionStatus(id, newStatus);
            if (!success) return NotFound();
            return Ok();
        }
    }
}