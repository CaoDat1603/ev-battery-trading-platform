using Rating.Application.Contracts;
using Rating.Application.DTOs;
using Rating.Domain.Abstractions;
using Rating.Application.Mappers;

namespace Rating.Application.Services
{
    public class RateQueries : IRateQueries
    {
        private readonly IRateRepository _repo;

        public RateQueries(IRateRepository repo)
        {
            _repo = repo;
        }

        public async Task<RateResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var e = await _repo.GetRateByIdAsync(id, ct);
            return e?.ToDto();
        }

        public async Task<IReadOnlyList<RateResponse>> GetAsync(
            int? rateId,
            int? feedbackId,
            int? userId,
            int? productId,
            int? rateBy,
            int? score,
            CancellationToken ct = default)
        {
            var list = await _repo.GetRatesAsync(rateId, feedbackId, userId, productId, rateBy, score, ct);
            return list.Select(x => x.ToDto()).ToList();
        }

        public Task<int> GetCountAsync(int? userId, int? productId, CancellationToken ct = default)
            => _repo.GetRateCountAsync(userId, productId, ct);

        public Task<double?> GetAverageAsync(int? userId, int? productId, CancellationToken ct = default)
            => _repo.GetAverageScoreAsync(userId, productId, ct);

        public async Task<IReadOnlyList<RateImageDto>> GetImagesAsync(int rateId, CancellationToken ct = default)
        {
            var images = await _repo.GetImagesAsync(rateId, ct);
            return images.Select(i => i.ToDto()).ToList();
        }
    }
}
