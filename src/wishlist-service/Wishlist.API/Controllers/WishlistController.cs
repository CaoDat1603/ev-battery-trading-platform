using Wishlist.Application.Contracts;
using Wishlist.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Wishlist.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistQueries _queries;
        private readonly IWishlistCommands _commands;

        public WishlistController(IWishlistQueries queries, IWishlistCommands commands)
        {
            _queries = queries;
            _commands = commands;
        }

        // ======================
        // 🔹 QUERIES
        // ======================

        /// <summary>
        /// Health check for the wishlist service
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("health")]
        public IActionResult Health()
        {
            Response.Headers["Cache-Control"] = "public, max-age=30";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(30).ToString("R");
            return new OkObjectResult(new { ok = true, svc = "wishlist" });
        }

        /// <summary>
        /// Get all wishlist items (cached for 60s)
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllWishlist(CancellationToken ct)
        {
            Response.Headers["Cache-Control"] = "public, max-age=60";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

            var result = await _queries.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Get wishlist item by ID
        /// </summary>
        /// <param name="wishlistId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{wishlistId:int}")]
        public async Task<IActionResult> GetWishlistById(int wishlistId, CancellationToken ct)
        {
            var result = await _queries.SearchByWishlistIdAsync(wishlistId, ct);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Wishlist not found" });

            return Ok(result.First());
        }

        /// <summary>
        /// Get wishlist items by User ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetWishlistByUserId(int userId, CancellationToken ct)
        {
            var result = await _queries.SearchByUserIdAsync(userId, ct);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Wishlist not found for the user" });
            return Ok(result);
        }

        /// <summary>
        /// Search wishlist items with pagination and sorting
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="sortBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchWishlist(
            [FromQuery] int? userId,
            [FromQuery] int? productId,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedAsync(
                pageNumber,
                pageSize,
                sortBy,
                userId,
                productId,
                ct);

            return Ok(result);
        }

        /// <summary>
        /// Get wishlist count based on filters
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult> GetWishlistCount(
            [FromQuery] int? userId,
            [FromQuery] int? productId,
            CancellationToken ct = default)
        {
            var count = await _queries.GetWishlistCountAsync(
                userId,
                productId,
                ct);

            return Ok(count);
        }

        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Create a new wishlist item
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateWishlistItem([FromForm] CreateWishlistRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new CreateWishlistDto
            {
                UserId = request.UserId,
                ProductId = request.ProductId
            };

            var wishlistId = await _commands.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetWishlistById), new { wishlistId = wishlistId }, new { wishlistId = wishlistId });
        }

        /// <summary>
        /// Delete a wishlist item by ID
        /// </summary>
        /// <param name="wishlistId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("{wishlistId:int}")]
        public async Task<IActionResult> DeleteWishlistItem(int wishlistId, CancellationToken ct)
        {
            var success = await _commands.DeleteAsync(wishlistId, ct);
            if (!success)
                return NotFound(new { message = "Wishlist item not found" });
            return NoContent();
        }
    }
}
