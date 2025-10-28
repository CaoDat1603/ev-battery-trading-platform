using Complaints.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Complaints.Application.DTOs
{
    public class ComplaintCreateRequest
    {
        public int TransactionId { get; set; } // id giao dịch
        public int ComplaintantId { get; set; } // người khiếu nại
        public int AgainstUserId { get; set; } // người bị khiếu nại
        public ReasonComplaint ReasonComplaint { get; set; } // Lý do khiếu nại
        [MaxLength(2000)]
        public string Description { get; set; }
        public IFormFile? EvidenceUrl { get; set; }
    }
}
