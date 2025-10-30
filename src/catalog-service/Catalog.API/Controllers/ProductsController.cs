using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

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
        /// Search products for buyer (Available only)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchForBuyer(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedProductsAsync(
                pageNumber,
                pageSize,
                q,
                minPrice,
                maxPrice,
                pickupAddress,
                ProductStatus.Available,
                null,
                ct);

            return Ok(result);
        }

        /// <summary>
        /// Search products for admin/seller (all statuses)
        /// </summary>
        [HttpGet("search/all")]
        public async Task<IActionResult> SearchForAdmin(
            [FromQuery] string? q,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? pickupAddress,
            [FromQuery] ProductStatus? status,
            [FromQuery] int? sellerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _queries.GetPagedProductsAsync(
                pageNumber,
                pageSize,
                q,
                minPrice,
                maxPrice,
                pickupAddress,
                status,
                sellerId,
                ct);

            return Ok(result);
        }

        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Create a new product (support file upload)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new CreateProductDto
            {
                Title = request.Title,
                Price = request.Price,
                SellerId = request.SellerId,
                PickupAddress = request.PickupAddress,
                ProductName = request.ProductName,
                Description = request.Description,
                ProductType = request.ProductType,
                RegistrationCard = request.RegistrationCard,
                FileUrl = request.FileUrl,
                ImageUrl = request.ImageUrl
            };

            var id = await _commands.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetProductById), new { productId = id }, new { ProductId = id });
        }

        /// <summary>
        /// Update product status
        /// </summary>
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateProductStatusRequest request, CancellationToken ct)
        {
            var success = await _commands.UpdateStatusAsync(request.ProductId, request.NewStatus, ct);
            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Status updated successfully" });
        }
    }
}