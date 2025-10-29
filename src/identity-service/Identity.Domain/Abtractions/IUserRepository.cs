using Identity.Domain.Entities;

namespace Identity.Domain.Abtractions
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task AddAsync(User entity, CancellationToken ct = default);
        Task UpdateAsync(User entity, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default);
    }
}