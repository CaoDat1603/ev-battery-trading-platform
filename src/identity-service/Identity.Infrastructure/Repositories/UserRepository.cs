using Identity.Application.DTOs;
using Identity.Domain.Abtractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(User entity, CancellationToken ct = default)
            => await _db.Users.AddAsync(entity, ct);

        public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.Users.Include(u => u.UserProfile)
                              .FirstOrDefaultAsync(u => u.UserId == id, ct);

        public void Update(User entity)
            => _db.Users.Update(entity);

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.UserEmail == email, ct);

        public Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.UserPhone == phone, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users.Include(u => u.UserProfile)
                        .FirstOrDefaultAsync(u => u.UserEmail == email, ct);

        public Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
            => _db.Users.Include(u => u.UserProfile)
                        .FirstOrDefaultAsync(u => u.UserPhone == phone, ct);
        public async Task<IReadOnlyList<User>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default)
        {
            return await _db.Users
                .Include(u => u.UserProfile)
                .Where(u => u.UserProfile != null && u.UserProfile.Status == status)
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(ct);
        }


        public async Task<IReadOnlyList<User>> SearchAsync(
            string q,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            int take = 50,
            int page = 1,
            CancellationToken ct = default)
        {
            var query = _db.Users
                .Include(u => u.UserProfile)
                .AsQueryable();

            // --- Search by q ---
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    (u.UserFullName != null && EF.Functions.ILike(u.UserFullName, $"%{q}%")) ||
                    (u.UserEmail != null && EF.Functions.ILike(u.UserEmail, $"%{q}%")) ||
                    (u.UserPhone != null && EF.Functions.ILike(u.UserPhone, $"%{q}%")) ||
                    (u.UserProfile != null
                        && u.UserProfile.CitizenIdCard != null
                        && EF.Functions.ILike(u.UserProfile.CitizenIdCard, $"%{q}%"))
                );
            }

            // --- Filter by UserStatus ---
            if (userStatus.HasValue)
                query = query.Where(u => u.UserStatus == userStatus.Value);

            // --- Filter by ProfileVerificationStatus ---
            if (profileStatus.HasValue)
                query = query.Where(u =>
                    u.UserProfile != null
                    && u.UserProfile.Status == profileStatus.Value);

            // --- Filter by UserRole ---
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            // --- Filter by CreatedAt (date only) ---
            if (createdAt.HasValue)
            {
                var date = createdAt.Value.Date;
                query = query.Where(u => u.CreatedAt.Date == date);
            }

            // --- Pagination ---
            query = query
                .OrderBy(u => u.UserFullName)
                .Skip((page - 1) * take)
                .Take(take);

            return await query
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<int> CountAsync(
            string q,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            CancellationToken ct = default)
        {
            var query = _db.Users
                .Include(u => u.UserProfile)
                .AsQueryable();

            // --- Search by q ---
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    (u.UserFullName != null && EF.Functions.ILike(u.UserFullName, $"%{q}%")) ||
                    (u.UserEmail != null && EF.Functions.ILike(u.UserEmail, $"%{q}%")) ||
                    (u.UserPhone != null && EF.Functions.ILike(u.UserPhone, $"%{q}%")) ||
                    (u.UserProfile != null
                        && u.UserProfile.CitizenIdCard != null
                        && EF.Functions.ILike(u.UserProfile.CitizenIdCard, $"%{q}%"))
                );
            }

            // --- Filter by UserStatus ---
            if (userStatus.HasValue)
                query = query.Where(u => u.UserStatus == userStatus.Value);

            // --- Filter by ProfileVerificationStatus ---
            if (profileStatus.HasValue)
                query = query.Where(u =>
                    u.UserProfile != null
                    && u.UserProfile.Status == profileStatus.Value);

            // --- Filter by UserRole ---
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            // --- Filter by CreatedAt (date only) ---
            if (createdAt.HasValue)
            {
                var date = createdAt.Value.Date;
                query = query.Where(u => u.CreatedAt.Date == date);
            }

            return await query.CountAsync(ct);
        }
        public async Task<IReadOnlyList<User>> GetUsersByIdsAsync(List<int> ids, CancellationToken ct = default)
        {
            if (ids == null || ids.Count == 0)
                return new List<User>();

            return await _db.Users
                .Include(u => u.UserProfile)
                .Where(u => ids.Contains(u.UserId))
                .AsNoTracking()
                .ToListAsync(ct);
        }

    }
}