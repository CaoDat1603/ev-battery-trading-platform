using Rating.Application.DTOs;

namespace Rating.Application.Contracts
{
    public interface IRateQueries
    {
        Task<RateResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<RateResponse>> GetAsync(
            int? rateId,
            int? feedbackId,
            int? userId,
            int? productId,
            int? rateBy,
            int? score,
            CancellationToken ct = default);

        Task<int> GetCountAsync(int? userId, int? productId, CancellationToken ct = default);
        Task<double?> GetAverageAsync(int? userId, int? productId, CancellationToken ct = default);

        Task<IReadOnlyList<RateImageDto>> GetImagesAsync(int rateId, CancellationToken ct = default);
    }
}
