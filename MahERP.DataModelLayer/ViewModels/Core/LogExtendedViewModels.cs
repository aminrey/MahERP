using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.Core
{
    /// <summary>
    /// مدل ایجاد لاگ جدید
    /// این مدل برای ثبت سریع لاگ‌های سیستم استفاده می‌شود
    /// </summary>
    public class CreateLogViewModel
    {
        /// <summary>
        /// شناسه کاربر انجام دهنده
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// </summary>
        [Required(ErrorMessage = "نوع فعالیت الزامی است")]
        public byte ActivityType { get; set; }

        /// <summary>
        /// نام ماژول
        /// </summary>
        [Required(ErrorMessage = "نام ماژول الزامی است")]
        [MaxLength(100, ErrorMessage = "نام ماژول نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string ModuleName { get; set; }

        /// <summary>
        /// نام عمل انجام شده
        /// </summary>
        [Required(ErrorMessage = "نام عمل الزامی است")]
        [MaxLength(100, ErrorMessage = "نام عمل نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string ActionName { get; set; }

        /// <summary>
        /// شناسه رکورد تحت تاثیر
        /// </summary>
        [MaxLength(50)]
        public string RecordId { get; set; }

        /// <summary>
        /// نوع انتیتی تحت تاثیر
        /// </summary>
        [MaxLength(100)]
        public string EntityType { get; set; }

        /// <summary>
        /// عنوان رکورد
        /// </summary>
        [MaxLength(500)]
        public string RecordTitle { get; set; }

        /// <summary>
        /// شرح فعالیت
        /// </summary>
        [Required(ErrorMessage = "شرح فعالیت الزامی است")]
        [MaxLength(2000, ErrorMessage = "شرح فعالیت نمی‌تواند بیش از 2000 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// مقادیر قبل از تغییر (JSON)
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// مقادیر جدید (JSON)
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// پارامترهای درخواست (JSON)
        /// </summary>
        public string RequestParameters { get; set; }

        /// <summary>
        /// نتیجه عملیات
        /// </summary>
        public byte ResultStatus { get; set; } = 0; // پیش‌فرض: موفق

        /// <summary>
        /// پیام خطا
        /// </summary>
        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        [MaxLength(45)] // IPv6 support
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent مرورگر
        /// </summary>
        [MaxLength(500)]
        public string UserAgent { get; set; }

        /// <summary>
        /// URL درخواست
        /// </summary>
        [MaxLength(2000)]
        public string RequestUrl { get; set; }

        /// <summary>
        /// نوع درخواست HTTP
        /// </summary>
        [MaxLength(10)]
        public string HttpMethod { get; set; }

        /// <summary>
        /// زمان پردازش (میلی‌ثانیه)
        /// </summary>
        public long? ProcessingTimeMs { get; set; }

        /// <summary>
        /// اندازه پاسخ (بایت)
        /// </summary>
        public long? ResponseSize { get; set; }

        /// <summary>
        /// شناسه جلسه
        /// </summary>
        [MaxLength(200)]
        public string SessionId { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// آیا این فعالیت حساس است؟
        /// </summary>
        public bool IsSensitive { get; set; } = false;

        /// <summary>
        /// سطح اهمیت لاگ
        /// </summary>
        public byte ImportanceLevel { get; set; } = 0; // پیش‌فرض: عادی

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public byte? DeviceType { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [MaxLength(500)]
        public string DeviceInfo { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        [MaxLength(200)]
        public string Location { get; set; }

        /// <summary>
        /// تگ‌های اضافی
        /// </summary>
        [MaxLength(500)]
        public string Tags { get; set; }

        /// <summary>
        /// شناسه همبستگی
        /// </summary>
        public Guid? CorrelationId { get; set; }
    }

    /// <summary>
    /// مدل خلاصه آمار روزانه
    /// </summary>
    public class DailyStatsViewModel
    {
        /// <summary>
        /// تاریخ
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// تاریخ فارسی
        /// </summary>
        public string PersianDate { get; set; }

        /// <summary>
        /// تعداد کل فعالیت‌ها
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// تعداد کاربران فعال
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// تعداد خطاها
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// تعداد فعالیت‌های حساس
        /// </summary>
        public int SensitiveActivities { get; set; }

        /// <summary>
        /// میانگین زمان پردازش
        /// </summary>
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// مدل نتایج جستجو
    /// </summary>
    public class SearchResultsViewModel
    {
        /// <summary>
        /// نتایج جستجو
        /// </summary>
        public LogListViewModel Results { get; set; }

        /// <summary>
        /// معیارهای جستجو
        /// </summary>
        public LogSearchViewModel SearchCriteria { get; set; }

        /// <summary>
        /// زمان انجام جستجو (میلی‌ثانیه)
        /// </summary>
        public long SearchTimeMs { get; set; }

        /// <summary>
        /// پیام‌های اضافی
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// مدل تنظیمات لاگینگ
    /// </summary>
    public class LoggingSettingsViewModel
    {
        /// <summary>
        /// آیا لاگینگ فعال است؟
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// حداکثر روزهای نگهداری لاگ
        /// </summary>
        [Range(1, 3650, ErrorMessage = "روزهای نگهداری باید بین 1 تا 3650 روز باشد")]
        public int RetentionDays { get; set; } = 365;

        /// <summary>
        /// آیا لاگ‌های عادی ثبت شود؟
        /// </summary>
        public bool LogNormalActivities { get; set; } = true;

        /// <summary>
        /// آیا لاگ‌های حساس ثبت شود؟
        /// </summary>
        public bool LogSensitiveActivities { get; set; } = true;

        /// <summary>
        /// آیا خطاها ثبت شود؟
        /// </summary>
        public bool LogErrors { get; set; } = true;

        /// <summary>
        /// حداقل سطح اهمیت برای ثبت
        /// </summary>
        [Range(0, 2, ErrorMessage = "سطح اهمیت باید بین 0 تا 2 باشد")]
        public byte MinimumImportanceLevel { get; set; } = 0;

        /// <summary>
        /// آیا لاگ User Agent ثبت شود؟
        /// </summary>
        public bool LogUserAgent { get; set; } = true;

        /// <summary>
        /// آیا لاگ IP ثبت شود؟
        /// </summary>
        public bool LogIpAddress { get; set; } = true;

        /// <summary>
        /// آیا پارامترهای درخواست ثبت شود؟
        /// </summary>
        public bool LogRequestParameters { get; set; } = false;

        /// <summary>
        /// آیا زمان پردازش ثبت شود؟
        /// </summary>
        public bool LogProcessingTime { get; set; } = true;

        /// <summary>
        /// حداکثر اندازه پیام خطا (کاراکتر)
        /// </summary>
        [Range(100, 5000, ErrorMessage = "حداکثر اندازه پیام خطا باید بین 100 تا 5000 کاراکتر باشد")]
        public int MaxErrorMessageLength { get; set; } = 2000;

        /// <summary>
        /// لیست ماژول‌هایی که لاگ نمی‌شوند
        /// </summary>
        public string ExcludedModules { get; set; }

        /// <summary>
        /// لیست IP هایی که لاگ نمی‌شوند
        /// </summary>
        public string ExcludedIpAddresses { get; set; }
    }

    /// <summary>
    /// مدل آمار عملکرد سیستم
    /// </summary>
    public class SystemPerformanceViewModel
    {
        /// <summary>
        /// میانگین زمان پردازش در 24 ساعت گذشته
        /// </summary>
        public double AvgProcessingTime24h { get; set; }

        /// <summary>
        /// میانگین زمان پردازش در هفته گذشته
        /// </summary>
        public double AvgProcessingTimeWeek { get; set; }

        /// <summary>
        /// کندترین درخواست‌ها در 24 ساعت گذشته
        /// </summary>
        public List<SlowestRequestViewModel> SlowestRequests { get; set; } = new List<SlowestRequestViewModel>();

        /// <summary>
        /// پرترافیک‌ترین ماژول‌ها
        /// </summary>
        public List<BusiestModuleViewModel> BusiestModules { get; set; } = new List<BusiestModuleViewModel>();

        /// <summary>
        /// روند عملکرد هفتگی
        /// </summary>
        public List<DailyStatsViewModel> WeeklyTrend { get; set; } = new List<DailyStatsViewModel>();
    }

    /// <summary>
    /// مدل کندترین درخواست‌ها
    /// </summary>
    public class SlowestRequestViewModel
    {
        /// <summary>
        /// URL درخواست
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// زمان پردازش (میلی‌ثانیه)
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// زمان درخواست
        /// </summary>
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// کاربر درخواست کننده
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// نوع درخواست HTTP
        /// </summary>
        public string HttpMethod { get; set; }
    }

    /// <summary>
    /// مدل پرترافیک‌ترین ماژول‌ها
    /// </summary>
    public class BusiestModuleViewModel
    {
        /// <summary>
        /// نام ماژول
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// تعداد درخواست‌ها
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// تعداد کاربران منحصر به فرد
        /// </summary>
        public int UniqueUsers { get; set; }

        /// <summary>
        /// میانگین زمان پردازش
        /// </summary>
        public double AvgProcessingTime { get; set; }

        /// <summary>
        /// درصد از کل ترافیک
        /// </summary>
        public decimal TrafficPercentage { get; set; }
    }
}