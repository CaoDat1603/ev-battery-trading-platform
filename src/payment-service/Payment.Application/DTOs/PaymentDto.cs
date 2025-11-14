namespace Payment.Application.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int TransactionId { get; set; }
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
