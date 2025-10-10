using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    public class TaskOperationsRepository : ITaskOperationsRepository
    {
        private readonly AppDbContext _context;
        private readonly ITaskRepository _taskRepository;

        public TaskOperationsRepository(AppDbContext context, ITaskRepository taskRepository)
        {
            _context = context;
            _taskRepository = taskRepository;
        }

        #region Task Operation CRUD

        public async Task<TaskOperation?> GetOperationByIdAsync(int id, bool includeWorkLogs = false)
        {
            var query = _context.TaskOperation_Tbl.AsQueryable();

            if (includeWorkLogs)
            {
                query = query.Include(o => o.WorkLogs)
                            .ThenInclude(w => w.User);
            }

            query = query.Include(o => o.Task)
                        .Include(o => o.CompletedByUser);

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public TaskOperation? GetOperationById(int id)
        {
            return _context.TaskOperation_Tbl
                .Include(o => o.Task)
                .FirstOrDefault(o => o.Id == id);
        }

        public async Task<List<TaskOperation>> GetTaskOperationsAsync(int taskId, bool includeCompleted = true)
        {
            var query = _context.TaskOperation_Tbl
                .Include(o => o.WorkLogs.Where(w => !w.IsDeleted))
                    .ThenInclude(w => w.User)
                .Include(o => o.CompletedByUser)
                .Where(o => o.TaskId == taskId);

            if (!includeCompleted)
            {
                query = query.Where(o => !o.IsCompleted);
            }

            return await query
                .OrderByDescending(o => o.IsStarred)
                .ThenBy(o => o.IsCompleted)
                .ThenBy(o => o.OperationOrder)
                .ToListAsync();
        }

        public async Task<bool> CanUserAccessOperationAsync(int operationId, string userId)
        {
            var operation = await GetOperationByIdAsync(operationId);
            if (operation == null) return false;

            return _taskRepository.IsUserRelatedToTask(userId, operation.TaskId);
        }

        #endregion

        #region Toggle Actions

        public async Task<(bool Success, string Message)> ToggleOperationStarAsync(int operationId, string userId)
        {
            try
            {
                var operation = await GetOperationByIdAsync(operationId);
                if (operation == null)
                {
                    return (false, "عملیات یافت نشد");
                }

                if (!await CanUserAccessOperationAsync(operationId, userId))
                {
                    return (false, "شما مجاز به انجام این عملیات نیستید");
                }

                operation.IsStarred = !operation.IsStarred;
                _context.TaskOperation_Tbl.Update(operation);
                await _context.SaveChangesAsync();

                var message = operation.IsStarred ? "عملیات ستاره‌دار شد" : "ستاره عملیات حذف شد";
                return (true, message);
            }
            catch (Exception ex)
            {
                return (false, $"خطا در تغییر وضعیت ستاره: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleOperationCompleteAsync(
            int operationId,
            string userId,
            string? completionNote = null,
            bool addWorkLog = false,
            string? workDescription = null,
            int? durationMinutes = null)
        {
            try
            {
                var operation = await GetOperationByIdAsync(operationId);
                if (operation == null)
                {
                    return (false, "عملیات یافت نشد");
                }

                if (!await CanUserAccessOperationAsync(operationId, userId))
                {
                    return (false, "شما مجاز به انجام این عملیات نیستید");
                }

                var wasCompleted = operation.IsCompleted;
                operation.IsCompleted = !operation.IsCompleted;

                if (operation.IsCompleted)
                {
                    // تکمیل عملیات
                    operation.CompletionDate = DateTime.Now;
                    operation.CompletedByUserId = userId;
                    operation.CompletionNote = completionNote;

                    // اگر کاربر خواسته WorkLog هم ثبت کند
                    if (addWorkLog && !string.IsNullOrWhiteSpace(workDescription))
                    {
                        var workLog = new TaskOperationWorkLog
                        {
                            TaskOperationId = operationId,
                            UserId = userId,
                            WorkDescription = workDescription,
                            WorkDate = DateTime.Now,
                            DurationMinutes = durationMinutes,
                            ProgressPercentage = 100,
                            CreatedDate = DateTime.Now,
                            IsDeleted = false
                        };

                        _context.TaskOperationWorkLog_Tbl.Add(workLog);
                    }
                }
                else
                {
                    // بازگشت به حالت انتظار
                    operation.CompletionDate = null;
                    operation.CompletedByUserId = null;
                    operation.CompletionNote = null;
                }

                _context.TaskOperation_Tbl.Update(operation);
                await _context.SaveChangesAsync();

                var message = operation.IsCompleted ? "عملیات تکمیل شد" : "عملیات به حالت انتظار برگشت";
                return (true, message);
            }
            catch (Exception ex)
            {
                return (false, $"خطا در تغییر وضعیت تکمیل: {ex.Message}");
            }
        }

        #endregion

        #region Work Log Management

        public async Task<(bool Success, string Message, int? WorkLogId)> AddWorkLogAsync(
            OperationWorkLogViewModel model, 
            string userId)
        {
            try
            {
                if (!await CanUserAccessOperationAsync(model.TaskOperationId, userId))
                {
                    return (false, "شما مجاز به انجام این عملیات نیستید", null);
                }

                var workLog = new TaskOperationWorkLog
                {
                    TaskOperationId = model.TaskOperationId ,
                    UserId = userId,
                    WorkDescription = model.WorkDescription,
                    WorkDate = DateTime.Now,
                    DurationMinutes = model.DurationMinutes,
                    ProgressPercentage = model.ProgressPercentage,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _context.TaskOperationWorkLog_Tbl.Add(workLog);
                await _context.SaveChangesAsync();

                return (true, "گزارش کار با موفقیت ثبت شد", workLog.Id);
            }
            catch (Exception ex)
            {
                return (false, $"خطا در ثبت گزارش کار: {ex.Message}", null);
            }
        }

        public async Task<List<OperationWorkLogViewModel>> GetOperationWorkLogsAsync(int operationId, int take = 0)
        {
            var query = _context.TaskOperationWorkLog_Tbl
                .Include(w => w.User)
                .Include(w => w.TaskOperation)
                    .ThenInclude(o => o.Task)
                .Where(w => w.TaskOperationId == operationId && !w.IsDeleted)
                .OrderByDescending(w => w.WorkDate);

            if (take > 0)
            {
                query = (IOrderedQueryable<TaskOperationWorkLog>)query.Take(take);
            }

            var workLogs = await query.ToListAsync();

            return workLogs.Select(w => new OperationWorkLogViewModel
            {
                Id = w.Id,
                TaskOperationId = w.TaskOperationId,
                OperationTitle = w.TaskOperation?.Title ?? "نامشخص",
                TaskTitle = w.TaskOperation?.Task?.Title ?? "نامشخص",
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

        public async Task<List<OperationWorkLogSummaryViewModel>> GetTaskWorkLogsSummaryAsync(int taskId, int take = 10)
        {
            var workLogs = await _context.TaskOperationWorkLog_Tbl
                .Include(w => w.TaskOperation)
                .Include(w => w.User)
                .Where(w => w.TaskOperation.TaskId == taskId && !w.IsDeleted)
                .OrderByDescending(w => w.WorkDate)
                .Take(take)
                .ToListAsync();

            return workLogs.Select(w => new OperationWorkLogSummaryViewModel
            {
                OperationId = w.TaskOperationId,
                OperationTitle = w.TaskOperation?.Title ?? "نامشخص",
                TaskId = taskId,
                LastWorkDate = w.WorkDate,
                LastWorkDatePersian = ConvertDateTime.ConvertMiladiToShamsi(w.WorkDate, "yyyy/MM/dd HH:mm"),
                LastWorkDescription = w.WorkDescription,
                LastWorkerName = w.User != null ? $"{w.User.FirstName} {w.User.LastName}" : "نامشخص"
            }).ToList();
        }

        public async Task<(bool Success, string Message)> DeleteWorkLogAsync(int workLogId, string userId)
        {
            try
            {
                var workLog = await _context.TaskOperationWorkLog_Tbl
                    .FirstOrDefaultAsync(w => w.Id == workLogId && !w.IsDeleted);

                if (workLog == null)
                {
                    return (false, "گزارش کار یافت نشد");
                }

                // بررسی دسترسی
                if (workLog.UserId != userId && 
                    !await CanUserAccessOperationAsync(workLog.TaskOperationId, userId))
                {
                    return (false, "شما مجاز به حذف این گزارش نیستید");
                }

                // Soft Delete
                workLog.IsDeleted = true;
                workLog.DeletedDate = DateTime.Now;

                _context.TaskOperationWorkLog_Tbl.Update(workLog);
                await _context.SaveChangesAsync();

                return (true, "گزارش کار حذف شد");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در حذف گزارش کار: {ex.Message}");
            }
        }

        public async Task<int> GetWorkLogsCountAsync(int operationId)
        {
            return await _context.TaskOperationWorkLog_Tbl
                .CountAsync(w => w.TaskOperationId == operationId && !w.IsDeleted);
        }

        #endregion

        #region Statistics & Helper Methods

        public async Task<int> CalculateOperationProgressAsync(int operationId)
        {
            var latestWorkLog = await _context.TaskOperationWorkLog_Tbl
                .Where(w => w.TaskOperationId == operationId && !w.IsDeleted && w.ProgressPercentage.HasValue)
                .OrderByDescending(w => w.WorkDate)
                .FirstOrDefaultAsync();

            return latestWorkLog?.ProgressPercentage ?? 0;
        }

        public async Task<int> GetTotalDurationMinutesAsync(int operationId)
        {
            return await _context.TaskOperationWorkLog_Tbl
                .Where(w => w.TaskOperationId == operationId && !w.IsDeleted)
                .SumAsync(w => w.DurationMinutes ?? 0);
        }

        public async Task<TaskWorkLogsStatsViewModel> GetTaskWorkLogsStatsAsync(int taskId)
        {
            var operationIds = await _context.TaskOperation_Tbl
                .Where(o => o.TaskId == taskId)
                .Select(o => o.Id)
                .ToListAsync();

            var workLogs = await _context.TaskOperationWorkLog_Tbl
                .Where(w => operationIds.Contains(w.TaskOperationId) && !w.IsDeleted)
                .ToListAsync();

            var lastWorkLog = workLogs.OrderByDescending(w => w.WorkDate).FirstOrDefault();

            return new TaskWorkLogsStatsViewModel
            {
                TotalWorkLogs = workLogs.Count,
                TotalOperationsWithWorkLogs = workLogs.Select(w => w.TaskOperationId).Distinct().Count(),
                TotalDurationMinutes = workLogs.Sum(w => w.DurationMinutes ?? 0),
                LastWorkDate = lastWorkLog?.WorkDate,
                LastWorkDatePersian = lastWorkLog != null 
                    ? ConvertDateTime.ConvertMiladiToShamsi(lastWorkLog.WorkDate, "yyyy/MM/dd HH:mm") 
                    : null,
                ActiveWorkersCount = workLogs.Select(w => w.UserId).Distinct().Count()
            };
        }

        #endregion
        public void AddTaskOperation(TaskOperation operation)
        {
            _context.TaskOperation_Tbl.Add(operation);
        }

    

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}