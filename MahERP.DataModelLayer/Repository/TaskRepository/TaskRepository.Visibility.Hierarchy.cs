using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// ⭐⭐⭐ مدیریت دسترسی سلسله مراتبی (HierarchyManager)
    /// شامل: مدیر بالاسری، مدیر تیم، زیرتیم‌ها، تنظیمات شعبه
    /// </summary>
    public partial class TaskRepository
    {
        #region Hierarchy Manager - NEW

        /// <summary>
        /// ⭐⭐⭐ بررسی اینکه آیا کاربر مدیر بالاسری (Hierarchy Manager) تسک است
        /// </summary>
        public async Task<bool> IsUserHierarchyManagerOfTaskAsync(string userId, int taskTeamId)
        {
            // دریافت تیم تسک
            var taskTeam = await _context.Team_Tbl
                .FirstOrDefaultAsync(t => t.Id == taskTeamId && t.IsActive);
            
            if (taskTeam == null) return false;

            // اگر مدیر همین تیم است → Manager (نه HierarchyManager)
            if (taskTeam.ManagerUserId == userId)
                return false;

            // بررسی تیم‌های والد
            var currentTeamId = taskTeam.ParentTeamId;
            
            while (currentTeamId.HasValue)
            {
                var parentTeam = await _context.Team_Tbl
                    .FirstOrDefaultAsync(t => t.Id == currentTeamId && t.IsActive);
                
                if (parentTeam == null) break;
                
                // ⭐ اگر مدیر تیم والد است → HierarchyManager
                if (parentTeam.ManagerUserId == userId)
                    return true;
                
                currentTeamId = parentTeam.ParentTeamId;
            }
            
            return false;
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های قابل مشاهده در سلسله مراتب (با استفاده از تنظیمات شعبه)
        /// </summary>
        public async Task<List<int>> GetHierarchyVisibleTasksAsync(string userId, int branchId)
        {
            var visibleTaskIds = new HashSet<int>();

            // ⭐ 1. تسک‌های تیم‌های مستقیم (Manager)
            var directManagedTeams = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && 
                           t.BranchId == branchId && 
                           t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            if (directManagedTeams.Any())
            {
                var directTasks = await _context.Tasks_Tbl
                    .Where(t => t.TeamId.HasValue &&
                               directManagedTeams.Contains(t.TeamId.Value) &&
                               !t.IsDeleted &&
                               !t.IsPrivate)
                    .Select(t => t.Id)
                    .ToListAsync();
                
                visibleTaskIds.UnionWith(directTasks);
            }

            // ⭐⭐⭐ 2. تسک‌های زیرتیم‌ها (HierarchyManager) - بر اساس تنظیمات
            var hierarchyTasks = await GetHierarchySubTeamTasksAsync(userId, branchId);
            visibleTaskIds.UnionWith(hierarchyTasks);

            return visibleTaskIds.ToList();
        }

        /// <summary>
        /// ⭐ دریافت تسک‌های زیرتیم‌ها بر اساس تنظیمات
        /// </summary>
        private async Task<List<int>> GetHierarchySubTeamTasksAsync(string userId, int branchId)
        {
            var visibleTaskIds = new HashSet<int>();
            
            // استفاده از متد Filtering که قبلاً تعریف شده
            var settings = await GetManagerVisibilitySettingsAsync(userId, branchId);

            if (settings != null)
            {
                if (settings.ShowAllSubTeamsByDefault)
                {
                    // نمایش همه زیرتیم‌ها
                    var allSubTeamTasks = await GetAllSubTeamTasksAsync(userId, branchId);
                    
                    if (settings.MaxTasksToShow > 0)
                    {
                        allSubTeamTasks = allSubTeamTasks.Take(settings.MaxTasksToShow).ToList();
                    }
                    
                    visibleTaskIds.UnionWith(allSubTeamTasks);
                }
                else if (!string.IsNullOrEmpty(settings.DefaultVisibleTeamIds))
                {
                    // نمایش تیم‌های خاص
                    var teamIds = settings.GetVisibleTeamIds();
                    
                    if (teamIds.Any())
                    {
                        var specificTeamTasks = await _context.Tasks_Tbl
                            .Where(t => t.TeamId.HasValue &&
                                       teamIds.Contains(t.TeamId.Value) &&
                                       t.BranchId == branchId &&
                                       !t.IsDeleted &&
                                       !t.IsPrivate)
                            .Select(t => t.Id)
                            .ToListAsync();
                        
                        visibleTaskIds.UnionWith(specificTeamTasks);
                    }
                }
            }
            
            return visibleTaskIds.ToList();
        }

        /// <summary>
        /// ⭐ دریافت تمام تسک‌های زیرتیم‌ها (برای HierarchyManager)
        /// </summary>
        private async Task<List<int>> GetAllSubTeamTasksAsync(string userId, int branchId)
        {
            var allTaskIds = new HashSet<int>();

            // تیم‌های مدیریت شده
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && 
                           t.BranchId == branchId && 
                           t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var teamId in managedTeamIds)
            {
                // تسک‌های زیرتیم‌ها (بازگشتی)
                var subTeamIds = await GetAllSubTeamIdsAsync(teamId);
                
                if (subTeamIds.Any())
                {
                    var subTeamTasks = await _context.Tasks_Tbl
                        .Where(t => t.TeamId.HasValue &&
                                   subTeamIds.Contains(t.TeamId.Value) &&
                                   !t.IsDeleted &&
                                   !t.IsPrivate)
                        .Select(t => t.Id)
                        .ToListAsync();
                    
                    allTaskIds.UnionWith(subTeamTasks);
                }
            }

            return allTaskIds.ToList();
        }

        /// <summary>
        /// دریافت تمام شناسه زیرتیم‌ها (بازگشتی)
        /// </summary>
        public async Task<List<int>> GetAllSubTeamIdsAsync(int parentTeamId)
        {
            var subTeamIds = new List<int>();

            var directSubTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            subTeamIds.AddRange(directSubTeams);

            // بازگشتی
            foreach (var subTeamId in directSubTeams)
            {
                var nestedSubTeams = await GetAllSubTeamIdsAsync(subTeamId);
                subTeamIds.AddRange(nestedSubTeams);
            }

            return subTeamIds;
        }

        #endregion

        #region Team Management

        /// <summary>
        /// بررسی اینکه آیا کاربر مدیر تیم است
        /// </summary>
        public async Task<bool> IsUserTeamManagerAsync(string userId, int teamId)
        {
            return await _context.Team_Tbl
                .AnyAsync(t => t.Id == teamId && t.ManagerUserId == userId && t.IsActive);
        }

        /// <summary>
        /// دریافت تسک‌های تیم‌های تحت مدیریت مستقیم (بدون زیرتیم)
        /// </summary>
        public async Task<List<int>> GetManagedTeamTasksAsync(string userId, int? branchId = null)
        {
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .Select(t => t.Id)
                .ToListAsync();

            if (!managedTeamIds.Any()) return new List<int>();

            return await _context.Tasks_Tbl
                .Where(t => t.TeamId.HasValue &&
                           managedTeamIds.Contains(t.TeamId.Value) &&
                           !t.IsDeleted &&
                           !t.IsPrivate)
                .Select(t => t.Id)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت گروه‌بندی شده
        /// </summary>
        public async Task<Dictionary<int, List<int>>> GetSubTeamTasksGroupedAsync(string userId, int? branchId = null)
        {
            var result = new Dictionary<int, List<int>>();

            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .Select(t => t.Id)
                .ToListAsync();

            if (!managedTeamIds.Any()) return result;

            foreach (var teamId in managedTeamIds)
            {
                var subTeamIds = await GetAllSubTeamIdsAsync(teamId);
                
                if (subTeamIds.Any())
                {
                    var subTeamTasks = await _context.Tasks_Tbl
                        .Where(t => t.TeamId.HasValue &&
                                   subTeamIds.Contains(t.TeamId.Value) &&
                                   !t.IsDeleted &&
                                   !t.IsPrivate)
                        .Select(t => t.Id)
                        .ToListAsync();

                    if (subTeamTasks.Any())
                    {
                        result[teamId] = subTeamTasks;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
