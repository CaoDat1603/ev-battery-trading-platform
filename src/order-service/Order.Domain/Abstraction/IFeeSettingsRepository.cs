using Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Abstraction
{
    public interface IFeeSettingsRepository
    {
        // Lấy cài đặt phí đang hoạt động cho 1 loại sản phẩm
        Task<FeeSettings> GetActiveFeeSettingsAsync(int productType);

        // Admin: Thêm cài đặt phí mới
        Task UpdateFeeSettingsAsync(FeeSettings feeSettings);
        // Admin: Lấy lịch sử thay đổi cài đặt phí
        Task<IEnumerable<FeeSettings>> GetHistoryAsync(); 
    }
}
