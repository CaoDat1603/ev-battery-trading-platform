using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Auction.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionQueries _queries;
        private readonly IAuctionCommand _commands;

        public AuctionsController(IAuctionQueries queries, IAuctionCommand commands)
        {
            _queries = queries;
            _commands = commands;
        }

        // ======================
        // 🔹 QUERIES
        // ======================

        /// <summary>
        /// Health check for the auction service
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            Response.Headers["Cache-Control"] = "public, max-age=30";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(30).ToString("R");
            return Ok(new { ok = true, svc = "auction" });
        }

        /// <summary>
        /// Get all auctions (cached for 60s)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAuctions(CancellationToken ct)
        {
            Response.Headers["Cache-Control"] = "public, max-age=60";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

            var result = await _queries.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Get auction by ID
        /// </summary>
        [HttpGet("{auctionId:int}")]
        public async Task<IActionResult> GetAuctionById(int auctionId, CancellationToken ct)
        {
            var result = await _queries.SearchByAuctionIDAsync(auctionId, ct);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Auction not found" });

            return Ok(result.First());
        }

        /// <summary>
        /// Search auctions by product ID
        /// </summary>
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
        {
            var result = await _queries.SearchByProductAsync(productId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Search auctions by winner ID
        /// </summary>
        [HttpGet("winner/{winnerId:int}")]
        public async Task<IActionResult> GetByWinner(int winnerId, CancellationToken ct)
        {
            var result = await _queries.SearchByWinnerAsync(winnerId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Search auctions by transaction ID
        /// </summary>
        [HttpGet("transaction/{transactionId:int}")]
        public async Task<IActionResult> GetByTransaction(int transactionId, CancellationToken ct)
        {
            var result = await _queries.SearchByTransactionAsync(transactionId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Search or filter auctions with pagination
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAuctions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int? productId = null,
            [FromQuery] int? winnerId = null,
            [FromQuery] string? sellerEmail = null,
            [FromQuery] string? sellerPhone = null,
            [FromQuery] int? transactionId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] DateTimeOffset? startTime = null,
            [FromQuery] DateTimeOffset? endTime = null,
            [FromQuery] AuctionStatus? status = null,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedAsync(
                pageNumber, pageSize, sortBy,
                productId, winnerId, sellerEmail, sellerPhone,
                transactionId, minPrice, maxPrice,
                startTime, endTime, status, ct);

            return Ok(result);
        }

        /// <summary>
        /// Get the total count of auctions matching filters
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetAuctionCount(
            [FromQuery] int? productId = null,
            [FromQuery] int? winnerId = null,
            [FromQuery] string? sellerEmail = null,
            [FromQuery] string? sellerPhone = null,
            [FromQuery] int? transactionId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] DateTimeOffset? startTime = null,
            [FromQuery] DateTimeOffset? endTime = null,
            [FromQuery] AuctionStatus? status = null,
            CancellationToken ct = default)
        {
            var result = await _queries.GetAuctionCountAsync(
                productId, winnerId, sellerEmail, sellerPhone,
                transactionId, minPrice, maxPrice,
                startTime, endTime, status, ct);

            return Ok(result);
        }


        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Create a new auction
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _commands.CreateAuctionAsync(dto, ct);
            return CreatedAtAction(nameof(GetAuctionById), new { auctionId = id }, new { AuctionId = id });
        }

        /// <summary>
        /// Update auction status (e.g. Pending → Active → Ended)
        /// </summary>
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateAuctionStatus([FromBody] UpdateAuctionStatusRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _commands.UpdateAuctionStatusAsync(request.AuctionId, request.AuctionStatus, ct);
            if (!success)
                return NotFound(new { message = "Auction not found or update failed" });

            return Ok(new { message = "Auction status updated successfully" });
        }

        /// <summary>
        /// Update current auction price (when a new valid bid is placed)
        /// </summary>
        [HttpPatch("{auctionId:int}/price")]
        public async Task<IActionResult> UpdateCurrentPrice(int auctionId, [FromQuery] decimal newPrice, CancellationToken ct)
        {
            if (newPrice <= 0)
                return BadRequest(new { message = "Invalid bid amount" });

            var success = await _commands.UpdateCurrentPrice(auctionId, newPrice, ct);
            if (!success)
                return NotFound(new { message = "Auction not found or price update failed" });

            return Ok(new { message = "Current price updated successfully" });
        }

        /// <summary>
        /// Update seller contact information
        /// </summary>
        [HttpPatch("{auctionId:int}/seller")]
        public async Task<IActionResult> UpdateSellerContact(
            int auctionId,
            [FromQuery] string? newEmail,
            [FromQuery] string? newPhone,
            CancellationToken ct)
        {
            var success = await _commands.UpdateSellerContact(auctionId, newEmail, newPhone, ct);
            if (!success)
                return NotFound(new { message = "Auction not found or update failed" });

            return Ok(new { message = "Seller contact updated successfully" });
        }

        /// <summary>
        /// Mark auction as completed and link with a transaction
        /// </summary>
        [HttpPatch("{auctionId:int}/complete")]
        public async Task<IActionResult> CompleteAuction(
            int auctionId,
            [FromQuery] int transactionId,
            CancellationToken ct)
        {
            var success = await _commands.CompleteAuctionAsync(auctionId, transactionId, ct);
            if (!success)
                return NotFound(new { message = "Auction not found or completion failed" });

            return Ok(new { message = "Auction completed successfully" });
        }

        /// <summary>
        /// Delete an auction (soft delete)
        /// </summary>
        [HttpDelete("{auctionId:int}")]
        public async Task<IActionResult> DeleteAuction(int auctionId, CancellationToken ct)
        {
            if (auctionId <= 0)
                return BadRequest(new { message = "Invalid auction ID" });

            var success = await _commands.DeleteAuctionAsync(auctionId, ct);
            if (!success)
                return NotFound(new { message = "Auction not found" });

            return Ok(new { message = "Auction deleted successfully" });
        }
    }
}
