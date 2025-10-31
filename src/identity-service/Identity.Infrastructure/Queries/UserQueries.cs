using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Queries
{
    public class UserQueries : IUserQueries
    {
        private readonly AppDbContext _context;

        public UserQueries(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserQueriesDTO>> GetByProfileStatusAsync(ProfileVerificationStatus status, CancellationToken ct = default)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .Where(u => u.UserProfile != null && u.UserProfile.Status == status)
                .AsNoTracking()
                .Select(u => new UserQueriesDTO
                {
                    UserId = u.UserId,
                    UserFullName = u.UserFullName,
                    Email = u.UserEmail,
                    Phone = u.UserPhone,
                    UserAddress = u.UserProfile.UserAddress,
                    UserBirthday = u.UserProfile.UserBirthday,
                    ContactPhone = u.UserProfile.ContactPhone,
                    Avatar = u.UserProfile.Avatar,
                    CitizenIdCard = u.UserProfile.CitizenIdCard,
                    UserStatus = u.UserStatus ?? UserStatus.Active,
                    ProfileStatus = u.UserProfile.Status,
                    RejectionReason = u.UserProfile.RejectionReason,
                    CreatedAt = u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(ct);
        }



        public async Task<IReadOnlyList<User>> SearchAsync(string q, int take = 50, CancellationToken ct = default)
        {
            return await _context.Users
                .Where(u =>
                    (u.UserFullName != null && u.UserFullName.Contains(q)) ||
                    (u.UserEmail != null && u.UserEmail.Contains(q)) ||
                    (u.UserPhone != null && u.UserPhone.Contains(q)))
                .AsNoTracking()
                .Take(take)
                .ToListAsync(ct);
        }

    }
}
