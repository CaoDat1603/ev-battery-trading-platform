using Order.Application.Contracts;
using Order.Application.DTOs;
using Order.Domain.Abstraction;
using Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Services
{
    public class FeeSettingsService : IFeeSettingsService
    {
        private readonly IFeeSettingsRepository _feeSettingsRepository;
        public FeeSettingsService(IFeeSettingsRepository feeSettingsRepository)
        {
            _feeSettingsRepository = feeSettingsRepository;
        }

        public async Task<IEnumerable<FeeSettings>> GetFeeSettingsHistoryAsync()
        {
            return await _feeSettingsRepository.GetHistoryAsync();
        }

        public async Task UpdateFeeSettingsAsync(UpdateFeeSettingsRequest request)
        {
            var newSettings = new Domain.Entities.FeeSettings(
                request.Type,
                request.FeePercent,
                request.CommissionPercent
            );
            await _feeSettingsRepository.UpdateFeeSettingsAsync(newSettings);
        }

        public async Task<FeeSettingsDto> GetActiveFeeSettingsAsync(int productType)
        {
            var feeSettings = await _feeSettingsRepository.GetActiveFeeSettingsAsync(productType);
            if (feeSettings == null) throw new InvalidOperationException($"Fee settings for product type {productType} not found.");
            return new FeeSettingsDto
            {
                FeeId = feeSettings.FeeId,
                Type = feeSettings.Type,
                FeePercent = feeSettings.FeePercent,
                CommissionPercent = feeSettings.CommissionPercent,
                EffectiveDate = feeSettings.EffectiveDate,
                EndedDate = feeSettings.EndedDate,
                IsActive = feeSettings.IsActive
            };
        }
    }
}
