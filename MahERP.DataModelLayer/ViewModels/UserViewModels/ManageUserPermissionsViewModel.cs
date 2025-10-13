using System.Collections.Generic;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class ManageUserPermissionsViewModel
    {
        public string UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserName { get; set; } // ✅ اضافه شد
        
        // نقش‌های کاربر
        public List<UserRoleInfo> UserRoles { get; set; } = new List<UserRoleInfo>();
        
        // درخت دسترسی‌ها
        public List<PermissionTreeViewModel> PermissionTree { get; set; } = new List<PermissionTreeViewModel>();
        
        // دسترسی‌های فعلی کاربر
        public List<UserPermissionDetail> UserPermissions { get; set; } = new List<UserPermissionDetail>();
        
        // ✅ اضافه شد: لیست ID های انتخاب شده
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }

    public class UserRoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }
        public string RoleIcon { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserPermissionDetail
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionCode { get; set; } // ✅ اضافه شد
        public string SourceType { get; set; } // "Role" یا "Manual"
        public int? SourceRoleId { get; set; }
        public string SourceRoleName { get; set; }
        public bool IsManuallyModified { get; set; }
        public bool IsActive { get; set; }
    }
}