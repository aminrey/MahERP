using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Organizations
{
    /// <summary>
    /// سمت‌های رایج سازمانی (Organization Standard Positions)
    /// این سمت‌ها در تمام سیستم مشترک هستند و می‌توان از آنها در بخش‌های مختلف استفاده کرد
    /// ⭐ نامگذاری: OrganizationPosition - سمت‌های استاندارد سازمانی
    /// </summary>
    [Table("OrganizationPosition_Tbl")]
    public class OrganizationPosition
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان سمت (فارسی)
        /// مثال: مدیرعامل، مدیر مالی، حسابدار
        /// </summary>
        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// عنوان انگلیسی
        /// مثال: CEO, CFO, Accountant
        /// </summary>
        [MaxLength(200)]
        public string? TitleEnglish { get; set; }

        /// <summary>
        /// دسته‌بندی سمت
        /// مثال: مدیریت، مالی، فنی، منابع انسانی، فروش، بازاریابی
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// توضیحات و شرح وظایف
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// سطح سازمانی
        /// 0 = مدیریت عالی (مدیرعامل، هیئت مدیره)
        /// 1 = مدیریت میانی (مدیر بخش، سرپرست)
        /// 2 = کارشناس ارشد
        /// 3 = کارشناس
        /// 4 = کارمند عملیاتی
        /// </summary>
        [Required]
        [Range(0, 10, ErrorMessage = "سطح باید بین 0 تا 10 باشد")]
        public int Level { get; set; } = 3;

        /// <summary>
        /// سطح قدرت پیش‌فرض (عدد کمتر = قدرت بیشتر)
        /// این مقدار می‌تواند در DepartmentPosition تغییر کند
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int DefaultPowerLevel { get; set; } = 50;

        /// <summary>
        /// آیا این سمت رایج است؟
        /// سمت‌های رایج در لیست‌های پیشنهادی نمایش داده می‌شوند
        /// </summary>
        public bool IsCommon { get; set; } = true;

        /// <summary>
        /// آیا نیاز به مدرک تحصیلی خاص دارد؟
        /// </summary>
        public bool RequiresDegree { get; set; } = false;

        /// <summary>
        /// حداقل مدرک تحصیلی مورد نیاز
        /// 0 = دیپلم، 1 = کاردانی، 2 = کارشناسی، 3 = کارشناسی ارشد، 4 = دکترا
        /// </summary>
        public byte? MinimumDegree { get; set; }

        /// <summary>
        /// حداقل سابقه کاری مورد نیاز (سال)
        /// </summary>
        public int? MinimumExperienceYears { get; set; }

        /// <summary>
        /// بازه حقوق پیشنهادی حداقلی (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SuggestedMinSalary { get; set; }

        /// <summary>
        /// بازه حقوق پیشنهادی حداکثری (ریال)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SuggestedMaxSalary { get; set; }

        /// <summary>
        /// آیا می‌تواند زیردست استخدام کند؟
        /// </summary>
        public bool CanHireSubordinates { get; set; } = false;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== اطلاعات سیستمی ==========
        
        [Required]
        public DateTime CreatedDate { get; set; }

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

        // ========== Computed Properties ==========
        
        /// <summary>
        /// نام کامل سمت (فارسی + انگلیسی)
        /// </summary>
        [NotMapped]
        public string FullTitle => string.IsNullOrEmpty(TitleEnglish) 
            ? Title 
            : $"{Title} ({TitleEnglish})";

        /// <summary>
        /// متن سطح سازمانی
        /// </summary>
        [NotMapped]
        public string LevelText => Level switch
        {
            0 => "مدیریت عالی",
            1 => "مدیریت میانی",
            2 => "کارشناس ارشد",
            3 => "کارشناس",
            4 => "کارمند عملیاتی",
            _ => $"سطح {Level}"
        };

        /// <summary>
        /// متن مدرک تحصیلی
        /// </summary>
        [NotMapped]
        public string MinimumDegreeText => MinimumDegree switch
        {
            0 => "دیپلم",
            1 => "کاردانی",
            2 => "کارشناسی",
            3 => "کارشناسی ارشد",
            4 => "دکترا",
            _ => "تعیین نشده"
        };

        /// <summary>
        /// بازه حقوق به صورت متن
        /// </summary>
        [NotMapped]
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
    }
}
