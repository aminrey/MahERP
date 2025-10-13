using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.DataModelLayer.Repository
{
    public class UserPermissionRepository : IUserPermissionService
    {
        private readonly AppDbContext _context;

        public UserPermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        #region User Roles Management

        public async Task<List<UserRole>> GetUserRolesAsync(string userId)
        {
            return await _context.UserRole_Tbl
                .Include(ur => ur.Role)
                .Include(ur => ur.AssignedByUser)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.StartDate == null || ur.StartDate <= DateTime.Now)
                .Where(ur => ur.EndDate == null || ur.EndDate >= DateTime.Now)
                .OrderBy(ur => ur.Role.Priority)
                .ToListAsync();
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, int roleId, string assignedByUserId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // بررسی اینکه قبلاً این نقش داده نشده باشد
                var existingRole = await _context.UserRole_Tbl
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);

                if (existingRole != null)
                    return false; // قبلاً وجود دارد

                // ایجاد UserRole
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedByUserId = assignedByUserId,
                    AssignDate = DateTime.Now,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true
                };

                _context.UserRole_Tbl.Add(userRole);
                await _context.SaveChangesAsync();

                // همگام‌سازی دسترسی‌ها از نقش
                await SyncUserPermissionsFromRoleAsync(userId, roleId, assignedByUserId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, int roleId)
        {
            try
            {
                var userRole = await _context.UserRole_Tbl
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);

                if (userRole == null)
                    return false;

                userRole.IsActive = false;

                // حذف دسترسی‌هایی که از این نقش آمده‌اند
                var permissions = await _context.UserPermission_Tbl
                    .Where(up => up.UserId == userId && up.SourceRoleId == roleId && !up.IsManuallyModified)
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    permission.IsActive = false;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region User Permissions Management

        public async Task<List<UserPermission>> GetUserPermissionsAsync(string userId)
        {
            return await _context.UserPermission_Tbl
                .Include(up => up.Permission)
                .Include(up => up.SourceRole)
                .Where(up => up.UserId == userId && up.IsActive)
                .OrderBy(up => up.Permission.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserPermissionIdsAsync(string userId)
        {
            return await _context.UserPermission_Tbl
                .Where(up => up.UserId == userId && up.IsActive)
                .Select(up => up.PermissionId)
                .Distinct()
                .ToListAsync();
        }

        // ✅ متد جدید: مدیریت دسترسی‌های کاربر (حذف دستی‌ها + اضافه کردن جدیدها)
        public async Task<bool> ManageUserPermissionsAsync(string userId, List<int> selectedPermissionIds, string currentUserId)
        {
            try
            {
                // 1️⃣ حذف تمام دسترسی‌های دستی قبلی
                var existingManualPermissions = await _context.UserPermission_Tbl
                    .Where(up => up.UserId == userId && up.SourceType == 2)
                    .ToListAsync();

                _context.UserPermission_Tbl.RemoveRange(existingManualPermissions);

                // 2️⃣ دریافت دسترسی‌های از نقش (برای جلوگیری از تکراری)
                var roleBasedPermissionIds = await _context.UserPermission_Tbl
                    .Where(up => up.UserId == userId && up.SourceType == 1 && up.IsActive)
                    .Select(up => up.PermissionId)
                    .ToListAsync();

                // 3️⃣ اضافه کردن دسترسی‌های جدید (فقط آنهایی که از نقش نیستند)
                if (selectedPermissionIds != null && selectedPermissionIds.Any())
                {
                    foreach (var permissionId in selectedPermissionIds)
                    {
                        // اگر از نقش نیامده، به عنوان دستی اضافه کن
                        if (!roleBasedPermissionIds.Contains(permissionId))
                        {
                            var userPermission = new UserPermission
                            {
                                UserId = userId,
                                PermissionId = permissionId,
                                SourceType = 2, // دستی
                                AssignedByUserId = currentUserId,
                                AssignDate = DateTime.Now,
                                IsActive = true,
                                IsManuallyModified = true
                            };

                            _context.UserPermission_Tbl.Add(userPermission);

                            // ثبت لاگ
                            await LogPermissionChangeAsync(new PermissionChangeLog
                            {
                                UserId = userId,
                                PermissionId = permissionId,
                                ChangeType = 3,
                                ChangeDescription = "دسترسی دستی اضافه شد",
                                NewIsActive = true,
                                ChangedByUserId = currentUserId
                            });
                        }
                    }
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SyncUserPermissionsFromRoleAsync(string userId, int roleId, string currentUserId)
        {
            try
            {
                // دریافت دسترسی‌های نقش
                var rolePermissions = await _context.RolePermission_Tbl
                    .Where(rp => rp.RoleId == roleId && rp.IsActive)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                foreach (var permissionId in rolePermissions)
                {
                    // بررسی اینکه آیا کاربر قبلاً این دسترسی را دارد
                    var existingPermission = await _context.UserPermission_Tbl
                        .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

                    if (existingPermission == null)
                    {
                        // اضافه کردن دسترسی جدید
                        var userPermission = new UserPermission
                        {
                            UserId = userId,
                            PermissionId = permissionId,
                            SourceType = 1, // از نقش
                            SourceRoleId = roleId,
                            AssignedByUserId = currentUserId,
                            AssignDate = DateTime.Now,
                            IsActive = true,
                            IsManuallyModified = false
                        };

                        _context.UserPermission_Tbl.Add(userPermission);

                        // ثبت لاگ
                        await LogPermissionChangeAsync(new PermissionChangeLog
                        {
                            UserId = userId,
                            PermissionId = permissionId,
                            ChangeType = 1, // اضافه شدن از نقش
                            ChangeDescription = "دسترسی از نقش اضافه شد",
                            NewSourceRoleId = roleId,
                            NewIsActive = true,
                            ChangedByUserId = currentUserId
                        });
                    }
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ModifyUserPermissionAsync(string userId, int permissionId, bool isActive, string modifiedByUserId)
        {
            try
            {
                var userPermission = await _context.UserPermission_Tbl
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

                if (userPermission == null)
                    return false;

                var oldIsActive = userPermission.IsActive;
                var oldSourceType = userPermission.SourceType;

                userPermission.IsActive = isActive;
                userPermission.IsManuallyModified = true;
                userPermission.SourceType = 3; // ترکیبی (نقش + دستی)
                userPermission.ModifiedByUserId = modifiedByUserId;
                userPermission.ModifiedDate = DateTime.Now;

                // ثبت لاگ
                await LogPermissionChangeAsync(new PermissionChangeLog
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    ChangeType = 3, // تغییر دستی
                    ChangeDescription = isActive ? "دسترسی فعال شد" : "دسترسی غیرفعال شد",
                    OldIsActive = oldIsActive,
                    NewIsActive = isActive,
                    ChangedByUserId = modifiedByUserId
                });

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddManualPermissionToUserAsync(string userId, int permissionId, string assignedByUserId)
        {
            try
            {
                // بررسی وجود دسترسی
                var existingPermission = await _context.UserPermission_Tbl
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

                if (existingPermission != null)
                {
                    // فعال کردن مجدد
                    existingPermission.IsActive = true;
                    existingPermission.IsManuallyModified = true;
                    existingPermission.SourceType = 2; // دستی
                    existingPermission.ModifiedByUserId = assignedByUserId;
                    existingPermission.ModifiedDate = DateTime.Now;
                }
                else
                {
                    // اضافه کردن جدید
                    var userPermission = new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permissionId,
                        SourceType = 2, // دستی
                        AssignedByUserId = assignedByUserId,
                        AssignDate = DateTime.Now,
                        IsActive = true,
                        IsManuallyModified = true
                    };

                    _context.UserPermission_Tbl.Add(userPermission);
                }

                // ثبت لاگ
                await LogPermissionChangeAsync(new PermissionChangeLog
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    ChangeType = 3,
                    ChangeDescription = "دسترسی دستی اضافه شد",
                    NewIsActive = true,
                    ChangedByUserId = assignedByUserId
                });

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveManualPermissionFromUserAsync(string userId, int permissionId)
        {
            try
            {
                var userPermission = await _context.UserPermission_Tbl
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId && up.SourceType == 2);

                if (userPermission == null)
                    return false;

                _context.UserPermission_Tbl.Remove(userPermission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Permission Checking

        public async Task<bool> UserHasPermissionAsync(string userId, string permissionCode)
        {
            return await _context.UserPermission_Tbl
                .Where(up => up.UserId == userId && up.IsActive)
                .Join(_context.Permission_Tbl, 
                    up => up.PermissionId, 
                    p => p.Id, 
                    (up, p) => p)
                .AnyAsync(p => p.Code == permissionCode && p.IsActive);
        }

        public async Task<bool> UserHasAnyPermissionAsync(string userId, List<string> permissionCodes)
        {
            return await _context.UserPermission_Tbl
                .Where(up => up.UserId == userId && up.IsActive)
                .Join(_context.Permission_Tbl, 
                    up => up.PermissionId, 
                    p => p.Id, 
                    (up, p) => p)
                .AnyAsync(p => permissionCodes.Contains(p.Code) && p.IsActive);
        }

        public async Task<List<string>> GetUserPermissionCodesAsync(string userId)
        {
            return await _context.UserPermission_Tbl
                .Where(up => up.UserId == userId && up.IsActive)
                .Join(_context.Permission_Tbl, 
                    up => up.PermissionId, 
                    p => p.Id, 
                    (up, p) => p)
                .Where(p => p.IsActive)
                .Select(p => p.Code)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region Change Log

        public async Task<bool> LogPermissionChangeAsync(PermissionChangeLog log)
        {
            try
            {
                log.ChangeDate = DateTime.Now;
                _context.PermissionChangeLog_Tbl.Add(log);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<PermissionChangeLog>> GetUserPermissionLogsAsync(string userId, int? permissionId = null)
        {
            var query = _context.PermissionChangeLog_Tbl
                .Include(pcl => pcl.Permission)
                .Include(pcl => pcl.ChangedByUser)
                .Where(pcl => pcl.UserId == userId);

            if (permissionId.HasValue)
                query = query.Where(pcl => pcl.PermissionId == permissionId.Value);

            return await query
                .OrderByDescending(pcl => pcl.ChangeDate)
                .Take(100)
                .ToListAsync();
        }

        #endregion
    }
}