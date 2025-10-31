using Identity.Application.DTOs;

namespace Identity.Application.Contracts
{
    public interface IUserCommands
    {
        Task<int> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default);
        Task<int> UpdateUserAsync(int userId, UpdateUserDto request, CancellationToken cancellationToken = default);
        Task VerifyUserAsync(int userId, CancellationToken cancellationToken = default);
        Task RejectUserProfileAsync(int userId, string? reason = null, CancellationToken ct = default);
        Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task DisableUserAsync(int userId, CancellationToken cancellationToken = default);
        Task EnableUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
