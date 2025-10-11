using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک سازمان
    /// </summary>
    public class OrganizationDetailsViewModel
    {
        // اطلاعات پایه
        public int Id { get; set; }

        /// <summary>
        /// ⭐ نام نمایشی (Computed از Entity)
        /// </summary>
        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; }

        /// <summary>
        /// ⭐ نام سازمان (اصلی)
        /// </summary>
        [Display(Name = "نام سازمان")]
        public string Name { get; set; }

        /// <summary>
        /// ⭐ نام برند/تجاری
        /// </summary>
        [Display(Name = "نام برند")]
        public string Brand { get; set; }

        [Obsolete("از Name استفاده کنید")]
        [Display(Name = "نام رسمی")]
        public string LegalName { get; set; }

        [Display(Name = "شماره ثبت")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "کد اقتصادی")]
        public string EconomicCode { get; set; }

        /// <summary>
        /// ⭐ تاریخ ثبت
        /// </summary>
        [Display(Name = "تاریخ ثبت")]
        public DateTime? RegistrationDate { get; set; }

        [Display(Name = "شناسه ملی")]
        public string NationalId { get; set; }

        /// <summary>
        /// ⭐ تلفن اصلی
        /// </summary>
        [Display(Name = "تلفن اصلی")]
        public string PrimaryPhone { get; set; }

        /// <summary>
        /// ⭐ تلفن دوم
        /// </summary>
        [Display(Name = "تلفن دوم")]
        public string SecondaryPhone { get; set; }

        [Display(Name = "تلفن")]
        public string Phone { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "وب‌سایت")]
        public string Website { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        /// <summary>
        /// ⭐ کد پستی
        /// </summary>
        [Display(Name = "کد پستی")]
        public string PostalCode { get; set; }

        /// <summary>
        /// ⭐ نوع سازمان
        /// </summary>
        [Display(Name = "نوع سازمان")]
        public byte OrganizationType { get; set; }

        /// <summary>
        /// ⭐ توضیحات
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        /// <summary>
        /// ⭐ تاریخ ایجاد (CreatedDate در Entity)
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        // روابط
        [Display(Name = "بخش‌ها")]
        public List<OrganizationDepartment> Departments { get; set; } = new();

        [Display(Name = "افراد مرتبط")]
        public List<OrganizationContact> RelatedContacts { get; set; } = new();

        // آمار
        public OrganizationStatisticsViewModel Statistics { get; set; }

        // Computed
        public string StatusText => IsActive ? "فعال" : "غیرفعال";
        public string CreateDatePersian => CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(CreateDate, "yyyy/MM/dd");

        /// <summary>
        /// ⭐ متن نوع سازمان
        /// </summary>
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