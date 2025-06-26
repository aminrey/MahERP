using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class RolePatternViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الگوی نقش الزامی است")]
        [Display(Name = "نام الگوی نقش")]
        [MaxLength(100, ErrorMessage = "نام الگوی نقش حداکثر 100 کاراکتر باشد")]
        public string PatternName { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر باشد")]
        public string? Description { get; set; }

        [Display(Name = "سطح دسترسی")]
        public byte AccessLevel { get; set; }

        [Display(Name = "سطح دسترسی")]
        public string AccessLevelText { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "الگوی سیستمی")]
        public bool IsSystemPattern { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "ایجاد کننده")]
        public string CreatorName { get; set; }

        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        [Display(Name = "آخرین بروزرسانی کننده")]
        public string? LastUpdaterName { get; set; }

        // برای ویرایش دسترسی‌ها
        public List<RolePatternDetailsViewModel> Details { get; set; } = new List<RolePatternDetailsViewModel>();

        // آمار
        public int UsersCount { get; set; }
        public int ActiveUsersCount { get; set; }
    }

    public class ControllerInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ActionInfo[] Actions { get; set; }
    }

    public class ActionInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class RolePatternPermissionViewModel
    {
        public int RolePatternId { get; set; }
        public string PatternName { get; set; }
        public List<ControllerInfo> Controllers { get; set; } = new List<ControllerInfo>();
        public List<RolePatternDetailsViewModel> CurrentPermissions { get; set; } = new List<RolePatternDetailsViewModel>();
        public List<RolePatternDetailsViewModel> Permissions { get; set; } = new List<RolePatternDetailsViewModel>();
    }
}