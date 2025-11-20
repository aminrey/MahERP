using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت دسترسی‌ها و مجوزهای تسک (Visibility & Permissions)
    /// شامل: تسک‌های قابل مشاهده، مجوزها، نظارت، سلسله مراتب تیمی
    /// </summary>
    public partial class TaskRepository 
    {
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
                    // فقط سازنده و افراد منتصبه می‌توانند ببینند
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
        /// متد قدیمی - حفظ شده برای سازگاری
        /// </summary>
        public bool CanUserViewTask(string userId, int taskId)
        {
            var now = DateTime.Now;

            return _context.TaskViewer_Tbl
                .Any(tv => tv.UserId == userId &&
                          tv.TaskId == taskId &&
                          tv.IsActive &&
                          (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                          (!tv.EndDate.HasValue || tv.EndDate >= now));
        }

        /// <summary>
        /// دریافت لیست شناسه تسک‌هایی که کاربر می‌تواند مشاهده کند - بازنویسی شده
        /// </summary>
        public async Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId = null, int? teamId = null)
        {
            var visibleTaskIds = new HashSet<int>();
            var currentTime = DateTime.Now;

            // ⭐ 0. دریافت شعبه‌های کاربر
            List<int> userBranchIds;
            if (branchId.HasValue)
            {
                userBranchIds = new List<int> { branchId.Value };
            }
            else
            {
                userBranchIds = await _context.BranchUser_Tbl
                    .Where(bu => bu.UserId == userId && bu.IsActive)
                    .Select(bu => bu.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!userBranchIds.Any())
                {
                    Console.WriteLine($"⚠️ کاربر {userId} در هیچ شعبه‌ای عضو نیست");
                    return new List<int>();
                }
            }

            Console.WriteLine($"🔍 شعبه‌های کاربر: {string.Join(", ", userBranchIds)}");

            // ⭐ 1. تسک‌های ساخته شده توسط کاربر (در شعبه‌های خودش)
            var createdTasks = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId &&
                            userBranchIds.Contains(t.BranchId ?? 0) &&
                            !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();
            visibleTaskIds.UnionWith(createdTasks);
            Console.WriteLine($"   ✅ تسک‌های ساخته شده: {createdTasks.Count}");

            // ⭐ 2. تسک‌های مستقیماً منتصب شده به کاربر
            var assignedTasks = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            userBranchIds.Contains(ta.Task.BranchId ?? 0) &&
                            !ta.Task.IsDeleted)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();
            visibleTaskIds.UnionWith(assignedTasks);
            Console.WriteLine($"   ✅ تسک‌های منتصب شده: {assignedTasks.Count}");

            // ⭐ 3. تسک‌های تیم‌های تحت مدیریت مستقیم
            foreach (var branchIdItem in userBranchIds)
            {
                var managedTeams = await _context.Team_Tbl
                    .Where(t => t.ManagerUserId == userId && 
                               t.BranchId == branchIdItem && 
                               t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                if (managedTeams.Any())
                {
                    var managedTeamTasks = await _context.Tasks_Tbl
                        .Where(t => t.TeamId.HasValue &&
                                   managedTeams.Contains(t.TeamId.Value) &&
                                   t.BranchId == branchIdItem &&
                                   !t.IsDeleted &&
                                   !t.IsPrivate)
                        .Select(t => t.Id)
                        .ToListAsync();

                    visibleTaskIds.UnionWith(managedTeamTasks);
                    Console.WriteLine($"   ✅ تسک‌های تیم‌های مدیریت شده (شعبه {branchIdItem}): {managedTeamTasks.Count}");
                }
            }

            // ⭐ 4. تسک‌های قابل مشاهده بر اساس سمت در تیم
            foreach (var branchIdItem in userBranchIds)
            {
                var userMemberships = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .Include(tm => tm.Team)
                    .Where(tm => tm.UserId == userId &&
                                tm.Team.BranchId == branchIdItem &&
                                tm.IsActive)
                    .ToListAsync();

                foreach (var membership in userMemberships)
                {
                    // 4.1 - اگر عضو دارای سمت است
                    if (membership.Position != null)
                    {
                        // 4.1.1 - تسک‌های زیردستان (بر اساس PowerLevel)
                        if (membership.Position.CanViewSubordinateTasks)
                        {
                            var subordinateUserIds = await _context.TeamMember_Tbl
                                .Include(tm => tm.Position)
                                .Where(tm => tm.TeamId == membership.TeamId &&
                                            tm.IsActive &&
                                            tm.Position != null &&
                                            tm.Position.PowerLevel > membership.Position.PowerLevel)
                                .Select(tm => tm.UserId)
                                .ToListAsync();

                            if (subordinateUserIds.Any())
                            {
                                var subordinateTasks = await _context.TaskAssignment_Tbl
                                    .Where(ta => subordinateUserIds.Contains(ta.AssignedUserId) &&
                                                ta.AssignedInTeamId == membership.TeamId &&
                                                ta.Task.BranchId == branchIdItem &&
                                                !ta.Task.IsDeleted)
                                    .Select(ta => ta.TaskId)
                                    .Distinct()
                                    .ToListAsync();

                                visibleTaskIds.UnionWith(subordinateTasks);
                                Console.WriteLine($"   ✅ تسک‌های زیردستان (تیم {membership.TeamId}): {subordinateTasks.Count}");
                            }
                        }

                        // 4.1.2 - تسک‌های همسطح‌ها (بر اساس PowerLevel)
                        if (membership.Position.CanViewPeerTasks)
                        {
                            var peerUserIds = await _context.TeamMember_Tbl
                                .Include(tm => tm.Position)
                                .Where(tm => tm.TeamId == membership.TeamId &&
                                            tm.IsActive &&
                                            tm.UserId != userId &&
                                            tm.Position != null &&
                                            tm.Position.PowerLevel == membership.Position.PowerLevel)
                                .Select(tm => tm.UserId)
                                .ToListAsync();

                            if (peerUserIds.Any())
                            {
                                var peerTasks = await _context.TaskAssignment_Tbl
                                    .Where(ta => peerUserIds.Contains(ta.AssignedUserId) &&
                                                ta.AssignedInTeamId == membership.TeamId &&
                                                ta.Task.BranchId == branchIdItem &&
                                                !ta.Task.IsDeleted)
                                    .Select(ta => ta.TaskId)
                                    .Distinct()
                                    .ToListAsync();

                                visibleTaskIds.UnionWith(peerTasks);
                                Console.WriteLine($"   ✅ تسک‌های همسطح (تیم {membership.TeamId}): {peerTasks.Count}");
                            }
                        }
                    }

                    // 4.2 - اگر ناظر است (MembershipType = 1)
                    if (membership.MembershipType == 1)
                    {
                        var supervisedUserIds = await _context.TeamMember_Tbl
                            .Where(tm => tm.TeamId == membership.TeamId &&
                                        tm.IsActive &&
                                        tm.UserId != userId &&
                                        tm.MembershipType == 0)
                            .Select(tm => tm.UserId)
                            .ToListAsync();

                        if (supervisedUserIds.Any())
                        {
                            var supervisedTasks = await _context.TaskAssignment_Tbl
                                .Where(ta => supervisedUserIds.Contains(ta.AssignedUserId) &&
                                            ta.AssignedInTeamId == membership.TeamId &&
                                            ta.Task.BranchId == branchIdItem &&
                                            !ta.Task.IsDeleted)
                                .Select(ta => ta.TaskId)
                                .Distinct()
                                .ToListAsync();

                            visibleTaskIds.UnionWith(supervisedTasks);
                            Console.WriteLine($"   ✅ تسک‌های نظارتی (تیم {membership.TeamId}): {supervisedTasks.Count}");
                        }
                    }

                    // 4.3 - تسک‌های تیم با VisibilityLevel >= 2 (تیمی)
                    if (membership.Team != null)
                    {
                        var teamVisibleTasks = await _context.Tasks_Tbl
                            .Where(t => t.TeamId == membership.TeamId &&
                                       t.BranchId == branchIdItem &&
                                       !t.IsDeleted &&
                                       !t.IsPrivate &&
                                       (
                                           t.CreatorUserId == userId ||
                                           _context.TaskAssignment_Tbl.Any(ta =>
                                               ta.TaskId == t.Id &&
                                               ta.AssignedUserId == userId)
                                       ))
                            .Select(t => t.Id)
                            .ToListAsync();

                        visibleTaskIds.UnionWith(teamVisibleTasks);
                        Console.WriteLine($"   ✅ تسک‌های تیمی قابل مشاهده (تیم {membership.TeamId}): {teamVisibleTasks.Count}");
                    }
                }
            }

            // ⭐ 5. تسک‌های با مجوز خاص (TaskViewPermission)
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
                List<int> permissionTasks = new List<int>();

                switch (permission.PermissionType)
                {
                    case 0: // مشاهده تسک‌های یک کاربر خاص
                        if (!string.IsNullOrEmpty(permission.TargetUserId))
                        {
                            permissionTasks = await _context.TaskAssignment_Tbl
                                .Where(ta => ta.AssignedUserId == permission.TargetUserId &&
                                            userBranchIds.Contains(ta.Task.BranchId ?? 0) &&
                                            !ta.Task.IsDeleted &&
                                            !ta.Task.IsPrivate)
                                .Select(ta => ta.TaskId)
                                .Distinct()
                                .ToListAsync();
                        }
                        break;

                    case 1: // مشاهده تسک‌های یک تیم خاص
                        if (permission.TargetTeamId.HasValue)
                        {
                            var targetTeam = await _context.Team_Tbl.FindAsync(permission.TargetTeamId.Value);
                            if (targetTeam != null && userBranchIds.Contains(targetTeam.BranchId))
                            {
                                permissionTasks = await _context.Tasks_Tbl
                                    .Where(t => t.TeamId == permission.TargetTeamId &&
                                               t.BranchId == targetTeam.BranchId &&
                                               !t.IsDeleted &&
                                               !t.IsPrivate)
                                    .Select(t => t.Id)
                                    .ToListAsync();
                            }
                        }
                        break;

                    case 2: // مشاهده تسک‌های تیم و زیرتیم‌ها
                        if (permission.TargetTeamId.HasValue)
                        {
                            var targetTeam = await _context.Team_Tbl.FindAsync(permission.TargetTeamId.Value);
                            if (targetTeam != null && userBranchIds.Contains(targetTeam.BranchId))
                            {
                                var allSubTeamIds = await GetAllSubTeamIdsAsync(permission.TargetTeamId.Value);
                                allSubTeamIds.Add(permission.TargetTeamId.Value);

                                permissionTasks = await _context.Tasks_Tbl
                                    .Where(t => t.TeamId.HasValue &&
                                               allSubTeamIds.Contains(t.TeamId.Value) &&
                                               t.BranchId == targetTeam.BranchId &&
                                               !t.IsDeleted &&
                                               !t.IsPrivate)
                                    .Select(t => t.Id)
                                    .ToListAsync();
                            }
                        }
                        break;
                }

                visibleTaskIds.UnionWith(permissionTasks);
                if (permissionTasks.Any())
                {
                    Console.WriteLine($"   ✅ تسک‌های مجوز خاص (نوع {permission.PermissionType}): {permissionTasks.Count}");
                }
            }

            // ⭐ 6. تسک‌های با مجوز مستقیم (TaskViewer)
            var directPermissionTasks = await _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId &&
                            tv.TaskId > 0 &&
                            tv.IsActive &&
                            (tv.StartDate == null || tv.StartDate <= currentTime) &&
                            (tv.EndDate == null || tv.EndDate > currentTime) &&
                            userBranchIds.Contains(tv.Task.BranchId ?? 0) &&
                            !tv.Task.IsDeleted)
                .Select(tv => tv.TaskId)
                .ToListAsync();
            visibleTaskIds.UnionWith(directPermissionTasks);
            Console.WriteLine($"   ✅ تسک‌های مجوز مستقیم: {directPermissionTasks.Count}");

            Console.WriteLine($"📊 مجموع تسک‌های قابل مشاهده: {visibleTaskIds.Count}");
            return visibleTaskIds.ToList();
        }

        #endregion

        #region Visibility Core - Old Methods

        /// <summary>
        /// دریافت تسک‌های قابل مشاهده برای کاربر بر اساس سیستم سلسله مراتبی
        /// </summary>
        public async Task<List<Tasks>> GetVisibleTasksForUserAsync(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);

            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }

        /// <summary>
        /// متد قدیمی - حفظ شده برای سازگاری
        /// </summary>
        public List<Tasks> GetVisibleTasksForUser(string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = _context.TaskViewer_Tbl
                .Where(tv => tv.UserId == userId && tv.IsActive)
                .Where(tv => !tv.StartDate.HasValue || tv.StartDate <= DateTime.Now)
                .Where(tv => !tv.EndDate.HasValue || tv.EndDate >= DateTime.Now)
                .Select(tv => tv.TaskId)
                .ToList();

            var query = _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return query.OrderByDescending(t => t.CreateDate).ToList();
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

            var teamBranchId = await _context.Team_Tbl
                .Where(t => t.Id == membership.TeamId)
                .Select(t => t.BranchId)
                .FirstOrDefaultAsync();

            var taskIds = await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.VisibilityLevel != 1 &&
                            t.BranchId == teamBranchId &&
                            (
                                subordinateUserIds.Contains(t.CreatorUserId) ||
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    subordinateUserIds.Contains(ta.AssignedUserId)) ||
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

            var teamBranchId = await _context.Team_Tbl
                .Where(t => t.Id == membership.TeamId)
                .Select(t => t.BranchId)
                .FirstOrDefaultAsync();

            var taskIds = await _context.Tasks_Tbl
                .Where(t => !t.IsDeleted &&
                            !t.IsPrivate &&
                            t.VisibilityLevel != 1 &&
                            t.BranchId == teamBranchId &&
                            (
                                peerUserIds.Contains(t.CreatorUserId) ||
                                _context.TaskAssignment_Tbl.Any(ta =>
                                    ta.TaskId == t.Id &&
                                    peerUserIds.Contains(ta.AssignedUserId)) ||
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
                           !t.IsPrivate &&
                           t.VisibilityLevel != 1)
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

        /// <summary>
        /// دریافت تمام شناسه زیرتیم‌ها - متد بازگشتی
        /// </summary>
        public async Task<List<int>> GetAllSubTeamIdsAsync(int parentTeamId)
        {
            var subTeamIds = new List<int>();

            var directSubTeams = await _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            subTeamIds.AddRange(directSubTeams);

            foreach (var subTeamId in directSubTeams)
            {
                var nestedSubTeams = await GetAllSubTeamIdsAsync(subTeamId);
                subTeamIds.AddRange(nestedSubTeams);
            }

            return subTeamIds;
        }

        #endregion

        #region Special Permissions

        /// <summary>
        /// بررسی مجوزهای خاص (تبصره‌ها)
        /// </summary>
        public async Task<bool> HasSpecialPermissionAsync(string userId, Tasks task)
        {
            var currentTime = DateTime.Now;

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
        /// دریافت تسک‌های با مجوز خاص
        /// </summary>
        public async Task<List<int>> GetSpecialPermissionTasksAsync(string userId)
        {
            var currentTime = DateTime.Now;
            var visibleTasks = new List<int>();

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

        #region User Tasks with Permissions

        /// <summary>
        /// دریافت تسک‌های کاربر با در نظر گیری سیستم مجوزهای جدید
        /// </summary>
        public async Task<List<Tasks>> GetTasksByUserWithPermissionsAsync(string userId, bool includeAssigned = true, bool includeCreated = false, bool includeDeleted = false, bool includeSupervisedTasks = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
            var query = _context.Tasks_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            var supervisedTaskIds = new List<int>();
            if (includeSupervisedTasks)
            {
                supervisedTaskIds = await GetSupervisedTaskIdsAsync(userId);
            }

            if (includeAssigned && includeCreated)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeAssigned)
            {
                query = query.Where(t =>
                    visibleTaskIds.Contains(t.Id) ||
                    _context.TaskAssignment_Tbl.Any(a => a.TaskId == t.Id && a.AssignedUserId == userId) ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeCreated)
            {
                query = query.Where(t =>
                    t.CreatorUserId == userId ||
                    (includeSupervisedTasks && supervisedTaskIds.Contains(t.Id)));
            }
            else if (includeSupervisedTasks)
            {
                query = query.Where(t => supervisedTaskIds.Contains(t.Id));
            }

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های شعبه با در نظر گیری سیستم مجوزهای جدید
        /// </summary>
        public async Task<List<Tasks>> GetTasksByBranchWithPermissionsAsync(int branchId, string userId, bool includeDeleted = false)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId, branchId);
            var query = _context.Tasks_Tbl.Where(t => visibleTaskIds.Contains(t.Id));

            if (!includeDeleted)
                query = query.Where(t => !t.IsDeleted);

            return await query.OrderByDescending(t => t.CreateDate).ToListAsync();
        }

        #endregion

        #region Supervised Tasks

        /// <summary>
        /// دریافت شناسه تسک‌هایی که کاربر ناظر آن‌هاست
        /// </summary>
        private async Task<List<int>> GetSupervisedTaskIdsAsync(string userId)
        {
            try
            {
                var supervisedTaskIds = new HashSet<int>();

                // 1. نظارت بر اساس سمت
                var supervisoryPositions = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.Position != null &&
                                tm.Position.CanViewSubordinateTasks)
                    .ToListAsync();

                foreach (var supervisoryPosition in supervisoryPositions)
                {
                    var subordinateMembers = await _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == supervisoryPosition.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.Position != null &&
                                   tm.Position.PowerLevel > supervisoryPosition.Position.PowerLevel)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => subordinateMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => subordinateMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                // 2. نظارت بر اساس MembershipType = 1
                var supervisoryMemberships = await _context.TeamMember_Tbl
                    .Where(tm => tm.UserId == userId &&
                                tm.IsActive &&
                                tm.MembershipType == 1)
                    .ToListAsync();

                foreach (var supervisoryMembership in supervisoryMemberships)
                {
                    var ordinaryMembers = await _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == supervisoryMembership.TeamId &&
                                   tm.IsActive &&
                                   tm.UserId != userId &&
                                   tm.MembershipType == 0)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    var assignedTaskIds = await _context.TaskAssignment_Tbl
                        .Where(ta => ordinaryMembers.Contains(ta.AssignedUserId))
                        .Select(ta => ta.TaskId)
                        .ToListAsync();

                    var createdTaskIds = await _context.Tasks_Tbl
                        .Where(t => ordinaryMembers.Contains(t.CreatorUserId))
                        .Select(t => t.Id)
                        .ToListAsync();

                    foreach (var taskId in assignedTaskIds.Union(createdTaskIds))
                    {
                        supervisedTaskIds.Add(taskId);
                    }
                }

                return supervisedTaskIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSupervisedTaskIdsAsync: {ex.Message}");
                return new List<int>();
            }
        }

        #endregion

        #region Supervisor Management

        /// <summary>
        /// دریافت ناظران یک تسک خاص
        /// </summary>
        public async Task<List<string>> GetTaskSupervisorsAsync(int taskId, bool includeCreator = false)
        {
            var supervisorIds = new HashSet<string>();

            try
            {
                var task = await _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Select(t => new { t.CreatorUserId, t.BranchId })
                    .FirstOrDefaultAsync();

                if (task == null) return new List<string>();

                if (includeCreator && !string.IsNullOrEmpty(task.CreatorUserId))
                {
                    supervisorIds.Add(task.CreatorUserId);
                }

                var assignments = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.TaskId == taskId)
                    .Select(ta => new
                    {
                        ta.AssignedUserId,
                        ta.AssignedInTeamId
                    })
                    .ToListAsync();

                foreach (var assignment in assignments)
                {
                    if (assignment.AssignedInTeamId.HasValue && task.BranchId.HasValue)
                    {
                        var userSupervisors = await GetUserSupervisorsInTeamAsync(
                            assignment.AssignedUserId,
                            assignment.AssignedInTeamId.Value,
                            task.BranchId.Value
                        );

                        foreach (var supervisorId in userSupervisors)
                        {
                            supervisorIds.Add(supervisorId);
                        }
                    }
                }

                var specialSupervisors = await GetSpecialPermissionSupervisorsForTaskAsync(taskId);
                foreach (var supervisorId in specialSupervisors)
                {
                    supervisorIds.Add(supervisorId);
                }

                return supervisorIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskSupervisorsAsync: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// دریافت ناظران یک کاربر در تیم خاص
        /// </summary>
        public async Task<List<string>> GetUserSupervisorsInTeamAsync(string userId, int teamId, int branchId)
        {
            var supervisorIds = new List<string>();

            try
            {
                var userMembership = await _context.TeamMember_Tbl
                    .Include(tm => tm.Position)
                    .FirstOrDefaultAsync(tm =>
                        tm.UserId == userId &&
                        tm.TeamId == teamId &&
                        tm.Team.BranchId == branchId &&
                        tm.IsActive
                    );

                if (userMembership == null) return supervisorIds;

                var teamManagerId = await _context.Team_Tbl
                    .Where(t => t.Id == teamId && t.BranchId == branchId)
                    .Select(t => t.ManagerUserId)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(teamManagerId) && teamManagerId != userId)
                {
                    supervisorIds.Add(teamManagerId);
                }

                var teamSupervisorIds = await _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId &&
                                tm.UserId != userId &&
                                tm.IsActive &&
                                tm.MembershipType == 1)
                    .Select(tm => tm.UserId)
                    .ToListAsync();

                supervisorIds.AddRange(teamSupervisorIds);

                if (userMembership.Position != null)
                {
                    var seniorMembersWithSupervisionPower = await _context.TeamMember_Tbl
                        .Include(tm => tm.Position)
                        .Where(tm => tm.TeamId == teamId &&
                                    tm.UserId != userId &&
                                    tm.IsActive &&
                                    tm.Position != null &&
                                    tm.Position.PowerLevel < userMembership.Position.PowerLevel &&
                                    tm.Position.CanViewSubordinateTasks)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    supervisorIds.AddRange(seniorMembersWithSupervisionPower);
                }

                return supervisorIds.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserSupervisorsInTeamAsync: {ex.Message}");
                return supervisorIds;
            }
        }

        /// <summary>
        /// دریافت ناظران با مجوز خاص برای یک تسک
        /// </summary>
        private async Task<List<string>> GetSpecialPermissionSupervisorsForTaskAsync(int taskId)
        {
            var supervisorIds = new HashSet<string>();
            var currentTime = DateTime.Now;

            try
            {
                var task = await _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Select(t => new { t.CreatorUserId, t.BranchId })
                    .FirstOrDefaultAsync();

                if (task == null) return new List<string>();

                var assignedUserIds = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.TaskId == taskId)
                    .Select(ta => ta.AssignedUserId)
                    .ToListAsync();

                var userBasedSupervisors = await _context.TaskViewPermission_Tbl
                    .Where(tvp => tvp.IsActive &&
                                 tvp.PermissionType == 0 &&
                                 (tvp.StartDate == null || tvp.StartDate <= currentTime) &&
                                 (tvp.EndDate == null || tvp.EndDate > currentTime))
                    .Where(tvp =>
                        tvp.TargetUserId == task.CreatorUserId ||
                        assignedUserIds.Contains(tvp.TargetUserId))
                    .Select(tvp => tvp.GranteeUserId)
                    .Distinct()
                    .ToListAsync();

                foreach (var supervisorId in userBasedSupervisors)
                {
                    supervisorIds.Add(supervisorId);
                }

                var directViewers = await _context.TaskViewer_Tbl
                    .Where(tv => tv.TaskId == taskId &&
                                tv.IsActive &&
                                (tv.StartDate == null || tv.StartDate <= currentTime) &&
                                (tv.EndDate == null || tv.EndDate > currentTime))
                    .Select(tv => tv.UserId)
                    .ToListAsync();

                foreach (var viewerId in directViewers)
                {
                    supervisorIds.Add(viewerId);
                }

                return supervisorIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetSpecialPermissionSupervisorsForTaskAsync: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Supervisor Management - Extended Methods

        /// <summary>
        /// دریافت ناظران یک کاربر در تمام تیم‌های او
        /// </summary>
        public async Task<List<string>> GetUserAllSupervisorsAsync(string userId, int? branchId = null)
        {
            var supervisorIds = new HashSet<string>();

            try
            {
                // دریافت تمام عضویت‌های کاربر
                var userMemberships = await _context.TeamMember_Tbl
                    .Include(tm => tm.Team)
                    .Where(tm => tm.UserId == userId && tm.IsActive)
                    .Where(tm => !branchId.HasValue || tm.Team.BranchId == branchId)
                    .ToListAsync();

                foreach (var membership in userMemberships)
                {
                    var teamSupervisors = await GetUserSupervisorsInTeamAsync(
                        userId,
                        membership.TeamId,
                        membership.Team.BranchId
                    );

                    foreach (var supervisorId in teamSupervisors)
                    {
                        supervisorIds.Add(supervisorId);
                    }
                }

                return supervisorIds.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserAllSupervisorsAsync: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// دریافت ناظران تسک با جزئیات کامل
        /// </summary>
        public async Task<List<SupervisorInfoViewModel>> GetTaskSupervisorsWithDetailsAsync(int taskId)
        {
            var supervisors = new List<SupervisorInfoViewModel>();

            try
            {
                var supervisorIds = await GetTaskSupervisorsAsync(taskId, includeCreator: false);

                foreach (var supervisorId in supervisorIds)
                {
                    var user = await _context.Users
                        .Where(u => u.Id == supervisorId)
                        .Select(u => new
                        {
                            u.Id,
                            u.FirstName,
                            u.LastName,
                            u.Email,
                            u.PhoneNumber,
                            u.ProfileImagePath
                        })
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        // تشخیص نوع نظارت
                        var supervisionType = await GetSupervisionTypeAsync(supervisorId, taskId);

                        supervisors.Add(new SupervisorInfoViewModel
                        {
                            UserId = user.Id,
                            FullName = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            ProfileImagePath = user.ProfileImagePath,
                            SupervisionType = supervisionType
                        });
                    }
                }

                return supervisors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskSupervisorsWithDetailsAsync: {ex.Message}");
                return supervisors;
            }
        }

        /// <summary>
        /// تشخیص نوع نظارت
        /// </summary>
        private async Task<string> GetSupervisionTypeAsync(string supervisorId, int taskId)
        {
            var types = new List<string>();

            // بررسی مدیر تیم
            var isTeamManager = await _context.TaskAssignment_Tbl
                .Where(ta => ta.TaskId == taskId)
                .Join(_context.Team_Tbl,
                    ta => ta.AssignedInTeamId,
                    t => t.Id,
                    (ta, t) => new { t.ManagerUserId })
                .AnyAsync(x => x.ManagerUserId == supervisorId);

            if (isTeamManager) types.Add("مدیر تیم");

            // بررسی ناظر رسمی
            var isFormalSupervisor = await _context.TaskAssignment_Tbl
                .Where(ta => ta.TaskId == taskId)
                .Join(_context.TeamMember_Tbl.Where(tm => tm.UserId == supervisorId && tm.MembershipType == 1),
                    ta => ta.AssignedInTeamId,
                    tm => tm.TeamId,
                    (ta, tm) => tm)
                .AnyAsync();

            if (isFormalSupervisor) types.Add("ناظر تیم");

            // بررسی سمت بالاتر
            var isPositionBased = await _context.TaskAssignment_Tbl
                .Where(ta => ta.TaskId == taskId)
                .Join(_context.TeamMember_Tbl.Include(tm => tm.Position),
                    ta => ta.AssignedInTeamId,
                    tm => tm.TeamId,
                    (ta, tm) => tm)
                .Join(_context.TeamMember_Tbl.Include(tm => tm.Position).Where(tm => tm.UserId == supervisorId),
                    userMembership => userMembership.TeamId,
                    supervisorMembership => supervisorMembership.TeamId,
                    (userMembership, supervisorMembership) => new
                    {
                        UserPowerLevel = userMembership.Position != null ? userMembership.Position.PowerLevel : 999,
                        SupervisorPowerLevel = supervisorMembership.Position != null ? supervisorMembership.Position.PowerLevel : 999,
                        CanView = supervisorMembership.Position != null && supervisorMembership.Position.CanViewSubordinateTasks
                    })
                .AnyAsync(x => x.SupervisorPowerLevel < x.UserPowerLevel && x.CanView);

            if (isPositionBased) types.Add("سمت بالاتر");

            // بررسی مجوز خاص
            var hasSpecialPermission = await _context.TaskViewPermission_Tbl
                .AnyAsync(tvp => tvp.GranteeUserId == supervisorId &&
                                tvp.IsActive &&
                                (tvp.PermissionType == 0 || tvp.PermissionType == 1 || tvp.PermissionType == 2));

            if (hasSpecialPermission) types.Add("مجوز خاص");

            return types.Any() ? string.Join(", ", types) : "نامشخص";
        }

        #endregion

        #region Chart Generation & Stats

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

        #region Private Helper Methods for Chart

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

        #region SubTeam Tasks Grouped - جدید

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
            string currentUserId = null)
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
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.AssignedUser)
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
                                Tasks = g.Select(t => MapTaskToViewModel(t, currentUserId)).ToList()
                            });

                    subTeamViewModel.TasksByUser = tasksByUser;
                    subTeamViewModel.TotalTasks = tasks.Count;

                    teamGroup.SubTeams[subTeam.Id] = subTeamViewModel;
                }

                // بارگذاری زیرتیم‌های بعدی (بازگشتی)
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
                CompletionDate = !string.IsNullOrEmpty(currentUserId)
                    ? task.TaskAssignments?
                        .FirstOrDefault(t => t.CompletionDate.HasValue && t.AssignedUserId == currentUserId)
                        ?.CompletionDate
                    : null,
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
                        Id = a.Id,
                        TaskId = a.TaskId,
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null
                            ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                            : "نامشخص",
                        CompletionDate = a.CompletionDate,
                        AssignDate = a.AssignmentDate,
                        Description = a.Description
                    }).ToList() ?? new List<TaskAssignmentViewModel>()
            };
        }

        #endregion
    }
}
