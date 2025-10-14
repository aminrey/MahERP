using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;

namespace MahERP.DataModelLayer.Repository
{
    public class PermissionRepository : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllPermissionsAsync(bool includeInactive = false)
        {
            var query = _context.Permission_Tbl
                .Include(p => p.Parent)
                .Include(p => p.Creator)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return await query
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<PermissionTreeViewModel>> GetPermissionTreeAsync(int? parentId = null)
        {
            var permissions = await _context.Permission_Tbl
                .Where(p => p.IsActive && p.ParentId == parentId)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new PermissionTreeViewModel
                {
                    Id = p.Id,
                    NameEn = p.NameEn,
                    NameFa = p.NameFa,
                    Code = p.Code,
                    Icon = p.Icon,
                    Color = p.Color,
                    ParentId = p.ParentId,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            foreach (var permission in permissions)
            {
                permission.Children = await GetPermissionTreeAsync(permission.Id);
            }

            return permissions;
        }

        public async Task<Permission> GetPermissionByIdAsync(int id)
        {
            return await _context.Permission_Tbl
                .Include(p => p.Parent)
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CreatePermissionAsync(Permission permission, string currentUserId)
        {
            try
            {
                permission.CreatorUserId = currentUserId;
                permission.CreateDate = DateTime.Now;
                
                _context.Permission_Tbl.Add(permission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePermissionAsync(Permission permission, string currentUserId)
        {
            try
            {
                permission.LastUpdaterUserId = currentUserId;
                permission.LastUpdateDate = DateTime.Now;
                
                _context.Permission_Tbl.Update(permission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            try
            {
                var permission = await GetPermissionByIdAsync(id);
                if (permission != null && !permission.IsSystemPermission)
                {
                    permission.IsActive = false;
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PermissionExistsAsync(string nameEn, int? excludeId = null)
        {
            var query = _context.Permission_Tbl.Where(p => p.NameEn == nameEn);
            
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<List<PermissionTreeViewModel>> BuildPermissionTreeAsync(List<int> selectedIds = null)
        {
            var tree = await GetPermissionTreeAsync();
            
            if (selectedIds != null && selectedIds.Any())
            {
                MarkSelectedPermissions(tree, selectedIds);
            }
            
            return tree;
        }

        private void MarkSelectedPermissions(List<PermissionTreeViewModel> tree, List<int> selectedIds)
        {
            foreach (var node in tree)
            {
                node.IsSelected = selectedIds.Contains(node.Id);
                if (node.Children.Any())
                {
                    MarkSelectedPermissions(node.Children, selectedIds);
                }
            }
        }

        public async Task<List<int>> GetAllChildPermissionIdsAsync(int permissionId)
        {
            var childIds = new List<int>();
            await GetChildIdsRecursive(permissionId, childIds);
            return childIds;
        }

        private async Task GetChildIdsRecursive(int parentId, List<int> childIds)
        {
            var children = await _context.Permission_Tbl
                .Where(p => p.ParentId == parentId && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            childIds.AddRange(children);

            foreach (var childId in children)
            {
                await GetChildIdsRecursive(childId, childIds);
            }
        }

        public async Task<bool> HasChildrenAsync(int permissionId)
        {
            return await _context.Permission_Tbl
                .AnyAsync(p => p.ParentId == permissionId && p.IsActive);
        }

        // ⭐⭐⭐ متدهای جدید برای Import از JSON
        
        /// <summary>
        /// دریافت دسترسی بر اساس Code
        /// </summary>
        public async Task<Permission> GetPermissionByCodeAsync(string code)
        {
            return await _context.Permission_Tbl
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        /// <summary>
        /// ایجاد یا به‌روزرسانی دسترسی (Upsert Pattern)
        /// 
        /// این متد به صورت خودکار تشخیص می‌دهد که دسترسی موجود است یا خیر:
        /// - اگر موجود باشد → به‌روزرسانی می‌کند
        /// - اگر جدید باشد → ایجاد می‌کند
        /// 
        /// خروجی:
        /// - success: آیا عملیات موفق بود؟
        /// - permissionId: شناسه دسترسی (برای ایجاد یا بروزرسانی)
        /// - isNew: آیا دسترسی جدید ایجاد شد؟ (true = جدید، false = بروزرسانی)
        /// </summary>
        public async Task<(bool success, int permissionId, bool isNew)> UpsertPermissionAsync(
            string code,
            string nameEn,
            string nameFa,
            string description,
            string icon,
            string color,
            int? parentId,
            int displayOrder,
            bool isSystemPermission,
            string currentUserId)
        {
            try
            {
                var existingPermission = await GetPermissionByCodeAsync(code);

                if (existingPermission != null)
                {
                    // ⭐ به‌روزرسانی دسترسی موجود
                    existingPermission.NameEn = nameEn;
                    existingPermission.NameFa = nameFa;
                    existingPermission.Description = description;
                    existingPermission.Icon = icon;
                    existingPermission.Color = color;
                    existingPermission.ParentId = parentId;
                    existingPermission.DisplayOrder = displayOrder;
                    existingPermission.IsSystemPermission = isSystemPermission;
                    existingPermission.LastUpdaterUserId = currentUserId;
                    existingPermission.LastUpdateDate = DateTime.Now;

                    _context.Permission_Tbl.Update(existingPermission);
                    await _context.SaveChangesAsync();

                    return (true, existingPermission.Id, false); // موفق، شناسه، بروزرسانی شده
                }
                else
                {
                    // ⭐ ایجاد دسترسی جدید
                    var newPermission = new Permission
                    {
                        Code = code,
                        NameEn = nameEn,
                        NameFa = nameFa,
                        Description = description,
                        Icon = icon,
                        Color = color,
                        ParentId = parentId,
                        DisplayOrder = displayOrder,
                        IsActive = true,
                        IsSystemPermission = isSystemPermission,
                        CreateDate = DateTime.Now,
                        CreatorUserId = currentUserId
                    };

                    _context.Permission_Tbl.Add(newPermission);
                    await _context.SaveChangesAsync();

                    return (true, newPermission.Id, true); // موفق، شناسه، جدید ایجاد شده
                }
            }
            catch
            {
                return (false, 0, false); // ناموفق
            }
        }
    }
}