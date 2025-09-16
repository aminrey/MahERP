using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels
{
    /// <summary>
    /// ویو مدل نمایش یک نوتیفیکیشن
    /// </summary>
    public class CoreNotificationViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// شناسه سیستم اصلی ERP
        /// </summary>
        public byte SystemId { get; set; }

        /// <summary>
        /// نام سیستم برای نمایش
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده
        /// </summary>
        public string RecipientUserId { get; set; }

        /// <summary>
        /// نام کاربر دریافت‌کننده برای نمایش
        /// </summary>
        public string RecipientUserName { get; set; }

        /// <summary>
        /// شناسه کاربر فرستنده
        /// </summary>
        public string SenderUserId { get; set; }

        /// <summary>
        /// نام کاربر فرستنده برای نمایش
        /// </summary>
        public string SenderUserName { get; set; }

        /// <summary>
        /// نوع کلی نوتیفیکیشن
        /// </summary>
        public byte NotificationTypeGeneral { get; set; }

        /// <summary>
        /// نام نوع نوتیفیکیشن برای نمایش
        /// </summary>
        public string NotificationTypeName { get; set; }

        /// <summary>
        /// عنوان نوتیفیکیشن
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// متن پیام نوتیفیکیشن
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// تاریخ ایجاد نوتیفیکیشن
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد به شمسی
        /// </summary>
        public string CreateDatePersian { get; set; }

        /// <summary>
        /// زمان ایجاد (ساعت:دقیقه)
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// آیا نوتیفیکیشن خوانده شده؟
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// تاریخ خواندن نوتیفیکیشن
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// تاریخ خواندن به شمسی
        /// </summary>
        public string ReadDatePersian { get; set; }

        /// <summary>
        /// آیا کاربر روی نوتیفیکیشن کلیک کرده؟
        /// </summary>
        public bool IsClicked { get; set; }

        /// <summary>
        /// تاریخ کلیک روی نوتیفیکیشن
        /// </summary>
        public DateTime? ClickDate { get; set; }

        /// <summary>
        /// اولویت نوتیفیکیشن
        /// </summary>
        public byte Priority { get; set; }

        /// <summary>
        /// نام اولویت برای نمایش
        /// </summary>
        public string PriorityName { get; set; }

        /// <summary>
        /// کلاس CSS برای اولویت
        /// </summary>
        public string PriorityClass { get; set; }

        /// <summary>
        /// URL عمل برای هدایت کاربر
        /// </summary>
        public string ActionUrl { get; set; }

        /// <summary>
        /// شناسه رکورد مرتبط
        /// </summary>
        public string RelatedRecordId { get; set; }

        /// <summary>
        /// نوع رکورد مرتبط
        /// </summary>
        public string RelatedRecordType { get; set; }

        /// <summary>
        /// عنوان رکورد مرتبط
        /// </summary>
        public string RelatedRecordTitle { get; set; }

        /// <summary>
        /// آیا این نوتیفیکیشن فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// جزئیات تغییرات (در صورت وجود)
        /// </summary>
        public List<CoreNotificationDetailViewModel> Details { get; set; } = new List<CoreNotificationDetailViewModel>();

        /// <summary>
        /// اطلاعات تحویل (ایمیل، تلگرام و...)
        /// </summary>
        public List<CoreNotificationDeliveryViewModel> Deliveries { get; set; } = new List<CoreNotificationDeliveryViewModel>();

        /// <summary>
        /// متن کامل برای نمایش (ترکیب عنوان و پیام)
        /// </summary>
        public string FullText => $"{Title}: {Message}";

        /// <summary>
        /// متن کوتاه برای نمایش (محدود به 100 کاراکتر)
        /// </summary>
        public string ShortText => FullText.Length > 100 ? FullText.Substring(0, 100) + "..." : FullText;

        /// <summary>
        /// زمان نسبی (5 دقیقه پیش، یک ساعت پیش و...)
        /// </summary>
        public string RelativeTime { get; set; }

        /// <summary>
        /// آیکون مربوط به نوع نوتیفیکیشن
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// کلاس CSS برای استایل نوتیفیکیشن
        /// </summary>
        public string CssClass { get; set; }
    }

    /// <summary>
    /// ویو مدل جزئیات تغییرات نوتیفیکیشن
    /// </summary>
    public class CoreNotificationDetailViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// شناسه نوتیفیکیشن اصلی
        /// </summary>
        public int CoreNotificationId { get; set; }

        /// <summary>
        /// نوع تخصصی نوتیفیکیشن
        /// </summary>
        public byte NotificationTypeSpecific { get; set; }

        /// <summary>
        /// نام فیلد تغییر یافته
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// نام فیلد برای نمایش
        /// </summary>
        public string FieldDisplayName { get; set; }

        /// <summary>
        /// مقدار قبلی
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// مقدار جدید
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// توضیحات تکمیلی
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا این تغییر مهم است؟
        /// </summary>
        public bool IsImportant { get; set; }

        /// <summary>
        /// کلاس CSS برای highlight
        /// </summary>
        public string HighlightClass { get; set; }
    }

    /// <summary>
    /// ویو مدل تحویل نوتیفیکیشن
    /// </summary>
    public class CoreNotificationDeliveryViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// شناسه نوتیفیکیشن اصلی
        /// </summary>
        public int CoreNotificationId { get; set; }

        /// <summary>
        /// روش تحویل
        /// </summary>
        public byte DeliveryMethod { get; set; }

        /// <summary>
        /// نام روش تحویل برای نمایش
        /// </summary>
        public string DeliveryMethodName { get; set; }

        /// <summary>
        /// آدرس مقصد
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// وضعیت تحویل
        /// </summary>
        public byte DeliveryStatus { get; set; }

        /// <summary>
        /// نام وضعیت تحویل برای نمایش
        /// </summary>
        public string DeliveryStatusName { get; set; }

        /// <summary>
        /// تاریخ تلاش برای ارسال
        /// </summary>
        public DateTime? AttemptDate { get; set; }

        /// <summary>
        /// تاریخ تحویل موفق
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ارسال
        /// </summary>
        public int AttemptCount { get; set; }

        /// <summary>
        /// متن خطا
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// شناسه خارجی
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string StatusClass { get; set; }

        /// <summary>
        /// آیکون وضعیت
        /// </summary>
        public string StatusIcon { get; set; }
    }

    /// <summary>
    /// ویو مدل لیست نوتیفیکیشن‌ها
    /// </summary>
    public class CoreNotificationListViewModel
    {
        /// <summary>
        /// لیست نوتیفیکیشن‌ها
        /// </summary>
        public List<CoreNotificationViewModel> Notifications { get; set; } = new List<CoreNotificationViewModel>();

        /// <summary>
        /// تعداد کل نوتیفیکیشن‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد نوتیفیکیشن‌های خوانده نشده
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// شماره صفحه جاری
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد رکورد در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// آیا صفحه قبلی وجود دارد؟
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// آیا صفحه بعدی وجود دارد؟
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// درصد خوانده شده
        /// </summary>
        public double ReadPercentage => TotalCount > 0 ? ((double)(TotalCount - UnreadCount) / TotalCount) * 100 : 0;

        /// <summary>
        /// فیلترهای اعمال شده
        /// </summary>
        public CoreNotificationFilterViewModel Filters { get; set; } = new CoreNotificationFilterViewModel();
    }

    /// <summary>
    /// ویو مدل فیلتر نوتیفیکیشن‌ها
    /// </summary>
    public class CoreNotificationFilterViewModel
    {
        /// <summary>
        /// شناسه سیستم برای فیلتر
        /// </summary>
        public byte? SystemId { get; set; }

        /// <summary>
        /// نوع نوتیفیکیشن برای فیلتر
        /// </summary>
        public byte? NotificationTypeGeneral { get; set; }

        /// <summary>
        /// فقط خوانده نشده‌ها
        /// </summary>
        public bool UnreadOnly { get; set; } = false;

        /// <summary>
        /// اولویت مشخص
        /// </summary>
        public byte? Priority { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// کلمه کلیدی جستجو
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// نوع رکورد مرتبط
        /// </summary>
        public string RelatedRecordType { get; set; }
    }

    /// <summary>
    /// ویو مدل تنظیمات نوتیفیکیشن کاربر
    /// </summary>
    public class CoreNotificationSettingViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// شناسه سیستم اصلی ERP
        /// </summary>
        [Required(ErrorMessage = "شناسه سیستم الزامی است")]
        public byte SystemId { get; set; }

        /// <summary>
        /// نام سیستم برای نمایش
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// نوع کلی نوتیفیکیشن
        /// </summary>
        [Required(ErrorMessage = "نوع نوتیفیکیشن الزامی است")]
        public byte NotificationTypeGeneral { get; set; }

        /// <summary>
        /// نام نوع نوتیفیکیشن برای نمایش
        /// </summary>
        public string NotificationTypeName { get; set; }

        /// <summary>
        /// آیا نوتیفیکیشن سیستمی فعال است؟
        /// </summary>
        public bool IsSystemEnabled { get; set; } = true;

        /// <summary>
        /// آیا ارسال ایمیل فعال است؟
        /// </summary>
        public bool IsEmailEnabled { get; set; } = false;

        /// <summary>
        /// آیا ارسال پیامک فعال است؟
        /// </summary>
        public bool IsSmsEnabled { get; set; } = false;

        /// <summary>
        /// آیا ارسال تلگرام فعال است؟
        /// </summary>
        public bool IsTelegramEnabled { get; set; } = false;

        /// <summary>
        /// ساعت شروع دریافت نوتیفیکیشن
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// ساعت پایان دریافت نوتیفیکیشن
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// آیا در روزهای تعطیل نوتیفیشن ارسال شود؟
        /// </summary>
        public bool SendOnHolidays { get; set; } = true;

        /// <summary>
        /// آیا این تنظیمات فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ویو مدل آمار نوتیفیکیشن‌های کاربر
    /// </summary>
    public class CoreNotificationStatsViewModel
    {
        /// <summary>
        /// تعداد کل نوتیفیکیشن‌ها
        /// </summary>
        public int TotalNotifications { get; set; }

        /// <summary>
        /// تعداد نوتیفیکیشن‌های خوانده نشده
        /// </summary>
        public int UnreadNotifications { get; set; }

        /// <summary>
        /// تعداد نوتیفیکیشن‌های خوانده شده
        /// </summary>
        public int ReadNotifications { get; set; }

        /// <summary>
        /// تعداد نوتیفیکیشن‌های کلیک شده
        /// </summary>
        public int ClickedNotifications { get; set; }

        /// <summary>
        /// درصد خوانده شده
        /// </summary>
        public double ReadPercentage => TotalNotifications > 0 ? (double)ReadNotifications / TotalNotifications * 100 : 0;

        /// <summary>
        /// درصد کلیک شده
        /// </summary>
        public double ClickPercentage => TotalNotifications > 0 ? (double)ClickedNotifications / TotalNotifications * 100 : 0;

        /// <summary>
        /// آمار بر اساس سیستم
        /// </summary>
        public List<SystemNotificationStat> NotificationsBySystem { get; set; } = new List<SystemNotificationStat>();

        /// <summary>
        /// آمار بر اساس اولویت
        /// </summary>
        public List<PriorityNotificationStat> NotificationsByPriority { get; set; } = new List<PriorityNotificationStat>();

        /// <summary>
        /// آمار بر اساس نوع نوتیفیکیشن
        /// </summary>
        public List<TypeNotificationStat> NotificationsByType { get; set; } = new List<TypeNotificationStat>();

        /// <summary>
        /// فعالیت روزانه
        /// </summary>
        public List<DailyNotificationActivity> DailyActivity { get; set; } = new List<DailyNotificationActivity>();

        /// <summary>
        /// آمار بر اساس سیستم (Backward compatibility)
        /// </summary>
        public List<SystemNotificationStat> SystemStats => NotificationsBySystem;

        /// <summary>
        /// آمار بر اساس اولویت (Backward compatibility)
        /// </summary>
        public List<PriorityNotificationStat> PriorityStats => NotificationsByPriority;
    }

    /// <summary>
    /// آمار نوتیفیکیشن بر اساس سیستم
    /// </summary>
    public class SystemNotificationStat
    {
        public byte SystemId { get; set; }
        public string SystemName { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// آمار نوتیفیکیشن بر اساس اولویت
    /// </summary>
    public class PriorityNotificationStat
    {
        public byte Priority { get; set; }
        public string PriorityName { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// آمار نوتیفیکشن بر اساس نوع
    /// </summary>
    public class TypeNotificationStat
    {
        public byte NotificationType { get; set; }
        public string TypeName { get; set; }
        public int Count { get; set; }
        public int ReadCount { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastNotificationDate { get; set; }
    }

    /// <summary>
    /// فعالیت روزانه نوتیفیکیشن‌ها
    /// </summary>
    public class DailyNotificationActivity
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// ویو مدل آمار کلی سیستم نوتیفیکیشن
    /// </summary>
    public class CoreNotificationSystemStatsViewModel
    {
        /// <summary>
        /// تعداد کل نوتیفیکیشن‌های سیستم
        /// </summary>
        public int TotalSystemNotifications { get; set; }

        /// <summary>
        /// تعداد کاربران دریافت‌کننده
        /// </summary>
        public int TotalRecipients { get; set; }

        /// <summary>
        /// تعداد نوتیفیکیشن‌های ارسال شده امروز
        /// </summary>
        public int TodayNotifications { get; set; }

        /// <summary>
        /// میانگین نوتیفیکیشن روزانه
        /// </summary>
        public double DailyAverage { get; set; }

        /// <summary>
        /// آمار ارسال موفق
        /// </summary>
        public int SuccessfulDeliveries { get; set; }

        /// <summary>
        /// آمار ارسال ناموفق
        /// </summary>
        public int FailedDeliveries { get; set; }

        /// <summary>
        /// درصد موفقیت ارسال
        /// </summary>
        public double DeliverySuccessRate => (SuccessfulDeliveries + FailedDeliveries) > 0 ? 
            (double)SuccessfulDeliveries / (SuccessfulDeliveries + FailedDeliveries) * 100 : 0;

        /// <summary>
        /// آمار بر اساس روش ارسال
        /// </summary>
        public List<DeliveryMethodStat> DeliveryMethodStats { get; set; } = new List<DeliveryMethodStat>();
    }

    /// <summary>
    /// آمار بر اساس روش ارسال
    /// </summary>
    public class DeliveryMethodStat
    {
        public byte DeliveryMethod { get; set; }
        public string DeliveryMethodName { get; set; }
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public double SuccessRate => TotalSent > 0 ? (double)SuccessCount / TotalSent * 100 : 0;
    }
}