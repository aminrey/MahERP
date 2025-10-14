using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای عضو بخش سازمانی
    /// </summary>
    public class DepartmentMemberViewModel
    {
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Display(Name = "نام بخش")]
        public string DepartmentTitle { get; set; }

        [Required(ErrorMessage = "انتخاب فرد الزامی است")]
        [Display(Name = "فرد")]
        public int ContactId { get; set; }

        [Display(Name = "نام فرد")]
        public string ContactName { get; set; }

        [Display(Name = "شماره تماس")]
        public string? ContactPhone { get; set; }

        [Display(Name = "سمت")]
        public int? PositionId { get; set; } 

        [Display(Name = "نام سمت")]
        public string? PositionTitle { get; set; }

        [Required(ErrorMessage = "تاریخ پیوستن الزامی است")]
        [Display(Name = "تاریخ پیوستن")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ پیوستن (شمسی)")]
        public string? JoinDatePersian { get; set; }

        [Display(Name = "تاریخ ترک")]
        [DataType(DataType.Date)]
        public DateTime? LeaveDate { get; set; }

        [Display(Name = "تاریخ ترک (شمسی)")]
        public string? LeaveDatePersian { get; set; }

        [Display(Name = "نوع استخدام")]
        public byte EmploymentType { get; set; } = 0;

        [Display(Name = "ناظر")]
        public bool IsSupervisor { get; set; } = false;

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "یادداشت‌ها")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ========== Computed Properties ==========
        
        [Display(Name = "نوع استخدام")]
        public string EmploymentTypeText => EmploymentType switch
        {
            0 => "تمام‌وقت",
            1 => "پاره‌وقت",
            2 => "قراردادی",
            3 => "پروژه‌ای",
            _ => "نامشخص"
        };

        [Display(Name = "مدت خدمت")]
        public string? ServiceDurationText { get; set; }
    }
}