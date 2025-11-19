using Rating.Domain.Entities;
using Rating.Application.DTOs;

namespace Rating.Application.Abstractions
{
    public interface IRateRepository
    {
        // Rate
        Task AddRateAsync(Rate rate, CancellationToken ct = default);
        Task<Rate?> GetRateByIdAsync(int rateId, CancellationToken ct = default);
        Task UpdateRateAsync(int rateId, int? score, string? comment, CancellationToken ct = default);
        Task DeleteRateAsync(int rateId, CancellationToken ct = default);
        Task<bool> CheckExistAsync(int? userId, int? productId, int rateBy, CancellationToken ct = default);

        // Truy vấn linh hoạt (hoặc chuyển sang 1 RateFilter object)
        Task<PaginatedResult<Rate>> GetRatesAsync(
            int? rateId,
            int? feedbackId,
            int? userId,
            int? productId,
            int? rateBy,
            int? score,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        // Tổng quan thống kê
        Task<int> GetRateCountAsync(int? userId, int? productId, CancellationToken ct = default);
        Task<double?> GetAverageScoreAsync(int? userId, int? productId, CancellationToken ct = default);

        // ----- Ảnh -----
        Task<RateImage> AddImageAsync(int rateId, string imageUrl, CancellationToken ct = default);
        Task UpdateImageUrlAsync(int rateImageId, string newUrl, CancellationToken ct = default);
        Task RemoveImageAsync(int rateImageId, CancellationToken ct = default);
        Task ReplaceImagesAsync(int rateId, IEnumerable<string> newUrls, CancellationToken ct = default);
        Task<IReadOnlyList<RateImage>> GetImagesAsync(int rateId, CancellationToken ct = default);
    }
}
