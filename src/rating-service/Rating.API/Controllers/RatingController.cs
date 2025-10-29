using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rating.Application.Contracts;
using Rating.Application.DTOs;

namespace Rating.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRateCommands _commands;
        private readonly IRateQueries _queries;

        public RatingController(IRateCommands commands, IRateQueries queries)
        {
            _commands = commands;
            _queries = queries;
        }

        // GET: api/rate/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var rate = await _queries.GetByIdAsync(id, ct);
            return rate is not null ? Ok(rate) : NotFound();
        }

        // GET: api/rate
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int? rateId,
            [FromQuery] int? feedbackId,
            [FromQuery] int? userId,
            [FromQuery] int? productId,
            [FromQuery] int? rateBy,
            [FromQuery] int? score,
            CancellationToken ct)
        {
            var list = await _queries.GetAsync(rateId, feedbackId, userId, productId, rateBy, score, ct);
            return Ok(list);
        }

        // GET: api/rate/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount(
            [FromQuery] int? userId,
            [FromQuery] int? productId,
            CancellationToken ct)
        {
            var count = await _queries.GetCountAsync(userId, productId, ct);
            return Ok(count);
        }

        // GET: api/rate/average
        [HttpGet("average")]
        public async Task<IActionResult> GetAverage(
            [FromQuery] int? userId,
            [FromQuery] int? productId,
            CancellationToken ct)
        {
            var avg = await _queries.GetAverageAsync(userId, productId, ct);
            return Ok(avg);
        }

        // GET: api/rate/{rateId}/images
        [HttpGet("{rateId:int}/images")]
        public async Task<IActionResult> GetImages(int rateId, CancellationToken ct)
        {
            var imgs = await _queries.GetImagesAsync(rateId, ct);
            return Ok(imgs);
        }

        // POST: api/rate
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRateRequest request, CancellationToken ct)
        {
            var result = await _commands.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.RateId }, result);
        }

        // POST: api/rate/user/{userId}
        [HttpPost("user/{userId:int}")]
        public async Task<IActionResult> CreateForUser(int userId, [FromBody] CreateRateRequest request, CancellationToken ct)
        {
            var result = await _commands.CreateUserAsync(userId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.RateId }, result);
        }

        // POST: api/rate/product/{productId}
        [HttpPost("product/{productId:int}")]
        public async Task<IActionResult> CreateForProduct(int productId, [FromBody] CreateRateRequest request, CancellationToken ct)
        {
            var result = await _commands.CreateProductAsync(productId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.RateId }, result);
        }

        // PUT: api/rate/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRateRequest request, CancellationToken ct)
        {
            await _commands.UpdateAsync(id, request, ct);
            return NoContent();
        }

        // DELETE: api/rate/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _commands.DeleteAsync(id, ct);
            return NoContent();
        }

        // POST: api/rate/{rateId}/images
        [HttpPost("{rateId:int}/images")]
        public async Task<IActionResult> AddImages(int rateId, [FromForm] IFormFileCollection files, CancellationToken ct)
        {
            var result = await _commands.AddImagesAsync(rateId, files, ct);
            return Ok(result);
        }

        // PUT: api/rate/{rateId}/images
        [HttpPut("{rateId:int}/images")]
        public async Task<IActionResult> ReplaceImages(int rateId, [FromForm] IFormFileCollection files, CancellationToken ct)
        {
            await _commands.ReplaceImagesAsync(rateId, files, ct);
            return NoContent();
        }

        // PUT: api/rate/image/{imageId}/url
        [HttpPut("image/{imageId:int}/url")]
        public async Task<IActionResult> UpdateImageUrl(int imageId, [FromBody] string newUrl, CancellationToken ct)
        {
            await _commands.UpdateImageUrlAsync(imageId, newUrl, ct);
            return NoContent();
        }

        // DELETE: api/rate/image/{imageId}
        [HttpDelete("image/{imageId:int}")]
        public async Task<IActionResult> RemoveImage(int imageId, CancellationToken ct)
        {
            await _commands.RemoveImageAsync(imageId, ct);
            return NoContent();
        }
    }
}
