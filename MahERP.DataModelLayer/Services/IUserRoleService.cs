using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// ✅ نام تصحیح شده: IUserRoleRepository
    /// </summary>
    public interface IUserRoleRepository
    {
        // Role Management
        Task<List<Role>> GetAllRolesAsync(bool includeInactive = false);
        Task<Role> GetRoleByIdAsync(int id);
        Task<bool> CreateRoleAsync(Role role, string currentUserId);
        Task<bool> UpdateRoleAsync(Role role, string currentUserId);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> RoleExistsAsync(string nameEn, int? excludeId = null);

        // Role Permissions Management
        Task<List<int>> GetRolePermissionIdsAsync(int roleId);
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, string currentUserId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);

        // Role Statistics
        Task<int> GetRoleUsersCountAsync(int roleId);
        Task<int> GetRolePermissionsCountAsync(int roleId);

        // ✅ Permission Checking & Logging
        /// <summary>
        /// بررسی دسترسی کاربر به یک permission خاص
        /// </summary>
        Task<bool> HasPermission(string userId, string permissionCode);

        /// <summary>
        /// ثبت لاگ دسترسی کاربر به سیستم
        /// </summary>
        Task<bool> LogPermissionAccess(
            string userId,
            string permissionCode,
            string action,
            bool result,
            string ipAddress = null,
            string userAgent = null);
    }
}