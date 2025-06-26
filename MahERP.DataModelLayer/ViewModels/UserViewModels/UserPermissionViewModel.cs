using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class UserPermissionViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "نام کاربر")]
        public string UserName { get; set; }

        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "سمت")]
        public string? PositionName { get; set; }

        [Display(Name = "شعبه")]
        public string BranchName { get; set; }

        [Display(Name = "نقش‌های سیستم")]
        public List<string> SystemRoles { get; set; } = new List<string>();

        [Display(Name = "الگوهای نقش")]
        public List<UserRolePatternInfo> RolePatterns { get; set; } = new List<UserRolePatternInfo>();

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ عضویت")]
        public DateTime RegisterDate { get; set; }
    }

    public class UserRolePatternInfo
    {
        public int Id { get; set; }
        public int RolePatternId { get; set; }
        public string PatternName { get; set; }
        public string? Description { get; set; }
        public DateTime AssignDate { get; set; }
        public string AssignedByName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }


    public class UserPermissionDetailViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> SystemRoles { get; set; } = new List<string>();
        public List<UserRolePatternInfo> RolePatterns { get; set; } = new List<UserRolePatternInfo>();
        public Dictionary<string, List<string>> Permissions { get; set; } = new Dictionary<string, List<string>>();
    }
}