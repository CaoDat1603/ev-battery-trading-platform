using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using Complaints.Application.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> UpdateComplaint(int complaintId, [FromBody] ComplaintUpdateRequest request, CancellationToken ct)
        {
            if (complaintId != request.ComplaintId)
                return BadRequest("ComplaintId không khớp giữa URL và body.");

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
            var complaint = await _queries.GetComplaintByIdAsync(complaintId, ct);
            if (complaint == null) return NotFound();
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
            ComplaintStatusDto? dtoStatus = Enum.TryParse(status, true, out ComplaintStatusDto parsedStatus)
                ? parsedStatus
                : (ComplaintStatusDto?)null;

            var domainStatus = dtoStatus.ToDomainNullable();
            var results = await _queries.GetComplaintsAsync(transactionId, complaintantId, againstUserId, resolvedBy, domainStatus, fromDate, toDate, ct);
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
    }
}
