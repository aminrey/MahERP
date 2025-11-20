using MahERP.DataModelLayer.Services;
using Microsoft.Extensions.Configuration;

namespace MahERP.DataModelLayer.Extensions
{
    /// <summary>
    /// سرویس مدیریت کدهای تسک با قابلیت تنظیم پیشوند و تعداد ارقام
    /// </summary>
    public class TaskCodeGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public TaskCodeGenerator(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// تولید کد تسک اتوماتیک بر اساس تنظیمات
        /// </summary>
        /// <returns>کد تسک جدید</returns>
        public string GenerateTaskCode()
        {
            var settings = GetTaskCodeSettings();
            
            // دریافت آخرین شماره تسک سیستمی
            var lastSystemTaskNumber = GetLastSystemTaskNumber(settings.SystemPrefix);
            
            // شماره بعدی
            var nextNumber = lastSystemTaskNumber + 1;
            
            // تولید کد با فرمت مشخص
            var code = $"{settings.SystemPrefix}-{nextNumber.ToString().PadLeft(settings.DigitCount, '0')}";
            
            return code;
        }

        /// <summary>
        /// بررسی معتبر بودن کد تسک
        /// </summary>
        /// <param name="taskCode">کد تسک</param>
        /// <param name="excludeId">شناسه تسک جهت استثنا در ویرایش</param>
        /// <returns>true اگر کد معتبر باشد</returns>
        public bool ValidateTaskCode(string taskCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taskCode))
                return false;

            var settings = GetTaskCodeSettings();
            
            // بررسی عدم استفاده از پیشوند سیستمی در کد دستی
            if (taskCode.StartsWith(settings.SystemPrefix + "-", StringComparison.OrdinalIgnoreCase))
                return false;

            // بررسی یکتا بودن کد
            return IsCodeUnique(taskCode, excludeId);
        }

        /// <summary>
        /// دریافت تنظیمات کد تسک از appsettings
        /// </summary>
        /// <returns>تنظیمات کد تسک</returns>
        public TaskCodeSettings GetTaskCodeSettings()
        {
            return new TaskCodeSettings
            {
                SystemPrefix = _configuration.GetValue<string>("TaskCodeSettings:SystemPrefix") ?? "TSK",
                DigitCount = _configuration.GetValue("TaskCodeSettings:DigitCount", 6),
                AllowManualInput = _configuration.GetValue("TaskCodeSettings:AllowManualInput", true)
            };
        }

        /// <summary>
        /// دریافت آخرین شماره تسک سیستمی - اصلاح شده
        /// </summary>
        /// <param name="systemPrefix">پیشوند سیستمی</param>
        /// <returns>آخرین شماره</returns>
        private int GetLastSystemTaskNumber(string systemPrefix)
        {
            var prefix = systemPrefix + "-";
            
            // ⭐⭐⭐ دریافت تمام کدهای تسک‌های سیستمی
            var systemTaskCodes = _unitOfWork.TaskUW
                .Get(t => t.TaskCode != null && t.TaskCode.StartsWith(prefix))
                .Where(t => !string.IsNullOrEmpty(t.TaskCode))
                .Select(t => t.TaskCode)
                .ToList();

            if (!systemTaskCodes.Any())
                return 0;

            // ⭐⭐⭐ استخراج شماره‌ها و مرتب‌سازی عددی
            var maxNumber = 0;
            
            foreach (var code in systemTaskCodes)
            {
                var numberPart = code.Substring(prefix.Length);
                
                if (int.TryParse(numberPart, out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            return maxNumber;
        }

        /// <summary>
        /// بررسی یکتا بودن کد تسک
        /// </summary>
        /// <param name="taskCode">کد تسک</param>
        /// <param name="excludeId">شناسه جهت استثنا</param>
        /// <returns>true اگر یکتا باشد</returns>
        private bool IsCodeUnique(string taskCode, int? excludeId = null)
        {
            var query = _unitOfWork.TaskUW.Get(t => t.TaskCode == taskCode);

            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);

            return !query.Any();
        }
    }

    /// <summary>
    /// تنظیمات کد تسک
    /// </summary>
    public class TaskCodeSettings
    {
        /// <summary>
        /// پیشوند سیستمی (پیش‌فرض: TSK)
        /// </summary>
        public string SystemPrefix { get; set; } = "TSK";

        /// <summary>
        /// تعداد ارقام شماره (پیش‌فرض: 6)
        /// </summary>
        public int DigitCount { get; set; } = 6;

        /// <summary>
        /// امکان ورود دستی کد توسط کاربر
        /// </summary>
        public bool AllowManualInput { get; set; } = true;
    }
}