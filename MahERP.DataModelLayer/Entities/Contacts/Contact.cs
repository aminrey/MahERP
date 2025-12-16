using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Contacts
{
    /// <summary>
    /// افراد - جدول مرکزی ثبت اطلاعات اشخاص حقیقی
    /// </summary>
    [Table("Contact_Tbl")]
    [Index(nameof(NationalCode), IsUnique = true, Name = "IX_Contact_NationalCode")]
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نوع Contact - تعیین وضعیت فرد در سیستم
        /// Lead = سرنخ (هنوز خرید نکرده)
        /// Customer = مشتری (خرید کرده)
        /// Partner = شریک تجاری
        /// Other = سایر
        /// </summary>
        [Display(Name = "نوع")]
        public ContactType ContactType { get; set; } = ContactType.Lead;

        /// <summary>
        /// نام
        /// </summary>
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// کد ملی (یکتا - اختیاری)
        /// </summary>
        [MaxLength(10)]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        /// <summary>
        /// ایمیل اصلی
        /// </summary>
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string? PrimaryEmail { get; set; }

        /// <summary>
        /// ایمیل دوم
        /// </summary>
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string? SecondaryEmail { get; set; }

        /// <summary>
        /// آدرس اصلی
        /// </summary>
        [MaxLength(500)]
        public string? PrimaryAddress { get; set; }

        /// <summary>
        /// آدرس دوم
        /// </summary>
        [MaxLength(500)]
        public string? SecondaryAddress { get; set; }

        /// <summary>
        /// کد پستی آدرس اصلی
        /// </summary>
        [MaxLength(20)]
        public string? PrimaryPostalCode { get; set; }

        /// <summary>
        /// کد پستی آدرس دوم
        /// </summary>
        [MaxLength(20)]
        public string? SecondaryPostalCode { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// جنسیت
        /// 0 = مرد
        /// 1 = زن
        /// 2 = سایر
        /// </summary>
        public byte? Gender { get; set; }

        /// <summary>
        /// مسیر تصویر پروفایل
        /// </summary>
        [MaxLength(500)]
        public string? ProfileImagePath { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

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
        /// شماره تلفن‌های این شخص
        /// </summary>
        public virtual ICollection<ContactPhone> Phones { get; set; } = new HashSet<ContactPhone>();

        /// <summary>
        /// لیست بخش‌هایی که این شخص مدیر آن‌هاست (اختیاری)
        /// </summary>
        public virtual ICollection<OrganizationDepartment> ManagedDepartments { get; set; } = new HashSet<OrganizationDepartment>();

        /// <summary>
        /// عضویت‌های این شخص در بخش‌های مختلف
        /// </summary>
        public virtual ICollection<DepartmentMember> DepartmentMemberships { get; set; } = new HashSet<DepartmentMember>();

        /// <summary>
        /// روابط این شخص با سازمان‌های مختلف
        /// </summary>
        public virtual ICollection<OrganizationContact> OrganizationRelations { get; set; } = new HashSet<OrganizationContact>();

        // ========== Computed Properties ==========
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [NotMapped]
        public string GenderText => Gender switch
        {
            0 => "مرد",
            1 => "زن",
            2 => "سایر",
            _ => "نامشخص"
        };

        /// <summary>
        /// شماره تماس پیش‌فرض (برای احراز هویت)
        /// </summary>
        [NotMapped]
        public ContactPhone? DefaultPhone => Phones?.FirstOrDefault(p => p.IsDefault && p.IsActive);

        /// <summary>
        /// تمام شماره‌های فعال به ترتیب نمایش
        /// </summary>
        [NotMapped]
        public IEnumerable<ContactPhone> ActivePhones => 
            Phones?.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder) ?? Enumerable.Empty<ContactPhone>();
    }
}