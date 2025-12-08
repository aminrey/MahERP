using System.Linq;
using System.Text.RegularExpressions;

namespace MahERP.CommonLayer.Helpers
{
    /// <summary>
    /// کلاس کمکی برای نرمال‌سازی و اعتبارسنجی شماره‌های تلفن ایران
    /// </summary>
    public static class PhoneNumberHelper
    {
        /// <summary>
        /// نرمال‌سازی شماره تلفن به فرمت استاندارد
        /// مثال: +989123335566 => 09123335566
        /// مثال: 989123335566 => 09123335566
        /// مثال: 9123335566 => 09123335566
        /// مثال: 02633355444 => 02633355444
        /// </summary>
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // حذف تمام کاراکترهای غیر عددی (فاصله، خط تیره، پرانتز و...)
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // اگر خالی شد، برگردان همان ورودی
            if (string.IsNullOrEmpty(digitsOnly))
                return phoneNumber;

            // ⭐ نرمال‌سازی شماره موبایل
            if (digitsOnly.StartsWith("98") && digitsOnly.Length == 12)
            {
                // 989123335566 => 09123335566
                return "0" + digitsOnly.Substring(2);
            }
            else if (digitsOnly.StartsWith("0098") && digitsOnly.Length == 14)
            {
                // 00989123333566 => 09123335566
                return "0" + digitsOnly.Substring(4);
            }
            else if (digitsOnly.Length == 10 && !digitsOnly.StartsWith("0"))
            {
                // 9123335566 => 09123335566
                return "0" + digitsOnly;
            }

            // ⭐ شماره تلفن ثابت یا موبایل که با 0 شروع می‌شود
            // 09123335566 یا 02633355444
            if (digitsOnly.StartsWith("0") && (digitsOnly.Length == 11 || digitsOnly.Length == 10))
            {
                return digitsOnly;
            }

            // اگر هیچ‌کدام نبود، همان ورودی را برگردان
            return digitsOnly;
        }

        /// <summary>
        /// اعتبارسنجی شماره تلفن ایران
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="errorMessage">پیام خطا</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool ValidateIranianPhoneNumber(string phoneNumber, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                errorMessage = "شماره تلفن نمی‌تواند خالی باشد";
                return false;
            }

            // نرمال‌سازی
            var normalized = NormalizePhoneNumber(phoneNumber);

            // بررسی طول
            if (normalized.Length < 10)
            {
                errorMessage = "شماره تلفن باید حداقل 10 رقم باشد";
                return false;
            }

            if (normalized.Length > 11)
            {
                errorMessage = "شماره تلفن نامعتبر است";
                return false;
            }

            // ⭐⭐⭐ بررسی شماره موبایل (11 رقمی که با 09 شروع می‌شود)
            if (normalized.Length == 11 && normalized.StartsWith("09"))
            {
                return true; // ✅ موبایل معتبر
            }

            // ⭐⭐⭐ بررسی شماره تلفن ثابت (11 رقمی که با 0 شروع می‌شود اما نه 09)
            if (normalized.Length == 11 && normalized.StartsWith("0") && !normalized.StartsWith("09"))
            {
                return true; // ✅ تلفن ثابت 11 رقمی معتبر (مثل 02634305257)
            }

            // ⭐⭐⭐ بررسی شماره تلفن ثابت (10 رقمی که با 0 شروع می‌شود)
            if (normalized.Length == 10 && normalized.StartsWith("0"))
            {
                return true; // ✅ تلفن ثابت 10 رقمی معتبر
            }

            errorMessage = "فرمت شماره تلفن نامعتبر است";
            return false;
        }

        /// <summary>
        /// فرمت کردن شماره برای نمایش
        /// مثال: 09123335566 => 0912 333 5566
        /// مثال: 02633355444 => 026 3335 5444
        /// </summary>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var normalized = NormalizePhoneNumber(phoneNumber);

            // موبایل: 0912 333 5566
            if (normalized.Length == 11 && normalized.StartsWith("09"))
            {
                return $"{normalized.Substring(0, 4)} {normalized.Substring(4, 3)} {normalized.Substring(7)}";
            }

            // تلفن ثابت: 026 3335 5444
            if (normalized.Length == 11 && normalized.StartsWith("0"))
            {
                return $"{normalized.Substring(0, 3)} {normalized.Substring(3, 4)} {normalized.Substring(7)}";
            }

            // تلفن ثابت 10 رقمی: 021 1234567
            if (normalized.Length == 10 && normalized.StartsWith("0"))
            {
                return $"{normalized.Substring(0, 3)} {normalized.Substring(3)}";
            }

            return normalized;
        }

        /// <summary>
        /// تشخیص نوع شماره
        /// </summary>
        public static byte DetectPhoneType(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return 0; // پیش‌فرض موبایل

            var normalized = NormalizePhoneNumber(phoneNumber);

            // موبایل
            if (normalized.Length == 11 && normalized.StartsWith("09"))
            {
                return 0; // Mobile
            }

            // تلفن ثابت
            if ((normalized.Length == 10 || normalized.Length == 11) && normalized.StartsWith("0"))
            {
                return 1; // Landline
            }

            return 0; // پیش‌فرض
        }
    }
}