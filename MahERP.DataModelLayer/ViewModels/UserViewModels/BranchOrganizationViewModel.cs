using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    /// <summary>
    /// ViewModel برای اتصال سازمان به شعبه
    /// </summary>
    public class BranchOrganizationViewModel
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Display(Name = "نام شعبه")]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "انتخاب سازمان الزامی است")]
        [Display(Name = "سازمان")]
        public int OrganizationId { get; set; }

        [Display(Name = "نام سازمان")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "نوع رابطه الزامی است")]
        [Display(Name = "نوع رابطه")]
        public byte RelationType { get; set; }

        [Display(Name = "نمایش تمام اعضا")]
        public bool IncludeAllMembers { get; set; } = true;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ اختصاص")]
        public DateTime AssignDate { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ اختصاص (شمسی)")]
        public string AssignDatePersian { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Stats
        [Display(Name = "تعداد بخش‌ها")]
        public int DepartmentsCount { get; set; }

        [Display(Name = "تعداد اعضا")]
        public int MembersCount { get; set; }

        // Computed
        [Display(Name = "نوع رابطه")]
        public string RelationTypeText => RelationType switch
        {
            0 => "مشتری",
            1 => "تامین‌کننده",
            2 => "همکار",
            3 => "شریک",
            _ => "نامشخص"
        };
    }
}