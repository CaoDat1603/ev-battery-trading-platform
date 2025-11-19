using Microsoft.EntityFrameworkCore;
using Rating.Application.Abstractions;
using Rating.Application.DTOs;
using Rating.Domain.Entities;
using System.Linq;

namespace Rating.Infrastructure.Repositories
{
    public class RateRepository : IRateRepository
    {
        private readonly AppDbContext _appDbContext;
        public RateRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<RateImage> AddImageAsync(int rateId, string imageUrl, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates
                .Include(r => r.Images)
                .FirstOrDefaultAsync(x => x.RateId == rateId && x.DeletedAt == null, ct)
                ?? throw new KeyNotFoundException(imageUrl);
            var img = rate.AddImage(imageUrl);
            return img;
        }

        public async Task AddRateAsync(Rate rate, CancellationToken ct = default)
        {
            await _appDbContext.Rates.AddAsync(rate, ct);
        }

        public async Task DeleteRateAsync(int rateId, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates.FirstOrDefaultAsync(r => r.RateId == rateId && r.DeletedAt == null, ct);
            if (rate != null)
            {
                rate.Delete();
                _appDbContext.Rates.Update(rate);
            }
            else
            {
                throw new KeyNotFoundException($"Không tìm thấy rate với Id = {rateId}");
            }
        }

        public async Task<double?> GetAverageScoreAsync(int? userId, int? productId, CancellationToken ct = default)
        {
            if (userId.HasValue && productId.HasValue) throw new ArgumentException($"Only userid or productid.");
            var list = _appDbContext.Rates.AsQueryable();
            list = list.Where(x => x.DeletedAt == null && x.Score.HasValue);
            if (userId.HasValue) list = list.Where(x => x.UserId == userId.Value);
            if (productId.HasValue) list = list.Where(x => x.ProductId == productId.Value);
            return await list.Select(x => (double?)x.Score).AverageAsync(ct);
        }

        public async Task<IReadOnlyList<RateImage>> GetImagesAsync(int rateId, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates
                .Include(r => r.Images)
                .FirstOrDefaultAsync(x => x.RateId == rateId && x.DeletedAt == null, ct)
                ?? throw new KeyNotFoundException($"Rate not found {rateId}");
            return rate.Images.ToList();
        }

        public async Task<Rate?> GetRateByIdAsync(int rateId, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates
                .FirstOrDefaultAsync(x=> x.RateId == rateId && x.DeletedAt == null,ct)
                ?? throw new KeyNotFoundException($"Rate not found{rateId}");
            return rate;
        }

        public async Task<int> GetRateCountAsync(int? userId, int? productId, CancellationToken ct = default)
        {
            if (userId.HasValue && productId.HasValue) throw new ArgumentException($"Only userId or productId");
            var rate = _appDbContext.Rates.AsQueryable();
            if (userId.HasValue) rate = rate.Where(r => r.UserId == userId.Value);
            if (productId.HasValue) rate = rate.Where(r => r.ProductId == productId.Value);
            return await rate.CountAsync(ct);
        }

        // Repo method (defensive)
        public async Task<PaginatedResult<Rate>> GetRatesAsync(
            int? rateId,
            int? feedbackId,
            int? userId,
            int? productId,
            int? rateBy,
            int? score,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            // Defensive normalization
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            Console.WriteLine($"GetRatesAsync filters: rateId={rateId}, pageNumber={pageNumber}, pageSize={pageSize}", rateId, pageNumber, pageSize);

            var query = _appDbContext.Rates
                .Include(r => r.Images)
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (rateId.HasValue) query = query.Where(x => x.RateId == rateId.Value);
            if (feedbackId.HasValue) query = query.Where(x => x.FeedBackId == feedbackId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
            if (rateBy.HasValue) query = query.Where(x => x.RateBy == rateBy.Value);
            if (score.HasValue) query = query.Where(x => x.Score == score.Value);

            var totalItems = await query.CountAsync(ct);

            // Safe compute pages
            var totalPages = pageSize > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 0;

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            Console.WriteLine($"GetRatesAsync result: totalItems={totalItems}, itemsCount={items.Count}, totalPages={totalPages}");

            return new PaginatedResult<Rate>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }


        public async Task RemoveImageAsync(int rateImageId, CancellationToken ct = default)
        {
            var image = await _appDbContext.ImageRatings
                .Include( r => r.Rate)
                .FirstOrDefaultAsync( x=> x.RateImageId == rateImageId, ct)
                ?? throw new KeyNotFoundException($"RateImageId is not found {rateImageId}");
            if (image.Rate != null)
            {
                image.Rate.RemoveImage(rateImageId);
                image.Rate.UpdatedAt = DateTimeOffset.UtcNow;
                _appDbContext.Rates.Update(image.Rate);
            }
             _appDbContext.ImageRatings.Remove(image);
        }

        public async Task ReplaceImagesAsync(int rateId, IEnumerable<string> newUrls, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.RateId == rateId && r.DeletedAt == null, ct)
                ?? throw new KeyNotFoundException($"Rate is not found {rateId}");

            rate.ReplaceImages(newUrls); // domain method đã Clear + Add + UpdatedAt
            _appDbContext.Rates.Update(rate);
        }

        public async Task UpdateImageUrlAsync(int rateImageId, string newUrl, CancellationToken ct = default)
        {
            var img = await _appDbContext.ImageRatings
                .Include(i => i.Rate)
                .FirstOrDefaultAsync(i => i.RateImageId == rateImageId && i.Rate.DeletedAt == null, ct)
                ?? throw new KeyNotFoundException($"RateImageId is not found {rateImageId}");

            img.UpdateUrl(newUrl);
            if (img.Rate is not null)
            {
                img.Rate.UpdatedAt = DateTimeOffset.UtcNow;
                _appDbContext.Rates.Update(img.Rate);
            }
            _appDbContext.ImageRatings.Update(img);
        }

        public async Task UpdateRateAsync(int rateId, int? score, string? comment, CancellationToken ct = default)
        {
            var rate = await _appDbContext.Rates
               .FirstOrDefaultAsync(r => r.RateId == rateId && r.DeletedAt == null, ct)
               ?? throw new KeyNotFoundException($"Rate is not found {rateId}.");

            rate.Update(score, comment);
            _appDbContext.Rates.Update(rate);
        }
        public async Task<bool> CheckExistAsync(int? userId, int? productId, int rateBy, CancellationToken ct = default)
        {
            return await _appDbContext.Rates.AnyAsync(x =>
                (x.RateBy == userId ||
                x.ProductId == productId) && x.RateBy == rateBy, ct);
        }

    }
}
