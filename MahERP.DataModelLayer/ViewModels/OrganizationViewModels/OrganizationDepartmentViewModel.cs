using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای بخش سازمانی
    /// </summary>
    public class OrganizationDepartmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Display(Name = "نام سازمان")]
        public string OrganizationName { get; set; }

        [Display(Name = "بخش والد")]
        public int? ParentDepartmentId { get; set; }

        [Display(Name = "نام بخش والد")]
        public string? ParentDepartmentTitle { get; set; }

        [Required(ErrorMessage = "عنوان بخش الزامی است")]
        [Display(Name = "عنوان بخش")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Display(Name = "کد بخش")]
        [MaxLength(50)]
        public string? Code { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "سطح")]
        public int Level { get; set; } = 0;

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "مدیر بخش")]
        public int? ManagerContactId { get; set; }

        [Display(Name = "نام مدیر")]
        public string? ManagerName { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; } = true;

        // ========== Computed Properties ==========
        
        [Display(Name = "مسیر کامل")]
        public string FullPath { get; set; }

        [Display(Name = "تعداد اعضا")]
        public int ActiveMembersCount { get; set; }

        [Display(Name = "تعداد سمت‌ها")]
        public int ActivePositionsCount { get; set; }

        // ========== Related Data ==========
        
        /// <summary>
        /// زیربخش‌ها
        /// </summary>
        public List<OrganizationDepartmentViewModel> ChildDepartments { get; set; } = new List<OrganizationDepartmentViewModel>();

        /// <summary>
        /// سمت‌های این بخش
        /// </summary>
        public List<DepartmentPositionViewModel> Positions { get; set; } = new List<DepartmentPositionViewModel>();

        /// <summary>
        /// اعضای این بخش
        /// </summary>
        public List<DepartmentMemberViewModel> Members { get; set; } = new List<DepartmentMemberViewModel>();
    }
}