using System.Net.Mail;

namespace Identity.Application.Common
{
    public static class EmailValidator
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email.Trim());
                return true; // chỉ cần khởi tạo thành công là hợp lệ
            }
            catch
            {
                return false;
            }
        }
    }
}
