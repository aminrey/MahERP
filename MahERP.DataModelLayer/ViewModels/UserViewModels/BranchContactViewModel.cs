using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    /// <summary>
    /// ViewModel برای اتصال فرد به شعبه
    /// </summary>
    public class BranchContactViewModel
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Display(Name = "نام شعبه")]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        [Display(Name = "فرد")]
        public int ContactId { get; set; }

        [Display(Name = "نام فرد")]
        public string ContactName { get; set; }

        [Display(Name = "شماره تماس")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "نوع رابطه الزامی است")]
        [Display(Name = "نوع رابطه")]
        public byte RelationType { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ اختصاص")]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ اختصاص (شمسی)")]
        public string AssignDatePersian { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Computed
        [Display(Name = "نوع رابطه")]
        public string RelationTypeText => RelationType switch
        {
            0 => "مشتری",
            1 => "تامین‌کننده",
            2 => "همکار",
            3 => "سایر",
            _ => "نامشخص"
        };
    }
}