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

        public async Task<IReadOnlyList<User>> SearchAsync(string q, int take = 50, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return new List<User>();

            q = q.Trim();

            return await _db.Users
                .Include(u => u.UserProfile)
                .Where(u =>
                    (u.UserFullName != null && EF.Functions.ILike(u.UserFullName, $"%{q}%")) ||
                    (u.UserEmail != null && EF.Functions.ILike(u.UserEmail, $"%{q}%")) ||
                    (u.UserPhone != null && EF.Functions.ILike(u.UserPhone, $"%{q}%")) ||
                    (u.UserProfile != null && u.UserProfile.CitizenIdCard != null && EF.Functions.ILike(u.UserProfile.CitizenIdCard, $"%{q}%"))
                )
                .AsNoTracking()
                .Take(take)
                .OrderBy(u => u.UserFullName)
                .ToListAsync(ct);
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