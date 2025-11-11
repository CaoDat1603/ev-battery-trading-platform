using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Auction.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidsController : ControllerBase
    {
        private readonly IBidQueries _queries;
        private readonly IBidCommand _commands;

        public BidsController(IBidQueries queries, IBidCommand commands)
        {
            _queries = queries;
            _commands = commands;
        }

        // ======================
        // 🔹 QUERIES
        // ======================

        /// <summary>
        /// Health check for the bid service
        /// </summary>
        [Authorize]
        [HttpGet("health")]
        public IActionResult Health()
        {
            Response.Headers["Cache-Control"] = "public, max-age=30";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(30).ToString("R");
            return Ok(new { ok = true, svc = "bid" });
        }

        /// <summary>
        /// Get all bids (cached for 60s)
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllBids(CancellationToken ct)
        {
            Response.Headers["Cache-Control"] = "public, max-age=60";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

            var result = await _queries.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Get bid details by ID
        /// </summary>
        [Authorize]
        [HttpGet("{bidId:int}")]
        public async Task<IActionResult> GetBidById(int bidId, CancellationToken ct)
        {
            var result = await _queries.SearchByBidIDAsync(bidId, ct);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Bid not found" });

            return Ok(result.First());
        }

        /// <summary>
        /// Get bids for a specific auction
        /// </summary>
        [Authorize]
        [HttpGet("auction/{auctionId:int}")]
        public async Task<IActionResult> GetByAuction(int auctionId, CancellationToken ct)
        {
            var result = await _queries.SearchByAuctionAsync(auctionId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get bids placed by a specific bidder
        /// </summary>
        [Authorize]
        [HttpGet("bidder/{bidderId:int}")]
        public async Task<IActionResult> GetByBidder(int bidderId, CancellationToken ct)
        {
            var result = await _queries.SearchByBidderAsync(bidderId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Search bids with pagination and filters
        /// </summary>
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchBids(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "highest",
            [FromQuery] int? auctionId = null,
            [FromQuery] int? bidderId = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] DateTimeOffset? placedAfter = null,
            [FromQuery] DateTimeOffset? placedBefore = null,
            [FromQuery] DepositStatus? statusDeposit = null,
            [FromQuery] bool? isWinning = null,
            [FromQuery] DateTimeOffset? createAt = null,
            [FromQuery] DateTimeOffset? updateAt = null,
            [FromQuery] DateTimeOffset? deleteAt = null,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedBidsAsync(
                pageNumber, pageSize, sortBy,
                auctionId, bidderId, minAmount, maxAmount,
                placedAfter, placedBefore, statusDeposit, isWinning, createAt, updateAt, deleteAt, ct);

            return Ok(result);
        }

        /// <summary>
        /// Get bid count matching filters
        /// </summary>
        [Authorize]
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetBidCount(
            [FromQuery] int? auctionId = null,
            [FromQuery] int? bidderId = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] DateTimeOffset? placedAfter = null,
            [FromQuery] DateTimeOffset? placedBefore = null,
            [FromQuery] DepositStatus? statusDeposit = null,
            [FromQuery] bool? isWinning = null,
            [FromQuery] DateTimeOffset? createAt = null,
            [FromQuery] DateTimeOffset? updateAt = null,
            [FromQuery] DateTimeOffset? deleteAt = null,
            CancellationToken ct = default)
        {
            var count = await _queries.GetBidCountAsync(
                auctionId, bidderId, minAmount, maxAmount,
                placedAfter, placedBefore, statusDeposit, isWinning, createAt, updateAt, deleteAt, ct);

            return Ok(count);
        }


        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Place a new bid on an auction
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            dto.BidderId = userId;

            var bidId = await _commands.PlaceBidAsync(dto, ct);
            return CreatedAtAction(nameof(GetBidById), new { bidId = bidId }, new { BidId = bidId });
        }

        /// <summary>
        /// Update the deposit status of a bid
        /// </summary>
        [Authorize]
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateBidStatus([FromBody] UpdateBidStatusRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _commands.UpdateBidStatusAsync(request.BidId, request.NewStatus, ct);
            if (!success)
                return NotFound(new { message = "Bid not found or update failed" });

            return Ok(new { message = "Bid status updated successfully" });
        }

        /// <summary>
        /// Update bidder contact info (email/phone)
        /// </summary>
        [Authorize]
        [HttpPatch("{bidId:int}/contact")]
        public async Task<IActionResult> UpdateContact(
            int bidId,
            [FromQuery] string? newEmail,
            [FromQuery] string? newPhone,
            CancellationToken ct)
        {
            var success = await _commands.UpdateContactInfoAsync(bidId, newEmail, newPhone, ct);
            if (!success)
                return NotFound(new { message = "Bid not found or update failed" });

            return Ok(new { message = "Contact info updated successfully" });
        }

        /// <summary>
        /// Update bid amount (only valid for active auctions)
        /// </summary>
        [Authorize]
        [HttpPatch("{bidId:int}/amount")]
        public async Task<IActionResult> UpdateBidAmount(int bidId, [FromQuery] decimal newAmount, CancellationToken ct)
        {
            if (newAmount <= 0)
                return BadRequest(new { message = "Invalid bid amount" });

            var success = await _commands.UpdateBidAmountAsync(bidId, newAmount, ct);
            if (!success)
                return NotFound(new { message = "Bid not found or update failed" });

            return Ok(new { message = "Bid amount updated successfully" });
        }

        /// <summary>
        /// Mark a bid as winning or not
        /// </summary>
        [Authorize]
        [HttpPatch("{bidId:int}/winning")]
        public async Task<IActionResult> UpdateWinningStatus(int bidId, [FromQuery] bool isWinning, CancellationToken ct)
        {
            var success = await _commands.UpdateWinningBidAsync(bidId, isWinning, ct);
            if (!success)
                return NotFound(new { message = "Bid not found or update failed" });

            return Ok(new { message = "Winning status updated successfully" });
        }

        /// <summary>
        /// Delete a bid (soft delete)
        /// </summary>
        [Authorize]
        [HttpDelete("{bidId:int}")]
        public async Task<IActionResult> DeleteBid(int bidId, CancellationToken ct)
        {
            if (bidId <= 0)
                return BadRequest(new { message = "Invalid bid ID" });

            var success = await _commands.DeleteBidAsync(bidId, ct);
            if (!success)
                return NotFound(new { message = "Bid not found" });

            return Ok(new { message = "Bid deleted successfully" });
        }
    }
}
