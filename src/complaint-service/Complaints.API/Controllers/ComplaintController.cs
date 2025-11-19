using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using Complaints.Application.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Complaints.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintCommands _commands;
        private readonly IComplaintQueries _queries;

        public ComplaintController(
            IComplaintCommands commands,
            IComplaintQueries queries
            )
        {
            _commands = commands;
            _queries = queries;
        }

        // ======================
        // 🔹 COMMANDS
        // ======================

        /// <summary>
        /// Tạo complaint mới.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComplaint([FromForm] ComplaintCreateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var complaintId = await _commands.CreateComplaintAsync(request, ct);
            return CreatedAtAction(nameof(GetComplaintById), new { complaintId }, new { ComplaintId = complaintId });
        }

        /// <summary>
        /// Cập nhật trạng thái hoặc kết quả xử lý complaint.
        /// </summary>
        [HttpPut("{complaintId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateComplaint(int complaintId,
            [FromBody] ComplaintUpdateRequest request,
            CancellationToken ct)
        {
            if (complaintId != request.ComplaintId)
                return BadRequest("ComplaintId không khớp giữa URL và body.");

            // Lấy id admin đang xử lý complaint
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null)
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            request.resolvedBy = int.Parse(claim);

            var success = await _commands.UpdateComplaintAsync(request, ct);

            return success ? NoContent() : NotFound();
        }


        /// <summary>
        /// Xóa mềm complaint (soft delete).
        /// </summary>
        [HttpDelete("{complaintId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComplaint(int complaintId, CancellationToken ct)
        {
            await _commands.DeleteComplaintAsync(complaintId, ct);
            return NoContent();
        }

        // ======================
        // 🔹 QUERIES
        // ======================

        /// <summary>
        /// Lấy chi tiết complaint theo ID.
        /// </summary>
        [HttpGet("{complaintId:int}")]
        [Authorize]
        public async Task<IActionResult> GetComplaintById(int complaintId, CancellationToken ct)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (claim == null)
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            var complaint = await _queries.GetComplaintByIdAsync(complaintId, ct);
            if (complaint == null) return NotFound();


            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (complaint.ComplaintantId != int.Parse(claim))
                {
                    return Unauthorized("UserId trong token không khớp hoặc bạn không có quyền xem khiếu nại này.");
                }
            }

            return Ok(complaint);
        }


        /// <summary>
        /// Lọc complaint theo các điều kiện.
        /// </summary>
        [HttpGet("filter")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComplaints(
            [FromQuery] int? transactionId,
            [FromQuery] int? complaintantId,
            [FromQuery] int? againstUserId,
            [FromQuery] int? resolvedBy,
            [FromQuery] string? status,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            CancellationToken ct)
        {
            ComplaintStatusDto? dtoStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse(status, true, out ComplaintStatusDto parsedStatus))
                {
                    // Nếu "Rejected" bị lỗi, lỗi này sẽ bắt nó
                    return BadRequest($"Invalid ComplaintStatus value: {status}");
                }
                dtoStatus = parsedStatus;
            }
            // 3. Xử lý thành công (Convert DTO Enum sang Domain Enum)
            var domainStatus = dtoStatus.ToDomainNullable();
            var results = await _queries.GetComplaintsAsync(transactionId, complaintantId, againstUserId, resolvedBy, domainStatus, fromDate, toDate, ct);
            /*ComplaintStatusDto? dtoStatus = Enum.TryParse(status, true, out ComplaintStatusDto parsedStatus)
                ? parsedStatus
                : (ComplaintStatusDto?)null;*/

            //var domainStatus = dtoStatus.ToDomainNullable();
            //var results = await _queries.GetComplaintsAsync(transactionId, complaintantId, againstUserId, resolvedBy, domainStatus, fromDate, toDate, ct);
            return Ok(results);
        }

        /// <summary>
        /// Lấy complaint có phân trang.
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComplaintsPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] int? userId = null,
            CancellationToken ct = default)
        {
            ComplaintStatusDto? dtoStatus = Enum.TryParse(status, true, out ComplaintStatusDto parsedStatus)
                ? parsedStatus
                : (ComplaintStatusDto?)null;

            var domainStatus = dtoStatus.ToDomainNullable();

            var pagedResult = await _queries.GetComplaintsPagedAsync(
                pageNumber,
                pageSize,
                domainStatus,
                userId,
                ct);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy thống kê complaint (total, pending, resolved, v.v.).
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComplaintStatistics(CancellationToken ct)
        {
            var stats = await _queries.GetComplaintStatisticsAsync(ct);
            return Ok(stats);
        }

        /// <summary>
        /// Lấy số lượng complaint liên quan đến một user.
        /// </summary>
        [HttpGet("user/{userId:int}/count")]
        [Authorize]
        public async Task<IActionResult> GetComplaintCountByUser(int userId, CancellationToken ct)
        {
            var count = await _queries.GetComplaintCountByUserAsync(userId, ct);
            return Ok(new { UserId = userId, ComplaintCount = count });
        }
        [HttpGet("by-complaintant/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByComplaintant(int userId, CancellationToken ct)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null)
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }
            userId = int.Parse(claim);
            var result = await _queries.GetComplaintsAsync(null, userId, null, null, null, null, null, ct);

            return Ok(result);
        }
    }
}
