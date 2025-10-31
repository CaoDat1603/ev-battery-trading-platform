using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Application.Contracts
{
    public interface IUserQueries
    {
        Task<IReadOnlyList<User>> SearchAsync(string q, int take = 50, CancellationToken ct = default);
        Task<IReadOnlyList<UserQueriesDTO>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default);
    }
}
