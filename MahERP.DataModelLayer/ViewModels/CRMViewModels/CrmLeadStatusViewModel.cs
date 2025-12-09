using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CrmViewModels
{
    /// <summary>
    /// ViewModel برای نمایش وضعیت سرنخ
    /// </summary>
    public class CrmLeadStatusViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان وضعیت الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نباید بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(100)]
        [Display(Name = "عنوان انگلیسی")]
        public string? TitleEnglish { get; set; }

        [MaxLength(20)]
        [Display(Name = "کد رنگ")]
        public string? ColorCode { get; set; } = "#6c757d";

        [MaxLength(50)]
        [Display(Name = "آیکون")]
        public string? Icon { get; set; } = "fa-circle";

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "وضعیت پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "وضعیت نهایی")]
        public bool IsFinal { get; set; }

        [Display(Name = "نتیجه مثبت")]
        public bool IsPositive { get; set; }

        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // Computed
        public string DisplayTitle => string.IsNullOrEmpty(TitleEnglish) ? Title : $"{Title} ({TitleEnglish})";
        public int LeadsCount { get; set; }
        public string BadgeClass { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterName { get; set; }
    }

    /// <summary>
    /// ViewModel برای لیست وضعیت‌ها
    /// </summary>
    public class CrmLeadStatusListViewModel
    {
        public List<CrmLeadStatusViewModel> Statuses { get; set; } = new();
        public int TotalCount => Statuses?.Count ?? 0;
    }
}
