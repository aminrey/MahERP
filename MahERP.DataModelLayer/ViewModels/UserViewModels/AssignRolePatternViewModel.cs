using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class AssignRolePatternViewModel
    {
        [Required(ErrorMessage = "کاربر الزامی است")]
        public string UserId { get; set; }

        [Display(Name = "کاربر")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "الگوی نقش الزامی است")]
        [Display(Name = "الگوی نقش")]
        public int RolePatternId { get; set; }

        [Display(Name = "الگوی نقش")]
        public string RolePatternName { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "یادداشت")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;
    }
}