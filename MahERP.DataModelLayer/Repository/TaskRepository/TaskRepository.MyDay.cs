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
    /// </summary>
    public partial class TaskRepository
    {
        #region My Day Methods

        /// <summary>
        /// اضافه کردن تسک به "روز من" - اصلاح شده برای استفاده از TaskAssignment
        /// </summary>
        public async Task<bool> AddTaskToMyDayAsync(int taskId, string userId, DateTime plannedDate, string? planNote = null)
        {
            try
            {
                // ⭐ دریافت TaskAssignment مربوطه
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null)
                {
                    Console.WriteLine($"❌ Assignment not found for Task {taskId} and User {userId}");
                    return false; // کاربر به این تسک اختصاص داده نشده
                }

                // ⭐ بررسی وجود رکورد قبلی در همین تاریخ
                var existingRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == plannedDate.Date &&
                        !tmd.IsRemoved);

                if (existingRecord != null)
                {
                    // ⭐ بروزرسانی یادداشت
                    existingRecord.PlanNote = planNote;
                    existingRecord.UpdatedDate = DateTime.Now;
                    _context.TaskMyDay_Tbl.Update(existingRecord);
                }
                else
                {
                    // ⭐ ایجاد رکورد جدید
                    var newRecord = new TaskMyDay
                    {
                        TaskAssignmentId = assignment.Id,
                        PlannedDate = plannedDate.Date,
                        PlanNote = planNote,
                        CreatedDate = DateTime.Now,
                        IsRemoved = false
                    };

                    await _context.TaskMyDay_Tbl.AddAsync(newRecord);
                }

                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                    taskId,
                    userId,
                    assignment.Task?.Title ?? "نامشخص",
                    assignment.Task?.TaskCode ?? string.Empty,
                    plannedDate);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddTaskToMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک - اصلاح شده
        /// </summary>
        public async Task<bool> LogTaskWorkAsync(int taskId, string userId, string? workNote = null, int? workDurationMinutes = null)
        {
            try
            {
                var today = DateTime.Now.Date;

                // ⭐ دریافت TaskAssignment
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null)
                {
                    Console.WriteLine($"❌ Assignment not found for Task {taskId} and User {userId}");
                    return false;
                }

                // ⭐ پیدا کردن یا ایجاد رکورد "روز من"
                var myDayRecord = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == today &&
                        !tmd.IsRemoved);

                if (myDayRecord == null)
                {
                    // اگر در "روز من" نیست، ایجاد کن
                    myDayRecord = new TaskMyDay
                    {
                        TaskAssignmentId = assignment.Id,
                        PlannedDate = today,
                        CreatedDate = DateTime.Now,
                        IsRemoved = false
                    };
                    await _context.TaskMyDay_Tbl.AddAsync(myDayRecord);
                }

                // ⭐ بروزرسانی اطلاعات کار
                myDayRecord.WorkStartDate = DateTime.Now;
                myDayRecord.WorkNote = workNote;
                myDayRecord.WorkDurationMinutes = workDurationMinutes;
                myDayRecord.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogTaskWorkAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت تسک‌های "روز من" برای کاربر - اصلاح شده برای نمایش امروز، فردا و دیروز
        /// </summary>
        public async Task<MyDayTasksViewModel> GetMyDayTasksAsync(string userId, DateTime? selectedDate = null)
        {
            var targetDate = selectedDate?.Date ?? DateTime.Now.Date;
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // ⭐ کوئری اصلاح شده
            var myDayTasks = await _context.TaskMyDay_Tbl
                .Include(tmd => tmd.TaskAssignment)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
  .Include(tmd => tmd.TaskAssignment.Task.Contact)
    .Include(tmd => tmd.TaskAssignment.Task.Organization)

    .Where(tmd =>
                    tmd.TaskAssignment.AssignedUserId == userId &&
                    !tmd.IsRemoved &&
                    (tmd.PlannedDate.Date == yesterday ||
                     tmd.PlannedDate.Date == today ||
                     tmd.PlannedDate.Date == tomorrow))
                .OrderBy(tmd => tmd.PlannedDate)
                .ThenBy(tmd => tmd.CreatedDate)
                .ToListAsync();

            var result = new MyDayTasksViewModel
            {
                SelectedDate = targetDate,
                SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(targetDate, "yyyy/MM/dd"),
                PlannedTasks = new List<MyDayTaskItemViewModel>(),
                WorkedTasks = new List<MyDayTaskItemViewModel>(),
                TasksByDate = new Dictionary<string, List<MyDayTaskItemViewModel>>()
            };

            foreach (var myDayTask in myDayTasks)
            {
                var task = myDayTask.TaskAssignment.Task;
                var isWorkedOn = !string.IsNullOrEmpty(myDayTask.WorkNote) || myDayTask.WorkStartDate.HasValue;
                // ⭐ تعیین نام (Contact یا Organization)
                string displayName = "ندارد";
                if (task.Contact != null)
                {
                    displayName = $"{task.Contact.FirstName} {task.Contact.LastName}";
                }
                else if (task.Organization != null)
                {
                    displayName = task.Organization.DisplayName;
                }
                var taskItem = new MyDayTaskItemViewModel
                {
                    TaskId = task.Id,
                    TaskAssignmentId = myDayTask.TaskAssignmentId,
                    TaskCode = task.TaskCode,
                    TaskTitle = task.Title,
                    TaskDescription = task.Description,
                    CategoryTitle = task.TaskCategory?.Title,
                    StakeholderName = displayName,
                    TaskPriority = task.Priority,
                    IsImportant = task.Important,
                    IsFocused = myDayTask.TaskAssignment.IsFocused,
                    PlanNote = myDayTask.PlanNote,
                    WorkNote = myDayTask.WorkNote,
                    WorkDurationMinutes = myDayTask.WorkDurationMinutes,
                    IsWorkedOn = isWorkedOn,
                    WorkStartDate = myDayTask.WorkStartDate,
                    CreatedDate = myDayTask.CreatedDate,
                    TaskStatus = myDayTask.TaskAssignment.Status,
                    ProgressPercentage = CalculateTaskProgress(task),
                    PlannedDate = myDayTask.PlannedDate,
                    PlannedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(myDayTask.PlannedDate, "yyyy/MM/dd"),
                };

                // گروه‌بندی بر اساس تاریخ
                var dateKey = taskItem.PlannedDatePersian;
                if (!result.TasksByDate.ContainsKey(dateKey))
                {
                    result.TasksByDate[dateKey] = new List<MyDayTaskItemViewModel>();
                }
                result.TasksByDate[dateKey].Add(taskItem);

                // همچنان نگهداری لیست‌های قدیمی برای سازگاری
                if (isWorkedOn)
                    result.WorkedTasks.Add(taskItem);
                else
                    result.PlannedTasks.Add(taskItem);
            }

            // محاسبه آمار کلی
            result.Stats = new MyDayStatsViewModel
            {
                TotalPlannedTasks = result.PlannedTasks.Count + result.WorkedTasks.Count,
                WorkedTasks = result.WorkedTasks.Count,
                CompletedTasks = result.WorkedTasks.Count(x => x.TaskStatus >= 2),
                TotalWorkTimeMinutes = result.WorkedTasks.Sum(x => x.WorkDurationMinutes ?? 0)
            };

            return result;
        }


        /// <summary>
        /// بررسی اینکه آیا تسک در "روز من" وجود دارد - اصلاح شده
        /// </summary>
        public async Task<bool> IsTaskInMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            var checkDate = targetDate?.Date ?? DateTime.Now.Date;

            // ⭐ کوئری اصلاح شده
            var assignment = await _context.TaskAssignment_Tbl
                .FirstOrDefaultAsync(a =>
                    a.TaskId == taskId &&
                    a.AssignedUserId == userId);

            if (assignment == null) return false;

            return await _context.TaskMyDay_Tbl
                .AnyAsync(tmd =>
                    tmd.TaskAssignmentId == assignment.Id &&
                    tmd.PlannedDate.Date == checkDate &&
                    !tmd.IsRemoved);
        }

        /// <summary>
        /// حذف تسک از "روز من" - اصلاح شده
        /// </summary>
        public async Task<bool> RemoveTaskFromMyDayAsync(int taskId, string userId, DateTime? targetDate = null)
        {
            try
            {
                var checkDate = targetDate?.Date ?? DateTime.Now.Date;

                // ⭐ کوئری اصلاح شده
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a =>
                        a.TaskId == taskId &&
                        a.AssignedUserId == userId);

                if (assignment == null) return false;

                var record = await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment.Task)
                    .FirstOrDefaultAsync(tmd =>
                        tmd.TaskAssignmentId == assignment.Id &&
                        tmd.PlannedDate.Date == checkDate &&
                        !tmd.IsRemoved);

                if (record != null)
                {
                    record.IsRemoved = true;
                    record.RemovedDate = DateTime.Now;
                    record.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // ⭐ ثبت در تاریخچه
                    await _taskHistoryRepository.LogTaskRemovedFromMyDayAsync(
                        taskId,
                        userId,
                        record.TaskAssignment.Task?.Title ?? "نامشخص",
                        record.TaskAssignment.Task?.TaskCode ?? string.Empty);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveTaskFromMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای کاربر - اصلاح شده
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
        #endregion

       
    }
}
