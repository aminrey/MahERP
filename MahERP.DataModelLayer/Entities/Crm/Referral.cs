using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// توصیه/ارجاع - ثبت معرفی افراد توسط مشتریان
    /// شرط: توصیه‌کننده (Referrer) باید مشتری باشد (ContactType = Customer)
    /// قابل توسعه برای بازاریاب در آینده
    /// </summary>
    [Table("Referral_Tbl")]
    public class Referral
    {
        [Key]
        public int Id { get; set; }

        // ========== طرفین توصیه ==========

        /// <summary>
        /// مشتری توصیه‌کننده (باید ContactType = Customer باشد)
        /// </summary>
        [Required(ErrorMessage = "توصیه‌کننده الزامی است")]
        public int ReferrerContactId { get; set; }

        [ForeignKey(nameof(ReferrerContactId))]
        public virtual Contact ReferrerContact { get; set; } = null!;

        /// <summary>
        /// فرد معرفی‌شده (معمولاً Lead جدید)
        /// </summary>
        [Required(ErrorMessage = "معرفی‌شده الزامی است")]
        public int ReferredContactId { get; set; }

        [ForeignKey(nameof(ReferredContactId))]
        public virtual Contact ReferredContact { get; set; } = null!;

        // ========== تعاملات مرتبط ==========

        /// <summary>
        /// تعامل با توصیه‌کننده که در آن معرفی انجام شد (اختیاری)
        /// سناریو: در تعامل با مشتری، او کسی را معرفی می‌کند
        /// </summary>
        public int? ReferrerInteractionId { get; set; }

        [ForeignKey(nameof(ReferrerInteractionId))]
        public virtual Interaction? ReferrerInteraction { get; set; }

        /// <summary>
        /// اولین تعامل با فرد معرفی‌شده (اختیاری)
        /// سناریو: در اولین تماس با لید، متوجه می‌شویم معرفی شده
        /// </summary>
        public int? ReferredInteractionId { get; set; }

        [ForeignKey(nameof(ReferredInteractionId))]
        public virtual Interaction? ReferredInteraction { get; set; }

        // ========== اطلاعات تکمیلی ==========

        /// <summary>
        /// تاریخ ثبت توصیه
        /// </summary>
        [Required]
        public DateTime ReferralDate { get; set; } = DateTime.Now;

        /// <summary>
        /// یادداشت
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// وضعیت نتیجه توصیه
        /// </summary>
        public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

        /// <summary>
        /// تاریخ تغییر وضعیت به موفق/ناموفق
        /// </summary>
        public DateTime? StatusChangeDate { get; set; }

        // ========== فیلدهای آینده برای بازاریاب ==========
        // این فیلدها فعلاً استفاده نمی‌شوند اما برای توسعه آینده آماده هستند

        /// <summary>
        /// نوع ارجاع
        /// 0 = توصیه مشتری (پیش‌فرض)
        /// 1 = بازاریاب (آینده)
        /// </summary>
        public byte ReferralType { get; set; } = 0;

        /// <summary>
        /// شناسه بازاریاب (آینده - وقتی ReferralType = 1)
        /// </summary>
        [MaxLength(450)]
        public string? MarketerUserId { get; set; }

        // ========== Audit ==========
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }
    }
}
