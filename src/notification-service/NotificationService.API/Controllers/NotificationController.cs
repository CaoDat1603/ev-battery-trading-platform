using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Contracts;
using NotificationService.Application.DTOs;
using System.Security.Claims;

namespace NotificationService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationAppService _notificationService;
        public NotificationController(INotificationAppService notificationService) => _notificationService = notificationService;

        [HttpGet("user/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(int userId, CancellationToken ct)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null)
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }
            userId = int.Parse(claim);
            var notiList = await _notificationService.GetNotificationByUserId(userId, ct);
            return Ok(notiList);
        }
        [HttpPatch("{id:guid}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
        {
            await _notificationService.ReadNotificationAsync(id, ct);
            return NoContent();
        }
        [HttpPost("send")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> SendManual([FromBody] NotificationDto dto, CancellationToken ct)
        {
            await _notificationService.CreateAndNotifyAsync(dto, ct);
            return Ok(new { message = "Notification sent", dto });
        }
    }
}
