using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت گزارش کار تسک‌ها (Task Work Log)
    /// شامل: افزودن، دریافت، آماده‌سازی مودال ثبت کار
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Work Log Methods

        /// <summary>
        /// ثبت گزارش کار انجام شده روی تسک (سطح کلی تسک)
        /// </summary>
        public async Task<(bool Success, string Message, int? WorkLogId)> AddTaskWorkLogAsync(
            int taskId,
            string userId,
            string workDescription,
            int? durationMinutes = null,
            int? progressPercentage = null)
        {
            try
            {
                // بررسی اینکه کاربر عضو تسک است
                var isAssigned = await _context.TaskAssignment_Tbl
                    .AnyAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (!isAssigned)
                {
                    return (false, "شما عضو این تسک نیستید", null);
                }

                // ایجاد WorkLog
                var workLog = new TaskWorkLog
                {
                    TaskId = taskId,
                    UserId = userId,
                    WorkDescription = workDescription,
                    WorkDate = DateTime.Now,
                    DurationMinutes = durationMinutes,
                    ProgressPercentage = progressPercentage,
                    CreatedDate = DateTime.Now
                };

                _context.TaskWorkLog_Tbl.Add(workLog);
                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskWorkLogAddedAsync(
                    taskId,
                    userId,
                    workLog.Id,
                    workDescription,
                    durationMinutes
                );

                return (true, "گزارش کار با موفقیت ثبت شد", workLog.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AddTaskWorkLogAsync: {ex.Message}");
                return (false, $"خطا در ثبت گزارش کار: {ex.Message}", null);
            }
        }

        /// <summary>
        /// دریافت لیست گزارش کارهای یک تسک
        /// </summary>
        public async Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(
            int taskId,
            int take = 0)
        {
            try
            {
                var query = _context.TaskWorkLog_Tbl
                    .Include(wl => wl.User)
                    .Include(wl => wl.Task)
                    .Where(wl => wl.TaskId == taskId)
                    .OrderByDescending(wl => wl.WorkDate);

                if (take > 0)
                {
                    query = (IOrderedQueryable<TaskWorkLog>)query.Take(take);
                }

                var workLogs = await query.ToListAsync();

                return workLogs.Select(wl => new TaskWorkLogViewModel
                {
                    Id = wl.Id,
                    TaskId = wl.TaskId,
                    TaskTitle = wl.Task?.Title ?? "نامشخص",
                    TaskCode = wl.Task?.TaskCode ?? "نامشخص",
                    WorkDescription = wl.WorkDescription,
                    WorkDate = wl.WorkDate,
                    WorkDatePersian = ConvertDateTime.ConvertMiladiToShamsi(wl.WorkDate, "yyyy/MM/dd HH:mm"),
                    DurationMinutes = wl.DurationMinutes,
                    ProgressPercentage = wl.ProgressPercentage,
                    UserId = wl.UserId,
                    UserName = wl.User != null ? $"{wl.User.FirstName} {wl.User.LastName}" : "نامشخص",
                    CreatedDate = wl.CreatedDate,
                    CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(wl.CreatedDate, "yyyy/MM/dd HH:mm")
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskWorkLogsAsync: {ex.Message}");
                return new List<TaskWorkLogViewModel>();
            }
        }

        /// <summary>
        /// آماده‌سازی مودال ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<TaskWorkLogViewModel?> PrepareLogTaskWorkModalAsync(
            int taskId,
            string userId)
        {
            try
            {
                // بررسی عضویت کاربر در تسک
                var assignment = await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                        .ThenInclude(t => t.TaskCategory)
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                {
                    return null;
                }

                var task = assignment.Task;

                // محاسبه پیشرفت فعلی
                var operations = await _context.TaskOperation_Tbl
                    .Where(o => o.TaskId == taskId)
                    .ToListAsync();

                var currentProgress = 0;
                if (operations.Any())
                {
                    currentProgress = (int)((double)operations.Count(o => o.IsCompleted) / operations.Count * 100);
                }

                // محاسبه مجموع زمان کار
                var totalWorkTime = await _context.TaskWorkLog_Tbl
                    .Where(wl => wl.TaskId == taskId)
                    .SumAsync(wl => wl.DurationMinutes ?? 0);

                return new TaskWorkLogViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    CategoryTitle = task.TaskCategory?.Title,
                    UserId = userId,
                    WorkDate = DateTime.Now,
                    ProgressPercentage = currentProgress,
                    TotalWorkTime = totalWorkTime,
                    DurationMinutes = null,
                    WorkDescription = string.Empty
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PrepareLogTaskWorkModalAsync: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
