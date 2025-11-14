using Order.Application.DTOs;
using Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Contracts
{
    public interface IFeeSettingsService
    {
        // Lấy cài đặt phí đang hoạt động cho 1 loại sản phẩm
        Task<FeeSettingsDto> GetActiveFeeSettingsAsync(int productType);
        // Admin: Thêm cài đặt phí mới
        Task UpdateFeeSettingsAsync(UpdateFeeSettingsRequest request);
        // Admin: Lấy lịch sử thay đổi cài đặt phí
        Task<IEnumerable<FeeSettings>> GetFeeSettingsHistoryAsync();
    }
}
