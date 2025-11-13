using Complaints.Application.DTOs;

namespace Complaints.Application.Contracts
{
    public interface IComplaintCommands
    {
        Task<int> CreateComplaintAsync(ComplaintCreateRequest request, CancellationToken ct = default);
        Task<bool> UpdateComplaintAsync(ComplaintUpdateRequest request, CancellationToken ct = default);
        Task<bool> DeleteComplaintAsync(int complaintId, CancellationToken ct = default);
    }
}
