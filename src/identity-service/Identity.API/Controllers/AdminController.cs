using Identity.Application.Contracts;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] ProfileVerificationStatus status, CancellationToken ct)
        {
            var users = await _userQueries.GetByProfileStatusAsync(status, ct);
            return Ok(users);
        }


        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int take = 50, CancellationToken ct=default)
        {
            var users = await _userQueries.SearchAsync(q, take, ct);
            return Ok(users);
        }


        [HttpPost("{id}/verify")]
        public async Task<IActionResult> Verify(int id, CancellationToken ct)
        {
            await _userCommands.VerifyUserAsync(id, ct);
            return Ok("User verified");
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
