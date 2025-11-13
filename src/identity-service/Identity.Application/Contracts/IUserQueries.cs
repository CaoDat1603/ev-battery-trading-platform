using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Application.Contracts
{
    public interface IUserQueries
    {
        Task<UserQueriesDTO?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<UserQueriesDTO>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<UserQueriesDTO>> SearchAsync(string query, int take = 50, CancellationToken ct = default);
    }
}
