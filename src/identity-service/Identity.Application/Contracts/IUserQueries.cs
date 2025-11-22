using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Application.Contracts
{
    public interface IUserQueries
    {
        Task<UserQueriesDTO?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<UserQueriesDTO>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<UserQueriesDTO>> SearchAsync(
            string query,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            int take = 50,
            int page = 1,
            CancellationToken ct = default);

        Task<int> CountAsync(
            string q,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            CancellationToken ct = default);
        Task<List<UserQueriesDTO>> GetUsersByIds(List<int> ids);
    }
}
