using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Abtractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Application.Services
{
    public class UserQueries : IUserQueries
    {
        private readonly IUserRepository _repo;

        public UserQueries(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserQueriesDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await _repo.GetByIdAsync(id, ct);
            return user?.ToDto();
        }

        public async Task<IReadOnlyList<UserQueriesDTO>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default)
        {
            var users = await _repo.GetByProfileStatusAsync(status, ct);
            return users.Select(u => u.ToDto()).ToList();
        }

        public async Task<List<UserQueriesDTO>> GetUsersByIds(List<int> ids)
        {
            var users = await _repo.GetUsersByIdsAsync(ids);

            return users.Select(u => u.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<UserQueriesDTO>> SearchAsync(
            string query,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            int take = 50,
            int page = 1,
            CancellationToken ct = default)
        {
            var users = await _repo.SearchAsync(query, userStatus, profileStatus, role, createdAt, take, page, ct);
            return users.Select(u => u.ToDto()).ToList();
        }

        public async Task<int> CountAsync(
            string query,
            UserStatus? userStatus,
            ProfileVerificationStatus? profileStatus,
            UserRole? role,
            DateTimeOffset? createdAt,
            CancellationToken ct = default)
        {
            var count = await _repo.CountAsync(query, userStatus, profileStatus, role, createdAt, ct);
            return count;
        }
    }
    public static class UserMappingExtensions
    {
        public static UserQueriesDTO ToDto(this User user)
        {
            return new UserQueriesDTO
            {
                UserId = user.UserId,
                UserFullName = user.UserFullName,
                Email = user.UserEmail,
                Phone = user.UserPhone,
                UserAddress = user.UserProfile?.UserAddress,
                UserBirthday = user.UserProfile?.UserBirthday,
                ContactPhone = user.UserProfile?.ContactPhone,
                Avatar = user.UserProfile?.Avatar,
                CitizenIdCard = user.UserProfile?.CitizenIdCard,
                UserStatus = user.UserStatus,
                ProfileStatus = user.UserProfile?.Status,
                RejectionReason = user.UserProfile?.RejectionReason,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
