using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Identity.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer,SystemBearer", Roles = "Admin,System")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserQueries _userQueries;
        private readonly IUserCommands _userCommands;
        public AdminController(IUserQueries userQueries, IUserCommands userCommands)
        {
            _userQueries = userQueries;
            _userCommands = userCommands;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserById(int userId, CancellationToken ct = default)
        {
            var users = await _userQueries.GetByIdAsync(userId, ct);
            return Ok(users);
        }
        [HttpPost("batch")]
        public async Task<List<UserQueriesDTO>> GetBatchUsers([FromBody] List<int> ids)
        {
            return await _userQueries.GetUsersByIds(ids);
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] ProfileVerificationStatus status, CancellationToken ct)
        {
            var users = await _userQueries.GetByProfileStatusAsync(status, ct);
            return Ok(users);
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? q = null,
            [FromQuery] UserStatus? userStatus = null,
            [FromQuery] ProfileVerificationStatus? profileStatus = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] DateTimeOffset? createdAt = null,
            [FromQuery] int take = 50,
            [FromQuery] int page = 1,
            CancellationToken ct = default)
        {
            var users = await _userQueries.SearchAsync(q, userStatus, profileStatus, role, createdAt, take, page, ct);
            return Ok(users);
        }

        [HttpGet("users/count")]
        public async Task<IActionResult> CountUsers(
            [FromQuery] string? q = null,
            [FromQuery] UserStatus? userStatus = null,
            [FromQuery] ProfileVerificationStatus? profileStatus = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] DateTimeOffset? createdAt = null,
            CancellationToken ct = default)
        {
            var count = await _userQueries.CountAsync(q, userStatus, profileStatus, role, createdAt, ct);
            return Ok(count);
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserDto request,
            CancellationToken cancellationToken)
        {
            var userId = await _userCommands.CreateUserAsync(request, cancellationToken);
            return Ok(userId);
        }

        [HttpPost("{id}/verify")]
        public async Task<IActionResult> Verify(int id, CancellationToken ct)
        {
            await _userCommands.VerifyUserAsync(id, ct);
            return Ok("User verified");
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, string? reason = null,  CancellationToken ct=default)
        {
            await _userCommands.RejectUserProfileAsync(id,reason,ct);
            return Ok("User rejected");
        }

        [HttpPost("{id}/disable")]
        public async Task<IActionResult> Disable(int id, CancellationToken ct)
        {
            await _userCommands.DisableUserAsync(id, ct);
            return Ok("User disabled");
        }

        [HttpPost("{id}/enable")]
        public async Task<IActionResult> Enable(int id, CancellationToken ct)
        {
            await _userCommands.EnableUserAsync(id, ct);
            return Ok("User enabled");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _userCommands.DeleteUserAsync(id, ct);
            return Ok("User deleted (soft)");
        }
    }
}
