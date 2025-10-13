using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.DataModelLayer.Services
{
    public interface IUserPermissionService
    {
        // User Roles Management
        Task<List<UserRole>> GetUserRolesAsync(string userId);
        Task<bool> AssignRoleToUserAsync(string userId, int roleId, string assignedByUserId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> RemoveRoleFromUserAsync(string userId, int roleId);
        
        // User Permissions Management
        Task<List<UserPermission>> GetUserPermissionsAsync(string userId);
        Task<List<int>> GetUserPermissionIdsAsync(string userId);
        Task<bool> SyncUserPermissionsFromRoleAsync(string userId, int roleId, string currentUserId);
        Task<bool> ModifyUserPermissionAsync(string userId, int permissionId, bool isActive, string modifiedByUserId);
        Task<bool> AddManualPermissionToUserAsync(string userId, int permissionId, string assignedByUserId);
        Task<bool> RemoveManualPermissionFromUserAsync(string userId, int permissionId);
        
        // Permission Checking
        Task<bool> UserHasPermissionAsync(string userId, string permissionCode);
        Task<bool> UserHasAnyPermissionAsync(string userId, List<string> permissionCodes);
        Task<List<string>> GetUserPermissionCodesAsync(string userId);
        
        // Change Log
        Task<bool> LogPermissionChangeAsync(PermissionChangeLog log);
        Task<List<PermissionChangeLog>> GetUserPermissionLogsAsync(string userId, int? permissionId = null);
        Task<bool> ManageUserPermissionsAsync(string userId, List<int> selectedPermissionIds, string currentUserId);
    }
}