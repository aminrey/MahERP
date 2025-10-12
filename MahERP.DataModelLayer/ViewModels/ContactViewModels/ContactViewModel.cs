using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel اصلی برای نمایش و ویرایش اطلاعات فرد
    /// </summary>
    public class ContactViewModel
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "نام الزامی است")]
        [Display(Name = "نام")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Display(Name = "نام کامل")]
        public string? FullName { get; set; }

        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string? NationalCode { get; set; }

        [Display(Name = "ایمیل اصلی")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(200)]
        public string? PrimaryEmail { get; set; }

        /// <summary>
        /// ⭐ شماره تلفن اصلی (اختیاری)
        /// </summary>
        [Display(Name = "شماره تلفن اصلی")]
        public string? PrimaryPhone { get; set; }

        /// <summary>
        /// ⭐ اضافه شده
        /// </summary>
        [Display(Name = "نوع رابطه")]
        public byte RelationType { get; set; }

        [Display(Name = "ایمیل دوم")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(200)]
        public string? SecondaryEmail { get; set; }

        [Display(Name = "آدرس اصلی")]
        [MaxLength(500)]
        public string? PrimaryAddress { get; set; }

        [Display(Name = "آدرس دوم")]
        [MaxLength(500)]
        public string? SecondaryAddress { get; set; }

        [Display(Name = "کد پستی آدرس اصلی")]
        [MaxLength(20)]
        public string? PrimaryPostalCode { get; set; }

        [Display(Name = "کد پستی آدرس دوم")]
        [MaxLength(20)]
        public string? SecondaryPostalCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string? BirthDatePersian { get; set; }

        [Display(Name = "جنسیت")]
        public byte? Gender { get; set; }

        [Display(Name = "تصویر پروفایل")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        // ========== Computed Properties ==========
        
    

        [Display(Name = "جنسیت")]
        public string GenderText => Gender switch
        {
            0 => "مرد",
            1 => "زن",
            2 => "سایر",
            _ => "نامشخص"
        };

        // ========== Related Data ==========
        
        /// <summary>
        /// لیست شماره‌های تماس
        /// </summary>
        public List<ContactPhoneViewModel> Phones { get; set; } = new List<ContactPhoneViewModel>();

        /// <summary>
        /// عضویت‌های سازمانی
        /// </summary>
        public List<DepartmentMemberViewModel> DepartmentMemberships { get; set; } = new List<DepartmentMemberViewModel>();

        /// <summary>
        /// ارتباطات با سازمان‌ها
        /// </summary>
        public List<OrganizationContactViewModel> OrganizationRelations { get; set; } = new List<OrganizationContactViewModel>();

        // ========== System Info ==========
        
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string? CreatedDatePersian { get; set; }

        [Display(Name = "ایجاد کننده")]
        public string? CreatorName { get; set; }

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string? LastUpdateDatePersian { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش Contact در لیست‌ها
    /// </summary>
    public class ContactListViewModel
    {
        public int Id { get; set; }
        
        public string? FirstName { get; set; }
        
        public string LastName { get; set; }
        
        /// <summary>
        /// نام کامل (برای نمایش در dropdown)
        /// </summary>
        public string? FullName { get; set; }
        
        public string? NationalCode { get; set; }
        
        /// <summary>
        /// شماره تلفن اصلی
        /// </summary>
        public string? PrimaryPhone { get; set; }
        
        /// <summary>
        /// نوع رابطه با شعبه (اگر از BranchContact گرفته شود)
        /// </summary>
        public byte? RelationType { get; set; }
        
        public bool IsActive { get; set; }
    }
}