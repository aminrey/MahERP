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
        /// ثبت کار انجام شده روی تسک
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
                // بررسی دسترسی کاربر
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (assignment == null)
                {
                    return (false, "شما عضو این تسک نیستید", null);
                }

                var workLog = new TaskWorkLog
                {
                    TaskId = taskId,
                    UserId = userId,
                    WorkDescription = workDescription,
                    WorkDate = DateTime.Now,
                    DurationMinutes = durationMinutes,
                    ProgressPercentage = progressPercentage,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _context.TaskWorkLog_Tbl.Add(workLog);
                await _context.SaveChangesAsync();

                // ثبت در تاریخچه
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
                return (false, $"خطا در ثبت گزارش کار: {ex.Message}", null);
            }
        }


        /// <summary>
        /// دریافت لیست WorkLog های یک تسک
        /// </summary>
        public async Task<List<TaskWorkLogViewModel>> GetTaskWorkLogsAsync(int taskId, int take = 0)
        {
            var query = _context.TaskWorkLog_Tbl
                .Include(w => w.User)
                .Where(w => w.TaskId == taskId && !w.IsDeleted)
                .OrderByDescending(w => w.WorkDate);

            if (take > 0)
            {
                query = (IOrderedQueryable<TaskWorkLog>)query.Take(take);
            }

            var workLogs = await query.ToListAsync();

            return workLogs.Select(w => new TaskWorkLogViewModel
            {
                Id = w.Id,
                TaskId = w.TaskId,
                WorkDescription = w.WorkDescription,
                WorkDate = w.WorkDate,
                WorkDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.WorkDate, "yyyy/MM/dd HH:mm"),
                DurationMinutes = w.DurationMinutes,
                ProgressPercentage = w.ProgressPercentage,
                UserId = w.UserId,
                UserName = w.User != null ? $"{w.User.FirstName} {w.User.LastName}" : "نامشخص",
                CreatedDate = w.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.CreatedDate, "yyyy/MM/dd HH:mm")
            }).ToList();
        }

        /// <summary>
        /// آماده‌سازی مودال ثبت کار انجام شده روی تسک
        /// </summary>
        public async Task<TaskWorkLogViewModel?> PrepareLogTaskWorkModalAsync(int taskId, string userId)
        {
            try
            {
                // بررسی دسترسی کاربر به تسک
                var assignment = await _context.TaskAssignment_Tbl
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);

                if (assignment == null)
                {
                    return null; // کاربر عضو این تسک نیست
                }

                // ⭐ دریافت اطلاعات تسک
                var task = assignment.Task;

                // ⭐ ایجاد ViewModel
                var model = new TaskWorkLogViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task?.Title ?? "نامشخص",
                    TaskCode = task?.TaskCode ?? string.Empty
                };

                return model;
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
