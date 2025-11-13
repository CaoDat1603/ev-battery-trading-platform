using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using Complaints.Domain.Abtractions;
using Complaints.Domain.Entities;
using Complaints.Domain.Enums;

namespace Complaints.Application.Services
{
    public class ComplaintQueries : IComplaintQueries
    {
        private readonly IComplaintRepository _repository;

        public ComplaintQueries(IComplaintRepository repository)
        {
            _repository = repository;
        }

        // === Lấy chi tiết ===
        public async Task<ComplaintResponse> GetComplaintByIdAsync(int complaintId, CancellationToken ct = default)
        {
            var complaint = await _repository.GetComplaintByIdAsync(complaintId, ct);
            return complaint is null ? null : MapToDto(complaint);
        }

        // === Lấy danh sách linh hoạt theo filter (refactor 8 hàm cũ vào đây) ===
        public async Task<List<ComplaintResponse>> GetComplaintsAsync(
            int? transactionId = null,
            int? complaintantId = null,
            int? againstUserId = null,
            int? resolvedBy = null,
            ComplaintStatus? status = null,
            DateTimeOffset? fromDate = null,
            DateTimeOffset? toDate = null,
            CancellationToken ct = default)
        {
            // Repository có thể dùng LINQ filter theo điều kiện truyền vào
            var complaints = await _repository.GetComplaintsAsync(
                transactionId,
                complaintantId,
                againstUserId,
                resolvedBy,
                status,
                fromDate,
                toDate,
                ct);

            return complaints.Select(MapToDto).ToList();
        }

        // === Phân trang ===
        public async Task<PagedResult<ComplaintResponse>> GetComplaintsPagedAsync(
            int pageNumber,
            int pageSize,
            ComplaintStatus? status = null,
            int? userId = null,
            CancellationToken ct = default)
        {
            var (complaints, totalCount) = await _repository.GetComplaintsPagedAsync(pageNumber, pageSize, status, userId, ct);

            if (status.HasValue)
                complaints = complaints.Where(c => c.ComplaintStatus == status.Value).ToList();

            if (userId.HasValue)
                complaints = complaints.Where(c =>
                    c.ComplaintantId == userId.Value || c.AgainstUserId == userId.Value).ToList();

            var dtoList = complaints.Select(MapToDto).ToList();
            return new PagedResult<ComplaintResponse>(dtoList, totalCount, pageNumber, pageSize);
        }

        // === Thống kê ===
        public async Task<ComplaintStatisticsDto> GetComplaintStatisticsAsync(CancellationToken ct = default)
        {
            var total = await _repository.GetTotalComplaintCountAsync(ct);
            var pending = await _repository.GetComplaintCountByStatusAsync(ComplaintStatus.Pending, ct);
            var inReview = await _repository.GetComplaintCountByStatusAsync(ComplaintStatus.InReview, ct);
            var resolved = await _repository.GetComplaintCountByStatusAsync(ComplaintStatus.Resolved ,ct);
            var cancelled = await _repository.GetComplaintCountByStatusAsync(ComplaintStatus.Cancelled, ct);
            

            return new ComplaintStatisticsDto
            {
                Total = total,
                Pending = pending,
                InReview = inReview,
                Resolved = resolved,
                Cancelled = cancelled,

            };
        }

        // === Đếm theo user ===
        public async Task<int> GetComplaintCountByUserAsync(int userId, CancellationToken ct = default)
        {
            return await _repository.GetComplaintCountByUserAsync(userId, ct);
        }

        // === Helper Mapper ===
        private static ComplaintResponse MapToDto(Complaint complaint)
        {
            return new ComplaintResponse
            {
                ComplaintId = complaint.ComplaintId,
                TransactionId = complaint.TransactionId,
                ComplaintantId = complaint.ComplaintantId,
                AgainstUserId = complaint.AgainstUserId,
                ReasonComplaint = complaint.ReasonComplaint.ToString(),
                Description = complaint.Description,
                ComplaintStatus = complaint.ComplaintStatus.ToString(),
                Resolution = complaint.Resolution.ToString(),
                ResolvedBy = complaint.ResolvedBy,
                EvidenceUrl = complaint.EvidenceUrl,
                CreatedAt = complaint.CreatedAt,
                ResolvedAt = complaint.ResolvedAt
            };
        }
    }
}
