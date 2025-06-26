using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext db)
        {
            _context = db;
        }

        #region متدهای موجود

        public string GetRoleId(string userId)
        {
            var getRoleId = _context.UserRoles.Where(ur => ur.UserId == userId).ToList();
            string getRollString = "";
            for (int i = 0; i < getRoleId.Count; i++)
            {
                getRollString += getRoleId[i].RoleId.ToString() + ",";
            }
            return getRollString;
        }

        public string GetRolePatternId(int RolePatternID)
        {
            var getRoleId = _context.RolePatternDetails_Tbl.Where(rp => rp.RolePatternId == RolePatternID).ToList();
            string getRollString = "";
            for (int i = 0; i < getRoleId.Count; i++)
            {
                getRollString += getRoleId[i].Id.ToString() + ",";
            }
            return getRollString;
        }

        #endregion

        #region RolePattern Management

        public List<RolePattern> GetAllRolePatterns(bool includeInactive = false)
        {
            var query = _context.RolePattern_Tbl
                .Include(rp => rp.Creator)
                .Include(rp => rp.LastUpdater)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(rp => rp.IsActive);

            return query.OrderBy(rp => rp.AccessLevel)
                       .ThenBy(rp => rp.PatternName)
                       .ToList();
        }

        public RolePattern GetRolePatternById(int id, bool includeDetails = false)
        {
            var query = _context.RolePattern_Tbl
                .Include(rp => rp.Creator)
                .Include(rp => rp.LastUpdater)
                .AsQueryable();

            if (includeDetails)
            {
                query = query.Include(rp => rp.RolePatternDetails);
            }

            return query.FirstOrDefault(rp => rp.Id == id);
        }

        public List<RolePatternDetails> GetRolePatternDetails(int rolePatternId)
        {
            return _context.RolePatternDetails_Tbl
                .Where(rpd => rpd.RolePatternId == rolePatternId && rpd.IsActive)
                .OrderBy(rpd => rpd.ControllerName)
                .ThenBy(rpd => rpd.ActionName)
                .ToList();
        }

        public bool CreateRolePattern(RolePattern rolePattern)
        {
            try
            {
                rolePattern.CreateDate = DateTime.Now;
                _context.RolePattern_Tbl.Add(rolePattern);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateRolePattern(RolePattern rolePattern)
        {
            try
            {
                rolePattern.LastUpdateDate = DateTime.Now;
                _context.RolePattern_Tbl.Update(rolePattern);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteRolePattern(int id)
        {
            try
            {
                var rolePattern = _context.RolePattern_Tbl.Find(id);
                if (rolePattern != null && !rolePattern.IsSystemPattern)
                {
                    // حذف منطقی
                    rolePattern.IsActive = false;
                    rolePattern.LastUpdateDate = DateTime.Now;
                    _context.RolePattern_Tbl.Update(rolePattern);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region User Role Pattern Management

        public List<UserRolePattern> GetUserRolePatterns(string userId)
        {
            return _context.UserRolePattern_Tbl
                .Include(urp => urp.RolePattern)
                .Include(urp => urp.AssignedByUser)
                .Where(urp => urp.UserId == userId && urp.IsActive)
                .OrderBy(urp => urp.AssignDate)
                .ToList();
        }

        public bool AssignRolePatternToUser(UserRolePattern userRolePattern)
        {
            try
            {
                userRolePattern.AssignDate = DateTime.Now;
                _context.UserRolePattern_Tbl.Add(userRolePattern);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveRolePatternFromUser(int userRolePatternId)
        {
            try
            {
                var userRolePattern = _context.UserRolePattern_Tbl.Find(userRolePatternId);
                if (userRolePattern != null)
                {
                    userRolePattern.IsActive = false;
                    _context.UserRolePattern_Tbl.Update(userRolePattern);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateUserRolePattern(UserRolePattern userRolePattern)
        {
            try
            {
                _context.UserRolePattern_Tbl.Update(userRolePattern);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Permission Checking

        public bool HasPermission(string userId, string controller, string action, byte actionType)
        {
            try
            {
                // دریافت الگوهای نقش فعال کاربر
                var userRolePatterns = _context.UserRolePattern_Tbl
                    .Include(urp => urp.RolePattern)
                    .ThenInclude(rp => rp.RolePatternDetails)
                    .Where(urp => urp.UserId == userId && 
                                  urp.IsActive && 
                                  urp.RolePattern.IsActive &&
                                  (urp.StartDate == null || urp.StartDate <= DateTime.Now) &&
                                  (urp.EndDate == null || urp.EndDate >= DateTime.Now))
                    .ToList();

                // بررسی دسترسی در هر الگوی نقش
                foreach (var userRolePattern in userRolePatterns)
                {
                    var details = userRolePattern.RolePattern.RolePatternDetails
                        .Where(rpd => rpd.IsActive && 
                                     (rpd.ControllerName == "*" || rpd.ControllerName == controller) &&
                                     (rpd.ActionName == "*" || rpd.ActionName == action))
                        .ToList();

                    foreach (var detail in details)
                    {
                        bool hasAccess = actionType switch
                        {
                            0 => detail.CanRead,
                            1 => detail.CanCreate,
                            2 => detail.CanEdit,
                            3 => detail.CanDelete,
                            4 => detail.CanApprove,
                            _ => false
                        };

                        if (hasAccess)
                            return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetUserPermissions(string userId)
        {
            var permissions = new List<string>();

            var userRolePatterns = _context.UserRolePattern_Tbl
                .Include(urp => urp.RolePattern)
                .ThenInclude(rp => rp.RolePatternDetails)
                .Where(urp => urp.UserId == userId && 
                              urp.IsActive && 
                              urp.RolePattern.IsActive &&
                              (urp.StartDate == null || urp.StartDate <= DateTime.Now) &&
                              (urp.EndDate == null || urp.EndDate >= DateTime.Now))
                .ToList();

            foreach (var userRolePattern in userRolePatterns)
            {
                foreach (var detail in userRolePattern.RolePattern.RolePatternDetails.Where(rpd => rpd.IsActive))
                {
                    var basePermission = $"{detail.ControllerName}.{detail.ActionName}";
                    
                    if (detail.CanRead) permissions.Add($"{basePermission}.Read");
                    if (detail.CanCreate) permissions.Add($"{basePermission}.Create");
                    if (detail.CanEdit) permissions.Add($"{basePermission}.Edit");
                    if (detail.CanDelete) permissions.Add($"{basePermission}.Delete");
                    if (detail.CanApprove) permissions.Add($"{basePermission}.Approve");
                }
            }

            return permissions.Distinct().ToList();
        }

        #endregion

        #region Permission Logging

        public List<PermissionLog> GetPermissionLogs(string userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.PermissionLog_Tbl
                .Include(pl => pl.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(pl => pl.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(pl => pl.ActionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pl => pl.ActionDate <= toDate.Value);

            return query.OrderByDescending(pl => pl.ActionDate)
                       .Take(1000) // محدود کردن نتایج
                       .ToList();
        }

        public void LogPermissionAccess(PermissionLog log)
        {
            try
            {
                log.ActionDate = DateTime.Now;
                _context.PermissionLog_Tbl.Add(log);
                _context.SaveChanges();
            }
            catch
            {
                // لاگ خطا
            }
        }

        #endregion

        #region Helper Methods

        public List<AppUsers> GetUsersWithoutRolePattern()
        {
            var usersWithRolePattern = _context.UserRolePattern_Tbl
                .Where(urp => urp.IsActive)
                .Select(urp => urp.UserId)
                .Distinct();

            return _context.Users
                .Where(u => u.IsActive && !u.IsRemoveUser && !usersWithRolePattern.Contains(u.Id))
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();
        }

        public List<AppUsers> GetUsersByRolePattern(int rolePatternId)
        {
            return _context.UserRolePattern_Tbl
                .Include(urp => urp.User)
                .Where(urp => urp.RolePatternId == rolePatternId && urp.IsActive)
                .Select(urp => urp.User)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();
        }

        public Dictionary<string, string> GetControllerActions()
        {
            // این متد باید کنترلرها و اکشن‌های موجود در سیستم را برگرداند
            // می‌توان از Reflection استفاده کرد یا لیست ثابتی تعریف کرد
            return new Dictionary<string, string>
            {
                {"Task", "تسک‌ها"},
                {"Task.Index", "لیست تسک‌ها"},
                {"Task.Create", "ایجاد تسک"},
                {"Task.Edit", "ویرایش تسک"},
                {"Task.Delete", "حذف تسک"},
                {"Task.Details", "جزئیات تسک"},
                {"Task.MyTasks", "تسک‌های من"},
                {"CRM", "مدیریت ارتباط با مشتری"},
                {"CRM.Index", "لیست تعاملات CRM"},
                {"CRM.Create", "ایجاد تعامل CRM"},
                {"CRM.Edit", "ویرایش تعامل CRM"},
                {"CRM.Delete", "حذف تعامل CRM"},
                {"Stakeholder", "طرف‌های حساب"},
                {"Stakeholder.Index", "لیست طرف‌های حساب"},
                {"Stakeholder.Create", "ایجاد طرف حساب"},
                {"Stakeholder.Edit", "ویرایش طرف حساب"},
                {"Stakeholder.Delete", "حذف طرف حساب"},
                {"Contract", "قراردادها"},
                {"Contract.Index", "لیست قراردادها"},
                {"Contract.Create", "ایجاد قرارداد"},
                {"Contract.Edit", "ویرایش قرارداد"},
                {"Contract.Delete", "حذف قرارداد"},
                {"User", "مدیریت کاربران"},
                {"Role", "مدیریت نقش‌ها"},
                {"RolePattern", "مدیریت الگوهای نقش"}
            };
        }

        #endregion
    }
}
