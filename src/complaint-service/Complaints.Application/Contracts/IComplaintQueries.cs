using Complaints.Application.DTOs;
using Complaints.Domain.Enums;

namespace Complaints.Application.Contracts
{
    public interface IComplaintQueries
    {
        //Truy vấn
        Task<ComplaintResponse> GetComplaintByIdAsync(int complaintId, CancellationToken ct = default);
        Task<List<ComplaintResponse>> GetComplaintsAsync(
            int? transactionId = null,
            int? complaintantId = null,
            int? againstUserId = null,
            int? resolvedBy = null,
            ComplaintStatus? status = null,
            DateTimeOffset? fromDate = null,
            DateTimeOffset? toDate = null,
            CancellationToken ct = default);
        Task<PagedResult<ComplaintResponse>> GetComplaintsPagedAsync(
            int pageNumber,
            int pageSize,
            ComplaintStatus? status = null,
            int? userId = null,
            CancellationToken ct = default);

        // === Thống kê ===
        Task<ComplaintStatisticsDto> GetComplaintStatisticsAsync(CancellationToken ct = default);
        Task<int> GetComplaintCountByUserAsync(int userId, CancellationToken ct = default);
    }
}
