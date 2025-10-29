using Identity.Domain.Enums;

namespace Identity.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string UserPassword { get; set; }
        public string UserFullName { get; set; }
        public UserRole Role { get; set; } = UserRole.Guest;
        public UserStatus? UserStatus { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public bool IsPhoneConfirmed { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public UserProfile? UserProfile { get; set; }

        private User() { }
        public static User Create(string email, string phone, string password, string UserFullName, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Either email or phone is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));


            return new User
            {
                UserEmail = email?.Trim(),
                UserPhone = phone?.Trim(),
                UserPassword = password,
                UserFullName = UserFullName.Trim(),
                Role = role,
                UserStatus = Enums.UserStatus.Active,
                IsEmailConfirmed = false,
                IsPhoneConfirmed = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public UserProfile AddOrUpdateProfile(string fullname, string address,DateTime? birthday,string? avatarUrl,string? citizenIdCard,string? contactPhone)
        {
            if (UserProfile == null)
            {
                UserProfile = new UserProfile(fullname, address, birthday,avatarUrl, citizenIdCard,ProfileVerificationStatus.Unverified, contactPhone);
            }
            else
            {
                bool changedCitizenId = citizenIdCard != UserProfile.CitizenIdCard;

                UserProfile.UserFullName = fullname;
                UserProfile.UserAddress = address;
                UserProfile.UserBirthday = birthday;
                UserProfile.Avatar = avatarUrl;
                UserProfile.CitizenIdCard = citizenIdCard;
                UserProfile.ContactPhone = contactPhone;

                if (changedCitizenId)
                    UserProfile.Status = ProfileVerificationStatus.Unverified;

            }

            return UserProfile;
        }



        public void SetRole(UserRole role) => Role = role;
        public void SetStatus(UserStatus status) => UserStatus = status;

        public void ConfirmEmail()
        {
            IsEmailConfirmed = true;
        }
        public void ConfirmPhone()
        {
            IsPhoneConfirmed = true;
        }

        public void VerifyProfileByCitizenId(string citizenIdCard)
        {
            if (UserProfile == null)
                throw new InvalidOperationException("UserProfile chưa tồn tại để xác minh.");
            UserProfile.CitizenIdCard = citizenIdCard;
            if (UserProfile.Status == ProfileVerificationStatus.Unverified)
                UserProfile.SetStatus(ProfileVerificationStatus.Pending);
            UserProfile.UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateProfile(string? fullName = null, string? address = null, DateTime? birthday = null, string? contactPhone = null)
        {
            if (UserProfile == null)
                throw new InvalidOperationException("UserProfile chưa tồn tại để xác minh.");

            if (!string.IsNullOrEmpty(fullName))
                UserProfile.UserFullName = fullName;

            if (!string.IsNullOrEmpty(address))
                UserProfile.UserAddress = address;

            if (birthday.HasValue)
                UserProfile.UserBirthday = birthday;

            if (!string.IsNullOrEmpty(contactPhone))
                UserProfile.ContactPhone = contactPhone;

            UserProfile.UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdatePassword(string newHashedPassword)
        {
            if (string.IsNullOrWhiteSpace(newHashedPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(newHashedPassword));

            UserPassword = newHashedPassword;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void BanUser()
        {
            UserStatus = Enums.UserStatus.Banned;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UnBanUser()
        {
            UserStatus = Enums.UserStatus.Active;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
            UserProfile.DeletedAt = DateTimeOffset.UtcNow;
        }

        public bool IsDeleted => DeletedAt.HasValue;
    }

}
