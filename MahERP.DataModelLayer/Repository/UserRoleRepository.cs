using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.DataModelLayer.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync(bool includeInactive = false)
        {
            var query = _context.Role_Tbl
                .Include(r => r.Creator)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.NameFa)
                .ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await _context.Role_Tbl
                .Include(r => r.Creator)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CreateRoleAsync(Role role, string currentUserId)
        {
            try
            {
                role.CreatorUserId = currentUserId;
                role.CreateDate = DateTime.Now;
                
                _context.Role_Tbl.Add(role);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateRoleAsync(Role role, string currentUserId)
        {
            try
            {
                role.LastUpdaterUserId = currentUserId;
                role.LastUpdateDate = DateTime.Now;
                
                _context.Role_Tbl.Update(role);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await GetRoleByIdAsync(id);
                if (role != null && !role.IsSystemRole)
                {
                    role.IsActive = false;
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RoleExistsAsync(string nameEn, int? excludeId = null)
        {
            var query = _context.Role_Tbl.Where(r => r.NameEn == nameEn);
            
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<List<int>> GetRolePermissionIdsAsync(int roleId)
        {
            return await _context.RolePermission_Tbl
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, string currentUserId)
        {
            try
            {
                // حذف دسترسی‌های قبلی
                var existingPermissions = await _context.RolePermission_Tbl
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();
                
                _context.RolePermission_Tbl.RemoveRange(existingPermissions);

                // اضافه کردن دسترسی‌های جدید
                var newPermissions = permissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    AssignedByUserId = currentUserId,
                    AssignDate = DateTime.Now,
                    IsActive = true
                }).ToList();

                await _context.RolePermission_Tbl.AddRangeAsync(newPermissions);

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            try
            {
                var rolePermission = await _context.RolePermission_Tbl
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (rolePermission != null)
                {
                    _context.RolePermission_Tbl.Remove(rolePermission);
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRoleUsersCountAsync(int roleId)
        {
            return await _context.UserRole_Tbl
                .Where(ur => ur.RoleId == roleId && ur.IsActive)
                .CountAsync();
        }

        public async Task<int> GetRolePermissionsCountAsync(int roleId)
        {
            return await _context.RolePermission_Tbl
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .CountAsync();
        }
        #region ✅ NEW METHODS: Permission Checking & Logging

        /// <summary>
        /// بررسی اینکه کاربر دسترسی خاصی دارد یا خیر
        /// </summary>
        public async Task<bool> HasPermission(string userId, string permissionCode)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permissionCode))
                return false;

            try
            {
                // 1️⃣ بررسی اینکه کاربر Admin است
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user?.IsAdmin == true)
                    return true;

                // 2️⃣ دریافت دسترسی‌های فعال کاربر
                var hasPermission = await _context.UserPermission_Tbl
                    .Where(up => up.UserId == userId && up.IsActive)
                    .Join(
                        _context.Permission_Tbl,
                        up => up.PermissionId,
                        p => p.Id,
                        (up, p) => p
                    )
                    .AnyAsync(p => p.Code == permissionCode && p.IsActive);

                return hasPermission;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ثبت لاگ دسترسی کاربر
        /// </summary>
        public async Task<bool> LogPermissionAccess(
            string userId,
            string permissionCode,
            string action,
            bool result,
            string ipAddress = null,
            string userAgent = null)
        {
            try
            {
                // پیدا کردن Permission بر اساس Code
                var permission = await _context.Permission_Tbl
                    .FirstOrDefaultAsync(p => p.Code == permissionCode);

                if (permission == null)
                    return false;

                // ایجاد لاگ جدید
                var log = new PermissionChangeLog
                {
                    UserId = userId,
                    PermissionId = permission.Id,
                    ChangeType = result ? (byte)5 : (byte)6, // 5 = Access Granted, 6 = Access Denied
                    ChangeDescription = result
                        ? $"دسترسی موفق به: {action}"
                        : $"دسترسی رد شده به: {action}",
                    ChangeDate = DateTime.Now,
                    ChangedByUserId = userId,
                    NewIsActive = result,
                    Notes = $"IP: {ipAddress}, UserAgent: {userAgent}"
                };

                _context.PermissionChangeLog_Tbl.Add(log);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
