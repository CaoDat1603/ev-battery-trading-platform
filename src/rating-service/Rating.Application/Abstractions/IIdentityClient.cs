using Rating.Application.DTOs;

namespace Rating.Application.Abstractions
{
    public interface IIdentityClient
    {
        Task<bool> UserExistsAsync(int userId, CancellationToken ct);
        Task<UserInfoDto> GetUserInfoAsync(int userId, CancellationToken ct = default);
        Task<List<UserInfoDto>> GetUsersInfoAsync(List<int> ids, CancellationToken ct);
    }
}
