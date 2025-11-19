using Rating.Application.Abstractions;
using Rating.Application.Contracts;
using Rating.Application.DTOs;
using Rating.Application.Mappers;
using Rating.Domain.Entities;

namespace Rating.Application.Services
{
    public class RateQueries : IRateQueries
    {
        private readonly IRateRepository _repo;
        private readonly IIdentityClient _identityClient;

        public RateQueries(IRateRepository repo, IIdentityClient identityClient)
        {
            _repo = repo;
            _identityClient = identityClient;
        }

        public async Task<RateResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var e = await _repo.GetRateByIdAsync(id, ct);
            if (e == null) return null;
            var dto = e.ToDto();

            var reviewer = await _identityClient.GetUserInfoAsync(e.RateBy, ct);
            if (reviewer != null)
            {
                dto.ReviwerIsName = reviewer.UserFullName;
                dto.ReviwerIsAvartar = reviewer.Avatar;
            }
            if (e.UserId != null)
            {
                var user = await _identityClient.GetUserInfoAsync((int)e.UserId, ct);
                if (user != null) dto.UserName = user.UserFullName;
            }
            return dto;
        }

        // Service method (where you call repo)
        public async Task<PaginatedResult<RateResponse>> GetAsync(
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
            // Normalize inputs & log
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            Console.WriteLine($"GetAsync called with pageNumber={pageNumber}, pageSize={pageSize}, filters: rateId={rateId}, feedbackId={feedbackId}, userId={userId}, productId={productId}, rateBy={rateBy}, score={score}");

            var result = await _repo.GetRatesAsync(
                rateId, feedbackId, userId, productId, rateBy, score,
                pageNumber, pageSize, ct);

            Console.WriteLine($"Repo returned TotalItems={result.TotalItems}, ItemsCount={result.Items?.Count}, RepoPageNumber={result.PageNumber}, RepoPageSize={result.PageSize}, RepoTotalPages={result.TotalPages}");

            var rates = result.Items ?? new List<Rate>();

            // ... rest mapping unchanged (with small null-safety)
            var reviewerIds = rates.Select(r => r.RateBy).Distinct().ToList();
            var userIds = rates.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
            var allIds = reviewerIds.Concat(userIds).Distinct().ToList();

            var usersInfo = allIds.Any() ? await _identityClient.GetUsersInfoAsync(allIds, ct) : new List<UserInfoDto>();
            var usersDict = usersInfo.ToDictionary(u => u.UserId);

            var items = new List<RateResponse>();

            foreach (var r in rates)
            {
                var dto = r.ToDto();

                if (usersDict.TryGetValue(r.RateBy, out var reviewer))
                {
                    dto.ReviwerIsName = reviewer.UserFullName;
                    dto.ReviwerIsAvartar = reviewer.Avatar;
                }

                if (r.UserId.HasValue && usersDict.TryGetValue(r.UserId.Value, out var userInfo))
                {
                    dto.UserName = userInfo.UserFullName;
                }

                items.Add(dto);
            }

            return new PaginatedResult<RateResponse>
            {
                Items = items,
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
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
