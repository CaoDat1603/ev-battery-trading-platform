using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        public int BuyerId { get; set; }
        //public int FeeId { get; set; }
        public int ProductType { get; set; }
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        //public decimal CommissionFee { get; set; }
        //public decimal? ServiceFee { get; set; }
        public decimal BuyerAmount { get; set; }
        public decimal SellerAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
