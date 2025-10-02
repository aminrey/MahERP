using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// انتیتی اصلی اطلاع‌رسانی کلی سامانه ERP - سطح 1 (کلی)
    /// برای تمام سیستم‌های اصلی ERP قابل استفاده
    /// </summary>
    public class CoreNotification
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه سیستم اصلی ERP
        /// 1 = سیستم مدیریت مالی
        /// 2 = سیستم منابع انسانی
        /// 3 = سیستم فروش و CRM
        /// 4 = سیستم خرید و تدارکات
        /// 5 = سیستم انبار و لجستیک
        /// 6 = سیستم تولید و کنترل کیفیت
        /// 7 = سیستم مدیریت پروژه و تسک‌ها
        /// </summary>
        [Required]
        public byte SystemId { get; set; }

        /// <summary>
        /// نام سیستم (برای نمایش)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SystemName { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده نوتیفیکیشن
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string RecipientUserId { get; set; }
        [ForeignKey("RecipientUserId")]
        public virtual AppUsers Recipient { get; set; }

        /// <summary>
        /// شناسه کاربری که باعث ایجاد این نوتیفیکیشن شده
        /// </summary>
        [MaxLength(450)]
        public string? SenderUserId { get; set; }
        [ForeignKey("SenderUserId")]
        public virtual AppUsers? Sender { get; set; }

        /// <summary>
        /// نوع کلی نوتیفیکیشن
        /// 0 = اطلاع‌رسانی عمومی
        /// 1 = ایجاد رکورد جدید
        /// 2 = ویرایش رکورد
        /// 3 = حذف رکورد
        /// 4 = تایید/رد
        /// 5 = هشدار
        /// 6 = یادآوری
        /// 7 = خطا/مشکل
        /// 8 = تکمیل فرآیند
        /// 9 = اختصاص/انتساب
        /// 10 = تغییر وضعیت
        /// </summary>
        [Required]
        public byte NotificationTypeGeneral { get; set; }

        /// <summary>
        /// عنوان نوتیفیکیشن
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// متن پیام نوتیفیکیشن
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// تاریخ ایجاد نوتیفیکیشن
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// آیا نوتیفیکیشن خوانده شده؟
        /// </summary>
        [Required]
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// تاریخ خواندن نوتیفیکیشن
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// آیا کاربر روی نوتیفیکیشن کلیک کرده؟
        /// </summary>
        public bool IsClicked { get; set; } = false;

        /// <summary>
        /// تاریخ کلیک روی نوتیفیکیشن
        /// </summary>
        public DateTime? ClickDate { get; set; }

        /// <summary>
        /// اولویت نوتیفیکیشن
        /// 0 = عادی، 1 = مهم، 2 = فوری، 3 = بحرانی
        /// </summary>
        public byte Priority { get; set; } = 0;

        /// <summary>
        /// URL عمل برای هدایت کاربر به رکورد مربوطه
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// شناسه رکورد مرتبط در سیستم مربوطه (مثل TaskId، InvoiceId و...)
        /// </summary>
        [MaxLength(50)]
        public string? RelatedRecordId { get; set; }

        /// <summary>
        /// نوع رکورد مرتبط (مثل Task، Invoice، Contract و...)
        /// </summary>
        [MaxLength(100)]
        public string? RelatedRecordType { get; set; }

        /// <summary>
        /// عنوان رکورد مرتبط (برای نمایش)
        /// </summary>
        [MaxLength(200)]
        public string? RelatedRecordTitle { get; set; }

        /// <summary>
        /// آیا این نوتیفیکیشن فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// شناسه شعبه (در صورت محدودیت شعبه‌ای)
        /// </summary>
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        /// <summary>
        /// Navigation Property به جزئیات سیستم‌های مختلف
        /// </summary>
        public virtual ICollection<CoreNotificationDetail> Details { get; set; }
        public virtual ICollection<CoreNotificationDelivery> Deliveries { get; set; }
    }
}