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
    }
}