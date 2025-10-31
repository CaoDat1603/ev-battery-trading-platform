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
    }
}