﻿using Identity.Application.DTOs;

namespace Identity.Application.Contracts
{
    public interface IUserCommands
    {
        Task<int> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default);
        Task<int> UpdateUserAsync(int userId, UpdateUserDto request, CancellationToken cancellationToken = default);

        // Xác thực tài khoản -> status á "verified" | "unverified"
        Task VerifyUserAsync(int userId, CancellationToken cancellationToken = default);

        // Soft delete
        Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Khóa tài khoản (disable).
        /// </summary>
        Task DisableUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mở khóa tài khoản (enable).
        /// </summary>
        Task EnableUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
