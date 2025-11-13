namespace Identity.Application.DTOs
{
    public class SystemTokenResponseDto
    {
        public bool IsSuccess { get; private set; }
        public string? Token { get; private set; }
        public string? Message { get; private set; }

        private SystemTokenResponseDto(bool isSuccess, string? token = null, string? message = null)
        {
            IsSuccess = isSuccess;
            Token = token;
            Message = message;
        }

        public static SystemTokenResponseDto Success(string token) => new(true, token);
        public static SystemTokenResponseDto Fail(string message) => new(false, null, message);
    }
}
