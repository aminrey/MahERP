using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.ViewModels;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// نمایش کارت تسک‌ها و لیست‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Task List and Cards

        /// <summary>
        /// دریافت لیست تسک‌ها با گروه‌بندی و فیلتر
        /// </summary>
        public async Task<TaskListViewModel> GetTaskListAsync(
            string userId,
            TaskViewType viewType,
            TaskGroupingType grouping,
            TaskFilterViewModel filters = null)
        {
            try
            {
                Console.WriteLine($"🔍 GetTaskListAsync - User: {userId}, ViewType: {viewType}");

                var model = new TaskListViewModel
                {
                    UserLoginid = userId,
                    CurrentViewType = viewType,
                    CurrentGrouping = grouping,
                    Filters = filters ?? new TaskFilterViewModel()
                };

                // ⭐⭐⭐ مرحله 1: دریافت تسک‌ها بدون فیلتر وضعیت برای محاسبه Stats
                var filtersWithoutStatus = filters != null ? new TaskFilterViewModel
                {
                    ViewType = filters.ViewType,
                    Grouping = filters.Grouping,
                    BranchId = filters.BranchId,
                    TeamId = filters.TeamId,
                    UserId = filters.UserId,
                    TaskPriority = filters.TaskPriority,
                    CategoryId = filters.CategoryId,
                    TaskStatus = null, // ⭐ حذف فیلتر وضعیت
                    StakeholderId = filters.StakeholderId,
                    FromDate = filters.FromDate,
                    ToDate = filters.ToDate,
                    SearchTerm = filters.SearchTerm,
                    CreateDateFromPersian = filters.CreateDateFromPersian,
                    CreateDateToPersian = filters.CreateDateToPersian,
                    TaskTitle = filters.TaskTitle,
                    TaskCode = filters.TaskCode,
                    CreatorUserId = filters.CreatorUserId,
                    AssignedUserId = filters.AssignedUserId
                } : null;

                // دریافت تسک‌ها بدون فیلتر وضعیت
                List<Tasks> allTasks = viewType switch
                {
                    TaskViewType.MyTasks => await GetMyTasksAsync(userId, filtersWithoutStatus),
                    TaskViewType.AssignedByMe => await  GetAssignedByMeTasksAsync(userId, filtersWithoutStatus),
                    TaskViewType.Supervised => await  GetSupervisedTasksAsync(userId, filtersWithoutStatus),
                    _ => new List<Tasks>()
                };

                var uniqueAllTasks = allTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();

                // ⭐⭐⭐ مرحله 2: محاسبه Stats از همه تسک‌ها (بدون فیلتر وضعیت)
                model.Stats =  CalculateStats(uniqueAllTasks, userId);
                Console.WriteLine($"📊 Stats (از {uniqueAllTasks.Count} تسک):");
                Console.WriteLine($"   - Pending: {model.Stats.TotalPending}");
                Console.WriteLine($"   - Completed: {model.Stats.TotalCompleted}");
                Console.WriteLine($"   - Overdue: {model.Stats.TotalOverdue}");

                // ⭐⭐⭐ مرحله 3: دریافت تسک‌های فیلتر شده (با فیلتر وضعیت) برای نمایش
                List<Tasks> filteredTasks = viewType switch
                {
                    TaskViewType.MyTasks => await  GetMyTasksAsync(userId, filters),
                    TaskViewType.AssignedByMe => await  GetAssignedByMeTasksAsync(userId, filters),
                    TaskViewType.Supervised => await  GetSupervisedTasksAsync(userId, filters),
                    _ => new List<Tasks>()
                };

                var uniqueTasks = filteredTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
                Console.WriteLine($"🔍 تسک‌های فیلتر شده برای نمایش: {uniqueTasks.Count}");

                // ⭐⭐⭐ مرحله 4: گروه‌بندی و نمایش
                model.TaskGroups = await _groupingRepository.GroupTasksAsync(uniqueTasks, grouping, userId, viewType);

                // ⭐ پر کردن لیست‌های قدیمی (compatibility)
                model.Tasks = uniqueTasks.Select(t => MapToTaskViewModel(t)).ToList();
                model.PendingTasks = model.Tasks.Where(t => !IsTaskCompletedForUser(t.Id, userId)).ToList();
                model.CompletedTasks = model.Tasks.Where(t => IsTaskCompletedForUser(t.Id, userId)).ToList();

                await FillLegacyStatsAsync(model, userId);

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return new TaskListViewModel { UserLoginid = userId };
            }
        }

        /// <summary>
        /// دریافت اطلاعات کارت تسک
        /// </summary>
        public async Task<TaskCardViewModel> GetTaskCardViewModelAsync(int taskId, string userId)
        {
            var task = await _context.Tasks_Tbl
                .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.AssignedUser)
                .Include(t => t.TaskOperations)
                .Include(t => t.TaskCategory)
                .Include(t => t.Contact)
                .Include(t => t.Organization)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return null;

            // محاسبه پیشرفت
            var totalOps = task.TaskOperations.Count;
            var completedOps = task.TaskOperations.Count(o => o.IsCompleted);
            var progressPercentage = totalOps > 0 ? (completedOps * 100 / totalOps) : 0;

            // بررسی تکمیل شدن برای کاربر جاری
            var userAssignment = task.TaskAssignments.FirstOrDefault(a => a.AssignedUserId == userId);
            var isCompleted = userAssignment?.CompletionDate.HasValue ?? false;

            // تعیین نام Stakeholder (Contact یا Organization)
            string stakeholderName = "ندارد";
            if (task.Contact != null)
            {
                stakeholderName = $"{task.Contact.FirstName} {task.Contact.LastName}";
            }
            else if (task.Organization != null)
            {
                stakeholderName = task.Organization.DisplayName;
            }

            // محاسبه DaysRemaining
            int? daysRemaining = null;
            if (task.DueDate.HasValue)
            {
                daysRemaining = (task.DueDate.Value.Date - DateTime.Now.Date).Days;
            }

            // تبدیل به ViewModel
            return new TaskCardViewModel
            {
                Id = task.Id,
                CardNumber = 0, // باید از بیرون set شود
                Title = task.Title,
                ShortDescription = task.Description?.Length > 100
                    ? task.Description.Substring(0, 100) + "..."
                    : task.Description,
                TaskCode = task.TaskCode,
                Priority = task.Priority,

                // وضعیت‌ها
                IsCompleted = isCompleted,
                IsOverdue = task.DueDate.HasValue &&
                           task.DueDate.Value < DateTime.Now &&
                           !isCompleted,

                // تاریخ‌ها
                DueDate = task.DueDate,
                DueDatePersian = task.DueDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd")
                    : null,
                CreateDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.CreateDate, "yyyy/MM/dd"),

                // افراد
                CreatorName = task.Creator != null
                    ? $"{task.Creator.FirstName} {task.Creator.LastName}"
                    : "نامشخص",
                StakeholderName = stakeholderName,

                // دسته‌بندی
                CategoryTitle = task.TaskCategory?.Title ?? "بدون دسته",
                CategoryBadgeClass = GetCategoryBadgeClass(task.TaskCategoryId),

                // اولویت
                PriorityText = GetPriorityText(task.Priority),
                PriorityBadgeClass = GetPriorityBadgeClass(task.Priority),

                // وضعیت
                StatusText = GetTaskStatusText(task.Status),
                StatusBadgeClass = GetTaskStatusBadgeClass(task.Status),

                // پیشرفت
                TotalOperations = totalOps,
                CompletedOperations = completedOps,
                ProgressPercentage = progressPercentage,

                // زمان باقیمانده
                DaysRemaining = daysRemaining,

                // دسترسی‌ها
                CanEdit = task.CreatorUserId == userId,
                CanDelete = task.CreatorUserId == userId,
                CanComplete = userAssignment != null && !isCompleted
            };
        }

        #region Helper Methods for Cards

      
        private async Task FillLegacyStatsAsync(TaskListViewModel model, string userId)
        {
            try
            {
                // ⭐ محاسبه آمار برای Compatibility با کدهای قدیمی
                model.FilterCounts = new TaskFilterCountsViewModel
                {
                    AllVisibleCount = model.Tasks.Count,
                    MyAssignedCount = model.Tasks.Count(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)),
                    AssignedByMeCount = model.Tasks.Count(t => t.CreatorUserId == userId),
                    MyTeamsCount = 0,
                    SupervisedCount = model.Tasks.Count(t => t.CreatorUserId != userId)
                };

                // ⭐ پر کردن GroupedTasks
                model.GroupedTasks = new TaskGroupedViewModel
                {
                    MyTasks = model.Tasks.Where(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId)).ToList(),

                    AssignedToMe = model.Tasks.Where(t =>
                        t.AssignmentsTaskUser != null &&
                        t.AssignmentsTaskUser.Any(a => a.AssignedUserId == userId) &&
                        t.CreatorUserId != userId).ToList(),

                    TeamMemberTasks = new Dictionary<string, List<TaskViewModel>>(),
                    SubTeamTasks = new Dictionary<string, List<TaskViewModel>>(),
                    MyTasksGrouped = new MyTasksGroupedViewModel(),
                    TeamTasksGrouped = new Dictionary<string, Dictionary<string, List<TaskViewModel>>>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in FillLegacyStatsAsync: {ex.Message}");
            }
        }

        private string GetCategoryBadgeClass(int? categoryId)
        {
            if (!categoryId.HasValue) return "bg-secondary";
            return "bg-info";
        }

        private string GetPriorityText(byte priority)
        {
            return priority switch
            {
                2 => "فوری",
                1 => "مهم",
                _ => "عادی"
            };
        }

        private string GetPriorityBadgeClass(byte priority)
        {
            return priority switch
            {
                2 => "bg-danger",
                1 => "bg-warning",
                _ => "bg-primary"
            };
        }

        #endregion

        #endregion
    }
}
