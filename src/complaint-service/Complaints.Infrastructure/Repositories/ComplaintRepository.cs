using Complaints.Domain.Abtractions;
using Complaints.Domain.Entities;
using Complaints.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Complaints.Infrastructure.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _appDbContext;

        public ComplaintRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // ===== CRUD =====
        public async Task AddComplaintAsync(Complaint complaint, CancellationToken cancellationToken = default)
            => await _appDbContext.Complaints.AddAsync(complaint, cancellationToken);

        public async Task<Complaint?> GetComplaintByIdAsync(int complaintId, CancellationToken cancellationToken = default)
        {
            var complaint = await _appDbContext.Complaints.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ComplaintId == complaintId && x.DeleteAt == null, cancellationToken);

            if (complaint == null)
                throw new KeyNotFoundException($"Không tìm thấy complaint với Id = {complaintId}");

            return complaint;
        }

        public async Task UpdateComplaintAsync(
            int complaintId,
            ComplaintStatus? complaintStatus,
            Resolution? resolution,
            int? resolvedBy,
            CancellationToken cancellationToken = default)
        {
            var complaint = await _appDbContext.Complaints
                .FirstOrDefaultAsync(x => x.ComplaintId == complaintId && x.DeleteAt == null, cancellationToken);

            if (complaint == null)
                throw new KeyNotFoundException($"Không tìm thấy complaint với Id = {complaintId}");

            complaint.Update(complaintStatus, resolution, resolvedBy);
            _appDbContext.Complaints.Update(complaint);
        }

        public async Task DeleteComplaintAsync(int complaintId, CancellationToken cancellationToken = default)
        {
            var complaint = await _appDbContext.Complaints
                .FirstOrDefaultAsync(x => x.ComplaintId == complaintId && x.DeleteAt == null, cancellationToken);

            if (complaint == null)
                throw new KeyNotFoundException($"Không tìm thấy complaint với Id = {complaintId}");

            complaint.Delete();
            _appDbContext.Complaints.Update(complaint);
        }

        // ===== FILTER LINH HOẠT =====
        public async Task<List<Complaint>> GetComplaintsAsync(
            int? transactionId = null,
            int? complaintantId = null,
            int? againstUserId = null,
            int? resolvedBy = null,
            ComplaintStatus? status = null,
            DateTimeOffset? fromDate = null,
            DateTimeOffset? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Complaints.AsQueryable();

            query = query.Where(x => x.DeleteAt == null);

            if (transactionId.HasValue)
                query = query.Where(x => x.TransactionId == transactionId.Value);
            if (complaintantId.HasValue)
                query = query.Where(x => x.ComplaintantId == complaintantId.Value);
            if (againstUserId.HasValue)
                query = query.Where(x => x.AgainstUserId == againstUserId.Value);
            if (resolvedBy.HasValue)
                query = query.Where(x => x.ResolvedBy == resolvedBy.Value);
            if (status.HasValue)
                query = query.Where(x => x.ComplaintStatus == status.Value);
            if (fromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(x => x.CreatedAt <= toDate.Value);
            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        }

        // ===== PHÂN TRANG =====
        public async Task<(List<Complaint> Complaints, int TotalCount)> GetComplaintsPagedAsync(
            int pageNumber,
            int pageSize,
            ComplaintStatus? status = null,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Complaints.AsQueryable();

            query = query.Where(x => x.DeleteAt == null);

            if (status.HasValue)
                query = query.Where(x => x.ComplaintStatus == status.Value);

            if (userId.HasValue)
                query = query.Where(x => x.ComplaintantId == userId.Value || x.AgainstUserId == userId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var complaints = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (complaints, totalCount);
        }

        // ===== THỐNG KÊ =====
        public async Task<int> GetTotalComplaintCountAsync(CancellationToken cancellationToken = default)
            => await _appDbContext.Complaints.CountAsync(x => x.DeleteAt == null, cancellationToken);

        public async Task<int> GetComplaintCountByStatusAsync(ComplaintStatus status, CancellationToken cancellationToken = default)
            => await _appDbContext.Complaints.CountAsync(x => x.ComplaintStatus == status && x.DeleteAt == null, cancellationToken);

        public async Task<int> GetComplaintCountByUserAsync(int userId, CancellationToken cancellationToken = default)
            => await _appDbContext.Complaints.CountAsync(
                x => (x.ComplaintantId == userId || x.AgainstUserId == userId) && x.DeleteAt == null,
                cancellationToken);
    }
}
