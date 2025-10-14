using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای ارتباط فرد با سازمان
    /// </summary>
    public class OrganizationContactViewModel
    {
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Display(Name = "نام سازمان")]
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        [Display(Name = "فرد")]
        public int ContactId { get; set; }

        [Display(Name = "نام فرد")]
        public string? ContactName { get; set; }

        [Display(Name = "شماره تماس")]
        public string? ContactPhone { get; set; }

        [Required(ErrorMessage = "نوع رابطه الزامی است")]
        [Display(Name = "نوع رابطه")]
        public byte RelationType { get; set; }

        [Display(Name = "عنوان شغلی")]
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [Display(Name = "بخش")]
        [MaxLength(100)]
        public string? Department { get; set; }

        [Display(Name = "تماس اصلی")]
        public bool IsPrimary { get; set; } = false;

        [Display(Name = "تصمیم‌گیرنده")]
        public bool IsDecisionMaker { get; set; } = false;

        [Display(Name = "سطح اهمیت")]
        public byte ImportanceLevel { get; set; } = 1;

        [Display(Name = "تاریخ شروع")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string? StartDatePersian { get; set; }

        [Display(Name = "تاریخ پایان")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string? EndDatePersian { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "یادداشت‌ها")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ========== Computed Properties ==========
        
        [Display(Name = "نوع رابطه")]
        public string RelationTypeText => RelationType switch
        {
            0 => "کارمند",
            1 => "مشتری",
            2 => "تامین‌کننده",
            3 => "شریک",
            4 => "مشاور",
            _ => "نامشخص"
        };

        [Display(Name = "سطح اهمیت")]
        public string ImportanceLevelText => ImportanceLevel switch
        {
            0 => "پایین",
            1 => "متوسط",
            2 => "بالا",
            3 => "خیلی بالا",
            _ => "نامشخص"
        };
    }
}