using System;
using System.Collections.Generic;
using System.Text;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Services
{
    public interface IRoleRepository
    {
        // متدهای موجود
        string GetRoleId(string userId);
        string GetRolePatternId(int RolePatternID);

        // متدهای جدید
        List<RolePattern> GetAllRolePatterns(bool includeInactive = false);
        RolePattern GetRolePatternById(int id, bool includeDetails = false);
        List<RolePatternDetails> GetRolePatternDetails(int rolePatternId);
        bool CreateRolePattern(RolePattern rolePattern);
        bool UpdateRolePattern(RolePattern rolePattern);
        bool DeleteRolePattern(int id);

        List<UserRolePattern> GetUserRolePatterns(string userId);
        bool AssignRolePatternToUser(UserRolePattern userRolePattern);
        bool RemoveRolePatternFromUser(int userRolePatternId);
        bool UpdateUserRolePattern(UserRolePattern userRolePattern);

        bool HasPermission(string userId, string controller, string action, byte actionType);
        List<string> GetUserPermissions(string userId);
        
        List<PermissionLog> GetPermissionLogs(string userId = null, DateTime? fromDate = null, DateTime? toDate = null);
        void LogPermissionAccess(PermissionLog log);

        // متدهای کمکی
        List<AppUsers> GetUsersWithoutRolePattern();
        List<AppUsers> GetUsersByRolePattern(int rolePatternId);
        Dictionary<string, string> GetControllerActions();
    }
}
