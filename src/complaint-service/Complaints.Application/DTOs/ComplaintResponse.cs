namespace Complaints.Application.DTOs
{
    public class ComplaintResponse
    {
        public int ComplaintId { get; set; }
        public int TransactionId { get; set; }
        public int ComplaintantId { get; set; }
        public int AgainstUserId { get; set; }
        public string ReasonComplaint { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? EvidenceUrl { get; set; }
        public string ComplaintStatus { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public int? ResolvedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
    }
}
