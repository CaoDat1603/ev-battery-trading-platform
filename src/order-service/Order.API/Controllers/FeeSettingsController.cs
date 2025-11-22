using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Contracts;
using Order.Application.DTOs;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/admin")]
    //[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class FeeSettingsController : ControllerBase
    {
        private readonly IFeeSettingsService _feeSettingsService;
        public FeeSettingsController(IFeeSettingsService feeSettingsService)
        {
            _feeSettingsService = feeSettingsService;
        }

        // GET /api/admin/fees/active/{productType}
        [HttpGet("active/{productType:int}")]
        [Authorize(AuthenticationSchemes = "Bearer,SystemBearer", Roles = "Admin,System")]
        public async Task<IActionResult> GetActiveSettings(int productType)
        {
            var settings = await _feeSettingsService.GetActiveFeeSettingsAsync(productType);
            if (settings == null)
            {
                return NotFound($"Active fee settings for product type {productType} not found."); // 404 Not Found (Không tìm thấy)
            }
            return Ok(settings);
        }


        [HttpGet("member/{productType:int}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetActiveMemberSettings(int productType)
        {
            var settings = await _feeSettingsService.GetActiveFeeSettingsAsync(productType);
            if (settings == null)
            {
                return NotFound($"Active fee settings for product type {productType} not found."); // 404 Not Found (Không tìm thấy)
            }
            return Ok(settings);
        }

        // GET /api/admin/fees/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            return Ok(await _feeSettingsService.GetFeeSettingsHistoryAsync());
        }

        // PUT /api/admin/fees/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateFees([FromBody] UpdateFeeSettingsRequest request)
        {
            await _feeSettingsService.UpdateFeeSettingsAsync(request);
            return NoContent(); // 204 No content (Thành công)
        }


    }
}
