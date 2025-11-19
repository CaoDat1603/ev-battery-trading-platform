using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Application.Common;
using Identity.Domain.Abtractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System;

namespace Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitofwork;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IRegisterCache _registerCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;
        private readonly string _frontendBaseUrl;
        private readonly string _adminFrontendBaseUrl;

        public AuthService(IUserRepository userRepo, IJwtProvider jwtProvider,IRefreshTokenService refreshTokenService, IUnitOfWork unitOfWork, IEmailService emailService, ISmsService smsService, ILogger<AuthService> logger,IRegisterCache cache,IHttpContextAccessor httpContextAccessor,ICacheService cacheService,IConfiguration config)
        {
            _userRepo = userRepo;
            _refreshTokenService = refreshTokenService;
            _jwtProvider = jwtProvider;
            _unitofwork = unitOfWork;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _registerCache = cache;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
            _frontendBaseUrl = config["Frontend:UserBaseUrl"];
            _adminFrontendBaseUrl = config["Fontend:AdminBaseUrl"];
        }

        //-------------LOGIN-------------
        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
                return null;

            if (!string.IsNullOrWhiteSpace(request.Email) && !EmailValidator.IsValidEmail(request.Email))
                return null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !PhoneValidator.IsValidPhone(request.PhoneNumber))
                return null;
            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user = await _userRepo.GetByPhoneAsync(request.PhoneNumber, cancellationToken);
            }

            if (user == null)
                return null;

            bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.UserPassword);
            if (!validPassword)
                return null;

            if (user.UserStatus != UserStatus.Active)
                return null;

            var token = _jwtProvider.GenerateToken(user);
            var refreshToken = _refreshTokenService.GenerateToken();
            _refreshTokenService.SetTokenCookie(_httpContextAccessor.HttpContext.Response, refreshToken);
            return new LoginResponse
            {
                FullName = user.UserFullName,
                Role = user.Role.ToString(),
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtProvider.ExpireMinutes),
                RefreshToken = refreshToken,
                UserId = user.UserId
            };

        }

        //------------Refresh Token----------
        public async Task<TokenResponse?> RefreshTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var refreshToken = _refreshTokenService.GetTokenFromCookie(httpContext.Request);

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Missing refresh token");

            var principal = _jwtProvider.GetPrincipalFromExpiredToken(accessToken);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier) ?? principal?.FindFirst("sub") // 👈 dùng key chuỗi
                ?? throw new UnauthorizedAccessException("Invalid token");

            var userId = int.Parse(userIdClaim.Value);
            var user = await _userRepo.GetByIdAsync(userId, cancellationToken)
                ?? throw new UnauthorizedAccessException("User not found");

            var newAccess = _jwtProvider.GenerateToken(user);
            var newRefresh = _refreshTokenService.GenerateToken();
            _refreshTokenService.SetTokenCookie(httpContext.Response, newRefresh);

            return new TokenResponse
            {
                AccessToken = newAccess,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtProvider.ExpireMinutes),
                RefreshToken = newRefresh,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };
        }

    //-----------REGISTER--------------
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate existence
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ArgumentException("Vui lòng nhập email hoặc số điện thoại.");
            if (!string.IsNullOrWhiteSpace(request.Email) && await _userRepo.ExistsByEmailAsync(request.Email))
                throw new ArgumentException("Email đã được sử dụng");
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && await _userRepo.ExistsByPhoneAsync(request.PhoneNumber))
                throw new ArgumentException("Số điện thoại đã được sử dụng");
            if (!string.IsNullOrWhiteSpace(request.Email) && !EmailValidator.IsValidEmail(request.Email))
                throw new ArgumentException("Email không hợp lệ.");

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !PhoneValidator.IsValidPhone(request.PhoneNumber))
                throw new ArgumentException("Số điện thoại không hợp lệ.");
            if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
                throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.");

            string otp = GenerateNumeric_Otp.GenerateNumericOtp(6);

            _registerCache.Save(request, otp, TimeSpan.FromMinutes(15));

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                await _emailService.SendEmailAsync(request.Email, "Mã OTP đăng ký",
                    $"Mã OTP của bạn là <strong>{otp}</strong>. Hết hạn sau 15 phút.");
            }
            else
            {
                await _smsService.SendSmsAsync(request.PhoneNumber!, $"Mã OTP của bạn là {otp}");
            }

            return new RegisterResponse { Message = "Đã gửi mã OTP. Vui lòng xác thực để hoàn tất đăng ký." };
        }


        // Xác thực tài khoản bằng OTP
        public async Task<RegisterResponse> VerifyOtpAndCreateUserAsync(VerifyOtpRequest request)
        {
            var pending = _registerCache.Get(request.EmailOrPhone) ??
               throw new InvalidOperationException("OTP đã hết hạn hoặc không tồn tại");
            var register = pending.Request;
            var hashed = BCrypt.Net.BCrypt.HashPassword(register.Password);
            var user = User.Create(
                register.Email?.Trim(),
                register.PhoneNumber?.Trim(),
                hashed,
                register.FullName.Trim(),
                UserRole.Member
                );
            if (!string.IsNullOrEmpty(register.Email)) user.ConfirmEmail();
            else if (!string.IsNullOrEmpty(register.PhoneNumber)) user.ConfirmPhone();

            await _userRepo.AddAsync(user);
            await _unitofwork.SaveChangesAsync();

            user.AddOrUpdateProfile(register.FullName ?? register.Email ?? register.PhoneNumber ?? "New User","", null, null, null, null);
            await _unitofwork.SaveChangesAsync();
            _registerCache.Remove(request.EmailOrPhone);

            return new RegisterResponse
            {
                UserId = user.UserId,
                Message = "Đăng ký thành công"
            };
        }

        // Gửi lại mã OTP
        public async Task<bool> ResendOtpAsync(string emailOrPhone)
        {
            var pending = _registerCache.Get(emailOrPhone)??
                throw new ArgumentException("Không tìm thấy yêu cầu đăng ký hoặc đã hết hạn. Vui lòng đăng ký lại.");

            string otp = GenerateNumeric_Otp.GenerateNumericOtp(6);

            // Cập nhật lại cache với OTP mới (reset thời gian hết hạn)
            _registerCache.Save(pending.Request, otp, TimeSpan.FromMinutes(15));

            // Gửi lại OTP qua email hoặc SMS
            if (!string.IsNullOrEmpty(pending.Request.Email))
            {
                await _emailService.SendEmailAsync(
                    pending.Request.Email,
                    "Mã OTP mới",
                    $"Mã OTP mới của bạn là <strong>{otp}</strong>. Hết hạn sau 15 phút."
                );
            }
            else 
            {
                await _smsService.SendSmsAsync(pending.Request.PhoneNumber!, $"Mã OTP mới của bạn là {otp}");
            }

            return true;
        }


        public async Task<bool> RequestResetPasswordAsync(string emailOrPhone)
        {
            User? user = null;
            if (emailOrPhone.Contains("@"))
                user = await _userRepo.GetByEmailAsync(emailOrPhone);
            else
                user = await _userRepo.GetByPhoneAsync(emailOrPhone);

            if (user == null)
                return false;

            // Sinh token hoặc OTP
            string tokenOrOtp;
            if (emailOrPhone.Contains("@"))
                tokenOrOtp = Guid.NewGuid().ToString(); // email → token link
            else
                tokenOrOtp = new Random().Next(100000, 999999).ToString(); // phone → 6-digit OTP

            // Lưu tạm vào cache
            _cacheService.Set($"reset:{tokenOrOtp}", user.UserId, TimeSpan.FromMinutes(15));
            string frontendBaseUrl = user.Role == UserRole.Admin? _adminFrontendBaseUrl: _frontendBaseUrl;


            // Gửi email hoặc SMS
            if (emailOrPhone.Contains("@"))
            {
                var link = $"{_frontendBaseUrl}/identity/reset-password?token={tokenOrOtp}&uid={user.UserId}";
                await _emailService.SendEmailAsync(
                    emailOrPhone,
                    "Đặt lại mật khẩu",
                    $"Nhấn vào link để đặt lại mật khẩu: <a href=\"{link}\">Reset Password</a>");
            }
            else
            {
                await _smsService.SendSmsAsync(emailOrPhone, $"Mã OTP đặt lại mật khẩu: {tokenOrOtp}");
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Tra userId từ token
            var userId = _cacheService.Get<int>($"reset:{request.TokenOrOtp}");
            if (userId == 0)
                return false; // token sai hoặc hết hạn

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Đổi mật khẩu
            if (!Regex.IsMatch(request.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
                throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatePassword(hashedPassword);

            // Xóa token để không reuse
            _cacheService.Remove($"reset:{request.TokenOrOtp}");

            await _unitofwork.SaveChangesAsync();
            return true;
        }


        public  Task LogoutAsync(string? _, CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            _refreshTokenService.ClearTokenCookie(httpContext.Response);
            return Task.CompletedTask;
        }
    }
}