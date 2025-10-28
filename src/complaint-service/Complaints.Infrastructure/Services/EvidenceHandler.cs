using Complaints.Domain.Abtractions;

namespace Complaints.Infrastructure.Services
{
    public class EvidenceHandler : IEvidenceHandler
    {
        private readonly ILocalFileStorage _storage;

        public EvidenceHandler(ILocalFileStorage storage)
        {
            _storage = storage;
        }

        public async Task<string?> HandleEvidenceAsync(IFormFile? evidenceFile, int complaintId, CancellationToken ct = default)
        {
            if (evidenceFile == null)
                return null;
            var ext = Path.GetExtension(evidenceFile.FileName);
            var fileName = $"Evidence_{complaintId}{ext}";
            var folderPath = $"uploads/evidences/{complaintId}";
            try
            {
                using var stream = evidenceFile.OpenReadStream();
                var evidenceUrl = await _storage.SaveFileAsync(folderPath, fileName, stream, ct);
                return evidenceUrl;
            }
            catch (Exception ex)
            {
                throw new IOException("Không thể lưu file minh chứng.", ex);
            }
        }
    }
}
