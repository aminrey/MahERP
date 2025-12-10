using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel اصلی برای نمایش و ویرایش اطلاعات سازمان
    /// </summary>
    public class OrganizationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "نام سازمان")]
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        public string Name { get; set; }

        [Display(Name = "نام نمایشی")]
        public string? DisplayName { get; set; }

        [Display(Name = "نام برند")]
        [MaxLength(100)]
        public string? Brand { get; set; }

        /// ✅ اضافه شده
        [Display(Name = "مسیر لوگو")]
        [MaxLength(500)]
        public string? LogoPath { get; set; }

        [Display(Name = "شماره ثبت")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره ثبت باید 11 رقم باشد")]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "کد اقتصادی")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "کد اقتصادی باید 12 رقم باشد")]
        public string? EconomicCode { get; set; }

        /// ✅ اضافه شده
        [Display(Name = "تاریخ ثبت")]
        public DateTime? RegistrationDate { get; set; }

        [Display(Name = "تاریخ ثبت (شمسی)")]
        public string? RegistrationDatePersian { get; set; }

        [Display(Name = "نماینده قانونی")]
        [MaxLength(200)]
        public string? LegalRepresentative { get; set; }

        [Display(Name = "وب‌سایت")]
        [Url(ErrorMessage = "فرمت وب‌سایت نامعتبر است")]
        [MaxLength(200)]
        public string? Website { get; set; }

        [Display(Name = "تلفن اصلی")]
        [Phone]
        [MaxLength(15)]
        public string? PrimaryPhone { get; set; }

        [Display(Name = "تلفن دوم")]
        [Phone]
        [MaxLength(15)]
        public string? SecondaryPhone { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(200)]
        public string? Email { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500)]
        public string? Address { get; set; }

        [Display(Name = "کد پستی")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        /// <summary>
        /// ⭐ شعبه (برای Quick Add)
        /// </summary>
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        // فیلدهای نمایشی

        [Display(Name = "تاریخ ایجاد")]
        public string? CreatedDatePersian { get; set; }

        /// ✅ اضافه شده
        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public string? LastUpdateDatePersian { get; set; }

        [Display(Name = "ایجادکننده")]
        public string? CreatorName { get; set; }

        [Display(Name = "تعداد بخش‌ها")]
        public int TotalDepartments { get; set; }

        [Display(Name = "تعداد اعضا")]
        public int TotalMembers { get; set; }

        /// ✅ اضافه شده - برای نمایش در Details
        [Display(Name = "بخش‌های سازمانی")]
        public List<OrganizationDepartment>? Departments { get; set; }

        /// ✅ اضافه شده - برای نمایش در Details
        [Display(Name = "افراد مرتبط")]
        public List<OrganizationContact>? Contacts { get; set; }

        /// ✅ اضافه شده - برای مدیریت شماره‌ها
        [Display(Name = "شماره‌های تماس")]
        public List<OrganizationPhoneViewModel>? Phones { get; set; }

        // Computed Properties

        public string OrganizationTypeText => OrganizationType switch
        {
            0 => "شرکت",
            1 => "سازمان",
            2 => "موسسه",
            3 => "نهاد",
            _ => "نامشخص"
        };
    }
}