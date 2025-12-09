using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// تعامل سرنخ CRM - ثبت مکالمات و رویدادها با سرنخ‌ها
    /// </summary>
    [Table("CrmLeadInteraction_Tbl")]
    public class CrmLeadInteraction
    {
        public CrmLeadInteraction()
        {
            FollowUps = new HashSet<CrmFollowUp>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه سرنخ
        /// </summary>
        [Required]
        public int LeadId { get; set; }

        [ForeignKey(nameof(LeadId))]
        public virtual CrmLead Lead { get; set; }

        // ========== نوع تعامل ==========

        /// <summary>
        /// نوع تعامل
        /// 0 = تماس تلفنی
        /// 1 = جلسه حضوری
        /// 2 = جلسه آنلاین
        /// 3 = ایمیل
        /// 4 = پیامک
        /// 5 = یادداشت
        /// </summary>
        [Required]
        public byte InteractionType { get; set; }

        /// <summary>
        /// جهت تعامل (برای تماس/ایمیل/پیامک)
        /// 0 = ورودی
        /// 1 = خروجی
        /// </summary>
        public byte? Direction { get; set; }

        // ========== جزئیات تعامل ==========

        /// <summary>
        /// موضوع
        /// </summary>
        [MaxLength(300)]
        public string? Subject { get; set; }

        /// <summary>
        /// شرح مکالمه / متن تعامل
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// نتیجه تعامل
        /// 0 = موفق
        /// 1 = ناموفق
        /// 2 = بدون پاسخ
        /// 3 = در انتظار
        /// 4 = نیاز به پیگیری
        /// </summary>
        public byte? Result { get; set; }

        /// <summary>
        /// مدت زمان (دقیقه) - برای تماس و جلسه
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// شماره تلفن تماس گرفته شده
        /// </summary>
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل ارسال شده به
        /// </summary>
        [MaxLength(200)]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// تاریخ و ساعت تعامل
        /// </summary>
        [Required]
        public DateTime InteractionDate { get; set; } = DateTime.Now;

        // ========== اتصال به تسک (اختیاری) ==========

        /// <summary>
        /// شناسه تسک مرتبط (از ماژول Tasking)
        /// </summary>
        public int? RelatedTaskId { get; set; }

        [ForeignKey(nameof(RelatedTaskId))]
        public virtual Tasks? RelatedTask { get; set; }

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

        // ========== Navigation Properties ==========

        /// <summary>
        /// پیگیری‌های مرتبط با این تعامل
        /// </summary>
        [InverseProperty(nameof(CrmFollowUp.Interaction))]
        public virtual ICollection<CrmFollowUp> FollowUps { get; set; }

        // ========== Computed Properties ==========

        /// <summary>
        /// متن نوع تعامل
        /// </summary>
        [NotMapped]
        public string InteractionTypeText => InteractionType switch
        {
            0 => "تماس تلفنی",
            1 => "جلسه حضوری",
            2 => "جلسه آنلاین",
            3 => "ایمیل",
            4 => "پیامک",
            5 => "یادداشت",
            _ => "نامشخص"
        };

        /// <summary>
        /// آیکون نوع تعامل
        /// </summary>
        [NotMapped]
        public string InteractionTypeIcon => InteractionType switch
        {
            0 => "fa-phone",
            1 => "fa-users",
            2 => "fa-video",
            3 => "fa-envelope",
            4 => "fa-comment-sms",
            5 => "fa-sticky-note",
            _ => "fa-circle"
        };

        /// <summary>
        /// رنگ نوع تعامل
        /// </summary>
        [NotMapped]
        public string InteractionTypeColor => InteractionType switch
        {
            0 => "primary",
            1 => "success",
            2 => "info",
            3 => "warning",
            4 => "secondary",
            5 => "dark",
            _ => "secondary"
        };

        /// <summary>
        /// متن نتیجه تعامل
        /// </summary>
        [NotMapped]
        public string? ResultText => Result switch
        {
            0 => "موفق",
            1 => "ناموفق",
            2 => "بدون پاسخ",
            3 => "در انتظار",
            4 => "نیاز به پیگیری",
            _ => null
        };

        /// <summary>
        /// متن جهت تعامل
        /// </summary>
        [NotMapped]
        public string? DirectionText => Direction switch
        {
            0 => "ورودی",
            1 => "خروجی",
            _ => null
        };

        /// <summary>
        /// آیکون جهت تعامل
        /// </summary>
        [NotMapped]
        public string? DirectionIcon => Direction switch
        {
            0 => "fa-arrow-down",
            1 => "fa-arrow-up",
            _ => null
        };

        /// <summary>
        /// مدت زمان به صورت متن
        /// </summary>
        [NotMapped]
        public string? DurationText
        {
            get
            {
                if (!DurationMinutes.HasValue) return null;
                
                var hours = DurationMinutes.Value / 60;
                var minutes = DurationMinutes.Value % 60;
                
                if (hours > 0 && minutes > 0)
                    return $"{hours} ساعت و {minutes} دقیقه";
                if (hours > 0)
                    return $"{hours} ساعت";
                return $"{minutes} دقیقه";
            }
        }

        /// <summary>
        /// آیا تسک مرتبط دارد؟
        /// </summary>
        [NotMapped]
        public bool HasRelatedTask => RelatedTaskId.HasValue;

        /// <summary>
        /// تعداد پیگیری‌های مرتبط
        /// </summary>
        [NotMapped]
        public int FollowUpsCount => FollowUps?.Count ?? 0;
    }
}
