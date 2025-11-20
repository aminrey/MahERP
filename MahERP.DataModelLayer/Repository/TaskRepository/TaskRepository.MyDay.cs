using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
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
        /// اضافه کردن تسک به "روز من"
        /// </summary>
        public async Task<bool> AddTaskToMyDayAsync(
            int taskId,
            string userId,
            DateTime plannedDate,
            string? planNote = null)
        {
            try
            {
                // بررسی وجود assignment
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                    return false;

                // بررسی تکراری نبودن
                var exists = await _context.TaskMyDay_Tbl
                    .AnyAsync(tmd => tmd.TaskAssignmentId == assignment.Id &&
                                    tmd.PlannedDate.Date == plannedDate.Date &&
                                    !tmd.IsRemoved);

                if (exists)
                    return false;

                var myDayEntry = new TaskMyDay
                {
                    TaskAssignmentId = assignment.Id,
                    PlannedDate = plannedDate.Date,
                    PlanNote = planNote,
                    AddedDate = DateTime.Now,
                    IsRemoved = false
                };

                _context.TaskMyDay_Tbl.Add(myDayEntry);
                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                var task = await GetTaskByIdAsync(taskId);
                await _taskHistoryRepository.LogTaskAddedToMyDayAsync(
                    taskId,
                    userId,
                    task.Title,
                    task.TaskCode,
                    plannedDate
                );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddTaskToMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<bool> LogTaskWorkAsync(
            int taskId,
            string userId,
            string? workNote = null,
            int? workDurationMinutes = null)
        {
            try
            {
                // بررسی وجود assignment
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                    return false;

                // بررسی وجود MyDay entry
                var myDayEntry = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd => tmd.TaskAssignmentId == assignment.Id &&
                                                tmd.PlannedDate.Date == DateTime.Now.Date &&
                                                !tmd.IsRemoved);

                if (myDayEntry == null)
                    return false;

                // به‌روزرسانی WorkNote
                if (!string.IsNullOrEmpty(workNote))
                {
                    myDayEntry.WorkNote = workNote;
                }

                // به‌روزرسانی WorkDuration
                if (workDurationMinutes.HasValue)
                {
                    myDayEntry.WorkDurationMinutes = (myDayEntry.WorkDurationMinutes ?? 0) + workDurationMinutes.Value;
                }

                myDayEntry.LastWorkDate = DateTime.Now;

                _context.TaskMyDay_Tbl.Update(myDayEntry);
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
        /// دریافت تسک‌های "روز من" برای کاربر
        /// </summary>
        public async Task<MyDayTasksViewModel> GetMyDayTasksAsync(
            string userId,
            DateTime? selectedDate = null)
        {
            try
            {
                var targetDate = selectedDate ?? DateTime.Now;

                var myDayTasks = await _context.TaskMyDay_Tbl
                    .Include(tmd => tmd.TaskAssignment)
                        .ThenInclude(ta => ta.Task)
                            .ThenInclude(t => t.TaskCategory)
                    .Include(tmd => tmd.TaskAssignment.Task.TaskOperations)
                    .Where(tmd => tmd.TaskAssignment.AssignedUserId == userId &&
                                  tmd.PlannedDate.Date == targetDate.Date &&
                                  !tmd.IsRemoved)
                    .OrderBy(tmd => tmd.AddedDate)
                    .ToListAsync();

                var taskViewModels = myDayTasks.Select(tmd =>
                {
                    var task = tmd.TaskAssignment.Task;
                    var viewModel = _mapper.Map<TaskViewModel>(task);

                    // اضافه کردن اطلاعات MyDay
                    viewModel.MyDayPlanNote = tmd.PlanNote;
                    viewModel.MyDayWorkNote = tmd.WorkNote;
                    viewModel.MyDayWorkDuration = tmd.WorkDurationMinutes;
                    viewModel.MyDayAddedDate = tmd.AddedDate;
                    viewModel.IsCompleted = tmd.TaskAssignment.CompletionDate.HasValue;

                    return viewModel;
                }).ToList();

                return new MyDayTasksViewModel
                {
                    SelectedDate = targetDate,
                    SelectedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(targetDate, "yyyy/MM/dd"),
                    Tasks = taskViewModels,
                    TotalTasks = taskViewModels.Count,
                    CompletedTasks = taskViewModels.Count(t => t.IsCompleted),
                    PendingTasks = taskViewModels.Count(t => !t.IsCompleted),
                    TotalWorkDuration = myDayTasks.Sum(tmd => tmd.WorkDurationMinutes ?? 0)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetMyDayTasksAsync: {ex.Message}");
                return new MyDayTasksViewModel
                {
                    SelectedDate = selectedDate ?? DateTime.Now,
                    Tasks = new List<TaskViewModel>()
                };
            }
        }

        /// <summary>
        /// بررسی اینکه آیا تسک در "روز من" وجود دارد
        /// </summary>
        public async Task<bool> IsTaskInMyDayAsync(
            int taskId,
            string userId,
            DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.Now;

                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                    return false;

                return await _context.TaskMyDay_Tbl
                    .AnyAsync(tmd => tmd.TaskAssignmentId == assignment.Id &&
                                    tmd.PlannedDate.Date == date.Date &&
                                    !tmd.IsRemoved);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// حذف تسک از "روز من"
        /// </summary>
        public async Task<bool> RemoveTaskFromMyDayAsync(
            int taskId,
            string userId,
            DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.Now;

                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                    return false;

                var myDayEntry = await _context.TaskMyDay_Tbl
                    .FirstOrDefaultAsync(tmd => tmd.TaskAssignmentId == assignment.Id &&
                                                tmd.PlannedDate.Date == date.Date &&
                                                !tmd.IsRemoved);

                if (myDayEntry == null)
                    return false;

                myDayEntry.IsRemoved = true;
                myDayEntry.RemovedDate = DateTime.Now;

                _context.TaskMyDay_Tbl.Update(myDayEntry);
                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                var task = await GetTaskByIdAsync(taskId);
                await _taskHistoryRepository.LogTaskRemovedFromMyDayAsync(
                    taskId,
                    userId,
                    task.Title,
                    task.TaskCode
                );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveTaskFromMyDayAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت تعداد تسک‌های "روز من" برای کاربر
        /// </summary>
        public async Task<int> GetMyDayTasksCountAsync(
            string userId,
            DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.Now;

                return await _context.TaskMyDay_Tbl
                    .CountAsync(tmd => tmd.TaskAssignment.AssignedUserId == userId &&
                                      tmd.PlannedDate.Date == date.Date &&
                                      !tmd.IsRemoved);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// محاسبه پیشرفت تسک بر اساس عملیات‌ها
        /// </summary>
        private int CalculateTaskProgress(TaskViewModel task)
        {
            if (task.Operations == null || !task.Operations.Any())
                return 0;

            var completedOps = task.Operations.Count(o => o.IsCompleted);
            return (int)((double)completedOps / task.Operations.Count * 100);
        }

        #endregion
    }
}
