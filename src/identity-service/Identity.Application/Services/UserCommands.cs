using Identity.Application.Common;
using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Abtractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using System.Text.RegularExpressions;


namespace Identity.Application.Services
{
    public class UserCommands : IUserCommands
    {
        private readonly Domain.Abtractions.IUserRepository _repo;
        private readonly Domain.Abtractions.IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IFileStorage _fileStorage;
        public UserCommands(IUserRepository repo, IUnitOfWork uow, IWebHostEnvironment env, IFileStorage fileStorage)
        {
            _repo = repo;
            _uow = uow;
            _env = env;
            _fileStorage = fileStorage;
        }
        // CREATE USER
        public async Task<int> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserEmail) && string.IsNullOrWhiteSpace(request.UserPhone))
                throw new ArgumentException("Vui lòng nhập email hoặc số điện thoại.");
            if (!string.IsNullOrWhiteSpace(request.UserEmail) && await _repo.ExistsByEmailAsync(request.UserEmail))
                throw new ArgumentException("Email đã được sử dụng");
            if (!string.IsNullOrWhiteSpace(request.UserPhone) && await _repo.ExistsByPhoneAsync(request.UserPhone))
                throw new ArgumentException("Số điện thoại đã được sử dụng");
            if (!string.IsNullOrWhiteSpace(request.UserEmail) && !EmailValidator.IsValidEmail(request.UserEmail))
                throw new ArgumentException("Email không hợp lệ.");

            if (!string.IsNullOrWhiteSpace(request.UserPhone) && !PhoneValidator.IsValidPhone(request.UserPhone))
                throw new ArgumentException("Số điện thoại không hợp lệ.");
            if (!Regex.IsMatch(request.UserPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
                throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.");

            string hashPassword = BCrypt.Net.BCrypt.HashPassword(request.UserPassword);

            
            var user = User.Create(
            request.UserEmail?.Trim(),
            request.UserPhone?.Trim(),
            hashPassword,
            request.UserFullName.Trim(),
            request.Role ?? UserRole.Member
            );

            if (!string.IsNullOrEmpty(user.UserEmail))
                user.IsEmailConfirmed = true;

            if (!string.IsNullOrEmpty(user.UserPhone))
                user.IsPhoneConfirmed = true;


            string? avatarUrl = null;
            if (request.Avatar != null)
            {
                var fileExt = Path.GetExtension(request.Avatar.FileName);
                var fileName = $"avatar_{user.UserId}{fileExt}";
                using (var stream = request.Avatar.OpenReadStream())
                {
                    avatarUrl = await _fileStorage.SaveFileAsync($"uploads/users/{user.UserId}", fileName, stream, cancellationToken);
                }
            }

            string? cicUrl = null;
            if (request.CitizenIdCard != null)
            {
                var fileExt = Path.GetExtension(request.CitizenIdCard.FileName);
                var fileName = $"cic_{user.UserId}{fileExt}";
                using var stream = request.CitizenIdCard.OpenReadStream();
                cicUrl = await _fileStorage.SaveFileAsync(
                    $"uploads/users/{user.UserId}",
                    fileName,
                    stream,
                    cancellationToken
                );
            }
            if (!string.IsNullOrWhiteSpace(request.ContactPhone) && !PhoneValidator.IsValidPhone(request.ContactPhone))
                throw new ArgumentException("Số điện thoại không hợp lệ.");
            user.AddOrUpdateProfile(
                fullname: request.UserFullName,
                address: request.UserAddress,
                birthday: request.UserBirthday,
                avatarUrl: avatarUrl,
                citizenIdCard: cicUrl,
                contactPhone: request.ContactPhone
            );

            await _repo.AddAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return user.UserId;
        }

        //(string fullname, string address, DateTime? birthday, string? avatarUrl, string? citizenIdCard)


        //ADMIN ACTIONS
        public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.GetByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found");

            user.SoftDelete();
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task DisableUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.GetByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found");

            user.BanUser();
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task EnableUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.GetByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException("User not found");

            user.UnBanUser();
            await _uow.SaveChangesAsync(cancellationToken);
        }


        public async Task VerifyUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.GetByIdAsync(userId, cancellationToken)
                 ?? throw new KeyNotFoundException("User not found");

            var profile = user.UserProfile;
            if (profile == null)
                throw new InvalidOperationException("User chưa có hồ sơ để xác minh");

            profile.SetStatus(ProfileVerificationStatus.Verified);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectUserProfileAsync(int userId, string? reason = null, CancellationToken ct = default)
        {
            var user = await _repo.GetByIdAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found");

            var profile = user.UserProfile ?? throw new InvalidOperationException("User chưa có hồ sơ để từ chối");

            profile.Reject(reason); // domain logic tự xử lý thời gian, status, lý do
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _uow.SaveChangesAsync(ct);
        }



        //UPDATE USER
        public async Task<int> UpdateUserAsync(int userId, UpdateUserDto request, CancellationToken cancellationToken = default)
        {
            var user = await _repo.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // --- Upload avatar ---
            string? avatarUrl = null;
            if (request.Avatar != null)
            {
                var ext = Path.GetExtension(request.Avatar.FileName);
                var fileName = $"avatar_{user.UserId}{ext}";
                using var stream = request.Avatar.OpenReadStream();
                avatarUrl = await _fileStorage.SaveFileAsync($"uploads/users/{user.UserId}", fileName, stream, cancellationToken);
            }

            // --- Upload CCCD ---
            string? cicUrl = null;
            bool uploadedCic = false;
            if (request.CitizenIdCard != null)
            {
                var ext = Path.GetExtension(request.CitizenIdCard.FileName);
                var fileName = $"cic_{user.UserId}{ext}";
                using var stream = request.CitizenIdCard.OpenReadStream();
                cicUrl = await _fileStorage.SaveFileAsync($"uploads/users/{user.UserId}", fileName, stream, cancellationToken);

                
            }

            // --- Quản lý profile ---
            var profile = user.UserProfile;
            if (profile == null)
            {
                profile = user.AddOrUpdateProfile(
                    fullname: request.UserFullName ?? "",
                    address: request.UserAddress ?? "",
                    birthday: request.UserBirthday,
                    avatarUrl: avatarUrl,
                    citizenIdCard: cicUrl,
                    contactPhone: request.ContactPhone ?? ""
                );

                if (uploadedCic)
                    profile.SetStatus(ProfileVerificationStatus.Pending);
            }
            else
            {
                profile = user.AddOrUpdateProfile(
                    fullname: request.UserFullName ?? profile.UserFullName,
                    address: request.UserAddress ?? profile.UserAddress,
                    birthday: request.UserBirthday ?? profile.UserBirthday,
                    avatarUrl: avatarUrl ?? profile.Avatar,
                    citizenIdCard: cicUrl ?? profile.CitizenIdCard,
                    contactPhone: request.ContactPhone ?? profile.ContactPhone
                );

                if (uploadedCic)
                    profile.SetStatus(ProfileVerificationStatus.Pending);
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _uow.SaveChangesAsync(cancellationToken);

            return user.UserId;
        }

    }
}