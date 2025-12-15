using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
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

        #region User Role Info - NEW

        /// <summary>
        /// ⭐⭐⭐ دریافت اطلاعات نقش کاربر در تسک و سلسله مراتب دسترسی
        /// </summary>
        public async Task<UserRoleInfoViewModel> GetUserRoleInfoAsync(int taskId, string userId)
        {
            var result = new UserRoleInfoViewModel();

            // بررسی Admin سیستم
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.IsAdmin == true)
            {
                result.IsSystemAdmin = true;
                result.PrimaryRole = "مدیر سیستم";
                result.RoleDescription = "شما مدیر سیستم هستید و به تمام تسک‌ها دسترسی دارید";
                return result;
            }

            // دریافت تسک با اطلاعات تیم
            var task = await _context.Tasks_Tbl
                .Include(t => t.Team)
                    .ThenInclude(t => t.Manager)
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .Include(t => t.TaskAssignments)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return result;

            // ⭐⭐⭐ دریافت assignment کاربر در این تسک
            var userAssignment = task.TaskAssignments?
                .FirstOrDefault(a => a.AssignedUserId == userId);

            // ⭐⭐⭐ تیم صحیح:
            // - اگر کاربر منتصب است: تیمی که در آن منتصب شده (AssignedInTeamId)
            // - اگر کاربر منتسب نیست (مثلاً فقط سازنده): تیم اصلی تسک (task.TeamId)
            int? relevantTeamId = userAssignment?.AssignedInTeamId ?? task.TeamId;

            // اطلاعات تیم تسک
            if (task.Team != null)
            {
                result.TaskTeamId = task.TeamId;
                result.TaskTeamName = task.Team.Title;
                result.TaskBranchName = task.Team.Branch?.Name;
            }

            // ⭐⭐⭐ 1. اول بررسی مدیر تیم (Manager) - بالاترین اولویت بعد از Admin
            if (relevantTeamId.HasValue)
            {
                string? teamManagerId = null;
                string? teamTitle = null;
                
                if (userAssignment?.AssignedInTeamId != null)
                {
                    var assignedTeam = await _context.Team_Tbl
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == userAssignment.AssignedInTeamId);
                    teamManagerId = assignedTeam?.ManagerUserId;
                    teamTitle = assignedTeam?.Title;
                }
                else
                {
                    teamManagerId = task.Team?.ManagerUserId;
                    teamTitle = task.Team?.Title;
                }

                if (teamManagerId == userId)
                {
                    result.PrimaryRole = "مدیر تیم";
                    result.RoleDescription = $"شما مدیر تیم «{teamTitle}» هستید";
                    
                    // اگر سازنده هم هست، اضافه کن
                    if (task.CreatorUserId == userId)
                    {
                        result.RoleDescription += " و همچنین سازنده این تسک هستید";
                    }
                    
                    result.UserTeamId = relevantTeamId;
                    result.UserTeamName = teamTitle;
                    return result;
                }
            }

            // 2. بررسی سازنده
            if (task.CreatorUserId == userId)
            {
                result.PrimaryRole = "سازنده";
                result.RoleDescription = "شما سازنده این تسک هستید";
                return result;
            }

            // 3. ⭐⭐⭐ بررسی عضو تسک (هر نوع Assignment)
            // AssignmentType: 0=اجراکننده، 1=رونوشت، 2=ناظر
            if (userAssignment != null)
            {
                // تعیین نوع عضویت بر اساس AssignmentType
                string roleTitle = userAssignment.AssignmentType switch
                {
                    0 => "عضو تسک",
                    1 => "رونوشت (سازنده)",
                    2 => "ناظر تسک",
                    _ => "عضو تسک"
                };
                
                string roleDesc = userAssignment.AssignmentType switch
                {
                    0 => "این تسک به شما محول شده است",
                    1 => "شما به عنوان رونوشت در این تسک هستید",
                    2 => "شما ناظر این تسک هستید",
                    _ => "این تسک به شما مرتبط است"
                };

                result.PrimaryRole = roleTitle;
                result.RoleDescription = roleDesc;
                
                // پیدا کردن تیمی که assignment در آن انجام شده
                if (userAssignment.AssignedInTeamId != null)
                {
                    var assignedTeam = await _context.Team_Tbl
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == userAssignment.AssignedInTeamId);
                    if (assignedTeam != null)
                    {
                        result.RoleDescription += $" (در قالب تیم «{assignedTeam.Title}»)";
                    }
                }
                return result;
            }

            // 4. ⭐⭐⭐ بررسی سرپرست (Supervisor) - از تیم صحیح
            if (relevantTeamId.HasValue)
            {
                var isSupervisor = await _context.TeamMember_Tbl
                    .AnyAsync(tm => tm.TeamId == relevantTeamId.Value &&
                                   tm.UserId == userId &&
                                   tm.MembershipType == 1 &&
                                   tm.IsActive);
                if (isSupervisor)
                {
                    var teamTitle = userAssignment?.AssignedInTeamId != null
                        ? (await _context.Team_Tbl.FindAsync(userAssignment.AssignedInTeamId))?.Title
                        : task.Team?.Title;
                    
                    result.PrimaryRole = "سرپرست";
                    result.RoleDescription = $"شما سرپرست تیم «{teamTitle}» هستید";
                    return result;
                }
            }

            // 5. بررسی رونوشت (Carbon Copy)
            var isCarbonCopy = await _context.TaskCarbonCopy_Tbl
                .AnyAsync(cc => cc.TaskId == taskId && cc.UserId == userId && cc.IsActive);
            if (isCarbonCopy)
            {
                result.PrimaryRole = "ناظر (رونوشت)";
                result.RoleDescription = "شما به عنوان ناظر (رونوشت) به این تسک دسترسی دارید";
                return result;
            }

            // 6. ⭐⭐⭐ بررسی مدیر بالاسری (Hierarchy Manager) - از تیم صحیح
            if (relevantTeamId.HasValue)
            {
                var hierarchyInfo = await GetHierarchyInfoAsync(relevantTeamId.Value, userId);
                if (hierarchyInfo.IsHierarchyManager)
                {
                    result.PrimaryRole = "مدیر بالاسری";
                    result.IsHierarchyAccess = true;
                    result.HierarchyChain = hierarchyInfo.HierarchyChain;
                    result.UserTeamId = hierarchyInfo.UserTeamId;
                    result.UserTeamName = hierarchyInfo.UserTeamName;

                    // ساخت توضیح سلسله مراتب
                    var taskTeamTitle = task.Team?.Title ?? "نامشخص";
                    result.RoleDescription = $"شما مدیر تیم «{hierarchyInfo.UserTeamName}» هستید";
                    result.HierarchyExplanation = BuildHierarchyExplanation(hierarchyInfo, taskTeamTitle);
                }
            }

            return result;
        }

        /// <summary>
        /// ⭐ متد کمکی برای دریافت نام کامل کاربر
        /// </summary>
        private string GetUserFullName(MahERP.DataModelLayer.Entities.AcControl.AppUsers? user)
        {
            if (user == null) return "نامشخص";
            
            var firstName = user.FirstName?.Trim() ?? "";
            var lastName = user.LastName?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return user.UserName ?? "نامشخص";
            
            return $"{firstName} {lastName}".Trim();
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت اطلاعات سلسله مراتب
        /// </summary>
        private async Task<HierarchyInfoResult> GetHierarchyInfoAsync(int taskTeamId, string userId)
        {
            var result = new HierarchyInfoResult();

            // دریافت تیم تسک
            var taskTeam = await _context.Team_Tbl
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.Id == taskTeamId && t.IsActive);

            if (taskTeam == null) return result;

            // اگر مدیر همین تیم است → نه HierarchyManager
            if (taskTeam.ManagerUserId == userId)
                return result;

            // ساخت زنجیره سلسله مراتب
            result.HierarchyChain.Add(new TeamHierarchyItem
            {
                TeamId = taskTeam.Id,
                TeamName = taskTeam.Title,
                ManagerName = GetUserFullName(taskTeam.Manager),
                ManagerUserId = taskTeam.ManagerUserId,
                Level = 0,
                IsCurrentUserManager = false
            });

            var currentTeamId = taskTeam.ParentTeamId;
            int level = 1;

            while (currentTeamId.HasValue)
            {
                var parentTeam = await _context.Team_Tbl
                    .Include(t => t.Manager)
                    .FirstOrDefaultAsync(t => t.Id == currentTeamId && t.IsActive);

                if (parentTeam == null) break;

                var isCurrentUserManager = parentTeam.ManagerUserId == userId;

                result.HierarchyChain.Add(new TeamHierarchyItem
                {
                    TeamId = parentTeam.Id,
                    TeamName = parentTeam.Title,
                    ManagerName = GetUserFullName(parentTeam.Manager),
                    ManagerUserId = parentTeam.ManagerUserId,
                    Level = level,
                    IsCurrentUserManager = isCurrentUserManager
                });

                // ⭐ اگر مدیر این تیم است → پیدا شد
                if (isCurrentUserManager)
                {
                    result.IsHierarchyManager = true;
                    result.UserTeamId = parentTeam.Id;
                    result.UserTeamName = parentTeam.Title;
                    break;
                }

                currentTeamId = parentTeam.ParentTeamId;
                level++;
            }

            return result;
        }

        /// <summary>
        /// ساخت متن توضیح سلسله مراتب
        /// </summary>
        private string BuildHierarchyExplanation(HierarchyInfoResult hierarchyInfo, string? taskTeamName)
        {
            if (!hierarchyInfo.IsHierarchyManager || !hierarchyInfo.HierarchyChain.Any())
                return "";

            var taskTeam = hierarchyInfo.HierarchyChain.FirstOrDefault(h => h.Level == 0);
            var userTeam = hierarchyInfo.HierarchyChain.FirstOrDefault(h => h.IsCurrentUserManager);

            if (taskTeam == null || userTeam == null)
                return "";

            // محاسبه فاصله سطوح
            var levelDiff = userTeam.Level;
            var levelText = levelDiff switch
            {
                1 => "زیرمجموعه مستقیم",
                2 => "دو سطح پایین‌تر",
                3 => "سه سطح پایین‌تر",
                _ => $"{levelDiff} سطح پایین‌تر"
            };

            // ساخت زنجیره تیم‌ها
            var chainTeams = hierarchyInfo.HierarchyChain
                .OrderByDescending(h => h.Level)
                .Select(h => h.TeamName)
                .ToList();

            var chainText = string.Join(" ← ", chainTeams);

            return $"تیم «{taskTeam.TeamName}» {levelText} تیم «{userTeam.TeamName}» است.\n" +
                   $"زنجیره سلسله مراتب: {chainText}";
        }

        #endregion
    }

    /// <summary>
    /// نتیجه بررسی سلسله مراتب
    /// </summary>
    internal class HierarchyInfoResult
    {
        public bool IsHierarchyManager { get; set; }
        public int? UserTeamId { get; set; }
        public string? UserTeamName { get; set; }
        public List<TeamHierarchyItem> HierarchyChain { get; set; } = new();
    }
}
