using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای سمت‌های رایج سازمانی
    /// </summary>
    public class PositionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [Display(Name = "عنوان سمت (فارسی)")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Display(Name = "عنوان انگلیسی")]
        [MaxLength(200)]
        public string? TitleEnglish { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [Display(Name = "دسته‌بندی")]
        [MaxLength(100)]
        public string Category { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "سطح سازمانی الزامی است")]
        [Display(Name = "سطح سازمانی")]
        [Range(0, 10, ErrorMessage = "سطح باید بین 0 تا 10 باشد")]
        public int Level { get; set; } = 3;

        [Required(ErrorMessage = "سطح قدرت پیش‌فرض الزامی است")]
        [Display(Name = "سطح قدرت پیش‌فرض")]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int DefaultPowerLevel { get; set; } = 50;

        [Display(Name = "سمت رایج")]
        public bool IsCommon { get; set; } = true;

        [Display(Name = "نیاز به مدرک")]
        public bool RequiresDegree { get; set; } = false;

        [Display(Name = "حداقل مدرک تحصیلی")]
        public byte? MinimumDegree { get; set; }

        [Display(Name = "حداقل سابقه کار (سال)")]
        public int? MinimumExperienceYears { get; set; }

        [Display(Name = "حداقل حقوق پیشنهادی (ریال)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? SuggestedMinSalary { get; set; }

        [Display(Name = "حداکثر حقوق پیشنهادی (ریال)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? SuggestedMaxSalary { get; set; }

        [Display(Name = "می‌تواند استخدام کند")]
        public bool CanHireSubordinates { get; set; } = false;

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // ========== Computed Properties ==========
        
        [Display(Name = "عنوان کامل")]
        public string FullTitle => string.IsNullOrEmpty(TitleEnglish) 
            ? Title 
            : $"{Title} ({TitleEnglish})";

        [Display(Name = "سطح")]
        public string LevelText => Level switch
        {
            0 => "مدیریت عالی",
            1 => "مدیریت میانی",
            2 => "کارشناس ارشد",
            3 => "کارشناس",
            4 => "کارمند عملیاتی",
            _ => $"سطح {Level}"
        };

        [Display(Name = "حداقل مدرک")]
        public string MinimumDegreeText => MinimumDegree switch
        {
            0 => "دیپلم",
            1 => "کاردانی",
            2 => "کارشناسی",
            3 => "کارشناسی ارشد",
            4 => "دکترا",
            _ => "تعیین نشده"
        };

        [Display(Name = "بازه حقوق")]
        public string SuggestedSalaryRangeText
        {
            get
            {
                if (SuggestedMinSalary.HasValue && SuggestedMaxSalary.HasValue)
                    return $"{SuggestedMinSalary:N0} - {SuggestedMaxSalary:N0} ریال";
                if (SuggestedMinSalary.HasValue)
                    return $"از {SuggestedMinSalary:N0} ریال";
                if (SuggestedMaxSalary.HasValue)
                    return $"تا {SuggestedMaxSalary:N0} ریال";
                return "تعیین نشده";
            }
        }

        public DateTime CreatedDate { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterName { get; set; }
    }
}
