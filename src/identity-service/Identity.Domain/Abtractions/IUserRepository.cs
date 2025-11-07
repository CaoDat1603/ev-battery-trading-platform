using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Domain.Abtractions
{
    public interface IUserRepository
    {            
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task AddAsync(User entity, CancellationToken ct = default);
        void Update(User entity); // không cần async
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default);
        Task<IReadOnlyList<User>> SearchAsync(string q, int take = 50, CancellationToken ct = default);
        Task<IReadOnlyList<User>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default);
    }
}