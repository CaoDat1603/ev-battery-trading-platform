using System.Security.Cryptography;

namespace Identity.Application.Common
{
    public class GenerateNumeric_Otp
    {
        public static string GenerateNumericOtp(int len)
        {
            if (len <= 0)
                throw new ArgumentOutOfRangeException(nameof(len), "Độ dài OTP phải lớn hơn 0.");

            // Tạo mảng byte ngẫu nhiên
            var bytes = new byte[len];
            RandomNumberGenerator.Fill(bytes);

            // Chuyển từng byte thành chữ số
            var otp = new char[len];
            for (int i = 0; i < len; i++)
            {
                // Chia lấy dư 10 để được số 0–9
                otp[i] = (char)('0' + (bytes[i] % 10));
            }

            return new string(otp);
        }
    }
}