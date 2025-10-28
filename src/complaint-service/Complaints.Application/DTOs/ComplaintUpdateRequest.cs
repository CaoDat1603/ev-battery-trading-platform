using Complaints.Domain.Enums;

namespace Complaints.Application.DTOs
{
    public class ComplaintUpdateRequest
    {
        public int ComplaintId { get; set; }
        public ComplaintStatus? complaintStatus { get; set; }
        public Resolution? resolution { get; set; }
        public int? resolvedBy { get; set; }
    }
}
