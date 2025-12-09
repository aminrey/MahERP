using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// پیگیری CRM - یادآوری و برنامه‌ریزی تماس‌ها
    /// </summary>
    [Table("CrmFollowUp_Tbl")]
    public class CrmFollowUp
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه سرنخ
        /// </summary>
        [Required]
        public int LeadId { get; set; }

        [ForeignKey(nameof(LeadId))]
        public virtual CrmLead Lead { get; set; }

        /// <summary>
        /// شناسه تعامل مرتبط (اختیاری)
        /// </summary>
        public int? InteractionId { get; set; }

        [ForeignKey(nameof(InteractionId))]
        public virtual CrmLeadInteraction? Interaction { get; set; }

        // ========== جزئیات پیگیری ==========

        /// <summary>
        /// نوع پیگیری
        /// 0 = تماس
        /// 1 = جلسه
        /// 2 = ایمیل
        /// 3 = پیامک
        /// 4 = سایر
        /// </summary>
        [Required]
        public byte FollowUpType { get; set; }

        /// <summary>
        /// عنوان پیگیری
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// تاریخ و ساعت موعد
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// اولویت
        /// 0 = کم
        /// 1 = متوسط
        /// 2 = زیاد
        /// 3 = فوری
        /// </summary>
        [Required]
        public byte Priority { get; set; } = 1;

        // ========== وضعیت ==========

        /// <summary>
        /// وضعیت پیگیری
        /// 0 = در انتظار
        /// 1 = انجام شده
        /// 2 = لغو شده
        /// 3 = به تعویق افتاده
        /// </summary>
        [Required]
        public byte Status { get; set; } = 0;

        /// <summary>
        /// تاریخ انجام/لغو
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// نتیجه پیگیری (پس از انجام)
        /// </summary>
        [MaxLength(500)]
        public string? CompletionResult { get; set; }

        // ========== یادآوری ==========

        /// <summary>
        /// آیا یادآوری دارد؟
        /// </summary>
        public bool HasReminder { get; set; } = true;

        /// <summary>
        /// تاریخ و ساعت یادآوری
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// آیا یادآوری ارسال شده؟
        /// </summary>
        public bool ReminderSent { get; set; } = false;

        /// <summary>
        /// دقایق قبل از موعد برای یادآوری
        /// </summary>
        public int ReminderMinutesBefore { get; set; } = 30;

        /// <summary>
        /// ارسال یادآور ایمیلی
        /// </summary>
        public bool SendEmailReminder { get; set; } = false;

        /// <summary>
        /// ارسال یادآور پیامکی
        /// </summary>
        public bool SendSmsReminder { get; set; } = false;

        // ========== کاربر مسئول ==========

        /// <summary>
        /// کاربر مسئول انجام پیگیری
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AssignedUserId { get; set; }

        [ForeignKey(nameof(AssignedUserId))]
        public virtual AppUsers AssignedUser { get; set; }

        // ========== اتصال به تسک (اختیاری) ==========

        /// <summary>
        /// شناسه تسک (اگر به تسک تبدیل شده باشد)
        /// </summary>
        public int? TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        public virtual Tasks? Task { get; set; }

        // ========== فعال/غیرفعال ==========

        /// <summary>
        /// فعال بودن رکورد
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== Audit Fields ==========

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Computed Properties ==========

        /// <summary>
        /// متن نوع پیگیری
        /// </summary>
        [NotMapped]
        public string FollowUpTypeText => FollowUpType switch
        {
            0 => "تماس",
            1 => "جلسه",
            2 => "ایمیل",
            3 => "پیامک",
            4 => "سایر",
            _ => "نامشخص"
        };

        /// <summary>
        /// آیکون نوع پیگیری
        /// </summary>
        [NotMapped]
        public string FollowUpTypeIcon => FollowUpType switch
        {
            0 => "fa-phone",
            1 => "fa-calendar-check",
            2 => "fa-envelope",
            3 => "fa-comment-sms",
            4 => "fa-tasks",
            _ => "fa-circle"
        };

        /// <summary>
        /// متن اولویت
        /// </summary>
        [NotMapped]
        public string PriorityText => Priority switch
        {
            0 => "کم",
            1 => "متوسط",
            2 => "زیاد",
            3 => "فوری",
            _ => "نامشخص"
        };

        /// <summary>
        /// رنگ اولویت
        /// </summary>
        [NotMapped]
        public string PriorityColor => Priority switch
        {
            0 => "secondary",
            1 => "info",
            2 => "warning",
            3 => "danger",
            _ => "secondary"
        };

        /// <summary>
        /// متن وضعیت
        /// </summary>
        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "در انتظار",
            1 => "انجام شده",
            2 => "لغو شده",
            3 => "به تعویق افتاده",
            _ => "نامشخص"
        };

        /// <summary>
        /// رنگ وضعیت
        /// </summary>
        [NotMapped]
        public string StatusColor => Status switch
        {
            0 => "warning",
            1 => "success",
            2 => "danger",
            3 => "info",
            _ => "secondary"
        };

        /// <summary>
        /// آیکون وضعیت
        /// </summary>
        [NotMapped]
        public string StatusIcon => Status switch
        {
            0 => "fa-clock",
            1 => "fa-check-circle",
            2 => "fa-times-circle",
            3 => "fa-pause-circle",
            _ => "fa-circle"
        };

        /// <summary>
        /// آیا سررسید گذشته؟
        /// </summary>
        [NotMapped]
        public bool IsOverdue => Status == 0 && DueDate < DateTime.Now;

        /// <summary>
        /// آیا امروز سررسید است؟
        /// </summary>
        [NotMapped]
        public bool IsDueToday => Status == 0 && DueDate.Date == DateTime.Today;

        /// <summary>
        /// روزهای باقی‌مانده تا سررسید
        /// </summary>
        [NotMapped]
        public int DaysUntilDue => (int)(DueDate.Date - DateTime.Today).TotalDays;

        /// <summary>
        /// آیا تسک شده؟
        /// </summary>
        [NotMapped]
        public bool IsConvertedToTask => TaskId.HasValue;

        /// <summary>
        /// عنوان نمایشی
        /// </summary>
        [NotMapped]
        public string DisplayTitle => string.IsNullOrEmpty(Title) 
            ? $"{FollowUpTypeText} - {Lead?.DisplayName ?? "سرنخ"}" 
            : Title;
    }
}
