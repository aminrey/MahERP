using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای سمت‌های بخش سازمانی
    /// </summary>
    public class DepartmentPositionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Display(Name = "نام بخش")]
        public string DepartmentTitle { get; set; }

        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [Display(Name = "عنوان سمت")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Display(Name = "کد سمت")]
        [MaxLength(50)]
        public string? Code { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "سطح قدرت الزامی است")]
        [Display(Name = "سطح قدرت")]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int PowerLevel { get; set; } = 50;

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "حداقل حقوق (ریال)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? MinSalary { get; set; }

        [Display(Name = "حداکثر حقوق (ریال)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? MaxSalary { get; set; }

        [Display(Name = "سمت پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "می‌تواند استخدام کند")]
        public bool CanHireSubordinates { get; set; } = false;

        [Display(Name = "نیاز به تایید")]
        public bool RequiresApproval { get; set; } = false;

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        // ========== Computed Properties ==========
        
        [Display(Name = "تعداد اعضا")]
        public int ActiveMembersCount { get; set; }

        [Display(Name = "بازه حقوق")]
        public string SalaryRangeText { get; set; }
    }
}