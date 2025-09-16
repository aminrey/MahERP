using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// انتیتی جزئیات نوتیفیکیشن‌های سیستم‌های مختلف ERP
    /// برای ذخیره اطلاعات تخصصی هر سیستم
    /// </summary>
    public class CoreNotificationDetail
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه نوتیفیکیشن اصلی
        /// </summary>
        [Required]
        public int CoreNotificationId { get; set; }
        [ForeignKey("CoreNotificationId")]
        public virtual CoreNotification CoreNotification { get; set; }

        /// <summary>
        /// نوع تخصصی نوتیفیکیشن بر اساس سیستم
        /// برای سیستم تسک‌ها: 0=تسک جدید، 1=تغییر تسک، 2=تکمیل تسک و...
        /// برای سیستم مالی: 0=فاکتور جدید، 1=پرداخت، 2=سررسید و...
        /// برای سیستم CRM: 0=مشتری جدید، 1=قرارملاقات، 2=پیگیری و...
        /// </summary>
        [Required]
        public byte NotificationTypeSpecific { get; set; }

        /// <summary>
        /// کلید فیلد تغییر یافته (برای نوتیفیکیشن‌های تغییر)
        /// </summary>
        [MaxLength(100)]
        public string? FieldName { get; set; }

        /// <summary>
        /// مقدار قبلی (قبل از تغییر)
        /// </summary>
        [MaxLength(1000)]
        public string? OldValue { get; set; }

        /// <summary>
        /// مقدار جدید (بعد از تغییر)
        /// </summary>
        [MaxLength(1000)]
        public string? NewValue { get; set; }

        /// <summary>
        /// اطلاعات اضافی به صورت JSON
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// توضیحات تکمیلی
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// تاریخ ایجاد جزئیات
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// آیا این جزئیات فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}