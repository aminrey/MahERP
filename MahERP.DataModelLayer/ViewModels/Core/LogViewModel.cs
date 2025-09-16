using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.Core
{
    /// <summary>
    /// مدل نمایش لاگ فعالیت کاربر
    /// </summary>
    public class LogViewModel
    {
        /// <summary>
        /// شناسه لاگ
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// نام و نام خانوادگی کاربر
        /// </summary>
        public string UserFullName { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// </summary>
        public byte ActivityType { get; set; }

        /// <summary>
        /// متن نوع فعالیت
        /// </summary>
        public string ActivityTypeText { get; set; }

        /// <summary>
        /// نام ماژول
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// نام عمل
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// شناسه رکورد تحت تاثیر
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// نوع انتیتی
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// عنوان رکورد
        /// </summary>
        public string RecordTitle { get; set; }

        /// <summary>
        /// شرح فعالیت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// مقادیر قبل از تغییر
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// مقادیر جدید
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// نتیجه عملیات
        /// </summary>
        public byte ResultStatus { get; set; }

        /// <summary>
        /// متن نتیجه عملیات
        /// </summary>
        public string ResultStatusText { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// زمان فعالیت
        /// </summary>
        public DateTime ActivityDateTime { get; set; }

        /// <summary>
        /// زمان فعالیت (فارسی)
        /// </summary>
        public string ActivityDateTimePersian { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// آدرس درخواست
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// نوع درخواست HTTP
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// زمان پردازش (میلی‌ثانیه)
        /// </summary>
        public long? ProcessingTimeMs { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// نام شعبه
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// آیا حساس است؟
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// سطح اهمیت
        /// </summary>
        public byte ImportanceLevel { get; set; }

        /// <summary>
        /// متن سطح اهمیت
        /// </summary>
        public string ImportanceLevelText { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public byte? DeviceType { get; set; }

        /// <summary>
        /// متن نوع دستگاه
        /// </summary>
        public string DeviceTypeText { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        public string DeviceInfo { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// شناسه همبستگی
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// آیا آرشیو شده؟
        /// </summary>
        public bool IsArchived { get; set; }
    }

    /// <summary>
    /// مدل لیست لاگ‌ها با صفحه‌بندی
    /// </summary>
    public class LogListViewModel
    {
        /// <summary>
        /// لیست لاگ‌ها
        /// </summary>
        public List<LogViewModel> Logs { get; set; } = new List<LogViewModel>();

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد رکورد در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// تعداد کل رکوردها
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        /// <summary>
        /// آیا صفحه قبلی وجود دارد؟
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// آیا صفحه بعدی وجود دارد؟
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// شماره صفحه قبلی
        /// </summary>
        public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;

        /// <summary>
        /// شماره صفحه بعدی
        /// </summary>
        public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;

        /// <summary>
        /// اطلاعات خلاصه صفحه‌بندی
        /// </summary>
        public string PaginationInfo => $"صفحه {CurrentPage} از {TotalPages} - مجموع {TotalRecords:N0} رکورد";
    }

    /// <summary>
    /// مدل جستجوی پیشرفته لاگ‌ها
    /// </summary>
    public class LogSearchViewModel
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام ماژول
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// نام عمل
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// </summary>
        public byte? ActivityType { get; set; }

        /// <summary>
        /// نتیجه عملیات
        /// </summary>
        public byte? ResultStatus { get; set; }

        /// <summary>
        /// تاریخ شروع جستجو
        /// </summary>
        [Display(Name = "از تاریخ")]
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان جستجو
        /// </summary>
        [Display(Name = "تا تاریخ")]
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// فقط فعالیت‌های حساس
        /// </summary>
        public bool? OnlySensitive { get; set; }

        /// <summary>
        /// سطح اهمیت
        /// </summary>
        public byte? ImportanceLevel { get; set; }

        /// <summary>
        /// فقط لاگ‌های خطا
        /// </summary>
        public bool? OnlyErrors { get; set; }

        /// <summary>
        /// متن جستجو در شرح
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// شناسه رکورد
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// نوع انتیتی
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد رکورد در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// نوع مرتب‌سازی
        /// </summary>
        public string SortBy { get; set; } = "ActivityDateTime";

        /// <summary>
        /// جهت مرتب‌سازی
        /// </summary>
        public string SortDirection { get; set; } = "DESC";
    }

    /// <summary>
    /// مدل آمار کلی لاگ‌ها
    /// </summary>
    public class LogStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل لاگ‌ها
        /// </summary>
        public long TotalLogs { get; set; }

        /// <summary>
        /// تعداد لاگ‌های امروز
        /// </summary>
        public int TodayLogs { get; set; }

        /// <summary>
        /// تعداد لاگ‌های هفته گذشته
        /// </summary>
        public int LastWeekLogs { get; set; }

        /// <summary>
        /// تعداد لاگ‌های ماه گذشته
        /// </summary>
        public int LastMonthLogs { get; set; }

        /// <summary>
        /// تعداد خطاها در 24 ساعت گذشته
        /// </summary>
        public int ErrorsLast24Hours { get; set; }

        /// <summary>
        /// تعداد فعالیت‌های حساس در هفته گذشته
        /// </summary>
        public int SensitiveActivitiesLastWeek { get; set; }

        /// <summary>
        /// تعداد کاربران فعال امروز
        /// </summary>
        public int ActiveUsersToday { get; set; }

        /// <summary>
        /// تعداد IP های منحصر به فرد امروز
        /// </summary>
        public int UniqueIpsToday { get; set; }

        /// <summary>
        /// میانگین زمان پردازش درخواست‌ها (میلی‌ثانیه)
        /// </summary>
        public double AverageProcessingTime { get; set; }

        /// <summary>
        /// تعداد لاگ‌های آرشیو شده
        /// </summary>
        public long ArchivedLogs { get; set; }

        /// <summary>
        /// حجم پایگاه داده لاگ‌ها (مگابایت)
        /// </summary>
        public decimal DatabaseSize { get; set; }
    }

    /// <summary>
    /// مدل آمار فعالیت کاربر
    /// </summary>
    public class UserActivityStatViewModel
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// نام و نام خانوادگی
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// تعداد کل فعالیت‌ها
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// تعداد ورودها
        /// </summary>
        public int LoginCount { get; set; }

        /// <summary>
        /// آخرین فعالیت
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// آخرین فعالیت (فارسی)
        /// </summary>
        public string LastActivityPersian { get; set; }

        /// <summary>
        /// تعداد خطاها
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// میانگین زمان آنلاین (دقیقه)
        /// </summary>
        public double AverageOnlineTime { get; set; }
    }

    /// <summary>
    /// مدل آمار فعالیت ماژول
    /// </summary>
    public class ModuleActivityStatViewModel
    {
        /// <summary>
        /// نام ماژول
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// تعداد کل فعالیت‌ها
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// تعداد کاربران منحصر به فرد
        /// </summary>
        public int UniqueUsers { get; set; }

        /// <summary>
        /// تعداد خطاها
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// میانگین زمان پردازش
        /// </summary>
        public double AverageProcessingTime { get; set; }

        /// <summary>
        /// درصد استفاده
        /// </summary>
        public decimal UsagePercentage { get; set; }
    }

    /// <summary>
    /// مدل کاربران پرفعالیت
    /// </summary>
    public class TopUserActivityViewModel
    {
        /// <summary>
        /// رتبه
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// نام و نام خانوادگی
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// تعداد فعالیت‌ها
        /// </summary>
        public int ActivityCount { get; set; }

        /// <summary>
        /// آخرین فعالیت
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// نام شعبه
        /// </summary>
        public string BranchName { get; set; }
    }

    /// <summary>
    /// مدل تلاش‌های ناموفق ورود
    /// </summary>
    public class FailedLoginAttemptViewModel
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// زمان تلاش
        /// </summary>
        public DateTime AttemptTime { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// دلیل شکست
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// تعداد تلاش‌ها از همین IP
        /// </summary>
        public int AttemptsFromIp { get; set; }
    }
}