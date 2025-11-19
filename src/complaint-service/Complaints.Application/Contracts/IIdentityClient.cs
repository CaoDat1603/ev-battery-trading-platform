using Complaints.Application.DTOs;

namespace Complaints.Application.Contracts
{
    public interface IIdentityClient
    {
        Task<bool> UserExistsAsync(int userId, CancellationToken ct);
        Task<UserInfoDto> GetUserInfoAsync(int userId, CancellationToken ct = default);
        Task<List<UserInfoDto>> GetUsersInfoAsync(List<int> ids, CancellationToken ct);

    }
}
