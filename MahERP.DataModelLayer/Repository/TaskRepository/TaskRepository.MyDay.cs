using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت "روز من" (My Day)
    /// شامل: افزودن به روز من، حذف، ثبت کار، دریافت تسک‌های روز
    /// ⭐⭐⭐ ادغام شده با MyDayTaskRepository
    /// </summary>
    public partial class TaskRepository
    {
        #region My Day Methods

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های "روز من" با گروه‌بندی پیشرفته
        /// </summary>
        public async Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddYears(-2);
            var end = endDate ?? DateTime.Now.AddYears(2);

            var myDayTasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                .Include(tmd => tmd.TaskAssignment.Task.Contact)
                .Include(tmd => tmd.TaskAssignment.Task.Organization)
                .Include(tmd => tmd.TaskAssignment.Task.TaskOperations.Where(o => !o.IsDeleted))
                .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    tmd.PlannedDate >= start.Date &&
                    tmd.PlannedDate <= end.Date)
                .OrderByDescending(tmd => tmd.PlannedDate)
                .ToListAsync();

            // ⭐⭐⭐ گروه‌بندی قدیمی (برای backward compatibility)
            var grouped = myDayTasks
                .GroupBy(tmd => tmd.PlannedDate.Date)
                .ToDictionary(
                    g => ConvertDateTime.ConvertMiladiToShamsi(g.Key, "yyyy/MM/dd"),
                    g => g.Select(MapToMyDayTaskItem).ToList()
                );

            // ⭐⭐⭐ NEW - گروه‌بندی دوبعدی: GroupTitle → Date → Tasks
            var groupedByTitleAndDate = myDayTasks
                .Select(MapToMyDayTaskItem)
                .GroupBy(t => string.IsNullOrWhiteSpace(t.GroupTitle) ? "بدون گروه" : t.GroupTitle)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(t => t.PlannedDatePersian)
                          .ToDictionary(
                              dateGroup => dateGroup.Key,
                              dateGroup => dateGroup.ToList()
                          )
                );

            var stats = CalculateStats(myDayTasks);

            return new MyDayTasksViewModel
            {
                TasksByDate = grouped,
                TasksByGroupAndDate = groupedByTitleAndDate,
                Stats = stats,
                SelectedDate = DateTime.Now.Date,
                SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd")
            };
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک‌های یک روز خاص
        /// </summary>
        public async Task<List<MyDayTaskItemViewModel>> GetTasksForDateAsync(string userId, DateTime date)
        {
            var tasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                .Include(tmd => tmd.TaskAssignment.Task.Contact)
                .Include(tmd => tmd.TaskAssignment.Task.Organization)
                .Include(tmd => tmd.TaskAssignment.Task.TaskOperations.Where(o => !o.IsDeleted))
                .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    tmd.PlannedDate.Date == date.Date)
                .OrderByDescending(tmd => tmd.TaskAssignment.IsFocused)
                .ThenBy(tmd => tmd.CreatedDate)
                .ToListAsync();

            return tasks.Select(MapToMyDayTaskItem).ToList();
        }

        /// <summary>
        /// ⭐⭐⭐ افزودن تسک به "روز من" با پشتیبانی از GroupTitle
        /// </summary>
        public async Task<(bool Success, string Message, int? MyDayId)> AddTaskToMyDayAsync(
            int taskId, 
            string userId, 
            DateTime plannedDate, 
            string? planNote = null,
            string? groupTitle = null)
        {
            try
            {
                // دریافت TaskAssignment
                var assignment = await _context.TaskAssignment_Tbl
                    .Where(ta=> ta.TaskId == taskId && ta.AssignedUserId == userId)
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    Console.WriteLine($"❌ Assignment not found for Task {taskId} and User {userId}");
                    return (false, "تسک یافت نشد یا به شما تخصیص داده نشده است", null);
                }

                // بررسی تکراری نبودن
                var existingRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == plannedDate.Date &&
                        !tmd.IsRemoved);

                if (existingRecord != null)
                {
                    // بروزرسانی
                    existingRecord.PlanNote = planNote;
                    existingRecord.GroupTitle = groupTitle;
                    existingRecord.UpdatedDate = DateTime.Now;
                    _context.TaskMyDay_Tbl.Update(existingRecord);
                    await _context.SaveChangesAsync();

                    return (true, "اطلاعات تسک در روز من بروزرسانی شد", existingRecord.Id);
                }

                // ایجاد رکورد جدید
                var myDayTask = new TaskMyDay
                {
                    TaskAssignmentId = assignment.Id,
                    PlannedDate = plannedDate.Date,
                    PlanNote = planNote,
                    GroupTitle = groupTitle,
                    CreatedDate = DateTime.Now,
                    IsRemoved = false
                };

                await _context.TaskMyDay_Tbl.AddAsync(myDayTask);
                await _context.SaveChangesAsync();

                // ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                    taskId,
                    userId,
                    assignment.Task?.Title ?? "نامشخص",
                    assignment.Task?.TaskCode ?? string.Empty,
                    plannedDate);

                return (true, "تسک با موفقیت به روز من اضافه شد", myDayTask.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddTaskToMyDayAsync: {ex.Message}");
                return (false, "خطا در افزودن تسک به روز من", null);
            }
        }

        /// <summary>
        /// ⭐⭐⭐ شروع کار روی تسک
        /// </summary>
        public async Task<(bool Success, string Message)> StartWorkOnTaskAsync(int myDayId, string userId)
        {
            var myDayTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

            if (myDayTask == null || myDayTask.TaskAssignment.AssignedUserId != userId)
                return (false, "تسک یافت نشد");

            myDayTask.WorkStartDate = DateTime.Now;
            myDayTask.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "شروع کار ثبت شد");
        }

        /// <summary>
        /// ⭐⭐⭐ ثبت گزارش کار انجام شده
        /// </summary>
        public async Task<(bool Success, string Message)> LogWorkAsync(
            int myDayId,
            string userId,
            string workNote,
            int? durationMinutes = null)
        {
            var myDayTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

            if (myDayTask == null || myDayTask.TaskAssignment.AssignedUserId != userId)
                return (false, "تسک یافت نشد");

            // بررسی تکمیل شده بودن
            if (myDayTask.TaskAssignment.CompletionDate.HasValue)
                return (false, "تسک تکمیل شده و قابل ویرایش نیست");

            myDayTask.WorkNote = workNote;
            myDayTask.WorkDurationMinutes = durationMinutes;
            myDayTask.UpdatedDate = DateTime.Now;

            if (!myDayTask.WorkStartDate.HasValue)
                myDayTask.WorkStartDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "گزارش کار ثبت شد");
        }

        /// <summary>
        /// ⭐⭐⭐ تنظیم تسک به عنوان متمرکز
        /// </summary>
        public async Task<(bool Success, string Message)> SetTaskAsFocusedAsync(int myDayId, string userId)
        {
            var myDayTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

            if (myDayTask == null || myDayTask.TaskAssignment.AssignedUserId != userId)
                return (false, "تسک یافت نشد");

            if (myDayTask.TaskAssignment.CompletionDate.HasValue)
                return (false, "تسک تکمیل شده و قابل تنظیم به عنوان متمرکز نیست");

            // حذف تمرکز از سایر تسک‌ها
            var otherAssignments = await _context.TaskAssignment_Tbl
                .Where(ta => ta.AssignedUserId == userId && ta.IsFocused)
                .ToListAsync();

            foreach (var assignment in otherAssignments)
            {
                assignment.IsFocused = false;
            }

            // تنظیم تمرکز جدید
            myDayTask.TaskAssignment.IsFocused = true;
            myDayTask.TaskAssignment.FocusedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "تسک به عنوان متمرکز تنظیم شد");
        }

        /// <summary>
        /// ⭐⭐⭐ حذف تمرکز از تسک
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveFocusFromTaskAsync(int myDayId, string userId)
        {
            var myDayTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

            if (myDayTask == null || myDayTask.TaskAssignment.AssignedUserId != userId)
                return (false, "تسک یافت نشد");

            myDayTask.TaskAssignment.IsFocused = false;
            await _context.SaveChangesAsync();

            return (true, "تمرکز از تسک حذف شد");
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک متمرکز فعلی
        /// </summary>
        public async Task<MyDayTaskItemViewModel?> GetFocusedTaskAsync(string userId)
        {
            var focusedTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                .Include(tmd => tmd.TaskAssignment.Task.Contact)
                .Include(tmd => tmd.TaskAssignment.Task.Organization)
                .FirstOrDefaultAsync(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    tmd.TaskAssignment.IsFocused &&
                    !tmd.IsRemoved);

            return focusedTask != null ? MapToMyDayTaskItem(focusedTask) : null;
        }

        /// <summary>
        /// بررسی وجود تسک در "روز من"
        /// </summary>
        public async Task<bool> IsTaskInMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            var assignment = await _context.TaskAssignment_Tbl
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

            if (assignment == null) return false;

            return await _context.TaskMyDay_Tbl
                .AnyAsync(tmd =>
                    tmd.TaskAssignmentId == assignment.Id &&
                    tmd.PlannedDate.Date == checkDate &&
                    !tmd.IsRemoved);
        }

        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveTaskFromMyDayAsync(int myDayId, string userId)
        {
            try
            {
                var myDayTask = await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment)
                        .ThenInclude(ta => ta.Task)
                    .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

                if (myDayTask == null)
                    return (false, "تسک یافت نشد");

                if (myDayTask.TaskAssignment.AssignedUserId != userId)
                    return (false, "شما مجاز به حذف این تسک نیستید");

                myDayTask.IsRemoved = true;
                myDayTask.RemovedDate = DateTime.Now;
                myDayTask.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                // ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskRemovedFromMyDayAsync(
                    myDayTask.TaskAssignment.TaskId,
                    userId,
                    myDayTask.TaskAssignment.Task.Title,
                    myDayTask.TaskAssignment.Task.TaskCode);

                return (true, "تسک از روز من حذف شد");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveTaskFromMyDayAsync: {ex.Message}");
                return (false, "خطا در حذف تسک از روز من");
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت آمار "روز من"
        /// </summary>
        public async Task<MyDayStatsViewModel> GetMyDayStatsAsync(string userId, DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Now.Date;

            var tasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    tmd.PlannedDate.Date == targetDate &&
                    !tmd.IsRemoved)
                .ToListAsync();

            return CalculateStats(tasks);
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من"
        /// </summary>
        public async Task<int> GetMyDayTasksCountAsync(string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            return await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .CountAsync(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    tmd.PlannedDate.Date == checkDate &&
                    !tmd.IsRemoved);
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست گزارش کارها
        /// </summary>
        public async Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId)
        {
            try
            {
                var workLogs = await _context.TaskWorkLog_Tbl
                    .Include(w => w.User)
                    .Where(w => w.TaskId == taskId && !w.IsDeleted)
                    .OrderByDescending(w => w.WorkDate)
                    .Select(w => new TaskWorkLogViewModel
                    {
                        Id = w.Id,
                        TaskId = w.TaskId,
                        WorkDescription = w.WorkDescription,
                        WorkDate = w.WorkDate,
                        WorkDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.WorkDate, "yyyy/MM/dd HH:mm"),
                        DurationMinutes = w.DurationMinutes,
                        ProgressPercentage = w.ProgressPercentage,
                        UserId = w.UserId,
                        UserName = w.User != null ? $"{w.User.FirstName} {w.User.LastName}" : "نامشخص"
                    })
                    .ToListAsync();

                return workLogs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskWorkLogsAsync: {ex.Message}");
                return new List<TaskWorkLogViewModel>();
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت اطلاعات تسک برای مودال حذف
        /// </summary>
        public async Task<(string TaskTitle, string TaskCode)?> GetMyDayTaskInfoForRemovalAsync(int myDayId)
        {
            try
            {
                var myDayTask = await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment.Task)
                    .Where(tmd => tmd.Id == myDayId)
                    .Select(tmd => new
                    {
                        tmd.TaskAssignment.Task.Title,
                        tmd.TaskAssignment.Task.TaskCode
                    })
                    .FirstOrDefaultAsync();

                if (myDayTask == null)
                    return null;

                return (myDayTask.Title, myDayTask.TaskCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetMyDayTaskInfoForRemovalAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت MyDayTask بر اساس taskId
        /// </summary>
        public async Task<TaskMyDay?> GetMyDayTaskByTaskIdAsync(int taskId, string userId)
        {
            try
            {
                return await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment)
                    .Where(tmd =>
                        tmd.TaskAssignment.TaskId == taskId &&
                        tmd.TaskAssignment.AssignedUserId == userId &&
                        !tmd.IsRemoved)
                    .OrderByDescending(tmd => tmd.PlannedDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetMyDayTaskByTaskIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست عناوین گروه‌های موجود برای یک کاربر
        /// </summary>
        public async Task<List<string>> GetMyDayGroupTitlesAsync(string userId)
        {
            try
            {
                return await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment)
                    .Where(tmd =>
                        tmd.TaskAssignment.AssignedUserId == userId &&
                        !tmd.IsRemoved &&
                        !string.IsNullOrWhiteSpace(tmd.GroupTitle))
                    .Select(tmd => tmd.GroupTitle)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetMyDayGroupTitlesAsync: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// محاسبه آمار
        /// </summary>
        private MyDayStatsViewModel CalculateStats(List<TaskMyDay> tasks)
        {
            return new MyDayStatsViewModel
            {
                TotalPlannedTasks = tasks.Count,
                WorkedTasks = tasks.Count(t => !string.IsNullOrEmpty(t.WorkNote) || t.WorkStartDate.HasValue),
                CompletedTasks = tasks.Count(t => t.TaskAssignment.CompletionDate.HasValue),
                TotalWorkTimeMinutes = tasks.Sum(t => t.WorkDurationMinutes ?? 0)
            };
        }

        /// <summary>
        /// محاسبه درصد پیشرفت
        /// </summary>
        private int CalculateProgress(Tasks task, TaskAssignment assignment)
        {
            if (assignment.CompletionDate.HasValue)
                return 100;

            if (task.TaskOperations == null || !task.TaskOperations.Any())
            {
                return assignment.Status switch
                {
                    0 => 0,
                    1 => 25,
                    2 => 50,
                    3 => 100,
                    _ => 0
                };
            }

            var totalOperations = task.TaskOperations.Count(o => !o.IsDeleted);
            if (totalOperations == 0) return 0;

            var completedOperations = task.TaskOperations.Count(o => o.IsCompleted && !o.IsDeleted);
            return (int)Math.Round((double)completedOperations / totalOperations * 100);
        }

        /// <summary>
        /// بررسی قابل ویرایش بودن
        /// </summary>
        private bool IsTaskEditable(TaskAssignment assignment)
        {
            return !assignment.CompletionDate.HasValue;
        }

        /// <summary>
        /// تبدیل به ViewModel
        /// </summary>
        private MyDayTaskItemViewModel MapToMyDayTaskItem(TaskMyDay myDayTask)
        {
            var task = myDayTask.TaskAssignment.Task;
            var assignment = myDayTask.TaskAssignment;

            var progressPercentage = CalculateProgress(task, assignment);
            var isEditable = IsTaskEditable(assignment);

            return new MyDayTaskItemViewModel
            {
                MyDayId = myDayTask.Id,
                TaskId = task.Id,
                TaskAssignmentId = myDayTask.TaskAssignmentId,
                TaskCode = task.TaskCode,
                TaskTitle = task.Title,
                TaskDescription = task.Description,
                CategoryTitle = task.TaskCategory?.Title,
                GroupTitle = myDayTask.GroupTitle,
                StakeholderName = task.Organization != null
                    ? task.Organization.DisplayName
                    : (task.Contact != null ? task.Contact.FullName : "نامشخص"),
                TaskPriority = task.Priority,
                IsImportant = task.Important,
                IsFocused = assignment.IsFocused,
                PlanNote = myDayTask.PlanNote,
                WorkNote = myDayTask.WorkNote,
                WorkDurationMinutes = myDayTask.WorkDurationMinutes,
                IsWorkedOn = !string.IsNullOrEmpty(myDayTask.WorkNote) || myDayTask.WorkStartDate.HasValue,
                WorkStartDate = myDayTask.WorkStartDate,
                CreatedDate = task.CreateDate,
                TaskStatus = assignment.Status,
                ProgressPercentage = progressPercentage,
                PlannedDate = myDayTask.PlannedDate,
                PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(myDayTask.PlannedDate, "yyyy/MM/dd")
            };
        }

        #endregion
    }
}
