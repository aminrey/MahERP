using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// Repository مدیریت قدرت مشاهده تسک‌ها بر اساس ساختار سازمانی
    /// </summary>
    public class TaskVisibilityRepository : ITaskVisibilityRepository
    {
        private readonly AppDbContext _context;

        public TaskVisibilityRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Core Visibility Logic

        /// <summary>
        /// بررسی اینکه آیا کاربر می‌تواند تسک خاصی را مشاهده کند
        /// </summary>
        public async Task<bool> CanUserViewTaskAsync(string userId, int taskId)
        {
            try
            {
                var currentTime = DateTime.Now;
                
                // دریافت تسک
                var task = await _context.Tasks_Tbl.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null || task.IsDeleted) return false;

                // ⭐ بررسی تسک خصوصی
                if (task.IsPrivate || task.VisibilityLevel == 1)
                {
                    // فقط سازنده و افراد منتصب شده می‌توانند ببینند
                    if (task.CreatorUserId == userId) return true;
                    
                    var isAssigned = await _context.TaskAssignment_Tbl
                        .AnyAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);
                    
                    return isAssigned;
                }

                // برای تسک‌های عمومی، ادامه منطق قبلی
                // 1. بررسی مجوزهای مستقیم TaskViewer
                var directPermission = await _context.TaskViewer_Tbl
                    .AnyAsync(tv => tv.UserId == userId &&
                                    tv.TaskId == taskId &&
                                    tv.IsActive &&
                                    (tv.StartDate == null || tv.StartDate <= currentTime) &&
                                    (tv.EndDate == null || tv.EndDate > currentTime));

                if (directPermission) return true;

                // 2. بررسی مالکیت تسک
                if (task.CreatorUserId == userId) return true;

                // 3. بررسی انتساب به تسک
                var isAssignedPublic = await _context.TaskAssignment_Tbl
                    .AnyAsync(ta => ta.TaskId == taskId &&
                                    ta.AssignedUserId == userId);

                if (isAssignedPublic) return true;

                // 4. بررسی مدیریت تیم
                if (task.TeamId.HasValue)
                {
                    var isTeamManager = await IsUserTeamManagerAsync(userId, task.TeamId.Value);
                    if (isTeamManager) return true;
                }

                // 5. بررسی عضویت در تیم و قدرت سمت
                var canViewBasedOnPosition = await CanViewBasedOnPositionAsync(userId, task);
                if (canViewBasedOnPosition) return true;

                // 6. بررسی مجوزهای خاص (تبصره‌ها)
                var hasSpecialPermission = await HasSpecialPermissionAsync(userId, task);
                if (hasSpecialPermission) return true;

                // 7. بررسی سطح عمومی بودن تسک
                if (task.VisibilityLevel >= 3) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// دریافت لیست شناسه تسک‌هایی که کاربر می‌تواند مشاهده کند - اصلاح شده برای چند شعبه
        /// </summary>
        public async Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId = null, int? teamId = null)
        {
            var visibleTaskIds = new HashSet<int>();
            var currentTime = DateTime.Now;

            // ⭐⭐⭐ 0. دریافت همه شعبه‌های کاربر (اگر مشخص نشده)
            List<int> userBranchIds;
            if (branchId.HasValue)
            {
                userBranchIds = new List<int> { branchId.Value };
            }
            else
            {
                // ⭐ اصلاح شده: دریافت همه شعبه‌ها
                userBranchIds = await _context.BranchUser_Tbl
                    .Where(bu => bu.UserId == userId && bu.IsActive)
                    .Select(bu => bu.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!userBranchIds.Any())
                {
                    userBranchIds = new List<int> { 1 }; // شعبه پیش‌فرض
                }
            }

            // ⭐⭐⭐ 1. همه تسک‌های غیرخصوصی همه شعبه‌های کاربر
            var branchTasks = await _context.Tasks_Tbl
                .Where(t => userBranchIds.Contains(t.BranchId ?? 0) &&
                           !t.IsDeleted &&
                           !t.IsPrivate &&
                           t.VisibilityLevel >= 2) // سطح تیمی یا بالاتر
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(branchTasks);

            // ⭐ 2. تسک‌های خصوصی که کاربر دسترسی دارد
            var privateTasksCreated = await _context.Tasks_Tbl
                .Where(t => (t.IsPrivate || t.VisibilityLevel == 1) &&
                            t.CreatorUserId == userId &&
                            !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(privateTasksCreated);

            var privateTasksAssigned = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            (ta.Task.IsPrivate || ta.Task.VisibilityLevel == 1))
                .Select(ta => ta.TaskId)
                .ToListAsync();
            visibleTaskIds.UnionWith(privateTasksAssigned);

            // 3. تسک‌های مالکیت خود کاربر (غیر خصوصی)
            var ownTasks = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId &&
                            !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.VisibilityLevel != 1)
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(ownTasks);

            // 4. تسک‌های منتصب شده (غیر خصوصی)
            var assignedTasks = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            !ta.Task.IsPrivate &&
                            ta.Task.VisibilityLevel != 1)
                .Select(ta => ta.TaskId)
                .ToListAsync();
            visibleTaskIds.UnionWith(assignedTasks);

            // 5. تسک‌های با مجوز مستقیم (غیر خصوصی)
            var directPermissionTasks = await _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId &&
                            tv.TaskId > 0 &&
                            tv.IsActive &&
                            (tv.StartDate == null || tv.StartDate <= currentTime) &&
                            (tv.EndDate == null || tv.EndDate > currentTime) &&
                            !tv.Task.IsPrivate &&
                            tv.Task.VisibilityLevel != 1)
                .Select(tv => tv.TaskId)
                .ToListAsync();
            visibleTaskIds.UnionWith(directPermissionTasks);

            // 6. تسک‌های تیم‌های تحت مدیریت مستقیم (برای همه شعبه‌ها)
            foreach (var branchIdItem in userBranchIds)
            {
                var managedTeamTasks = await GetManagedTeamTasksAsync(userId, branchIdItem);
                visibleTaskIds.UnionWith(managedTeamTasks);
            }

            // 7. تسک‌های قابل مشاهده بر اساس سمت (برای همه شعبه‌ها)
            foreach (var branchIdItem in userBranchIds)
            {
                var positionBasedTasks = await GetPositionBasedVisibleTasksAsync(userId, branchIdItem, teamId);
                visibleTaskIds.UnionWith(positionBasedTasks);
            }

            // 8. تسک‌های با مجوز خاص (غیر خصوصی)
            var specialPermissionTasks = await GetSpecialPermissionTasksAsync(userId);
            visibleTaskIds.UnionWith(specialPermissionTasks);

            // 9. تسک‌های عمومی (غیر خصوصی)
            var publicTasks = await _context.Tasks_Tbl
                .Where(t => t.VisibilityLevel >= 3 &&
                            !t.IsDeleted &&
                            !t.IsPrivate)
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(publicTasks);

            Console.WriteLine($"✅ GetVisibleTaskIdsAsync: شعبه‌ها={string.Join(", ", userBranchIds)}, مجموع تسک‌ها={visibleTaskIds.Count}");

            return visibleTaskIds.ToList();
        }
        #endregion

        #region Position-Based Visibility

        /// <summary>
        /// بررسی قابلیت مشاهده بر اساس سمت در تیم
        /// </summary>
        public async Task<bool> CanViewBasedOnPositionAsync(string userId, Tasks task)
        {
            if (!task.TeamId.HasValue) return false;

            // دریافت عضویت کاربر در تیم
            var membership = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .FirstOrDefaultAsync(tm => tm.UserId == userId &&
                                          tm.TeamId == task.TeamId &&
                                          tm.IsActive);

            if (membership?.Position == null) return false;

            // بررسی سمت سازنده تسک
            var taskCreatorMembership = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .FirstOrDefaultAsync(tm => tm.UserId == task.CreatorUserId &&
                                          tm.TeamId == task.TeamId &&
                                          tm.IsActive);

            // اگر سازنده تسک عضو همین تیم باشد
            if (taskCreatorMembership?.Position != null)
            {
                // بررسی قدرت مشاهده زیردستان
                if (membership.Position.CanViewSubordinateTasks &&
                    membership.Position.PowerLevel < taskCreatorMembership.Position.PowerLevel)
                {
                    return true;
                }

                // بررسی قدرت مشاهده همسطح
                if (membership.Position.CanViewPeerTasks &&
                    membership.Position.PowerLevel == taskCreatorMembership.Position.PowerLevel)
                {
                    return true;
                }
            }

            // بررسی تسک‌های اعضای تیم (اگر visibility level اجازه دهد)
            if (task.VisibilityLevel >= 2 && membership.Position.CanViewSubordinateTasks)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده بر اساس سمت
        /// </summary>
        public async Task<List<int>> GetPositionBasedVisibleTasksAsync(string userId, int? branchId = null, int? teamId = null)
        {
            var visibleTasks = new List<int>();

            // دریافت عضویت‌های کاربر در تیم‌ها
            var memberships = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Where(tm => !branchId.HasValue || tm.Team.BranchId == branchId)
                .Where(tm => !teamId.HasValue || tm.TeamId == teamId)
                .ToListAsync();

            foreach (var membership in memberships.Where(m => m.Position != null))
            {
                // تسک‌های زیردستان (غیر خصوصی)
                if (membership.Position.CanViewSubordinateTasks)
                {
                    var subordinateTasks = await GetSubordinateTasksAsync(membership);
                    visibleTasks.AddRange(subordinateTasks);
                }

                // تسک‌های همسطح (غیر خصوصی)
                if (membership.Position.CanViewPeerTasks)
                {
                    var peerTasks = await GetPeerTasksAsync(membership);
                    visibleTasks.AddRange(peerTasks);
                }
            }

            return visibleTasks.Distinct().ToList();
        }
        /// <summary>
        /// دریافت تسک‌های زیردستان - اصلاح شده با بررسی شعبه
        /// </summary>
        private async Task<List<int>> GetSubordinateTasksAsync(TeamMember membership)
        {
            var subordinateUserIds = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel > membership.Position.PowerLevel)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!subordinateUserIds.Any()) return new List<int>();

            // ⭐⭐⭐ دریافت BranchId تیم
            var teamBranchId = await _context.Team_Tbl
                .Where(t => t.Id == membership.TeamId)
                .Select(t => t.BranchId)
                .FirstOrDefaultAsync();

            // ⭐⭐⭐ اصلاح شده: اضافه کردن فیلتر شعبه
            var taskIds = await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.VisibilityLevel != 1 &&
                            t.BranchId == teamBranchId && // ⭐⭐⭐ فیلتر شعبه
                            (
                                // تسک‌های ساخته شده توسط زیردستان
                                subordinateUserIds.Contains(t.CreatorUserId) ||

                                // تسک‌های منتصب شده به زیردستان
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    subordinateUserIds.Contains(ta.AssignedUserId)) ||

                                // تسک‌های مربوط به تیم در همان شعبه
                                (t.TeamId.HasValue && t.TeamId == membership.TeamId)
                            ))
                .Select(t => t.Id)
                .Distinct()
                .ToListAsync();

            return taskIds;
        }
        /// <summary>
        /// دریافت تسک‌های همسطح - اصلاح شده با بررسی شعبه
        /// </summary>
        private async Task<List<int>> GetPeerTasksAsync(TeamMember membership)
        {
            var peerUserIds = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.TeamId == membership.TeamId &&
                            tm.IsActive &&
                            tm.Position != null &&
                            tm.Position.PowerLevel == membership.Position.PowerLevel &&
                            tm.UserId != membership.UserId)
                .Select(tm => tm.UserId)
                .ToListAsync();

            if (!peerUserIds.Any()) return new List<int>();

            // ⭐⭐⭐ دریافت BranchId تیم
            var teamBranchId = await _context.Team_Tbl
                .Where(t => t.Id == membership.TeamId)
                .Select(t => t.BranchId)
                .FirstOrDefaultAsync();

            // ⭐⭐⭐ اصلاح شده: اضافه کردن فیلتر شعبه
            var taskIds = await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.VisibilityLevel != 1 &&
                            t.BranchId == teamBranchId && // ⭐⭐⭐ فیلتر شعبه
                            (
                                // تسک‌های ساخته شده توسط همسطحان
                                peerUserIds.Contains(t.CreatorUserId) ||

                                // تسک‌های منتصب شده به همسطحان
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    peerUserIds.Contains(ta.AssignedUserId)) ||

                                // تسک‌های مربوط به تیم در همان شعبه
                                (t.TeamId.HasValue && t.TeamId == membership.TeamId)
                            ))
                .Select(t => t.Id)
                .Distinct()
                .ToListAsync();

            return taskIds;
        }
        #endregion

        #region Team Management Visibility

        /// <summary>
        /// بررسی اینکه آیا کاربر مدیر تیم است
        /// </summary>
        public async Task<bool> IsUserTeamManagerAsync(string userId, int teamId)
        {
            return await _context.Team_Tbl
                .AnyAsync(t => t.Id == teamId && t.ManagerUserId == userId);
        }

        /// <summary>
        /// دریافت تسک‌های تیم‌های تحت مدیریت - بدون زیرتیم‌ها
        /// </summary>
        public async Task<List<int>> GetManagedTeamTasksAsync(string userId, int? branchId = null)
        {
            // دریافت تیم‌های تحت مدیریت مستقیم (بدون زیرتیم‌ها)
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .Select(t => t.Id)
                .ToListAsync();

            if (!managedTeamIds.Any()) return new List<int>();

            // ⭐ فقط تسک‌های تیم‌های مستقیم (بدون زیرتیم‌ها)
            return await _context.Tasks_Tbl
                .Where(t => t.TeamId.HasValue &&
                           managedTeamIds.Contains(t.TeamId.Value) &&
                           !t.IsDeleted &&
                           !t.IsPrivate &&
                           t.VisibilityLevel != 1)
                .Select(t => t.Id)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت گروه‌بندی شده - متد جدید
        /// </summary>
        public async Task<Dictionary<int, List<int>>> GetSubTeamTasksGroupedAsync(string userId, int? branchId = null)
        {
            var result = new Dictionary<int, List<int>>();

            // دریافت تیم‌های تحت مدیریت مستقیم
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .Select(t => t.Id)
                .ToListAsync();

            if (!managedTeamIds.Any()) return result;

            // برای هر تیم، زیرتیم‌ها و تسک‌هایشان را دریافت کن
            foreach (var teamId in managedTeamIds)
            {
                var subTeamIds = await GetAllSubTeamIdsAsync(teamId);
                
                if (subTeamIds.Any())
                {
                    var subTeamTasks = await _context.Tasks_Tbl
                        .Where(t => t.TeamId.HasValue &&
                                   subTeamIds.Contains(t.TeamId.Value) &&
                                   !t.IsDeleted &&
                                   !t.IsPrivate &&
                                   t.VisibilityLevel != 1)
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

        #region Special Permissions - استفاده از TaskViewPermission جدید

        /// <summary>
        /// بررسی مجوزهای خاص (تبصره‌ها) - بروزرسانی شده برای استفاده از TaskViewPermission
        /// </summary>
        public async Task<bool> HasSpecialPermissionAsync(string userId, Tasks task)
        {
            var currentTime = DateTime.Now;

            // استفاده از جدول TaskViewPermission جدید
            var specialPermissions = await _context.TaskViewPermission_Tbl
                .Include(tvp => tvp.TargetUser)
                .Include(tvp => tvp.TargetTeam)
                .Where(tvp => tvp.GranteeUserId == userId &&
                             tvp.IsActive &&
                             (tvp.StartDate == null || tvp.StartDate <= currentTime) &&
                             (tvp.EndDate == null || tvp.EndDate > currentTime))
                .ToListAsync();

            foreach (var permission in specialPermissions)
            {
                switch (permission.PermissionType)
                {
                    case 0: // مشاهده تسک‌های یک کاربر خاص
                        if (permission.TargetUserId == task.CreatorUserId)
                            return true;
                        break;

                    case 1: // مشاهده تسک‌های یک تیم خاص
                        if (permission.TargetTeamId.HasValue && task.TeamId == permission.TargetTeamId)
                            return true;
                        break;

                    case 2: // مشاهده تسک‌های تیم و زیرتیم‌ها
                        if (permission.TargetTeamId.HasValue)
                        {
                            var allSubTeamIds = await GetAllSubTeamIdsAsync(permission.TargetTeamId.Value);
                            allSubTeamIds.Add(permission.TargetTeamId.Value);

                            if (task.TeamId.HasValue && allSubTeamIds.Contains(task.TeamId.Value))
                                return true;
                        }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// دریافت تسک‌های با مجوز خاص - بروزرسانی شده برای استفاده از TaskViewPermission
        /// </summary>
        public async Task<List<int>> GetSpecialPermissionTasksAsync(string userId)
        {
            var currentTime = DateTime.Now;
            var visibleTasks = new List<int>();

            // استفاده از جدول TaskViewPermission جدید
            var specialPermissions = await _context.TaskViewPermission_Tbl
                .Include(tvp => tvp.TargetUser)
                .Include(tvp => tvp.TargetTeam)
                .Where(tvp => tvp.GranteeUserId == userId &&
                             tvp.IsActive &&
                             (tvp.StartDate == null || tvp.StartDate <= currentTime) &&
                             (tvp.EndDate == null || tvp.EndDate > currentTime))
                .ToListAsync();

            foreach (var permission in specialPermissions)
            {
                switch (permission.PermissionType)
                {
                    case 0: // مشاهده تسک‌های یک کاربر خاص
                        if (!string.IsNullOrEmpty(permission.TargetUserId))
                        {
                            // ⭐ فقط تسک‌های غیر خصوصی
                            var userTasks = await _context.Tasks_Tbl
                                .Where(t => t.CreatorUserId == permission.TargetUserId && 
                                            !t.IsDeleted &&
                                            !t.IsPrivate &&
                                            t.VisibilityLevel != 1)
                                .Select(t => t.Id)
                                .ToListAsync();
                            visibleTasks.AddRange(userTasks);
                        }
                        break;

                    case 1: // مشاهده تسک‌های یک تیم خاص
                        if (permission.TargetTeamId.HasValue)
                        {
                            // ⭐ فقط تسک‌های غیر خصوصی
                            var teamTasks = await _context.Tasks_Tbl
                                .Where(t => t.TeamId == permission.TargetTeamId && 
                                            !t.IsDeleted &&
                                            !t.IsPrivate &&
                                            t.VisibilityLevel != 1)
                                .Select(t => t.Id)
                                .ToListAsync();
                            visibleTasks.AddRange(teamTasks);
                        }
                        break;

                    case 2: // مشاهده تسک‌های تیم و زیرتیم‌ها
                        if (permission.TargetTeamId.HasValue)
                        {
                            var allSubTeamIds = await GetAllSubTeamIdsAsync(permission.TargetTeamId.Value);
                            allSubTeamIds.Add(permission.TargetTeamId.Value);

                            // ⭐ فقط تسک‌های غیر خصوصی
                            var hierarchyTasks = await _context.Tasks_Tbl
                                .Where(t => t.TeamId.HasValue &&
                                           allSubTeamIds.Contains(t.TeamId.Value) &&
                                           !t.IsDeleted &&
                                           !t.IsPrivate &&
                                           t.VisibilityLevel != 1)
                                .Select(t => t.Id)
                                .ToListAsync();
                            visibleTasks.AddRange(hierarchyTasks);
                        }
                        break;
                }
            }

            return visibleTasks.Distinct().ToList();
        }

        #endregion

        #region Chart Generation

        /// <summary>
        /// ایجاد چارت قدرت مشاهده تسک‌ها
        /// </summary>
        public async Task<TaskVisibilityChartViewModel> GenerateVisibilityChartAsync(int branchId)
        {
            var branch = await _context.Branch_Tbl.FindAsync(branchId);
            if (branch == null) return null;

            var chart = new TaskVisibilityChartViewModel
            {
                BranchId = branchId,
                BranchName = branch.Name
            };

            // ایجاد ساختار سلسله مراتبی تیم‌ها
            chart.TeamHierarchy = await BuildTeamHierarchyAsync(branchId);

            // دریافت مجوزهای خاص - استفاده از TaskViewPermission جدید
            chart.SpecialPermissions = await GetSpecialPermissionsAsync(branchId);

            // محاسبه آمار
            chart.Stats = await CalculateVisibilityStatsAsync(branchId);

            return chart;
        }

        /// <summary>
        /// محاسبه آمار چارت قدرت مشاهده - بروزرسانی شده
        /// </summary>
        public async Task<TaskVisibilityStatsViewModel> CalculateVisibilityStatsAsync(int branchId)
        {
            var stats = new TaskVisibilityStatsViewModel();

            stats.TotalTeams = await _context.Team_Tbl
                .CountAsync(t => t.BranchId == branchId && t.IsActive);

            stats.TotalMembers = await _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .CountAsync(tm => tm.Team.BranchId == branchId && tm.IsActive);

            stats.TotalPositions = await _context.TeamPosition_Tbl
                .Include(tp => tp.Team)
                .CountAsync(tp => tp.Team.BranchId == branchId && tp.IsActive);

            // استفاده از TaskViewPermission جدید برای محاسبه آمار
            var specialPermissions = await _context.TaskViewPermission_Tbl
                .Include(tvp => tvp.Team)
                .Where(tvp => tvp.Team != null && tvp.Team.BranchId == branchId)
                .ToListAsync();

            stats.TotalSpecialPermissions = specialPermissions.Count;
            stats.ActiveSpecialPermissions = specialPermissions.Count(sp => sp.IsActive && sp.IsValidAtTime(DateTime.Now));
            stats.ExpiredSpecialPermissions = specialPermissions.Count(sp => sp.EndDate.HasValue && sp.EndDate < DateTime.Now);

            // توزیع قدرت بر اساس سطح
            var powerLevelDistribution = await _context.TeamPosition_Tbl
                .Include(tp => tp.Team)
                .Where(tp => tp.Team.BranchId == branchId && tp.IsActive)
                .GroupBy(tp => tp.PowerLevel)
                .Select(g => new { PowerLevel = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PowerLevel, x => x.Count);

            stats.PowerLevelDistribution = powerLevelDistribution;

            return stats;
        }

        #endregion

        #region User Access Information

        /// <summary>
        /// تشخیص منابع دسترسی کاربر - بروزرسانی شده
        /// </summary>
        public async Task<List<string>> GetUserAccessSourcesAsync(string userId)
        {
            var sources = new List<string>();

            // بررسی مدیریت تیم
            var managedTeamsCount = await _context.Team_Tbl
                .CountAsync(t => t.ManagerUserId == userId && t.IsActive);
            if (managedTeamsCount > 0)
                sources.Add($"مدیر {managedTeamsCount} تیم");

            // بررسی عضویت در تیم با سمت
            var positionMemberships = await _context.TeamMember_Tbl
                .Include(tm => tm.Position)
                .Where(tm => tm.UserId == userId && tm.PositionId.HasValue && tm.IsActive)
                .ToListAsync();

            foreach (var membership in positionMemberships)
            {
                if (membership.Position.CanViewSubordinateTasks)
                    sources.Add($"مشاهده زیردستان در {membership.Position.Title}");
                if (membership.Position.CanViewPeerTasks)
                    sources.Add($"مشاهده همسطح در {membership.Position.Title}");
            }

            // بررسی مجوزهای خاص - استفاده از TaskViewPermission جدید
            var specialPermissionsCount = await _context.TaskViewPermission_Tbl
                .CountAsync(tvp => tvp.GranteeUserId == userId && tvp.IsActive);
            if (specialPermissionsCount > 0)
                sources.Add($"{specialPermissionsCount} مجوز خاص");

            return sources;
        }

        /// <summary>
        /// دریافت اطلاعات قدرت مشاهده اعضای یک سمت
        /// </summary>
        public async Task<List<MemberTaskVisibilityInfo>> GetPositionMembersAsync(int positionId)
        {
            var members = await _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Where(tm => tm.PositionId == positionId && tm.IsActive)
                .ToListAsync();

            var memberInfos = new List<MemberTaskVisibilityInfo>();

            foreach (var member in members)
            {
                var info = new MemberTaskVisibilityInfo
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    UserFullName = $"{member.User.FirstName} {member.User.LastName}"
                };

                // محاسبه تعداد تسک‌های قابل مشاهده
                var visibleTaskIds = await GetVisibleTaskIdsAsync(member.UserId);
                info.VisibleTasksCount = visibleTaskIds.Count;

                // تشخیص منابع دسترسی
                info.AccessSources = await GetUserAccessSourcesAsync(member.UserId);

                memberInfos.Add(info);
            }

            return memberInfos;
        }

        /// <summary>
        /// دریافت اعضای بدون سمت
        /// </summary>
        public async Task<List<MemberTaskVisibilityInfo>> GetMembersWithoutPositionAsync(int teamId)
        {
            var members = await _context.TeamMember_Tbl
                .Include(tm => tm.User)
                .Where(tm => tm.TeamId == teamId && !tm.PositionId.HasValue && tm.IsActive)
                .ToListAsync();

            var memberInfos = new List<MemberTaskVisibilityInfo>();

            foreach (var member in members)
            {
                var info = new MemberTaskVisibilityInfo
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    UserFullName = $"{member.User.FirstName} {member.User.LastName}"
                };

                var visibleTaskIds = await GetVisibleTaskIdsAsync(member.UserId);
                info.VisibleTasksCount = visibleTaskIds.Count;
                info.AccessSources = await GetUserAccessSourcesAsync(member.UserId);

                memberInfos.Add(info);
            }

            return memberInfos;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// ایجاد ساختار سلسله مراتبی تیم‌ها
        /// </summary>
        private async Task<List<TeamTaskVisibilityNode>> BuildTeamHierarchyAsync(int branchId)
        {
            var rootTeams = await _context.Team_Tbl
                .Where(t => t.BranchId == branchId && !t.ParentTeamId.HasValue && t.IsActive)
                .ToListAsync();

            var hierarchy = new List<TeamTaskVisibilityNode>();

            foreach (var team in rootTeams)
            {
                var node = await BuildTeamNodeAsync(team, 0);
                hierarchy.Add(node);
            }

            return hierarchy;
        }

        /// <summary>
        /// ایجاد گره تیم
        /// </summary>
        private async Task<TeamTaskVisibilityNode> BuildTeamNodeAsync(Team team, int level)
        {
            var node = new TeamTaskVisibilityNode
            {
                TeamId = team.Id,
                TeamTitle = team.Title,
                Level = level,
                ManagerUserId = team.ManagerUserId
            };

            // دریافت نام مدیر
            if (!string.IsNullOrEmpty(team.ManagerUserId))
            {
                var manager = await _context.Users.FindAsync(team.ManagerUserId);
                node.ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "نامشخص";
            }

            // دریافت سمت‌ها
            node.Positions = await GetPositionVisibilityInfoAsync(team.Id);

            // دریافت اعضای بدون سمت
            node.MembersWithoutPosition = await GetMembersWithoutPositionAsync(team.Id);

            // دریافت زیرتیم‌ها
            var subTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == team.Id && t.IsActive)
                .ToListAsync();

            foreach (var subTeam in subTeams)
            {
                var subNode = await BuildTeamNodeAsync(subTeam, level + 1);
                node.SubTeams.Add(subNode);
            }

            return node;
        }

        /// <summary>
        /// دریافت اطلاعات قدرت مشاهده سمت‌ها
        /// </summary>
        private async Task<List<PositionTaskVisibilityInfo>> GetPositionVisibilityInfoAsync(int teamId)
        {
            var positions = await _context.TeamPosition_Tbl
                .Where(p => p.TeamId == teamId && p.IsActive)
                .OrderBy(p => p.PowerLevel)
                .ToListAsync();

            var positionInfos = new List<PositionTaskVisibilityInfo>();

            foreach (var position in positions)
            {
                var info = new PositionTaskVisibilityInfo
                {
                    PositionId = position.Id,
                    PositionTitle = position.Title,
                    PowerLevel = position.PowerLevel,
                    CanViewSubordinateTasks = position.CanViewSubordinateTasks,
                    CanViewPeerTasks = position.CanViewPeerTasks
                };

                // دریافت اعضای این سمت
                info.Members = await GetPositionMembersAsync(position.Id);

                // محاسبه سمت‌های قابل مشاهده
                info.VisiblePositionIds = await CalculateVisiblePositionsAsync(position);

                positionInfos.Add(info);
            }

            return positionInfos;
        }

        /// <summary>
        /// محاسبه سمت‌های قابل مشاهده
        /// </summary>
        private async Task<List<int>> CalculateVisiblePositionsAsync(TeamPosition position)
        {
            var visiblePositions = new List<int>();

            if (position.CanViewSubordinateTasks)
            {
                var subordinatePositions = await _context.TeamPosition_Tbl
                    .Where(p => p.TeamId == position.TeamId &&
                               p.PowerLevel > position.PowerLevel &&
                               p.IsActive)
                    .Select(p => p.Id)
                    .ToListAsync();
                visiblePositions.AddRange(subordinatePositions);
            }

            if (position.CanViewPeerTasks)
            {
                var peerPositions = await _context.TeamPosition_Tbl
                    .Where(p => p.TeamId == position.TeamId &&
                               p.PowerLevel == position.PowerLevel &&
                               p.Id != position.Id &&
                               p.IsActive)
                    .Select(p => p.Id)
                    .ToListAsync();
                visiblePositions.AddRange(peerPositions);
            }

            return visiblePositions;
        }

        /// <summary>
        /// دریافت مجوزهای خاص شعبه - بروزرسانی شده برای استفاده از TaskViewPermission
        /// </summary>
        private async Task<List<SpecialTaskPermissionNode>> GetSpecialPermissionsAsync(int branchId)
        {
            var currentTime = DateTime.Now;

            // استفاده از TaskViewPermission جدید به جای TaskViewer
            var permissions = await _context.TaskViewPermission_Tbl
                .Include(tvp => tvp.GranteeUser)
                .Include(tvp => tvp.TargetUser)
                .Include(tvp => tvp.TargetTeam)
                .Include(tvp => tvp.Team)
                .Include(tvp => tvp.AddedByUser)
                .Where(tvp => tvp.Team != null && tvp.Team.BranchId == branchId)
                .ToListAsync();

            var permissionNodes = new List<SpecialTaskPermissionNode>();

            foreach (var permission in permissions)
            {
                var node = new SpecialTaskPermissionNode
                {
                    ViewerId = permission.Id,
                    GranteeUserId = permission.GranteeUserId,
                    GranteeUserName = $"{permission.GranteeUser.FirstName} {permission.GranteeUser.LastName}",
                    GranteeTeamTitle = permission.Team?.Title,
                    PermissionType = permission.PermissionType,
                    PermissionTypeText = GetPermissionTypeText(permission.PermissionType),
                    TargetUserId = permission.TargetUserId,
                    TargetUserName = permission.TargetUser != null ? $"{permission.TargetUser.FirstName} {permission.TargetUser.LastName}" : null,
                    TargetTeamId = permission.TargetTeamId,
                    TargetTeamTitle = permission.TargetTeam?.Title,
                    StartDate = permission.StartDate,
                    EndDate = permission.EndDate,
                    IsActive = permission.IsActive,
                    IsExpired = permission.EndDate.HasValue && permission.EndDate < currentTime,
                    Description = permission.Description,
                    AddedDate = permission.AddedDate,
                    AddedByUserName = $"{permission.AddedByUser.FirstName} {permission.AddedByUser.LastName}"
                };

                permissionNodes.Add(node);
            }

            return permissionNodes;
        }

        private string GetPermissionTypeText(byte permissionType)
        {
            return permissionType switch
            {
                0 => "مشاهده تسک‌های کاربر خاص",
                1 => "مشاهده تسک‌های تیم خاص",
                2 => "مشاهده تسک‌های تیم و زیرتیم‌ها",
                _ => "نامشخص"
            };
        }

        #endregion
        #region Team Management Visibility

        

        /// <summary>
        /// دریافت تمام شناسه زیرتیم‌ها - متد بازگشتی
        /// </summary>
        public async Task<List<int>> GetAllSubTeamIdsAsync(int parentTeamId)
        {
            var subTeamIds = new List<int>();

            // دریافت زیرتیم‌های مستقیم
            var directSubTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            // اضافه کردن زیرتیم‌های مستقیم
            subTeamIds.AddRange(directSubTeams);

            // بازگشتی: دریافت زیرتیم‌های هر زیرتیم
            foreach (var subTeamId in directSubTeams)
            {
                var nestedSubTeams = await GetAllSubTeamIdsAsync(subTeamId);
                subTeamIds.AddRange(nestedSubTeams);
            }

            return subTeamIds;
        }

        #endregion
        #region جدید - پیاده‌سازی جزئیات دریافت تسک‌های زیرتیم‌ها

        /// <summary>
        /// دریافت تسک‌های زیرتیم‌ها به صورت کاملاً گروه‌بندی شده - اصلاح شده
        /// </summary>
        public async Task<SubTeamTasksGroupedViewModel> GetSubTeamTasksGroupedDetailedAsync(
            string userId,
            int? branchId = null)
        {
            var result = new SubTeamTasksGroupedViewModel();

            // دریافت تیم‌های تحت مدیریت مستقیم
            var managedTeams = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && t.IsActive)
                .Where(t => !branchId.HasValue || t.BranchId == branchId)
                .ToListAsync();

            if (!managedTeams.Any()) return result;

            // برای هر تیم اصلی
            foreach (var parentTeam in managedTeams)
            {
                var teamGroup = new SubTeamGroupViewModel
                {
                    ParentTeamId = parentTeam.Id,
                    ParentTeamName = parentTeam.Title
                };

                // ⭐⭐⭐ اصلاح شده: ارسال userId
                await LoadSubTeamsRecursiveAsync(teamGroup, parentTeam.Id, 1, userId);

                if (teamGroup.SubTeams.Any())
                {
                    teamGroup.TotalTasks = teamGroup.SubTeams.Values.Sum(st => st.TotalTasks);
                    result.TeamGroups[parentTeam.Id] = teamGroup;
                    result.TotalSubTeamTasks += teamGroup.TotalTasks;
                }
            }

            return result;
        }
        /// <summary>
        /// بارگذاری بازگشتی زیرتیم‌ها و تسک‌های آن‌ها - اصلاح شده
        /// </summary>
        private async Task LoadSubTeamsRecursiveAsync(
            SubTeamGroupViewModel teamGroup,
            int parentTeamId,
            int level,
            string currentUserId = null) // ⭐ اضافه شده
        {
            var subTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .ToListAsync();

            foreach (var subTeam in subTeams)
            {
                // دریافت تسک‌های این زیرتیم
                var tasks = await _context.Tasks_Tbl
                    .Where(t => t.TeamId == subTeam.Id &&
                               !t.IsDeleted &&
                               !t.IsPrivate &&
                               t.VisibilityLevel != 1)
                    .Include(t => t.Creator)
                    .Include(t => t.TaskAssignments) // ⭐ برای CompletionDate
                        .ThenInclude(ta => ta.AssignedUser) // ⭐ اضافه شده
                    .Include(t => t.TaskCategory)
                    .ToListAsync();

                if (tasks.Any())
                {
                    var subTeamViewModel = new SubTeamTasksViewModel
                    {
                        SubTeamId = subTeam.Id,
                        SubTeamName = subTeam.Title,
                        Level = level
                    };

                    // گروه‌بندی بر اساس کاربر سازنده
                    var tasksByUser = tasks
                        .GroupBy(t => t.CreatorUserId)
                        .ToDictionary(
                            g => g.Key,
                            g => new UserTasksGroupViewModel
                            {
                                UserId = g.Key,
                                UserFullName = g.First().Creator != null
                                    ? $"{g.First().Creator.FirstName} {g.First().Creator.LastName}"
                                    : "نامشخص",

                                // ⭐⭐⭐ اصلاح شده: ارسال currentUserId
                                Tasks = g.Select(t => MapTaskToViewModel(t, currentUserId)).ToList()
                            });

                    subTeamViewModel.TasksByUser = tasksByUser;
                    subTeamViewModel.TotalTasks = tasks.Count;

                    teamGroup.SubTeams[subTeam.Id] = subTeamViewModel;
                }

                // بارگذاری زیرتیم‌های بعدی (بازگشتی) - ⭐ ارسال currentUserId
                await LoadSubTeamsRecursiveAsync(teamGroup, subTeam.Id, level + 1, currentUserId);
            }
        }
        /// <summary>
        /// تبدیل Task Entity به TaskViewModel - اصلاح شده برای نمایش تاریخ تکمیل هر کاربر
        /// </summary>
        private TaskViewModel MapTaskToViewModel(Tasks task, string currentUserId = null)
        {
            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,

                // ⭐⭐⭐ اصلاح شده: تاریخ تکمیل بر اساس کاربر جاری
                CompletionDate = !string.IsNullOrEmpty(currentUserId)
                    ? task.TaskAssignments?
                        .FirstOrDefault(t => t.CompletionDate.HasValue && t.AssignedUserId == currentUserId)
                        ?.CompletionDate
                    : null, // اگر userId نداشتیم، تاریخ تکمیل کل تسک

                IsActive = task.IsActive,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,
                CreatorUserId = task.CreatorUserId,
                CategoryId = task.TaskCategoryId,
                CategoryTitle = task.TaskCategory?.Title,

                AssignmentsTaskUser = task.TaskAssignments?
                    .Select(a => new TaskAssignmentViewModel
                    {
                        Id = a.Id, // ⭐ اضافه شده
                        TaskId = a.TaskId, // ⭐ اضافه شده
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null
                            ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                            : "نامشخص",
                        CompletionDate = a.CompletionDate, // ⭐ تاریخ تکمیل هر assignment
                        AssignDate = a.AssignmentDate, // ⭐ اضافه شده
                        Description = a.Description // ⭐ اضافه شده
                    }).ToList() ?? new List<TaskAssignmentViewModel>()
            };
        }

        #endregion
    }
}