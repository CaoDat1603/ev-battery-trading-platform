using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductQueries _queries;
        private readonly IProductCommands _commands;

        public ProductsController(IProductQueries queries, IProductCommands commands)
        {
            _queries = queries;
            _commands = commands;
        }

        // ======================
        // 🔹 QUERIES
        // ======================

        /// <summary>
        /// Health check for the catalog service
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            Response.Headers["Cache-Control"] = "public, max-age=30";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(30).ToString("R");
            return Ok(new { ok = true, svc = "catalog" });
        }

        /// <summary>
        /// Get all products (cached for 60s)
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(CancellationToken ct)
        {
            Response.Headers["Cache-Control"] = "public, max-age=60";
            Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

            var result = await _queries.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Get a product by its ID
        /// </summary>
        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProductById(int productId, CancellationToken ct)
        {
            var result = await _queries.SearchByProductIDAsync(productId, ct);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Product not found" });

            return Ok(result.First());
        }

        [Authorize]
        [HttpGet("is-me/{productId:int}")]
        public async Task<IActionResult> GetIsMeProductById(int productId, CancellationToken ct)
        {
            var result = await _queries.SearchByProductIDAsync(productId, ct);
            if (result == null)
                return NotFound(new { message = "Product not found" });

            var product = result[0];

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            return Ok(userId == product.SellerId);
        }

        /// <summary>
        /// Get all products of a specific seller
        /// </summary>
        [HttpGet("seller/{sellerId:int}")]
        public async Task<IActionResult> GetBySeller(int sellerId, CancellationToken ct)
        {
            var result = await _queries.SearchBySellerAsync(sellerId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get all products of a specific moderated by
        /// </summary>
        [HttpGet("moderated/{moderatedBy:int}")]
        public async Task<IActionResult> GetByModeratedBy(int moderatedBy, CancellationToken ct)
        {
            var result = await _queries.SearchModeratedByAsync(moderatedBy, ct);
            return Ok(result);
        }


        /// <summary>
        /// Search products for buyer (Available only)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchForBuyer(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] SaleMethod? saleMethod,
            [FromQuery] int? sellerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "newestupdate",
            [FromQuery] bool? isVerified = null,
            [FromQuery] ProductType? productType = null,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedProductsAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                sortBy: sortBy,
                keyword: q,
                minPrice: minPrice,
                maxPrice: maxPrice,
                pickupAddress: pickupAddress,
                status: ProductStatus.Available,
                saleMethod: saleMethod,
                sellerId: sellerId,
                isSpam: false,
                isVerified: isVerified,
                productType: productType,
                ct: ct);

            return Ok(result);
        }

        /// <summary>
        /// Search products for admin/seller (all statuses)
        /// </summary>
        [Authorize]
        [HttpGet("search/all")]
        public async Task<IActionResult> SearchForAdmin(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] ProductStatus? status,
            [FromQuery] SaleMethod? saleMethod,
            [FromQuery] int? sellerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] bool? isSpam = null,
            [FromQuery] bool? isVerified = null,
            [FromQuery] ProductType? productType = null,
            [FromQuery] DateTimeOffset? createAt = null,
            [FromQuery] DateTimeOffset? updateAt = null,
            [FromQuery] DateTimeOffset? deleteAt = null,
            CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && sellerId.HasValue)
                return Forbid("Only Admin can set product status to Available");

            if (!isAdmin && !sellerId.HasValue)
            {
                sellerId = userId;
            }

            var result = await _queries.GetPagedProductsAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                sortBy: sortBy,
                keyword: q,
                minPrice: minPrice,
                maxPrice: maxPrice,
                pickupAddress: pickupAddress,
                status: status,
                saleMethod: saleMethod,
                sellerId: sellerId,
                isSpam: isSpam,
                isVerified: isVerified,
                productType: productType,
                createAt: createAt,
                updateAt: updateAt,
                deleteAt: deleteAt,
                ct: ct);

            return Ok(result);
        }

        /// <summary>
        /// Get count product
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetProductCount(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] ProductStatus? status,
            [FromQuery] int? sellerId,
            [FromQuery] SaleMethod? saleMethod,
            [FromQuery] bool? isSamp = null,
            [FromQuery] bool? isVerified = null,
            [FromQuery] ProductType? productType = null,
            [FromQuery] DateTimeOffset? createAt = null,
            [FromQuery] DateTimeOffset? updateAt = null,
            [FromQuery] DateTimeOffset? deleteAt = null,
            CancellationToken ct = default)
        {
            var result = await _queries.GetProductCountAsync(
                keyword: q,
                minPrice: minPrice,
                maxPrice: maxPrice,
                pickupAddress: pickupAddress,
                sellerId: sellerId,
                status: status,
                saleMethod: saleMethod,
                isSpam: isSamp,
                isVerified: isVerified,
                productType: productType,
                createAt: createAt,
                updateAt: updateAt,
                deleteAt: deleteAt,
                ct: ct
            );

            return Ok(result);
        }

        [HttpGet("count-seller")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetProductCountSeller(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] ProductStatus? status,
            [FromQuery] SaleMethod? saleMethod,
            [FromQuery] bool? isSamp = null,
            [FromQuery] bool? isVerified = null,
            [FromQuery] ProductType? productType = null,
            [FromQuery] DateTimeOffset? createAt = null,
            [FromQuery] DateTimeOffset? updateAt = null,
            [FromQuery] DateTimeOffset? deleteAt = null,
            CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var result = await _queries.GetProductCountAsync(
                keyword: q,
                minPrice: minPrice,
                maxPrice: maxPrice,
                pickupAddress: pickupAddress,
                sellerId: userId,
                status: status,
                saleMethod: saleMethod,
                isSpam: isSamp,
                isVerified: isVerified,
                productType: productType,
                createAt: createAt,
                updateAt: updateAt,
                deleteAt: deleteAt,
                ct: ct
            );

            return Ok(result);
        }

        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Create a new product (support file upload)
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var dto = new CreateProductDto
            {
                Title = request.Title,
                Price = request.Price,
                SellerId = userId,
                PickupAddress = request.PickupAddress,
                ProductName = request.ProductName,
                Description = request.Description,
                ProductType = request.ProductType,
                RegistrationCard = request.RegistrationCard,
                saleMethod = request.SaleMethod,
                IsSpam = request.IsSpam,
                FileUrl = request.FileUrl,
                ImageUrl = request.ImageUrl
            };

            var id = await _commands.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetProductById), new { productId = id }, new { ProductId = id });
        }

        /// <summary>
        /// Update product status
        /// </summary>
        [Authorize]
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateProductStatusRequest request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);

            if ((request.NewStatus == ProductStatus.Available
                || request.NewStatus == ProductStatus.Block) && !isAdmin)
                return Forbid("Only Admin can set product status to Available");

            var success = await _commands.UpdateStatusAsync(request.ProductId, request.NewStatus, ct);
            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Status updated successfully" });
        }

        /// <summary>
        /// Update product sale method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPatch("sale-method")]
        public async Task<IActionResult> UpdateSaleMethod([FromBody] UpdateSaleMethodRequest request, CancellationToken ct)
        {
            var success = await _commands.UpdateSaleMethodAsync(request.ProductId, request.NewMethod, ct);
            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Sale method updated successfully" });
        }

        [Authorize]
        [HttpPatch("{id}/verified")]
        public async Task<IActionResult> MarkAsVerified(int id, CancellationToken ct)
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
                return Forbid("Only Admin can set product status to Available");

            var result = await _commands.MarkAsVerifiedAsync(id, ct);
            return result ? Ok() : NotFound();
        }

        [Authorize]
        [HttpDelete("{id}/verified")]
        public async Task<IActionResult> UnmarkAsVerified(int id, CancellationToken ct)
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
                return Forbid("Only Admin can set product status to Available");

            var result = await _commands.UnmarkAsVerifiedAsync(id, ct);
            return result ? Ok() : NotFound();
        }

        [Authorize]
        [HttpPatch("{id}/spam")]
        public async Task<IActionResult> MarkAsSpam(int id, CancellationToken ct)
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
                return Forbid("Only Admin can set product status to Available");

            var result = await _commands.MarkAsSpamAsync(id, ct);
            return result ? Ok() : NotFound();
        }

        [Authorize]
        [HttpDelete("{id}/spam")]
        public async Task<IActionResult> UnmarkAsSpam(int id, CancellationToken ct)
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
                return Forbid("Only Admin can set product status to Available");

            var result = await _commands.UnmarkAsSpamAsync(id, ct);
            return result ? Ok() : NotFound();
        }

        /// <summary>
        /// Soft delete a product
        /// </summary>
        [Authorize]
        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> DeleteProduct(int productId, CancellationToken ct)
        {
            if (productId <= 0)
                return BadRequest(new { message = "Invalid product ID" });

            var success = await _commands.DeleteProductAsync(productId, ct);
            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Product deleted successfully" });
        }

        [Authorize]
        [HttpPatch("{productId:int}/verify-transaction")]
        public async Task<IActionResult> VerifyAndCompleteTransaction(
        int productId,
        [FromBody] VerifyTransactionRequest request,
        CancellationToken ct)
        {
            var success = await _commands.VerifyAndCompleteTransaction(
                request.TransactionId,
                productId,
                ct
            );

            if (!success)
                return BadRequest(new
                {
                    message = "Transaction is not valid or not completed."
                });

            return Ok(new
            {
                message = "Product marked as SoldOut.",
                productId = productId
            });
        }
    }
}