using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.Common
{
    /// <summary>
    /// ViewModel برای مدیریت بازگشت به صفحه مبدا بعد از عملیات
    /// این کلاس امکان تعیین URL بازگشت را فراهم می‌کند
    /// </summary>
    public class ReturnValueViewModel
    {
        /// <summary>
        /// URL بازگشت به صفحه قبلی
        /// اگر خالی باشد، از URL پیش‌فرض استفاده می‌شود
        /// </summary>
        [MaxLength(500, ErrorMessage = "URL بازگشت نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// آیا باید به همان صفحه بازگردد؟
        /// </summary>
        public bool ReturnToSamePage { get; set; }

        /// <summary>
        /// پارامترهای اضافی برای URL بازگشت (به صورت Query String)
        /// مثال: "tab=operations&id=123"
        /// </summary>
        [MaxLength(200, ErrorMessage = "پارامترهای بازگشت نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string? ReturnParams { get; set; }

        /// <summary>
        /// نام صفحه/بخش مبدا (برای لاگ و تشخیص)
        /// مثال: "TaskList", "TaskDetails", "MyDay"
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام صفحه مبدا نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string? SourcePage { get; set; }

        /// <summary>
        /// ساخت URL کامل بازگشت با در نظر گرفتن پارامترها
        /// </summary>
        public string GetFullReturnUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(ReturnParams))
                return ReturnUrl;

            var separator = ReturnUrl.Contains("?") ? "&" : "?";
            return $"{ReturnUrl}{separator}{ReturnParams}";
        }

        /// <summary>
        /// بررسی معتبر بودن URL بازگشت
        /// </summary>
        public bool IsValidReturnUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
                return false;

            // بررسی که URL با / شروع شود (Local URL)
            return ReturnUrl.StartsWith("/") || ReturnUrl.StartsWith("~/");
        }
    }

    /// <summary>
    /// Interface برای ViewModelهایی که نیاز به ReturnValue دارند
    /// </summary>
    public interface IHasReturnValue
    {
        /// <summary>
        /// URL بازگشت
        /// </summary>
        string? ReturnUrl { get; set; }

        /// <summary>
        /// نام صفحه مبدا
        /// </summary>
        string? SourcePage { get; set; }
    }
}
