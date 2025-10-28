namespace Complaints.Domain.Abtractions
{
    public interface IEvidenceHandler
    {
        Task<string?> HandleEvidenceAsync(IFormFile? evidenceFile, int complaintId, CancellationToken ct = default);
    }
}
