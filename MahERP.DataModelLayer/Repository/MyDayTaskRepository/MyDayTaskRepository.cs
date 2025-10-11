using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.MyDayTaskRepository
{
    public class MyDayTaskRepository : IMyDayTaskRepository
    {
        private readonly AppDbContext _context;
        private readonly ITaskHistoryRepository _taskHistoryRepository;

        public MyDayTaskRepository(
            AppDbContext context,
            ITaskHistoryRepository taskHistoryRepository)
        {
            _context = context;
            _taskHistoryRepository = taskHistoryRepository;
        }

        public async Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now.AddDays(7);

            var myDayTasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                .Include(tmd => tmd.TaskAssignment.Task.Contact)
                .Include(tmd => tmd.TaskAssignment.Task.Organization)
                .Include(tmd => tmd.TaskAssignment.Task.TaskOperations.Where(o => !o.IsDeleted)) // ⭐ اضافه شده
                .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    tmd.PlannedDate >= start.Date &&
                    tmd.PlannedDate <= end.Date)
                .OrderByDescending(tmd => tmd.PlannedDate)
                .ToListAsync();

            var grouped = myDayTasks
                .GroupBy(tmd => tmd.PlannedDate.Date)
                .ToDictionary(
                    g => ConvertDateTime.ConvertMiladiToShamsi(g.Key, "yyyy/MM/dd"),
                    g => g.Select(MapToMyDayTaskItem).ToList()
                );

            var stats = CalculateStats(myDayTasks);

            return new MyDayTasksViewModel
            {
                TasksByDate = grouped,
                Stats = stats,
                SelectedDate = DateTime.Now.Date,
                SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd")
            };
        }

        public async Task<List<MyDayTaskItemViewModel>> GetTasksForDateAsync(string userId, DateTime date)
        {
            var tasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                .Include(tmd => tmd.TaskAssignment.Task.Contact)
                .Include(tmd => tmd.TaskAssignment.Task.Organization)
                .Include(tmd => tmd.TaskAssignment.Task.TaskOperations.Where(o => !o.IsDeleted)) // ⭐ اضافه شده
                .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    tmd.PlannedDate.Date == date.Date)
                .OrderByDescending(tmd => tmd.TaskAssignment.IsFocused)
                .ThenBy(tmd => tmd.CreatedDate)
                .ToListAsync();

            return tasks.Select(MapToMyDayTaskItem).ToList();
        }
        public async Task<(bool Success, string Message, int? MyDayId)> AddTaskToMyDayAsync(
            int taskAssignmentId,
            string userId,
            DateTime plannedDate,
            string? planNote = null)
        {
            // بررسی وجود TaskAssignment
            var assignment = await _context.TaskAssignment_Tbl
                .Include(ta => ta.Task)
                .FirstOrDefaultAsync(ta => ta.Id == taskAssignmentId && ta.AssignedUserId == userId);

            if (assignment == null)
                return (false, "تسک یافت نشد یا به شما تخصیص داده نشده است", null);

            // بررسی تکراری نبودن
            var exists = await _context.TaskMyDay_Tbl
                .AnyAsync(tmd =>
                    tmd.TaskAssignmentId == taskAssignmentId &&
                    tmd.PlannedDate.Date == plannedDate.Date &&
                    !tmd.IsRemoved);

            if (exists)
                return (false, "این تسک قبلاً در این تاریخ به روز من اضافه شده است", null);

            var myDayTask = new TaskMyDay
            {
                TaskAssignmentId = taskAssignmentId,
                PlannedDate = plannedDate.Date,
                PlanNote = planNote,
                CreatedDate = DateTime.Now
            };

            _context.TaskMyDay_Tbl.Add(myDayTask);
            await _context.SaveChangesAsync();

            // ثبت در تاریخچه
            await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                assignment.TaskId,
                userId,
                assignment.Task.Title,
                assignment.Task.TaskCode,
                plannedDate);

            return (true, "تسک با موفقیت به روز من اضافه شد", myDayTask.Id);
        }

        public async Task<(bool Success, string Message)> RemoveTaskFromMyDayAsync(int myDayId, string userId)
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

            // ⭐⭐⭐ بررسی تکمیل شده بودن
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

        public async Task<(bool Success, string Message)> SetTaskAsFocusedAsync(int myDayId, string userId)
        {
            var myDayTask = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                .FirstOrDefaultAsync(tmd => tmd.Id == myDayId);

            if (myDayTask == null || myDayTask.TaskAssignment.AssignedUserId != userId)
                return (false, "تسک یافت نشد");

            // ⭐⭐⭐ بررسی تکمیل شده بودن
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

        public async Task<bool> IsTaskInMyDayAsync(int taskAssignmentId, string userId, DateTime date)
        {
            return await _context.TaskMyDay_Tbl
                .AnyAsync(tmd =>
                    tmd.TaskAssignmentId == taskAssignmentId &&
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    tmd.PlannedDate.Date == date.Date &&
                    !tmd.IsRemoved);
        }

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

        // ======== Helper Methods ========

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
        // ======== Helper Methods ======== (اصلاح شده)

        /// <summary>
        /// محاسبه درصد پیشرفت تسک
        /// </summary>
        private int CalculateProgress(Tasks task, TaskAssignment assignment)
        {
            // ⭐ اگر تسک تکمیل شده، پیشرفت 100%
            if (assignment.CompletionDate.HasValue)
                return 100;

            // ⭐ اگر عملیاتی نداره، بر اساس Status محاسبه کن
            if (task.TaskOperations == null || !task.TaskOperations.Any())
            {
                return assignment.Status switch
                {
                    0 => 0,   // تخصیص داده شده
                    1 => 25,  // مشاهده شده
                    2 => 50,  // در حال انجام
                    3 => 100, // تکمیل شده
                    _ => 0
                };
            }

            // ⭐ محاسبه بر اساس عملیات‌های تکمیل شده
            var totalOperations = task.TaskOperations.Count(o => !o.IsDeleted);
            if (totalOperations == 0) return 0;

            var completedOperations = task.TaskOperations.Count(o => o.IsCompleted && !o.IsDeleted);
            return (int)Math.Round((double)completedOperations / totalOperations * 100);
        }

        /// <summary>
        /// بررسی قابل ویرایش بودن تسک
        /// </summary>
        private bool IsTaskEditable(TaskAssignment assignment)
        {
            // ⭐ تسک تکمیل شده قابل ویرایش نیست
            return !assignment.CompletionDate.HasValue;
        }

        private MyDayTaskItemViewModel MapToMyDayTaskItem(TaskMyDay myDayTask)
        {
            var task = myDayTask.TaskAssignment.Task;
            var assignment = myDayTask.TaskAssignment;

            // ⭐⭐⭐ محاسبه پیشرفت
            var progressPercentage = CalculateProgress(task, assignment);

            // ⭐⭐⭐ بررسی قابل ویرایش بودن
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
                StakeholderName = task.Organization != null
    ? task.Organization.DisplayName
    : (task.Contact != null
        ? task.Contact.FullName
        : "نامشخص"),
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
                ProgressPercentage = progressPercentage, // ⭐ اضافه شده
               
                PlannedDate = myDayTask.PlannedDate,
                PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(myDayTask.PlannedDate, "yyyy/MM/dd")
            };
        }

        /// <summary>
        /// دریافت لیست گزارش کارهای یک تسک
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
        /// دریافت اطلاعات تسک در "روز من" برای مودال حذف
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
    }
}