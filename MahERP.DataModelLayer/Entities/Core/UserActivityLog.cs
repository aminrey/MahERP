using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// لاگ فعالیت‌های کاربران در سیستم
    /// این کلاس تمام عملیات کاربران را ثبت می‌کند
    /// </summary>
    public class UserActivityLog
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// شناسه کاربر انجام دهنده عملیات
        /// </summary>
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// 0: مشاهده
        /// 1: ایجاد
        /// 2: ویرایش
        /// 3: حذف
        /// 4: تایید
        /// 5: رد
        /// 6: ورود به سیستم
        /// 7: خروج از سیستم
        /// 8: دانلود فایل
        /// 9: آپلود فایل
        /// 10: جستجو
        /// 11: چاپ
        /// 12: ارسال ایمیل
        /// 13: ارسال پیامک
        /// </summary>
        public byte ActivityType { get; set; }

        /// <summary>
        /// ماژول سیستم (Controller)
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// عمل انجام شده (Action)
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// شناسه رکورد تحت تاثیر (در صورت وجود)
        /// </summary>
        public string? RecordId { get; set; }

        /// <summary>
        /// نوع انتیتی تحت تاثیر
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// عنوان رکورد تحت تاثیر (برای نمایش بهتر)
        /// </summary>
        public string? RecordTitle { get; set; }

        /// <summary>
        /// شرح فعالیت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// مقادیر قبل از تغییر (JSON format)
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// مقادیر جدید (JSON format)
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// پارامترهای ارسالی (JSON format)
        /// </summary>
        public string? RequestParameters { get; set; }

        /// <summary>
        /// نتیجه عملیات
        /// 0: موفق
        /// 1: ناموفق
        /// 2: خطا
        /// 3: دسترسی رد شده
        /// </summary>
        public byte ResultStatus { get; set; }

        /// <summary>
        /// پیام خطا (در صورت وجود)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// زمان انجام فعالیت
        /// </summary>
        public DateTime ActivityDateTime { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent مرورگر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// URL درخواست
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// نوع درخواست HTTP (GET, POST, PUT, DELETE)
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// زمان پردازش درخواست (میلی‌ثانیه)
        /// </summary>
        public long? ProcessingTimeMs { get; set; }

        /// <summary>
        /// اندازه پاسخ (بایت)
        /// </summary>
        public long? ResponseSize { get; set; }

        /// <summary>
        /// شناسه جلسه کاربر
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// شناسه شعبه کاربر (در زمان انجام عملیات)
        /// </summary>
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// آیا این فعالیت حساس است؟
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// سطح اهمیت لاگ
        /// 0: عادی
        /// 1: مهم
        /// 2: بحرانی
        /// </summary>
        public byte ImportanceLevel { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// 0: Desktop
        /// 1: Mobile
        /// 2: Tablet
        /// </summary>
        public byte? DeviceType { get; set; }

        /// <summary>
        /// نام دستگاه/مرورگر
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی (در صورت وجود)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// تگ‌های اضافی برای دسته‌بندی
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// شناسه فعالیت مرتبط (برای group کردن فعالیت‌های مرتبط)
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// آیا این لاگ آرشیو شده است؟
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// تاریخ آرشیو
        /// </summary>
        public DateTime? ArchivedDate { get; set; }
    }
}