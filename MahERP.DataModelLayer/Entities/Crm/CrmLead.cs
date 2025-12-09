using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// سرنخ CRM - اتصال Contact/Organization به شعبه با اطلاعات CRM
    /// </summary>
    [Table("CrmLead_Tbl")]
    public class CrmLead
    {
        public CrmLead()
        {
            Interactions = new HashSet<CrmLeadInteraction>();
            FollowUps = new HashSet<CrmFollowUp>();
        }

        [Key]
        public int Id { get; set; }

        // ========== اتصال به Core (یکی از این دو باید مقدار داشته باشد) ==========

        /// <summary>
        /// شناسه Contact (اگر سرنخ یک فرد است)
        /// </summary>
        public int? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// شناسه Organization (اگر سرنخ یک سازمان است)
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        // ========== شعبه و کاربر مسئول ==========

        /// <summary>
        /// شعبه مالک سرنخ
        /// </summary>
        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// کاربر مسئول پیگیری
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string AssignedUserId { get; set; }

        [ForeignKey(nameof(AssignedUserId))]
        public virtual AppUsers AssignedUser { get; set; }

        // ========== وضعیت ==========

        /// <summary>
        /// وضعیت سرنخ (از جدول پویا)
        /// </summary>
        [Required]
        public int StatusId { get; set; }

        [ForeignKey(nameof(StatusId))]
        public virtual CrmLeadStatus Status { get; set; }

        // ========== اطلاعات تکمیلی ==========

        /// <summary>
        /// منبع سرنخ
        /// </summary>
        [MaxLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// امتیاز سرنخ (0-100)
        /// </summary>
        [Range(0, 100)]
        public int Score { get; set; } = 0;

        /// <summary>
        /// یادداشت‌های کلی
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// برچسب‌ها (جدا شده با کاما)
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// تاریخ آخرین تماس
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// تاریخ پیگیری بعدی
        /// </summary>
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>
        /// ارزش تخمینی سرنخ (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
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
        /// تعاملات این سرنخ
        /// </summary>
        [InverseProperty(nameof(CrmLeadInteraction.Lead))]
        public virtual ICollection<CrmLeadInteraction> Interactions { get; set; }

        /// <summary>
        /// پیگیری‌های این سرنخ
        /// </summary>
        [InverseProperty(nameof(CrmFollowUp.Lead))]
        public virtual ICollection<CrmFollowUp> FollowUps { get; set; }

        // ========== Computed Properties ==========

        /// <summary>
        /// نوع سرنخ: Contact یا Organization
        /// </summary>
        [NotMapped]
        public string LeadType => ContactId.HasValue ? "Contact" : "Organization";

        /// <summary>
        /// آیا سرنخ فرد است؟
        /// </summary>
        [NotMapped]
        public bool IsContact => ContactId.HasValue;

        /// <summary>
        /// آیا سرنخ سازمان است؟
        /// </summary>
        [NotMapped]
        public bool IsOrganization => OrganizationId.HasValue;

        /// <summary>
        /// نام نمایشی سرنخ
        /// </summary>
        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (Contact != null)
                    return Contact.FullName;
                if (Organization != null)
                    return Organization.DisplayName;
                return $"سرنخ #{Id}";
            }
        }

        /// <summary>
        /// شماره تلفن اصلی
        /// </summary>
        [NotMapped]
        public string? PrimaryPhone
        {
            get
            {
                if (Contact != null)
                    return Contact.DefaultPhone?.PhoneNumber;
                if (Organization != null)
                    return Organization.PrimaryPhone;
                return null;
            }
        }

        /// <summary>
        /// ایمیل اصلی
        /// </summary>
        [NotMapped]
        public string? PrimaryEmail
        {
            get
            {
                if (Contact != null)
                    return Contact.PrimaryEmail;
                if (Organization != null)
                    return Organization.Email;
                return null;
            }
        }

        /// <summary>
        /// تعداد تعاملات
        /// </summary>
        [NotMapped]
        public int InteractionsCount => Interactions?.Count ?? 0;

        /// <summary>
        /// تعداد پیگیری‌های در انتظار
        /// </summary>
        [NotMapped]
        public int PendingFollowUpsCount => FollowUps?.Count(f => f.Status == 0) ?? 0;

        /// <summary>
        /// لیست برچسب‌ها
        /// </summary>
        [NotMapped]
        public List<string> TagsList => string.IsNullOrEmpty(Tags) 
            ? new List<string>() 
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

        /// <summary>
        /// آیا نیاز به پیگیری دارد؟
        /// </summary>
        [NotMapped]
        public bool NeedsFollowUp => NextFollowUpDate.HasValue && NextFollowUpDate.Value <= DateTime.Now;

        /// <summary>
        /// روزهای از آخرین تماس
        /// </summary>
        [NotMapped]
        public int? DaysSinceLastContact => LastContactDate.HasValue 
            ? (int)(DateTime.Now - LastContactDate.Value).TotalDays 
            : null;
    }
}
