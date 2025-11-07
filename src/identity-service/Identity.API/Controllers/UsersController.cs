using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserCommands _userService;
        private readonly IUserQueries _userQueries;

        public UsersController(IUserCommands userService, IUserQueries userQueries)
        {
            _userService = userService;
            _userQueries = userQueries;
        }
        [Authorize]
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
        [Authorize]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId, CancellationToken ct = default)
        {
            // Lấy userId từ JWT claims
            var jwtUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (jwtUserId == null)
                return Unauthorized("Invalid token.");

            if (jwtUserId != userId.ToString())
                return Forbid("You are not allowed to access this user's data.");

            var user = await _userQueries.GetByIdAsync(userId, ct);
            return Ok(user);
        }

        [Authorize]
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto request, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var updatedUserId = await _userService.UpdateUserAsync(userId, request, cancellationToken);
            return Ok(new { UserId = updatedUserId });
        }

    }
}
