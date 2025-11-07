using Rating.Application.Abstractions;
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
        private IRateQueries _queries;
        private IIdentityClient _identityClient;
        private IEventBus _eventBus;
        public RateCommands(IUnitOfWork uow, IRateRepository rep, IRateImageHandler imageHandler, IRateQueries queries, IIdentityClient identity, IEventBus eventBus)
        {
            _uow = uow;
            _rep = rep;
            _imageHandler = imageHandler;
            _queries = queries;
            _identityClient = identity;
            _eventBus = eventBus;
        }

        public async Task<RateResponse> CreateAsync(CreateRateRequest request, CancellationToken ct = default)
        {
            ValidateScore(request.Score);
            if (request.UserId.HasValue)
            {
                var exists = await _identityClient.UserExistsAsync(request.UserId.Value, ct);
                if (!exists)
                    throw new ArgumentException($"User with ID {request.UserId} does not exist.");
            }
            var rate = Rate.Create(request.FeedbackId, request.UserId, request.ProductId, request.RateBy, request.Score, request.Comment);
            await _rep.AddRateAsync(rate, ct);
            await _uow.SaveChangesAsync(ct);

            var evt = new RatingNotificationEvent(
                request.UserId ?? 0,
                request.RateBy,
                request.Score ?? 0
            );

            await _eventBus.PublishAsync("rating_exchange", evt);


            return rate.ToDto();
        }
        public async Task<RateResponse> CreateUserAsync(int userId, CreateRateRequest request, CancellationToken ct = default)
        {
            request.UserId = userId;
            request.ProductId = null;
            var check = await _queries.GetAsync(null, null, request.UserId, null, request.RateBy, null, ct);
            if (check.Any()) throw new ArgumentException("The rate already exists.");
            return await CreateAsync(request, ct);
        }
        public async Task<RateResponse> CreateProductAsync(int productId, CreateRateRequest request, CancellationToken ct = default)
        {
            request.ProductId = productId;
            request.UserId = null;
            var check = await _queries.GetAsync(null, null, null, request.ProductId, request.RateBy, null, ct);
            if (check.Any()) throw new ArgumentException("The rate already exists.");
            return await CreateAsync(request, ct);
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
    public record RatingNotificationEvent(int UserId, int rateBy, double Score);

}
