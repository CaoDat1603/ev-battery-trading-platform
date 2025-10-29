using System.Text.RegularExpressions;

namespace Identity.Application.Common
{
    public class PhoneValidator
    {
        /// <summary>
        /// Kiểm tra định dạng số điện thoại Việt Nam hợp lệ.
        /// Hỗ trợ cả 0xxxxxxxxx và +84xxxxxxxxx (9–10 số).
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = phone.Replace(" ", "").Replace("-", "");

            // Chuẩn Việt Nam: 10 số, bắt đầu bằng 03, 05, 07, 08, hoặc 09
            // hoặc dạng quốc tế +84 tương đương
            return Regex.IsMatch(phone, @"^(0|\+84)(3|5|7|8|9)[0-9]{8}$");
        }

    }
}