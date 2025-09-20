using AutoMapper;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class TaskViewerRepository : ITaskViewerRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TaskViewerRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool CanUserViewTask(string userId, int taskId)
        {
            var now = DateTime.Now;
            
            // بررسی وجود مجوز فعال
            return _context.TaskViewer_Tbl
                .Any(tv => tv.UserId == userId && 
                          tv.TaskId == taskId && 
                          tv.IsActive &&
                          (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                          (!tv.EndDate.HasValue || tv.EndDate >= now));
        }

        public List<int> GetVisibleTaskIds(string userId, bool includeDeleted = false)
        {
            var now = DateTime.Now;
            
            var query = _context.TaskViewer_Tbl
                .Include(tv => tv.Task)
                .Where(tv => tv.UserId == userId && 
                            tv.IsActive &&
                            (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                            (!tv.EndDate.HasValue || tv.EndDate >= now));

            if (!includeDeleted)
            {
                query = query.Where(tv => !tv.Task.IsDeleted);
            }

            return query.Select(tv => tv.TaskId).ToList();
        }

        public async Task<int> AddTaskViewerAsync(TaskViewer taskViewer)
        {
            // بررسی عدم وجود مجوز تکراری
            var exists = await _context.TaskViewer_Tbl
                .AnyAsync(tv => tv.UserId == taskViewer.UserId && 
                               tv.TaskId == taskViewer.TaskId &&
                               tv.AccessType == taskViewer.AccessType &&
                               tv.IsActive);

            if (exists)
                return 0; // مجوز از قبل وجود دارد

            taskViewer.AddedDate = DateTime.Now;
            _context.TaskViewer_Tbl.Add(taskViewer);
            await _context.SaveChangesAsync();
            return taskViewer.Id;
        }

        public async Task<bool> RemoveTaskViewerAsync(int taskViewerId)
        {
            try
            {
                var taskViewer = await _context.TaskViewer_Tbl.FindAsync(taskViewerId);
                if (taskViewer == null) return false;

                _context.TaskViewer_Tbl.Remove(taskViewer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<TaskViewer> GetTaskViewers(int taskId, bool activeOnly = true)
        {
            var query = _context.TaskViewer_Tbl
                .Include(tv => tv.User)
                .Include(tv => tv.Team)
                .Include(tv => tv.AddedByUser)
                .Where(tv => tv.TaskId == taskId);

            if (activeOnly)
            {
                var now = DateTime.Now;
                query = query.Where(tv => tv.IsActive &&
                                         (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                                         (!tv.EndDate.HasValue || tv.EndDate >= now));
            }

            return query.OrderBy(tv => tv.AccessType).ThenBy(tv => tv.AddedDate).ToList();
        }

        public List<TaskViewer> GetUserTaskViewers(string userId, bool activeOnly = true)
        {
            var query = _context.TaskViewer_Tbl
                .Include(tv => tv.Task)
                .Include(tv => tv.Team)
                .Include(tv => tv.AddedByUser)
                .Where(tv => tv.UserId == userId);

            if (activeOnly)
            {
                var now = DateTime.Now;
                query = query.Where(tv => tv.IsActive &&
                                         (!tv.StartDate.HasValue || tv.StartDate <= now) &&
                                         (!tv.EndDate.HasValue || tv.EndDate >= now));
            }

            return query.OrderByDescending(tv => tv.AddedDate).ToList();
        }

        public async Task<int> GrantSpecialPermissionAsync(string granteeUserId, string targetUserId = null, int? targetTeamId = null, byte permissionType = 0, string grantedByUserId = null, string description = null)
        {
            try
            {
                // دریافت تسک‌های هدف بر اساس نوع مجوز
                List<int> targetTaskIds = new List<int>();

                switch (permissionType)
                {
                    case 0: // تسک‌های یک کاربر خاص
                        if (!string.IsNullOrEmpty(targetUserId))
                        {
                            targetTaskIds = _context.Tasks_Tbl
                                .Where(t => !t.IsDeleted && 
                                           (t.CreatorUserId == targetUserId ||
                                            _context.TaskAssignment_Tbl.Any(ta => ta.TaskId == t.Id && ta.AssignedUserId == targetUserId)))
                                .Select(t => t.Id)
                                .ToList();
                        }
                        break;

                    case 1: // تسک‌های یک تیم خاص
                        if (targetTeamId.HasValue)
                        {
                            var teamMemberIds = _context.TeamMember_Tbl
                                .Where(tm => tm.TeamId == targetTeamId && tm.IsActive)
                                .Select(tm => tm.UserId)
                                .ToList();

                            targetTaskIds = _context.Tasks_Tbl
                                .Where(t => !t.IsDeleted &&
                                           (teamMemberIds.Contains(t.CreatorUserId) ||
                                            _context.TaskAssignment_Tbl.Any(ta => ta.TaskId == t.Id && teamMemberIds.Contains(ta.AssignedUserId))))
                                .Select(t => t.Id)
                                .ToList();
                        }
                        break;

                    case 2: // تسک‌های تیم و زیرتیم‌ها
                        if (targetTeamId.HasValue)
                        {
                            var allTeamIds = GetAllSubTeamIds(targetTeamId.Value);
                            allTeamIds.Add(targetTeamId.Value);

                            var allMemberIds = _context.TeamMember_Tbl
                                .Where(tm => allTeamIds.Contains(tm.TeamId) && tm.IsActive)
                                .Select(tm => tm.UserId)
                                .ToList();

                            targetTaskIds = _context.Tasks_Tbl
                                .Where(t => !t.IsDeleted &&
                                           (allMemberIds.Contains(t.CreatorUserId) ||
                                            _context.TaskAssignment_Tbl.Any(ta => ta.TaskId == t.Id && allMemberIds.Contains(ta.AssignedUserId))))
                                .Select(t => t.Id)
                                .ToList();
                        }
                        break;
                }

                // ایجاد TaskViewer برای هر تسک
                int createdCount = 0;
                foreach (var taskId in targetTaskIds)
                {
                    var taskViewer = new TaskViewer
                    {
                        TaskId = taskId,
                        UserId = granteeUserId,
                        AccessType = 0, // مجوز خاص
                        SpecialPermissionType = permissionType,
                        TeamId = targetTeamId,
                        AddedByUserId = grantedByUserId,
                        AddedDate = DateTime.Now,
                        IsActive = true,
                        Description = description
                    };

                    var created = await AddTaskViewerAsync(taskViewer);
                    if (created > 0) createdCount++;
                }

                return createdCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> MarkTaskAsViewedAsync(string userId, int taskId)
        {
            try
            {
                var taskViewer = await _context.TaskViewer_Tbl
                    .FirstOrDefaultAsync(tv => tv.UserId == userId && tv.TaskId == taskId && tv.IsActive);

                if (taskViewer == null) return false;

                taskViewer.IsViewed = true;
                taskViewer.ViewDate = DateTime.Now;
                taskViewer.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task GenerateAutomaticViewersForTask(int taskId)
        {
            var task = await _context.Tasks_Tbl
                .Include(t => t.TaskAssignments)
                .Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return;

            var viewersToAdd = new List<TaskViewer>();

            // 1. سازنده تسک
            if (!string.IsNullOrEmpty(task.CreatorUserId))
            {
                viewersToAdd.Add(new TaskViewer
                {
                    TaskId = taskId,
                    UserId = task.CreatorUserId,
                    AccessType = 4, // سازنده
                    AddedByUserId = task.CreatorUserId,
                    AddedDate = DateTime.Now,
                    IsActive = true,
                    Description = "سازنده تسک"
                });
            }

            // 2. منتصب‌ها
            foreach (var assignment in task.TaskAssignments)
            {
                viewersToAdd.Add(new TaskViewer
                {
                    TaskId = taskId,
                    UserId = assignment.AssignedUserId,
                    AccessType = 5, // منتصب
                    AddedByUserId = task.CreatorUserId,
                    AddedDate = DateTime.Now,
                    IsActive = true,
                    Description = "منتصب به تسک"
                });
            }

            // 3. مدیران تیم (اگر تسک تیمی باشد)
            if (task.TeamId.HasValue)
            {
                var team = await _context.Team_Tbl.FindAsync(task.TeamId.Value);
                if (team != null && !string.IsNullOrEmpty(team.ManagerUserId))
                {
                    viewersToAdd.Add(new TaskViewer
                    {
                        TaskId = taskId,
                        UserId = team.ManagerUserId,
                        AccessType = 1, // مدیر تیم
                        TeamId = task.TeamId,
                        AddedByUserId = task.CreatorUserId,
                        AddedDate = DateTime.Now,
                        IsActive = true,
                        Description = "مدیر تیم"
                    });
                }

                // اعضای تیم (اگر سطح visibility اجازه دهد)
                if (task.VisibilityLevel >= 2)
                {
                    var teamMembers = _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == task.TeamId && tm.IsActive)
                        .Select(tm => tm.UserId)
                        .ToList();

                    foreach (var memberId in teamMembers)
                    {
                        viewersToAdd.Add(new TaskViewer
                        {
                            TaskId = taskId,
                            UserId = memberId,
                            AccessType = 2, // عضو تیم
                            TeamId = task.TeamId,
                            AddedByUserId = task.CreatorUserId,
                            AddedDate = DateTime.Now,
                            IsActive = true,
                            Description = "عضو تیم"
                        });
                    }
                }
            }

            // 4. مدیران سلسله مراتبی
            var hierarchicalManagers = GetHierarchicalManagers(task.CreatorUserId);
            foreach (var managerId in hierarchicalManagers)
            {
                viewersToAdd.Add(new TaskViewer
                {
                    TaskId = taskId,
                    UserId = managerId,
                    AccessType = 1, // مدیر سلسله مراتبی
                    AddedByUserId = task.CreatorUserId,
                    AddedDate = DateTime.Now,
                    IsActive = true,
                    Description = "مدیر سلسله مراتبی"
                });
            }

            // اضافه کردن به دیتابیس
            foreach (var viewer in viewersToAdd)
            {
                await AddTaskViewerAsync(viewer);
            }
        }

        public async Task<int> CleanupExpiredPermissionsAsync()
        {
            var now = DateTime.Now;
            var expiredViewers = await _context.TaskViewer_Tbl
                .Where(tv => tv.EndDate.HasValue && tv.EndDate < now && tv.IsActive)
                .ToListAsync();

            foreach (var viewer in expiredViewers)
            {
                viewer.IsActive = false;
                viewer.LastUpdateDate = now;
            }

            await _context.SaveChangesAsync();
            return expiredViewers.Count;
        }

        #region Helper Methods

        private List<int> GetAllSubTeamIds(int parentTeamId)
        {
            var subTeamIds = new List<int>();
            var directSubTeams = _context.Team_Tbl
                .Where(t => t.ParentTeamId == parentTeamId && t.IsActive)
                .Select(t => t.Id)
                .ToList();

            subTeamIds.AddRange(directSubTeams);

            foreach (var subTeamId in directSubTeams)
            {
                subTeamIds.AddRange(GetAllSubTeamIds(subTeamId));
            }

            return subTeamIds;
        }

        private List<string> GetHierarchicalManagers(string userId)
        {
            var managers = new List<string>();
            var currentUserId = userId;

            while (!string.IsNullOrEmpty(currentUserId))
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
                if (user?.DirectManagerUserId != null)
                {
                    managers.Add(user.DirectManagerUserId);
                    currentUserId = user.DirectManagerUserId;
                }
                else
                {
                    break;
                }
            }

            return managers;
        }

        #endregion
    }
}