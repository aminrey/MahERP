using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// سازمان‌ها - اشخاص حقوقی
    /// </summary>
    [Table("Organization_Tbl")]
    [Index(nameof(RegistrationNumber), IsUnique = true, Name = "IX_Organization_RegistrationNumber")]
    [Index(nameof(EconomicCode), IsUnique = true, Name = "IX_Organization_EconomicCode")]
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نام سازمان
        /// </summary>
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// نام برند/تجاری
        /// </summary>
        [MaxLength(100)]
        public string? Brand { get; set; }

        /// <summary>
        /// شماره ثبت شرکت (یکتا)
        /// </summary>
        [MaxLength(11)]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// کد اقتصادی (یکتا)
        /// </summary>
        [MaxLength(12)]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        public string? EconomicCode { get; set; }

        /// <summary>
        /// تاریخ ثبت
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// وب‌سایت
        /// </summary>
        [MaxLength(200)]
        [Url(ErrorMessage = "فرمت وب‌سایت نامعتبر است")]
        public string? Website { get; set; }

        /// <summary>
        /// نماینده قانونی/مدیرعامل
        /// </summary>
        [MaxLength(200)]
        public string? LegalRepresentative { get; set; }

        /// <summary>
        /// مسیر لوگو
        /// </summary>
        [MaxLength(500)]
        public string? LogoPath { get; set; }

        /// <summary>
        /// تلفن اصلی
        /// </summary>
        [MaxLength(15)]
        [Phone]
        public string? PrimaryPhone { get; set; }

        /// <summary>
        /// تلفن دوم
        /// </summary>
        [MaxLength(15)]
        [Phone]
        public string? SecondaryPhone { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// کد پستی
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// نوع سازمان
        /// 0 = شرکت
        /// 1 = سازمان
        /// 2 = موسسه
        /// 3 = نهاد
        /// </summary>
        public byte OrganizationType { get; set; } = 0;

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== اطلاعات سیستمی ==========
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
        /// بخش‌های سازمانی
        /// </summary>
        [InverseProperty(nameof(OrganizationDepartment.Organization))]
        public virtual ICollection<OrganizationDepartment> Departments { get; set; } = new HashSet<OrganizationDepartment>();

        /// <summary>
        /// افراد مرتبط با سازمان
        /// </summary>
        [InverseProperty(nameof(OrganizationContact.Organization))]
        public virtual ICollection<OrganizationContact> Contacts { get; set; } = new HashSet<OrganizationContact>();

        /// <summary>
        /// شماره‌های تماس سازمان
        /// </summary>
        public virtual ICollection<OrganizationPhone> Phones { get; set; } = new HashSet<OrganizationPhone>();

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(Brand) ? Brand : Name;

        [NotMapped]
        public string OrganizationTypeText => OrganizationType switch
        {
            0 => "شرکت",
            1 => "سازمان",
            2 => "موسسه",
            3 => "نهاد",
            _ => "نامشخص"
        };

        [NotMapped]
        public IEnumerable<OrganizationDepartment> RootDepartments => 
            Departments?.Where(d => d.ParentDepartmentId == null && d.IsActive).OrderBy(d => d.DisplayOrder) 
            ?? Enumerable.Empty<OrganizationDepartment>();

        /// <summary>
        /// شماره تماس پیش‌فرض
        /// </summary>
        [NotMapped]
        public OrganizationPhone? DefaultPhone => Phones?.FirstOrDefault(p => p.IsDefault && p.IsActive);

        /// <summary>
        /// تمام شماره‌های فعال به ترتیب نمایش
        /// </summary>
        [NotMapped]
        public IEnumerable<OrganizationPhone> ActivePhones => 
            Phones?.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder) ?? Enumerable.Empty<OrganizationPhone>();
    }
}