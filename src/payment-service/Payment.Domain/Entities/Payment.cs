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
        public string? ReferenceCode { get; private set; } // Lưu vnp_TransactionNo từ VnPay nếu thanh toán thành công
        public DateTimeOffset CreatedAt { get; private set; }
        public string? VnPayPayDate { get; private set; } // Lưu vnp_PayDate từ VnPay nếu thanh toán thành công
        public string? VnPayCreateDate { get; private set; }
        public string? VnPayTxnRef { get; private set; }

        // Phương thức nghiệp vụ
        // Cập nhật trạng thái thanh toán, trả về mã giao dịch từ VnPay nếu thành công
        public void MarkAsSuccess(string referenceCode, string? vnPayPayDate = null)
        {
            if (Status == Enums.PaymentStatus.Success) return; // Nếu đã là Success thì không cần cập nhật lại
            Status = Enums.PaymentStatus.Success;
            ReferenceCode = referenceCode;

            if (!string.IsNullOrEmpty(vnPayPayDate))
            {
                VnPayPayDate = vnPayPayDate;
            }
        }

        // Cập nhật trạng thái thanh toán khi thất bại, có mã giao dịch từ VnPay (IPN handler cần)
        public void MarkAsFailed(string? referenceCode = null)
        {
            Status = Enums.PaymentStatus.Failed;
            if (!string.IsNullOrEmpty(referenceCode))
            {
                ReferenceCode = referenceCode;
            }
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

        public void SetVnPayCreateDate(string vnPayCreateDate)
        {
            VnPayCreateDate = vnPayCreateDate;
        }

        public void SetVnPayTxnRef(string vnPayTxnRef)
        {
            VnPayTxnRef = vnPayTxnRef;
        }

        // Models hỗ trợ nghiệp vụ
        public class PaymentInformationModel
        {
            public string OrderType { get; set; }
            public double Amount { get; set; }
            public string OrderDescription { get; set; }
            public string Name { get; set; }
        }
        // Model phản hồi sau khi xử lý thanh toán
        public class PaymentResponseModel
        {
            public string OrderDescription { get; set; }
            public string TransactionId { get; set; }
            public string OrderId { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentId { get; set; }
            public bool Success { get; set; }
            public string Token { get; set; }
            public string VnPayResponseCode { get; set; }
        }
    }
}
