using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Core;  // ⭐⭐⭐ اضافه شد
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// بخش فیلتر کردن و جستجوی تسک‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Filtering

        /// <summary>
        /// دریافت تسک‌های من
        /// </summary>
        public async Task<List<Tasks>> GetMyTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetMyTasksAsync - User: {userId}");

            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);

            var myAssignmentTaskIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var relevantTaskIds = visibleTaskIds.Intersect(myAssignmentTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// دریافت تسک‌های واگذار شده توسط من
        /// </summary>
        public async Task<List<Tasks>> GetAssignedByMeTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetAssignedByMeTasksAsync - User: {userId}");

            var myCreatedTaskIds = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();

            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = myCreatedTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// دریافت تسک‌های نظارتی
        /// </summary>
        public async Task<List<Tasks>> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetSupervisedTasksAsync - User: {userId}");

            // ⭐⭐⭐ دریافت شعبه‌های کاربر
            var userBranchIds = await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Select(bu => bu.BranchId)
                .ToListAsync();

            Console.WriteLine($"   User branches: {string.Join(", ", userBranchIds)}");

            var allSupervisedTaskIds = new HashSet<int>();

            // ⭐⭐⭐ برای هر شعبه، از تنظیمات استفاده کن
            foreach (var branchId in userBranchIds)
            {
                Console.WriteLine($"   📊 Processing branch {branchId}...");
                
                var branchTasks = await GetSupervisedTasksUsingSettingsAsync(userId, branchId);
                
                Console.WriteLine($"   ✅ Branch {branchId} tasks: {branchTasks.Count}");
                
                allSupervisedTaskIds.UnionWith(branchTasks);
            }

            Console.WriteLine($"   Total supervised tasks (before carbon copy): {allSupervisedTaskIds.Count}");

            // ⭐ تسک‌های رونوشت (CarbonCopy)
            var carbonCopyTaskIds = await _context.TaskCarbonCopy_Tbl
                .Where(cc => cc.UserId == userId && cc.IsActive)
                .Select(cc => cc.TaskId)
                .ToListAsync();
            
            Console.WriteLine($"   CarbonCopy tasks: {carbonCopyTaskIds.Count}");
            
            allSupervisedTaskIds.UnionWith(carbonCopyTaskIds);

            // حذف تسک‌هایی که خودش سازنده است
            var createdTaskIds = await _context.Tasks_Tbl
                .Where(t => t.CreatorUserId == userId)
                .Select(t => t.Id)
                .ToListAsync();
            
            Console.WriteLine($"   Created tasks (to exclude): {createdTaskIds.Count}");
            
            allSupervisedTaskIds.ExceptWith(createdTaskIds);

            // حذف تسک‌هایی که به او منتصب شده
            var assignedTaskIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId)
                .Select(ta => ta.TaskId)
                .ToListAsync();
            
            Console.WriteLine($"   Assigned tasks (to exclude): {assignedTaskIds.Count}");
            
            allSupervisedTaskIds.ExceptWith(assignedTaskIds);

            Console.WriteLine($"   📊 Final supervised tasks count: {allSupervisedTaskIds.Count}");

            // بارگذاری تسک‌ها
            var tasks = await _context.Tasks_Tbl
                .Where(t => allSupervisedTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            Console.WriteLine($"   ✅ Loaded tasks from DB: {tasks.Count}");

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// دریافت همه تسک‌های قابل مشاهده
        /// </summary>
        public async Task<List<Tasks>> GetAllVisibleTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);

            var tasks = await _context.Tasks_Tbl
                .Where(t => visibleTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// دریافت تسک‌های منتصب شده به من
        /// </summary>
        public async Task<List<Tasks>> GetAssignedToMeTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            var assignedTaskIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId &&
                            ta.Task.CreatorUserId != userId)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToListAsync();

            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = assignedTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id) && !t.IsDeleted)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// دریافت تسک‌های تیمی
        /// </summary>
        public async Task<List<Tasks>> GetTeamTasksAsync(string userId, TaskFilterViewModel filters = null)
        {
            Console.WriteLine($"🔍 GetTeamTasksAsync - User: {userId}");

            var userTeamIds = await _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            if (!userTeamIds.Any())
                return new List<Tasks>();

            var teamTaskIds = await _context.Tasks_Tbl
                .Where(t => t.TeamId.HasValue && userTeamIds.Contains(t.TeamId.Value) && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();

            var visibleTaskIds = await GetVisibleTaskIdsAsync(userId);
            var relevantTaskIds = teamTaskIds.Intersect(visibleTaskIds).ToList();

            var tasks = await _context.Tasks_Tbl
                .Where(t => relevantTaskIds.Contains(t.Id))
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.AssignedUser)
                .Include(t => t.TaskCategory)
                .Include(t => t.Creator)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.TaskOperations)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();

            return ApplyFilters(tasks, filters, userId);
        }

        /// <summary>
        /// اعمال فیلترها روی لیست تسک‌ها
        /// </summary>
        public List<Tasks> ApplyFilters(List<Tasks> tasks, TaskFilterViewModel filters, string userId = null)
        {
            if (filters == null) return tasks;

            Console.WriteLine($"🔍 Applying filters...");
            Console.WriteLine($"   Initial tasks count: {tasks.Count}");

            // ⭐⭐⭐ فیلتر شعبه
            if (filters.BranchId.HasValue)
            {
                tasks = tasks.Where(t => t.BranchId == filters.BranchId).ToList();
                Console.WriteLine($"   After BranchId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر دسته‌بندی
            if (filters.CategoryId.HasValue)
            {
                tasks = tasks.Where(t => t.TaskCategoryId == filters.CategoryId).ToList();
                Console.WriteLine($"   After CategoryId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر طرف حساب (Stakeholder - قدیمی)
            if (filters.StakeholderId.HasValue)
            {
                tasks = tasks.Where(t => t.StakeholderId == filters.StakeholderId).ToList();
                Console.WriteLine($"   After StakeholderId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر تیم (Team)
            if (filters.TeamId.HasValue)
            {
                tasks = tasks.Where(t =>
                    t.TaskAssignments != null &&
                    t.TaskAssignments.Any(a => a.AssignedInTeamId == filters.TeamId.Value)
                ).ToList();
                Console.WriteLine($"   After TeamId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر سازنده تسک (CreatorUserId)
            if (!string.IsNullOrEmpty(filters.CreatorUserId))
            {
                tasks = tasks.Where(t => t.CreatorUserId == filters.CreatorUserId).ToList();
                Console.WriteLine($"   After CreatorUserId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر کاربر منتصب شده (AssignedUserId)
            if (!string.IsNullOrEmpty(filters.AssignedUserId))
            {
                tasks = tasks.Where(t =>
                    t.TaskAssignments != null &&
                    t.TaskAssignments.Any(a => a.AssignedUserId == filters.AssignedUserId)
                ).ToList();
                Console.WriteLine($"   After AssignedUserId filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر عنوان تسک
            if (!string.IsNullOrEmpty(filters.TaskTitle))
            {
                var search = filters.TaskTitle.ToLower();
                tasks = tasks.Where(t =>
                    t.Title.ToLower().Contains(search)
                ).ToList();
                Console.WriteLine($"   After TaskTitle filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر کد تسک
            if (!string.IsNullOrEmpty(filters.TaskCode))
            {
                var search = filters.TaskCode.ToLower();
                tasks = tasks.Where(t =>
                    t.TaskCode.ToLower().Contains(search)
                ).ToList();
                Console.WriteLine($"   After TaskCode filter: {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر تاریخ ساخت (از) - از Persian به DateTime
            if (!string.IsNullOrEmpty(filters.CreateDateFromPersian))
            {
                try
                {
                    var fromDate = ConvertDateTime.ConvertShamsiToMiladi(filters.CreateDateFromPersian);
                    tasks = tasks.Where(t => t.CreateDate >= fromDate).ToList();
                    Console.WriteLine($"   After CreateDateFrom filter: {tasks.Count}");
                }
                catch
                {
                    Console.WriteLine($"   ⚠️ Invalid CreateDateFromPersian: {filters.CreateDateFromPersian}");
                }
            }

            // ⭐⭐⭐ فیلتر تاریخ ساخت (تا) - از Persian به DateTime
            if (!string.IsNullOrEmpty(filters.CreateDateToPersian))
            {
                try
                {
                    var toDate = ConvertDateTime.ConvertShamsiToMiladi(filters.CreateDateToPersian);
                    tasks = tasks.Where(t => t.CreateDate <= toDate).ToList();
                    Console.WriteLine($"   After CreateDateTo filter: {tasks.Count}");
                }
                catch
                {
                    Console.WriteLine($"   ⚠️ Invalid CreateDateToPersian: {filters.CreateDateToPersian}");
                }
            }

            // ⭐⭐⭐ فیلتر وضعیت (TaskStatus) - اصلاح شده با userId
            if (filters.TaskStatus.HasValue && filters.TaskStatus != TaskStatusFilter.All)
            {
                var today = DateTime.Now.Date;
                
                // ⭐ اگر userId موجود نیست، از منطق ساده استفاده کن
                if (string.IsNullOrEmpty(userId))
                {
                    tasks = filters.TaskStatus.Value switch
                    {
                        TaskStatusFilter.Pending => tasks.Where(t => t.Status == 0).ToList(),
                        TaskStatusFilter.InProgress => tasks.Where(t => t.Status == 1).ToList(),
                        TaskStatusFilter.Completed => tasks.Where(t => t.Status == 2).ToList(),
                        TaskStatusFilter.Overdue => tasks.Where(t => 
                            t.DueDate.HasValue && 
                            t.DueDate.Value.Date < today && 
                            t.Status != 2
                        ).ToList(),
                        TaskStatusFilter.Approved => tasks.Where(t => t.Status == 3).ToList(),
                        TaskStatusFilter.Rejected => tasks.Where(t => t.Status == 4).ToList(),
                        _ => tasks
                    };
                }
                else
                {
                    // ⭐⭐⭐ منطق اصلاح شده: استفاده از IsTaskCompletedForUser
                    tasks = filters.TaskStatus.Value switch
                    {
                        // Pending: تسک‌هایی که برای این کاربر تکمیل نشده و deadline ندارند یا نگذشته
                        TaskStatusFilter.Pending => tasks.Where(t => 
                            !IsTaskCompletedForUser(t.Id, userId) &&
                            (!t.DueDate.HasValue || t.DueDate.Value.Date >= today)
                        ).ToList(),
                        
                        // InProgress: تسک‌هایی که برای این کاربر تکمیل نشده‌اند (مثل CalculateStats)
                        TaskStatusFilter.InProgress => tasks.Where(t => 
                            !IsTaskCompletedForUser(t.Id, userId)
                        ).ToList(),
                        
                        // Completed: تسک‌های تکمیل شده برای این کاربر
                        TaskStatusFilter.Completed => tasks.Where(t => 
                            IsTaskCompletedForUser(t.Id, userId)
                        ).ToList(),
                        
                        // Overdue: تسک‌های عقب افتاده (deadline گذشته و برای این کاربر تکمیل نشده)
                        TaskStatusFilter.Overdue => tasks.Where(t => 
                            t.DueDate.HasValue && 
                            t.DueDate.Value.Date < today && 
                            !IsTaskCompletedForUser(t.Id, userId)
                        ).ToList(),
                        
                        // Approved: تایید شده
                        TaskStatusFilter.Approved => tasks.Where(t => 
                            t.Status == 3
                        ).ToList(),
                        
                        // Rejected: رد شده
                        TaskStatusFilter.Rejected => tasks.Where(t => 
                            t.Status == 4
                        ).ToList(),
                        
                        _ => tasks
                    };
                }
                
                Console.WriteLine($"   After TaskStatus filter ({filters.TaskStatus.Value}): {tasks.Count}");
            }

            // ⭐⭐⭐ فیلتر جستجوی کلی (SearchTerm)
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                tasks = tasks.Where(t =>
                    t.Title.ToLower().Contains(search) ||
                    t.TaskCode.ToLower().Contains(search) ||
                    (t.Description != null && t.Description.ToLower().Contains(search))
                ).ToList();
                Console.WriteLine($"   After SearchTerm filter: {tasks.Count}");
            }

            Console.WriteLine($"✅ Final filtered tasks count: {tasks.Count}");

            return tasks;
        }

        /// <summary>
        /// محاسبه آمار لیست تسک‌ها
        /// </summary>
        public TaskListStatsViewModel CalculateStats(List<Tasks> tasks, string userId)
        {
            return new TaskListStatsViewModel
            {
                TotalPending = tasks.Count(t => !IsTaskCompletedForUser(t.Id, userId)),
                TotalCompleted = tasks.Count(t => IsTaskCompletedForUser(t.Id, userId)),
                TotalOverdue = tasks.Count(t => t.DueDate.HasValue &&
                                                t.DueDate.Value < DateTime.Now &&
                                                !IsTaskCompletedForUser(t.Id, userId)),
                TotalUrgent = tasks.Count(t => t.Priority == 2),
                TotalImportant = tasks.Count(t => t.Important || t.Priority == 1)
            };
        }

        /// <summary>
        /// اعمال فیلتر سریع وضعیت
        /// </summary>
        public List<Tasks> ApplyQuickStatusFilter(List<Tasks> tasks, QuickStatusFilter filter, string userId)
        {
            return filter switch
            {
                QuickStatusFilter.Pending => tasks.Where(t => !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Completed => tasks.Where(t => IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Overdue => tasks.Where(t => 
                    t.DueDate.HasValue && 
                    t.DueDate.Value < DateTime.Now && 
                    !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                QuickStatusFilter.Urgent => tasks.Where(t => 
                    t.Priority == 2 && 
                    !IsTaskCompletedForUser(t.Id, userId)).ToList(),
                _ => tasks
            };
        }

        #endregion

        #region Branch Visibility Settings Helper Methods

        /// <summary>
        /// ⭐⭐⭐ دریافت تنظیمات پیش‌فرض نمایش تسک برای مدیر در شعبه
        /// </summary>
        private async Task<BranchTaskVisibilitySettings> GetManagerVisibilitySettingsAsync(string userId, int branchId)
        {
            // 1. جستجوی تنظیمات شخصی
            var personalSettings = await _context.BranchTaskVisibilitySettings_Tbl
                .FirstOrDefaultAsync(s => s.ManagerUserId == userId && 
                                         s.BranchId == branchId && 
                                         s.IsActive);
            
            if (personalSettings != null)
                return personalSettings;

            // 2. جستجوی تنظیمات پیش‌فرض شعبه (ManagerUserId = null)
            var defaultSettings = await _context.BranchTaskVisibilitySettings_Tbl
                .FirstOrDefaultAsync(s => s.ManagerUserId == null && 
                                         s.BranchId == branchId && 
                                         s.IsActive);
            
            return defaultSettings;
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های نظارتی با استفاده از تنظیمات شعبه
        /// </summary>
        private async Task<List<int>> GetSupervisedTasksUsingSettingsAsync(string userId, int branchId)
        {
            var supervisedTaskIds = new HashSet<int>();

            // دریافت تنظیمات
            var settings = await GetManagerVisibilitySettingsAsync(userId, branchId);

            if (settings == null)
            {
                // اگر تنظیماتی وجود نداشت، فقط تسک‌های تیم مستقیم را نمایش بده
                return await GetManagedTeamTasksAsync(userId, branchId);
            }

            // اگر ShowAllSubTeamsByDefault = true
            if (settings.ShowAllSubTeamsByDefault)
            {
                var allTeamTasks = await GetAllHierarchyTasksAsync(userId, branchId);
                
                // محدودیت تعداد
                if (settings.MaxTasksToShow > 0 && allTeamTasks.Count > settings.MaxTasksToShow)
                {
                    return allTeamTasks.Take(settings.MaxTasksToShow).ToList();
                }
                
                return allTeamTasks;
            }

            // اگر لیست تیم‌های خاص تعریف شده
            if (!string.IsNullOrEmpty(settings.DefaultVisibleTeamIds))
            {
                var teamIds = settings.GetVisibleTeamIds();
                
                if (teamIds.Any())
                {
                    var teamTasks = await _context.Tasks_Tbl
                        .Where(t => t.TeamId.HasValue && 
                                   teamIds.Contains(t.TeamId.Value) &&
                                   t.BranchId == branchId &&
                                   !t.IsDeleted &&
                                   !t.IsPrivate)
                        .Select(t => t.Id)
                        .ToListAsync();
                    
                    supervisedTaskIds.UnionWith(teamTasks);
                }
            }

            // تسک‌های تیم مستقیم
            var directTasks = await GetManagedTeamTasksAsync(userId, branchId);
            supervisedTaskIds.UnionWith(directTasks);

            var result = supervisedTaskIds.ToList();
            
            // محدودیت تعداد
            if (settings.MaxTasksToShow > 0 && result.Count > settings.MaxTasksToShow)
            {
                return result.Take(settings.MaxTasksToShow).ToList();
            }

            return result;
        }

        /// <summary>
        /// ⭐ دریافت تمام تسک‌های سلسله‌مراتب (شامل زیرتیم‌ها)
        /// </summary>
        private async Task<List<int>> GetAllHierarchyTasksAsync(string userId, int branchId)
        {
            var allTaskIds = new HashSet<int>();

            // دریافت تیم‌های مدیریت شده
            var managedTeamIds = await _context.Team_Tbl
                .Where(t => t.ManagerUserId == userId && 
                           t.BranchId == branchId && 
                           t.IsActive)
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var teamId in managedTeamIds)
            {
                // تسک‌های تیم مستقیم
                var directTasks = await _context.Tasks_Tbl
                    .Where(t => t.TeamId == teamId && 
                               !t.IsDeleted && 
                               !t.IsPrivate)
                    .Select(t => t.Id)
                    .ToListAsync();
                
                allTaskIds.UnionWith(directTasks);

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

        #endregion

        #region Filtering Helper Methods

        /// <summary>
        /// بررسی تکمیل شدن تسک برای کاربر خاص
        /// </summary>
        private bool IsTaskCompletedForUser(int taskId, string userId)
        {
            return _context.TaskAssignment_Tbl
                .Any(a => a.TaskId == taskId &&
                         a.AssignedUserId == userId &&
                         a.CompletionDate.HasValue);
        }

        #endregion
    }
}
