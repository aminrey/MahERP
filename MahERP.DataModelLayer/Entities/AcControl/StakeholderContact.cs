using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// افراد مرتبط با طرف حساب
    /// برای شخص حقیقی: افراد مرتبط (خانواده، همکار و...)
    /// برای شخص حقوقی: کارمندان و نمایندگان شرکت
    /// </summary>
    public class StakeholderContact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StakeholderId { get; set; }
        
        [ForeignKey(nameof(StakeholderId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Stakeholder Stakeholder { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// عنوان شغلی یا نقش
        /// </summary>
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        /// <summary>
        /// دپارتمان یا بخش
        /// </summary>
        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Mobile { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(10)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        /// <summary>
        /// نوع رابطه با طرف حساب
        /// 0- کارمند
        /// 1- مدیر
        /// 2- نماینده
        /// 3- تصمیم‌گیرنده
        /// 4- تماس‌گیرنده
        /// 5- سایر
        /// </summary>
        public byte ContactType { get; set; }

        /// <summary>
        /// سطح اهمیت
        /// 0- پایین
        /// 1- متوسط
        /// 2- بالا
        /// 3- خیلی بالا
        /// </summary>
        public byte ImportanceLevel { get; set; }

        /// <summary>
        /// آیا تماس اصلی است؟
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// آیا تصمیم‌گیرنده است؟
        /// </summary>
        public bool IsDecisionMaker { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [InverseProperty(nameof(StakeholderOrganizationMember.Contact))]
        public virtual ICollection<StakeholderOrganizationMember> OrganizationMemberships { get; set; } = new HashSet<StakeholderOrganizationMember>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string ContactTypeText => ContactType switch
        {
            0 => "کارمند",
            1 => "مدیر",
            2 => "نماینده",
            3 => "تصمیم‌گیرنده",
            4 => "تماس‌گیرنده",
            5 => "سایر",
            _ => "نامشخص"
        };

        [NotMapped]
        public string ImportanceLevelText => ImportanceLevel switch
        {
            0 => "پایین",
            1 => "متوسط",
            2 => "بالا",
            3 => "خیلی بالا",
            _ => "نامشخص"
        };
    }
}
