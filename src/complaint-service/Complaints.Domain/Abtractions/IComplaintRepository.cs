using Complaints.Domain.Entities;
using Complaints.Domain.Enums;

namespace Complaints.Domain.Abtractions
{
    public interface IComplaintRepository
    {
        // === CRUD cơ bản ===
        Task AddComplaintAsync(Complaint complaint, CancellationToken cancellationToken = default);
        Task<Complaint?> GetComplaintByIdAsync(int complaintId, CancellationToken cancellationToken = default);
        Task UpdateComplaintAsync(int complaintId, ComplaintStatus? complaintStatus, Resolution? resolution, int? resolvedBy, CancellationToken cancellationToken = default);
        Task DeleteComplaintAsync(int complaintId, CancellationToken cancellationToken = default);

        //Truy vấn linh hoạt (filter đa tiêu chí)
        Task<List<Complaint>> GetComplaintsAsync(
            int? transactionId = null,
            int? complaintantId = null,
            int? againstUserId = null,
            int? resolvedBy = null,
            ComplaintStatus? status = null,
            DateTimeOffset? fromDate = null,
            DateTimeOffset? toDate = null,
            CancellationToken cancellationToken = default);

        //Phân trang
        Task<(List<Complaint> Complaints, int TotalCount)> GetComplaintsPagedAsync(
            int pageNumber,
            int pageSize,
            ComplaintStatus? status = null,
            int? userId = null,
            CancellationToken cancellationToken = default);

        //Thống kê nhanh
        Task<int> GetTotalComplaintCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetComplaintCountByStatusAsync(ComplaintStatus status, CancellationToken cancellationToken = default);
        Task<int> GetComplaintCountByUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
