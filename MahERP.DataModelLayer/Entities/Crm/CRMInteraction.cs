using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts; // ⭐ اضافه شده
using MahERP.DataModelLayer.Entities.Core;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// تعاملات CRM (تماس‌ها، جلسات، ایمیل‌ها و...) - اطلاعات مربوط به ارتباطات با مشتریان
    /// </summary>
    public class CRMInteraction
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد یکتای تعامل CRM - شناسه قابل نمایش برای کاربر
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CRMCode { get; set; }

        /// <summary>
        /// عنوان تعامل - موضوع تماس یا جلسه
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تعامل - شرح کامل تعامل با مشتری
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// نوع تعامل CRM
        /// 0- تماس تلفنی
        /// 1- جلسه حضوری
        /// 2- ایمیل
        /// 3- پیامک
        /// 4- سایر
        /// </summary>
        public byte CRMType { get; set; }

        /// <summary>
        /// جهت تماس
        /// 0- ورودی (مشتری تماس گرفته)
        /// 1- خروجی (کارمند تماس گرفته)
        /// </summary>
        public byte Direction { get; set; }

        /// <summary>
        /// نتیجه تعامل
        /// 0- بی نتیجه
        /// 1- موفق
        /// 2- نیاز به پیگیری
        /// </summary>
        public byte Result { get; set; }

        /// <summary>
        /// مدت زمان تماس (به دقیقه) - مدت زمان مکالمه یا جلسه
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// شماره تماس - شماره تلفنی که مکالمه با آن انجام شده
        /// </summary>
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// آدرس ایمیل (برای تماس‌های ایمیلی)
        /// </summary>
        [MaxLength(100)]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// محل جلسه (برای جلسات حضوری)
        /// </summary>
        [MaxLength(200)]
        public string? MeetingLocation { get; set; }

        /// <summary>
        /// زمان شروع جلسه یا تماس
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// زمان پایان جلسه یا تماس
        /// </summary>
        public DateTime? EndTime { get; set; }

        // ========== ⭐⭐⭐ NEW: Contact & Organization (جایگزین Stakeholder) ==========
        
        /// <summary>
        /// فرد مرتبط با تعامل (سیستم جدید)
        /// </summary>
        public int? ContactId { get; set; }
        
        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// سازمان مرتبط با تعامل (سیستم جدید)
        /// </summary>
        public int? OrganizationId { get; set; }
        
        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        // ========== ❌ DEPRECATED: Stakeholder (سیستم قدیمی - نگه‌داری برای backward compatibility) ==========
        
        /// <summary>
        /// طرف حساب مرتبط با تعامل (DEPRECATED - از Contact/Organization استفاده کنید)
        /// </summary>
        [Obsolete("Use ContactId or OrganizationId instead")]
        public int? StakeholderId { get; set; }
        
        [ForeignKey(nameof(StakeholderId))]
        [Obsolete("Use Contact or Organization instead")]
        public virtual Stakeholder? Stakeholder { get; set; }

        /// <summary>
        /// شخص مرتبط با طرف حساب (DEPRECATED - از Contact استفاده کنید)
        /// </summary>
        [Obsolete("Use ContactId instead")]
        public int? StakeholderContactId { get; set; }
        
        [ForeignKey(nameof(StakeholderContactId))]
        [Obsolete("Use Contact instead")]
        public virtual StakeholderContact? StakeholderContact { get; set; }

        // ========== اطلاعات اصلی ==========

        /// <summary>
        /// شعبه مربوط به تعامل
        /// </summary>
        [Required(ErrorMessage = "شعبه الزامی است")]
        public int BranchId { get; set; }
        
        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// قرارداد مرتبط با تعامل (در صورت وجود)
        /// </summary>
        public int? ContractId { get; set; }
        
        [ForeignKey(nameof(ContractId))]
        public virtual Contract? Contract { get; set; }

        // ========== اطلاعات زمانی ==========
        
        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر ایجاد کننده
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers Creator { get; set; }

        // ========== اطلاعات پیگیری ==========
        
        /// <summary>
        /// تاریخ پیگیری بعدی
        /// </summary>
        public DateTime? NextFollowUpDate { get; set; }
        
        /// <summary>
        /// یادداشت پیگیری بعدی
        /// </summary>
        [MaxLength(1000)]
        public string? NextFollowUpNote { get; set; }
        
        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }
        
        /// <summary>
        /// کاربر آخرین بروزرسانی کننده
        /// </summary>
        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }
        
        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// آیا رکورد حذف شده است (حذف منطقی)
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // ========== Navigation Properties ==========
        
        /// <summary>
        /// پیوست‌های مربوط به تعامل CRM
        /// </summary>
        public virtual ICollection<CRMAttachment> CRMAttachments { get; set; } = new HashSet<CRMAttachment>();
        
        /// <summary>
        /// شرکت‌کنندگان در تعامل
        /// </summary>
        public virtual ICollection<CRMParticipant> CRMParticipants { get; set; } = new HashSet<CRMParticipant>();
        
        /// <summary>
        /// کامنت‌های تعامل
        /// </summary>
        public virtual ICollection<CRMComment> CRMComments { get; set; } = new HashSet<CRMComment>();
        
        /// <summary>
        /// ارتباط با فعالیت‌های مرکزی
        /// </summary>
        public virtual ICollection<ActivityCRM> ActivityCRMs { get; set; } = new HashSet<ActivityCRM>();
        
        /// <summary>
        /// تیم‌های مرتبط با تعامل
        /// </summary>
        public virtual ICollection<CRMTeam> CRMTeams { get; set; } = new HashSet<CRMTeam>();

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string DisplayContactName => Contact?.FullName ?? "بدون فرد";

        [NotMapped]
        public string DisplayOrganizationName => Organization?.DisplayName ?? "بدون سازمان";

        [NotMapped]
        public string CRMTypeText => CRMType switch
        {
            0 => "تماس تلفنی",
            1 => "جلسه حضوری",
            2 => "ایمیل",
            3 => "پیامک",
            4 => "سایر",
            _ => "نامشخص"
        };

        [NotMapped]
        public string DirectionText => Direction switch
        {
            0 => "ورودی",
            1 => "خروجی",
            _ => "نامشخص"
        };

        [NotMapped]
        public string ResultText => Result switch
        {
            0 => "بی نتیجه",
            1 => "موفق",
            2 => "نیاز به پیگیری",
            _ => "نامشخص"
        };
    }
}
