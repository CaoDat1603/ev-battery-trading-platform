namespace Catalog.Application.DTOs
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public decimal basePrice { get; set; }
        public string transactionStatus { get; set; } // 3 = Success
    }

}
