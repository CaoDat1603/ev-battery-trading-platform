using Complaints.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Complaints.Domain.Entities
{
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; }
        [Required]
        public int TransactionId { get; set; } // id giao dịch
        [Required]
        public int ComplaintantId { get; set; } // người khiếu nại
        [Required]
        public int AgainstUserId { get; set; } // người bị khiếu nại
        [Required]
        public ReasonComplaint ReasonComplaint { get; set; } // Lý do khiếu nại
        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }
        [MaxLength(200)]
        public string? EvidenceUrl { get; set; }
        public ComplaintStatus ComplaintStatus { get; set; } //Trạng thái hiện tại của khiếu nại
        public Resolution? Resolution { get; set; } // Kết quả xử lý (chỉ có khi status = Resolved)
        public DateTimeOffset CreatedAt { get; set; }
        public int? ResolvedBy { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        public DateTimeOffset? DeleteAt { get; set; }
        private Complaint() { }
        public static Complaint Create(int transactionId, int complaintantId, int againstUserId, ReasonComplaint reasonComplaint, string description)
        {
            if (transactionId <= 0) throw new ArgumentOutOfRangeException(nameof(transactionId), "Invalid transactionId");
            if (complaintantId <= 0) throw new ArgumentOutOfRangeException(nameof(complaintantId), "Invalid complaintantId");
            if (againstUserId <= 0) throw new ArgumentOutOfRangeException(nameof(againstUserId), "Invalid againstUserId");
            if (complaintantId == againstUserId)
                throw new InvalidOperationException("A user cannot file a complaint against themselves.");
            if (!Enum.IsDefined(typeof(ReasonComplaint), reasonComplaint))
                throw new ArgumentException("Invalid reasonComplaint value", nameof(reasonComplaint));
            if (string.IsNullOrEmpty(description)) throw new ArgumentNullException(nameof(description), "Description is required.");
            if (description.Length > 2000)
                throw new ArgumentException("Description exceeds maximum length of 2000 characters.");
            
            return new Complaint
            {
                TransactionId = transactionId,
                ComplaintantId = complaintantId,
                AgainstUserId = againstUserId,
                ReasonComplaint = reasonComplaint,
                Description = description,
                
                ComplaintStatus = ComplaintStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }
        public void AddEvidence (string? evidenceUrl)
        {
            if (!string.IsNullOrEmpty(evidenceUrl) && evidenceUrl.Length > 200)
                throw new ArgumentException("Evidence URL exceeds maximum length of 200 characters.");
            EvidenceUrl = evidenceUrl;
        }
        public void Update(ComplaintStatus? complaintStatus, Resolution? resolution, int? resolvedBy)
        {
            if (complaintStatus != null)
            {
                if (!Enum.IsDefined(typeof(ComplaintStatus), complaintStatus))
                    throw new ArgumentException("Invalid complaintStatus value", nameof(complaintStatus));
                ComplaintStatus = complaintStatus.Value;
                ResolvedBy = resolvedBy;
            }

            if (resolution != null)
            {
                if (!Enum.IsDefined(typeof(Resolution), resolution))
                    throw new ArgumentException("Invalid resolution value", nameof(resolution));
                Resolution = resolution.Value;
                ResolvedAt = DateTimeOffset.UtcNow;
            }
        }

        public void Delete()
        {
            DeleteAt = DateTimeOffset.UtcNow;
        }
    }
}
