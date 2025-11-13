using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Entities
{
    public class FeeSettings
    {
        public int FeeId { get; private set; }
        public int Type { get; private set; } // 1: Xe, 2: Pin, 3: Khác
        public decimal FeePercent { get; private set; } // Phí dịch vụ (%)
        public decimal CommissionPercent { get; private set; } // Hoa hồng (%)
        public DateTimeOffset? EffectiveDate { get; private set; } // Ngày hiệu lực
        public DateTimeOffset? EndedDate { get; private set; } // Ngày kết thúc
        public bool IsActive { get; private set; } // Trạng thái hoạt động

        // EF Core constructor
        public FeeSettings() { }

        // Constructor nghiệp vụ (Admin tạo cài đặt phí mới)
        public FeeSettings(int type, decimal feePercent, decimal commissionPercent)
        {
            // Validation
            if (type <= 0)
                throw new ArgumentException("Product type must be valid.", nameof(type));
            if (feePercent <= 0)
                throw new ArgumentException("Fee percent cannot be negative.", nameof(feePercent));
            if (commissionPercent < 0)
                throw new ArgumentException("Commission percent cannot be negative.", nameof(commissionPercent));


            Type = type;
            FeePercent = feePercent;
            CommissionPercent = commissionPercent;
            IsActive = false; // Mặc định là không active khi tạo mới
        }

        // Kích hoạt cài đặt phí
        public void Active()
        {
            IsActive = true;
            EffectiveDate = DateTimeOffset.UtcNow;
            EndedDate = null;
        }

        // Hủy kích hoạt cài đặt phí
        public void Deactive()
        {
            IsActive = false;
            EndedDate = DateTimeOffset.UtcNow;
        }
    }
}
