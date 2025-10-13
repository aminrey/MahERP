using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;

namespace MahERP.DataModelLayer.Services
{
    public interface IPermissionService
    {
        // Permission Management
        Task<List<Permission>> GetAllPermissionsAsync(bool includeInactive = false);
        Task<List<PermissionTreeViewModel>> GetPermissionTreeAsync(int? parentId = null);
        Task<Permission> GetPermissionByIdAsync(int id);
        Task<bool> CreatePermissionAsync(Permission permission, string currentUserId);
        Task<bool> UpdatePermissionAsync(Permission permission, string currentUserId);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> PermissionExistsAsync(string nameEn, int? excludeId = null);
        
        // Permission Tree Operations
        Task<List<PermissionTreeViewModel>> BuildPermissionTreeAsync(List<int> selectedIds = null);
        Task<List<int>> GetAllChildPermissionIdsAsync(int permissionId);
        
        // ✅ اضافه کردن متد جدید
        Task<bool> HasChildrenAsync(int permissionId);
    }
}