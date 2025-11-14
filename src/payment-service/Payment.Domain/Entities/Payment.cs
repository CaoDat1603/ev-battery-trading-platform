using System;

namespace Payment.Domain.Entities
{
    public class Payment
    {
        // Constructor rỗng
        public Payment() 
        {
            // Provide safe defaults for EF Core
            Method = string.Empty;
            Status = Enums.PaymentStatus.Pending;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        // Constructor nghiệp vụ
        public Payment(int transactionId, string method, decimal amount)
        {
            // Validation (ArgumentException)
            if (transactionId <= 0) throw new ArgumentException("Transaction ID must be valid.", nameof(transactionId));
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("Payment method cannot be empty.", nameof(method));

            TransactionId = transactionId;
            Method = method;
            Amount = amount;
            Status = Enums.PaymentStatus.Pending; // Mặc định trạng thái là Pending khi tạo mới
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public int PaymentId { get; private set; }
        public int TransactionId { get; private set; }
        public string Method { get; private set; }
        public decimal Amount { get; private set; }
        public Enums.PaymentStatus Status { get; private set; }
        public string? ReferenceCode { get; private set; } // Nullable string, mã giao dịch từ VnPay
        public DateTimeOffset CreatedAt { get; private set; }

        // Phương thức nghiệp vụ
        // Cập nhật trạng thái thanh toán, trả về mã giao dịch từ VnPay nếu thành công
        public void MarkAsSuccess(string referenceCode)
        {
            Status = Enums.PaymentStatus.Success;
            ReferenceCode = referenceCode;
        }

        public void MarkAsFailed()
        {
            Status = Enums.PaymentStatus.Failed;
        }

        public void MarkAsPending()
        {
            Status = Enums.PaymentStatus.Pending;
        }

        public void MarkAsRefunded()
        {
            Status = Enums.PaymentStatus.Refunded;
        }

        public void MarkAsCancelled()
        {
            Status = Enums.PaymentStatus.Cancelled;
        }
    }
}
