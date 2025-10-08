using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// طرف حساب - می‌تواند شخص حقیقی یا حقوقی باشد
    /// </summary>
    public class Stakeholder
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نوع طرف حساب
        /// 0- شخص حقیقی
        /// 1- شخص حقوقی (شرکت)
        /// </summary>
        [Required]
        public byte PersonType { get; set; }

        /// <summary>
        /// نوع طرف حساب از نظر نقش
        /// 0- مشتری
        /// 1- تامین کننده
        /// 2- همکار
        /// 3- سایر
        /// </summary>
        public byte StakeholderType { get; set; }

        // ========== فیلدهای مشترک ==========
        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Mobile { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // ========== فیلدهای شخص حقیقی ==========
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(10)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// جنسیت (برای شخص حقیقی)
        /// 0- مرد
        /// 1- زن
        /// </summary>
        public byte? Gender { get; set; }

        // ========== فیلدهای شخص حقوقی ==========
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? CompanyBrand { get; set; }

        [MaxLength(11)]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت شرکت باید 11 رقم باشد")]
        public string? RegistrationNumber { get; set; }

        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        public string? EconomicCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RegistrationDate { get; set; }

        [MaxLength(200)]
        public string? RegisteredAddress { get; set; }

        [MaxLength(200)]
        [Url]
        public string? Website { get; set; }

        /// <summary>
        /// نام مدیرعامل یا نماینده قانونی
        /// </summary>
        [MaxLength(200)]
        public string? LegalRepresentative { get; set; }

        // ========== فیلدهای سیستمی ==========
        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }
        
        [ForeignKey(nameof(CreatorUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }
        
        [ForeignKey(nameof(LastUpdaterUserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AppUsers? LastUpdater { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // ========== Navigation Properties ==========
        [InverseProperty(nameof(StakeholderBranch.Stakeholder))]
        public virtual ICollection<StakeholderBranch> StakeholderBranches { get; set; } = new HashSet<StakeholderBranch>();
        
        [InverseProperty(nameof(StakeholderContact.Stakeholder))]
        public virtual ICollection<StakeholderContact> StakeholderContacts { get; set; } = new HashSet<StakeholderContact>();
        
        [InverseProperty(nameof(StakeholderOrganization.Stakeholder))]
        public virtual ICollection<StakeholderOrganization> StakeholderOrganizations { get; set; } = new HashSet<StakeholderOrganization>();
        
        [InverseProperty(nameof(Contract.Stakeholder))]
        public virtual ICollection<Contract> Contracts { get; set; } = new HashSet<Contract>();
        
        [InverseProperty(nameof(Tasks.Stakeholder))]
        public virtual ICollection<Tasks> TaskList { get; set; } = new HashSet<Tasks>();

        // ========== Computed Properties ==========
        [NotMapped]
        public string DisplayName => PersonType == 0 
            ? $"{FirstName} {LastName}" 
            : CompanyName ?? "نامشخص";
            
        [NotMapped]
        public string PersonTypeText => PersonType switch
        {
            0 => "شخص حقیقی",
            1 => "شخص حقوقی",
            _ => "نامشخص"
        };

        [NotMapped]
        public string StakeholderTypeText => StakeholderType switch
        {
            0 => "مشتری",
            1 => "تامین‌کننده",
            2 => "همکار",
            3 => "سایر",
            _ => "نامشخص"
        };

        [NotMapped]
        public string GenderText => Gender switch
        {
            0 => "مرد",
            1 => "زن",
            _ => "-"
        };
    }
}
