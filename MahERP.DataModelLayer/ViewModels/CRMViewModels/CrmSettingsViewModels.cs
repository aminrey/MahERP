using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ViewModel صفحه اصلی تنظیمات CRM
    /// </summary>
    public class CrmSettingsIndexViewModel
    {
        // ========== آمار کلی ==========

        /// <summary>
        /// تعداد وضعیت‌های Lead فعال
        /// </summary>
        public int TotalLeadStatuses { get; set; }

        /// <summary>
        /// تعداد مراحل Pipeline فعال
        /// </summary>
        public int TotalPipelineStages { get; set; }

        /// <summary>
        /// تعداد منابع Lead
        /// </summary>
        public int TotalLeadSources { get; set; }

        /// <summary>
        /// تعداد دلایل از دست رفتن
        /// </summary>
        public int TotalLostReasons { get; set; }

        // ========== لیست‌ها ==========

        /// <summary>
        /// لیست وضعیت‌های Lead
        /// </summary>
        public List<CrmLeadStatusViewModel> LeadStatuses { get; set; } = new();

        /// <summary>
        /// لیست منابع Lead
        /// </summary>
        public List<CrmLeadSourceListViewModel> LeadSources { get; set; } = new();

        /// <summary>
        /// لیست دلایل از دست رفتن
        /// </summary>
        public List<CrmLostReasonListViewModel> LostReasons { get; set; } = new();

        /// <summary>
        /// تنظیمات عمومی CRM
        /// </summary>
        public CrmGeneralSettingsViewModel GeneralSettings { get; set; } = new();
    }

    // ========== CrmLeadSource ViewModels ==========

    /// <summary>
    /// ViewModel لیست منابع Lead
    /// </summary>
    public class CrmLeadSourceListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? NameEnglish { get; set; }
        public string? Code { get; set; }
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public int LeadsCount { get; set; }

        /// <summary>
        /// آیا قابل حذف است؟
        /// </summary>
        public bool CanDelete => !IsSystem && LeadsCount == 0;

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(NameEnglish) 
            ? Name 
            : $"{Name} ({NameEnglish})";
    }

    /// <summary>
    /// ViewModel ایجاد/ویرایش منبع Lead
    /// </summary>
    public class CrmLeadSourceFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام منبع الزامی است")]
        [MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
        [Display(Name = "نام منبع")]
        public string Name { get; set; }

        [MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
        [Display(Name = "نام انگلیسی")]
        public string? NameEnglish { get; set; }

        [MaxLength(50, ErrorMessage = "حداکثر 50 کاراکتر")]
        [Display(Name = "کد")]
        public string? Code { get; set; }

        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; } = "fa-globe";

        [MaxLength(20)]
        [Display(Name = "کد رنگ")]
        public string? ColorCode { get; set; } = "#6c757d";

        [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    // ========== CrmLostReason ViewModels ==========

    /// <summary>
    /// ViewModel لیست دلایل از دست رفتن
    /// </summary>
    public class CrmLostReasonListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? TitleEnglish { get; set; }
        public string? Code { get; set; }
        public byte AppliesTo { get; set; }
        public byte Category { get; set; }
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
        public bool RequiresNote { get; set; }
        public bool IsActive { get; set; }
        public int LeadCount { get; set; }
        public int OpportunityCount { get; set; }

        /// <summary>
        /// تعداد کل استفاده
        /// </summary>
        public int UsageCount => LeadCount + OpportunityCount;

        /// <summary>
        /// آیا قابل حذف است؟
        /// </summary>
        public bool CanDelete => !IsSystem && UsageCount == 0;

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayTitle => string.IsNullOrEmpty(TitleEnglish) 
            ? Title 
            : $"{Title} ({TitleEnglish})";

        /// <summary>
        /// متن نوع کاربرد
        /// </summary>
        public string AppliesToText => AppliesTo switch
        {
            0 => "Lead و Opportunity",
            1 => "فقط Lead",
            2 => "فقط Opportunity",
            _ => "نامشخص"
        };

        /// <summary>
        /// متن دسته‌بندی
        /// </summary>
        public string CategoryText => Category switch
        {
            0 => "قیمت",
            1 => "رقابت",
            2 => "زمان‌بندی",
            3 => "کیفیت",
            4 => "نیاز نداشتن",
            5 => "سایر",
            _ => "نامشخص"
        };
    }

    /// <summary>
    /// ViewModel ایجاد/ویرایش دلیل از دست رفتن
    /// </summary>
    public class CrmLostReasonFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
        [Display(Name = "عنوان انگلیسی")]
        public string? TitleEnglish { get; set; }

        [MaxLength(50, ErrorMessage = "حداکثر 50 کاراکتر")]
        [Display(Name = "کد")]
        public string? Code { get; set; }

        /// <summary>
        /// نوع کاربرد: 0=هر دو، 1=Lead، 2=Opportunity
        /// </summary>
        [Display(Name = "کاربرد")]
        public byte AppliesTo { get; set; } = 0;

        /// <summary>
        /// دسته‌بندی
        /// </summary>
        [Display(Name = "دسته‌بندی")]
        public byte Category { get; set; } = 5;

        [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; } = "fa-times-circle";

        [MaxLength(20)]
        [Display(Name = "کد رنگ")]
        public string? ColorCode { get; set; } = "#dc3545";

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "نیاز به توضیح")]
        public bool RequiresNote { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    // ========== Legacy ViewModels (keeping for compatibility) ==========

    /// <summary>
    /// ViewModel منبع Lead (قدیمی) - برای Modal
    /// </summary>
    public class CrmLeadSourceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام منبع الزامی است")]
        [Display(Name = "نام منبع")]
        public string Name { get; set; }
        
        [Display(Name = "نام انگلیسی")]
        public string? NameEnglish { get; set; }
        
        [Display(Name = "کد")]
        public string? Code { get; set; }
        
        [Display(Name = "رنگ")]
        public string? ColorCode { get; set; } = "#6c757d";
        
        [Display(Name = "آیکون")]
        public string? Icon { get; set; } = "fa-globe";
        
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;
        
        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }
        
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تعداد Leadهای این منبع
        /// </summary>
        public int LeadsCount { get; set; }

        /// <summary>
        /// آیا از داده‌های موجود استخراج شده؟
        /// </summary>
        public bool IsFromData { get; set; }

        /// <summary>
        /// آیا قابل حذف است؟
        /// </summary>
        public bool CanDelete => LeadsCount == 0 && !IsDefault;
    }

    /// <summary>
    /// ViewModel تنظیمات عمومی CRM
    /// </summary>
    public class CrmGeneralSettingsViewModel
    {
        /// <summary>
        /// فعال‌سازی اعلان‌های CRM
        /// </summary>
        [Display(Name = "فعال‌سازی اعلان‌ها")]
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// ارسال اعلان برای Lead جدید
        /// </summary>
        [Display(Name = "اعلان Lead جدید")]
        public bool NotifyOnNewLead { get; set; } = true;

        /// <summary>
        /// ارسال اعلان برای تغییر مرحله Opportunity
        /// </summary>
        [Display(Name = "اعلان تغییر مرحله")]
        public bool NotifyOnStageChange { get; set; } = true;

        /// <summary>
        /// ارسال اعلان برای برنده شدن فرصت
        /// </summary>
        [Display(Name = "اعلان برنده شدن")]
        public bool NotifyOnOpportunityWon { get; set; } = true;

        /// <summary>
        /// ارسال اعلان برای از دست رفتن فرصت
        /// </summary>
        [Display(Name = "اعلان از دست رفتن")]
        public bool NotifyOnOpportunityLost { get; set; } = true;

        /// <summary>
        /// تعداد روز برای هشدار عدم فعالیت Lead
        /// </summary>
        [Display(Name = "هشدار عدم فعالیت (روز)")]
        [Range(1, 365)]
        public int InactivityWarningDays { get; set; } = 7;

        /// <summary>
        /// تعداد روز برای یادآوری پیگیری
        /// </summary>
        [Display(Name = "یادآوری پیگیری (روز)")]
        [Range(1, 30)]
        public int FollowUpReminderDays { get; set; } = 3;

        /// <summary>
        /// واحد پول پیش‌فرض
        /// </summary>
        [Display(Name = "واحد پول پیش‌فرض")]
        [MaxLength(10)]
        public string DefaultCurrency { get; set; } = "IRR";

        /// <summary>
        /// فرمت نمایش ارزش
        /// </summary>
        [Display(Name = "فرمت نمایش ارزش")]
        public string ValueDisplayFormat { get; set; } = "N0";

        /// <summary>
        /// درصد احتمال پیش‌فرض برای Opportunity جدید
        /// </summary>
        [Display(Name = "درصد احتمال پیش‌فرض")]
        [Range(0, 100)]
        public int DefaultProbability { get; set; } = 50;

        /// <summary>
        /// ایجاد خودکار تسک برای پیگیری
        /// </summary>
        [Display(Name = "ایجاد خودکار تسک پیگیری")]
        public bool AutoCreateFollowUpTask { get; set; } = true;

        /// <summary>
        /// تخصیص خودکار Lead به کاربر
        /// </summary>
        [Display(Name = "تخصیص خودکار Lead")]
        public bool EnableAutoAssignment { get; set; } = false;

        /// <summary>
        /// نوع تخصیص خودکار
        /// 0 = Round Robin
        /// 1 = بر اساس بار کاری
        /// 2 = بر اساس تخصص
        /// </summary>
        [Display(Name = "نوع تخصیص خودکار")]
        public byte AutoAssignmentType { get; set; } = 0;
    }

    /// <summary>
    /// ViewModel دلایل از دست رفتن - برای Modal
    /// </summary>
    public class CrmLostReasonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(200)]
        [Display(Name = "عنوان")]
        public string Title { get; set; }
        
        [MaxLength(200)]
        [Display(Name = "عنوان انگلیسی")]
        public string? TitleEnglish { get; set; }
        
        [MaxLength(50)]
        [Display(Name = "کد")]
        public string? Code { get; set; }
        
        [Display(Name = "کاربرد")]
        public byte AppliesTo { get; set; } = 0;
        
        [Display(Name = "دسته‌بندی")]
        public byte Category { get; set; } = 5;
        
        [MaxLength(20)]
        [Display(Name = "کد رنگ")]
        public string? ColorCode { get; set; } = "#dc3545";
        
        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; } = "fa-times-circle";
        
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;
        
        [Display(Name = "نیاز به توضیح")]
        public bool RequiresNote { get; set; }

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تعداد استفاده
        /// </summary>
        public int UsageCount { get; set; }
    }

    /// <summary>
    /// ViewModel رتبه‌بندی Lead
    /// </summary>
    public class CrmLeadRatingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "کد رتبه الزامی است")]
        [MaxLength(5)]
        [Display(Name = "کد رتبه")]
        public string Code { get; set; } // A, B, C, D

        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(100)]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [MaxLength(20)]
        [Display(Name = "کد رنگ")]
        public string ColorCode { get; set; }

        [Display(Name = "حداقل امتیاز")]
        [Range(0, 100)]
        public int MinScore { get; set; }

        [Display(Name = "حداکثر امتیاز")]
        [Range(0, 100)]
        public int MaxScore { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }
}
