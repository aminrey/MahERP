using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;

namespace MahERP.DataModelLayer.Services
{
    public interface IPermissionService
    {
        Task<List<Permission>> GetAllPermissionsAsync(bool includeInactive = false);
        Task<List<PermissionTreeViewModel>> GetPermissionTreeAsync(int? parentId = null);
        Task<Permission> GetPermissionByIdAsync(int id);
        Task<bool> CreatePermissionAsync(Permission permission, string currentUserId);
        Task<bool> UpdatePermissionAsync(Permission permission, string currentUserId);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> PermissionExistsAsync(string nameEn, int? excludeId = null);
        Task<List<PermissionTreeViewModel>> BuildPermissionTreeAsync(List<int> selectedIds = null);
        Task<List<int>> GetAllChildPermissionIdsAsync(int permissionId);
        Task<bool> HasChildrenAsync(int permissionId);
        
        // ⭐⭐⭐ متدهای جدید برای Import از JSON
        /// <summary>
        /// دریافت دسترسی بر اساس Code
        /// </summary>
        Task<Permission> GetPermissionByCodeAsync(string code);
        
        /// <summary>
        /// ایجاد یا به‌روزرسانی دسترسی (Upsert)
        /// </summary>
        Task<(bool success, int permissionId, bool isNew)> UpsertPermissionAsync(
            string code, 
            string nameEn, 
            string nameFa, 
            string description, 
            string icon, 
            string color, 
            int? parentId, 
            int displayOrder, 
            bool isSystemPermission, 
            string currentUserId);
    }
}