using Complaints.Application.Contracts;
using Complaints.Application.DTOs;
using Complaints.Domain.Abtractions;
using Complaints.Domain.Entities;

namespace Complaints.Application.Services
{
    public class ComplaintCommands : IComplaintCommands
    {
        private readonly IComplaintRepository _rep;
        private readonly IUnitOfWork _uow;
        private readonly IEvidenceHandler _evidenceHandler;
        public ComplaintCommands(IComplaintRepository rep, IUnitOfWork uow, IEvidenceHandler evidenceHandler)
        {
            _rep = rep;
            _uow = uow;
            _evidenceHandler = evidenceHandler;
        }

        public async Task<int> CreateComplaintAsync(ComplaintCreateRequest request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            var complaint = Complaint.Create(request.TransactionId, request.ComplaintantId, request.AgainstUserId, request.ReasonComplaint, request.Description);

            var evidenceUrl = await _evidenceHandler.HandleEvidenceAsync(request.EvidenceUrl, complaint.ComplaintId, ct);
            if (evidenceUrl != null)
                complaint.AddEvidence(evidenceUrl);

            await _rep.AddComplaintAsync(complaint, ct);
            await _uow.SaveChangesAsync(ct);
            return complaint.ComplaintId;
        }

        public async Task<bool> UpdateComplaintAsync(ComplaintUpdateRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            await _rep.UpdateComplaintAsync(request.ComplaintId, request.complaintStatus, request.resolution, request.resolvedBy, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
        public async Task<bool> DeleteComplaintAsync(int complaintId, CancellationToken ct = default)
        {
            if(complaintId < 0) throw new ArgumentNullException(nameof(complaintId));
            await _rep.DeleteComplaintAsync(complaintId, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
