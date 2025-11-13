namespace Complaints.Application.DTOs
{
    public class ComplaintStatisticsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int InReview { get; set; }
        public int Resolved { get; set; }
        public int Cancelled { get; set; }
    }
}
