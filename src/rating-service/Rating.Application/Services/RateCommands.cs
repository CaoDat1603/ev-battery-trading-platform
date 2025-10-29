using Rating.Application.Contracts;
using Rating.Application.DTOs;
using Rating.Application.Mappers;
using Rating.Domain.Abstractions;
using Rating.Domain.Entities;

namespace Rating.Application.Services
{
    public class RateCommands : IRateCommands
    {
        private IUnitOfWork _uow;
        private IRateRepository _rep;
        private IRateImageHandler _imageHandler;
        public RateCommands(IUnitOfWork uow, IRateRepository rep, IRateImageHandler imageHandler)
        {
            _uow = uow;
            _rep = rep;
            _imageHandler = imageHandler;
        }

        public async Task<RateResponse> CreateAsync(CreateRateRequest request, CancellationToken ct = default)
        {
            ValidateScore(request.Score);

            var rate = Rate.Create(request.FeedbackId, request.UserId, request.ProductId, request.RateBy, request.Score, request.Comment);
            await _rep.AddRateAsync(rate, ct);
            await _uow.SaveChangesAsync(ct);
            return rate.ToDto();
        }
        public Task<RateResponse> CreateUserAsync(int userId, CreateRateRequest request, CancellationToken ct = default)
        {
            request.UserId = userId;
            request.ProductId = null;
            return CreateAsync(request, ct);
        }
        public Task<RateResponse> CreateProductAsync(int productId, CreateRateRequest request, CancellationToken ct = default)
        {
            request.ProductId = productId;
            request.UserId = null;
            return CreateAsync(request, ct);
        }

        public async Task UpdateAsync(int rateId, UpdateRateRequest request, CancellationToken ct = default)
        {
            if (request.Score.HasValue) ValidateScore(request.Score);
            await _rep.UpdateRateAsync(rateId, request.Score, request.Comment, ct);
            await _uow.SaveChangesAsync(ct);
        }
        public async Task DeleteAsync(int rateId, CancellationToken ct = default)
        {
            await _rep.DeleteRateAsync(rateId, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<RateImageDto>> AddImagesAsync(int rateId, IFormFileCollection files, CancellationToken ct = default)
        {
            var urls = await _imageHandler.HandleImagesAsync(files, rateId, ct);
            var created = new List<RateImageDto>(urls.Count);

            foreach (var url in urls)
            {
                var img = await _rep.AddImageAsync(rateId, url!, ct);
                created.Add(img.ToDto());
            }

            await _uow.SaveChangesAsync(ct);
            return created;
        }
        public async Task ReplaceImagesAsync(int rateId, IFormFileCollection files, CancellationToken ct = default)
        {
            var urls = await _imageHandler.HandleImagesAsync(files, rateId, ct);
            await _rep.ReplaceImagesAsync(rateId, urls, ct);
            await _uow.SaveChangesAsync(ct);
        }
        public async Task UpdateImageUrlAsync(int rateImageId, string newUrl, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(newUrl)) throw new ArgumentNullException(nameof(newUrl));
            await _rep.UpdateImageUrlAsync(rateImageId, newUrl.Trim(), ct);
            await _uow.SaveChangesAsync(ct);
        }
        public async  Task RemoveImageAsync(int rateImageId, CancellationToken ct = default)
        {
            await _rep.RemoveImageAsync(rateImageId, ct);
            await _uow.SaveChangesAsync(ct);
        }

        private static void ValidateScore(int? score)
        {
            if (score.HasValue && (score.Value < 1 || score.Value > 10))
                throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 10.");
        }
    }
}
