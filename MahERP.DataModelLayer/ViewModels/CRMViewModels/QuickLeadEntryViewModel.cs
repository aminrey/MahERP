using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ⭐⭐⭐ ViewModel برای ثبت سریع سرنخ با اقدام بعدی اجباری
    /// قانون طلایی: هیچ سرنخی بدون اقدام بعدی نباید ثبت شود!
    /// </summary>
    public class QuickLeadEntryViewModel : IHasReturnValue
    {
        #region Basic Info

        /// <summary>
        /// نوع سرنخ: Contact یا Organization
        /// </summary>
        [Required(ErrorMessage = "نوع سرنخ را انتخاب کنید")]
        [Display(Name = "نوع سرنخ")]
        public string LeadType { get; set; } = "Contact";

        /// <summary>
        /// آیا فرد/سازمان جدید است؟
        /// </summary>
        [Display(Name = "ایجاد جدید")]
        public bool IsNewEntity { get; set; } = true;

        /// <summary>
        /// شناسه Contact موجود
        /// </summary>
        [Display(Name = "فرد")]
        public int? ContactId { get; set; }

        /// <summary>
        /// شناسه Organization موجود
        /// </summary>
        [Display(Name = "سازمان")]
        public int? OrganizationId { get; set; }

        #endregion

        #region New Contact Fields

        /// <summary>
        /// نام (برای Contact جدید)
        /// </summary>
        [Display(Name = "نام")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string? FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی (برای Contact جدید)
        /// </summary>
        [Display(Name = "نام خانوادگی")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string? LastName { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        [Display(Name = "شماره موبایل")]
        [MaxLength(20)]
        [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
        public string? MobileNumber { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Display(Name = "ایمیل")]
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string? Email { get; set; }

        #endregion

        #region New Organization Fields

        /// <summary>
        /// نام سازمان (برای Organization جدید)
        /// </summary>
        [Display(Name = "نام سازمان")]
        [MaxLength(200, ErrorMessage = "نام سازمان نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string? OrganizationName { get; set; }

        /// <summary>
        /// شماره تلفن سازمان
        /// </summary>
        [Display(Name = "تلفن سازمان")]
        [MaxLength(20)]
        public string? OrganizationPhone { get; set; }

        #endregion

        #region Lead Info

        /// <summary>
        /// شعبه
        /// </summary>
        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        /// <summary>
        /// منبع سرنخ
        /// </summary>
        [Display(Name = "منبع سرنخ")]
        [MaxLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// یادداشت اولیه
        /// </summary>
        [Display(Name = "یادداشت")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// وضعیت اولیه سرنخ
        /// </summary>
        [Display(Name = "وضعیت")]
        public int? StatusId { get; set; }

        #endregion

        #region ⭐⭐⭐ NextAction (اجباری!)

        /// <summary>
        /// نوع اقدام بعدی (اجباری!)
        /// </summary>
        [Required(ErrorMessage = "نوع اقدام بعدی الزامی است")]
        [Display(Name = "نوع اقدام بعدی")]
        public CrmNextActionType NextActionType { get; set; } = CrmNextActionType.Call;

        /// <summary>
        /// تاریخ اقدام بعدی (اجباری!)
        /// </summary>
        [Required(ErrorMessage = "تاریخ اقدام بعدی الزامی است")]
        [Display(Name = "تاریخ اقدام بعدی")]
        public DateTime NextActionDate { get; set; } = DateTime.Now.AddDays(1);

        /// <summary>
        /// تاریخ اقدام بعدی (شمسی) - برای فرم
        /// </summary>
        [Required(ErrorMessage = "تاریخ اقدام بعدی الزامی است")]
        [Display(Name = "تاریخ اقدام بعدی")]
        public string NextActionDatePersian { get; set; } = string.Empty;

        /// <summary>
        /// ساعت اقدام بعدی
        /// </summary>
        [Display(Name = "ساعت")]
        public string? NextActionTime { get; set; } = "09:00";

        /// <summary>
        /// یادداشت اقدام بعدی
        /// </summary>
        [Display(Name = "یادداشت اقدام")]
        [MaxLength(500)]
        public string? NextActionNote { get; set; }

        /// <summary>
        /// آیا تسک برای اقدام بعدی ایجاد شود؟
        /// </summary>
        [Display(Name = "ایجاد تسک")]
        public bool CreateTaskForNextAction { get; set; } = true;

        /// <summary>
        /// اولویت تسک
        /// </summary>
        [Display(Name = "اولویت تسک")]
        public CrmTaskPriority TaskPriority { get; set; } = CrmTaskPriority.Normal;

        #endregion

        #region Return & Navigation

        /// <summary>
        /// URL بازگشت
        /// </summary>
        [MaxLength(500)]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// صفحه مبدا
        /// </summary>
        [MaxLength(50)]
        public string? SourcePage { get; set; }

        #endregion

        #region Initial Lists (برای پر کردن Dropdowns)

        /// <summary>
        /// لیست شعبه‌ها
        /// </summary>
        public List<CrmSelectListItem> BranchesInitial { get; set; } = new();

        /// <summary>
        /// لیست منابع سرنخ
        /// </summary>
        public List<CrmSelectListItem> SourcesInitial { get; set; } = new();

        /// <summary>
        /// لیست وضعیت‌ها
        /// </summary>
        public List<CrmSelectListItem> StatusesInitial { get; set; } = new();

        /// <summary>
        /// لیست انواع اقدام بعدی
        /// </summary>
        public List<CrmSelectListItem> NextActionTypesInitial { get; set; } = new();

        /// <summary>
        /// لیست اولویت‌های تسک
        /// </summary>
        public List<CrmSelectListItem> TaskPrioritiesInitial { get; set; } = new();

        #endregion

        #region Computed Properties

        /// <summary>
        /// نام کامل فرد (برای نمایش)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// نام نمایشی سرنخ
        /// </summary>
        public string DisplayName => LeadType == "Contact" ? FullName : OrganizationName ?? "";

        /// <summary>
        /// متن نوع اقدام بعدی
        /// </summary>
        public string NextActionTypeText => NextActionType switch
        {
            CrmNextActionType.Call => "تماس تلفنی",
            CrmNextActionType.Meeting => "جلسه حضوری",
            CrmNextActionType.Email => "ارسال ایمیل",
            CrmNextActionType.Sms => "ارسال پیامک",
            CrmNextActionType.SendQuote => "ارسال پیشنهاد قیمت",
            CrmNextActionType.FollowUpQuote => "پیگیری پیشنهاد",
            CrmNextActionType.Visit => "بازدید",
            CrmNextActionType.Demo => "دمو محصول",
            CrmNextActionType.Other => "سایر",
            _ => "نامشخص"
        };

        #endregion
    }

    /// <summary>
    /// آیتم ساده برای Select List
    /// </summary>
    public class CrmSelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
        public bool Disabled { get; set; }

        public CrmSelectListItem() { }

        public CrmSelectListItem(string value, string text, bool selected = false)
        {
            Value = value;
            Text = text;
            Selected = selected;
        }
    }

    /// <summary>
    /// نتیجه ثبت سریع سرنخ
    /// </summary>
    public class QuickLeadEntryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? LeadId { get; set; }
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }
        public int? TaskId { get; set; }
        public string? TaskCode { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
