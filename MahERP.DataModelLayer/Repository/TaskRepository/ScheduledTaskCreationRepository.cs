using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository برای مدیریت زمان‌بندی ساخت تسک‌ها
    /// </summary>
    public interface IScheduledTaskCreationRepository
    {
        /// <summary>
        /// ایجاد زمان‌بندی جدید برای ساخت تسک
        /// </summary>
        Task<int> CreateScheduledTaskAsync(TaskViewModel taskModel, string userId);

        /// <summary>
        /// دریافت لیست تسک‌های زمان‌بندی شده کاربر
        /// </summary>
        Task<List<ScheduledTaskCreation>> GetUserScheduledTasksAsync(string userId, bool includeInactive = false);

        /// <summary>
        /// دریافت زمان‌بندی‌های آماده برای اجرا
        /// </summary>
        Task<List<ScheduledTaskCreation>> GetDueScheduledTasksAsync();

        /// <summary>
        /// بروزرسانی وضعیت اجرا
        /// </summary>
        Task UpdateExecutionStatusAsync(int scheduleId, bool success, string? notes = null);

        /// <summary>
        /// محاسبه تاریخ بعدی اجرا
        /// </summary>
        DateTime? CalculateNextExecutionDate(ScheduledTaskCreation schedule, DateTime baseTime);

        /// <summary>
        /// دریافت اطلاعات تسک از JSON
        /// </summary>
        TaskViewModel? DeserializeTaskData(string taskDataJson);

        /// <summary>
        /// فعال/غیرفعال کردن زمان‌بندی
        /// </summary>
        Task ToggleScheduleAsync(int scheduleId, bool isEnabled);

        /// <summary>
        /// حذف زمان‌بندی
        /// </summary>
        Task DeleteScheduleAsync(int scheduleId);

        /// <summary>
        /// دریافت جزئیات یک زمان‌بندی
        /// </summary>
        Task<ScheduledTaskCreation?> GetScheduleByIdAsync(int scheduleId);
        
        /// <summary>
        /// ⭐⭐⭐ دریافت لیست تسک‌های زمان‌بندی شده برای نمایش
        /// </summary>
        Task<ScheduledTaskListViewModel> GetScheduledTasksListAsync(string userId);
        
        /// <summary>
        /// ⭐⭐⭐ دریافت لیست تسک‌های زمان‌بندی شده با پشتیبانی از Admin
        /// </summary>
        Task<ScheduledTaskListViewModel> GetScheduledTasksListAsync(string userId, bool isAdmin);
        
        /// <summary>
        /// ⭐⭐⭐ دریافت تسک زمان‌بندی شده برای ویرایش
        /// </summary>
        Task<TaskViewModel?> GetScheduledTaskForEditAsync(int scheduleId);
        
        /// <summary>
        /// ⭐⭐⭐ بروزرسانی زمان‌بندی موجود
        /// </summary>
        Task<bool> UpdateScheduledTaskAsync(int scheduleId, TaskViewModel taskModel, string userId);
    }

    /// <summary>
    /// پیاده‌سازی Repository
    /// </summary>
    public class ScheduledTaskCreationRepository : IScheduledTaskCreationRepository
    {
        private readonly AppDbContext _context;
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        public ScheduledTaskCreationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateScheduledTaskAsync(TaskViewModel taskModel, string userId)
        {
            // سریالایز کامل TaskViewModel
            var taskDataJson = JsonSerializer.Serialize(taskModel, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = false
            });

            var schedule = new ScheduledTaskCreation
            {
                ScheduleTitle = taskModel.TaskSchedule?.ScheduleTitle ?? taskModel.Title,
                ScheduleDescription = taskModel.TaskSchedule?.ScheduleDescription,
                TaskDataJson = taskDataJson,
                ScheduleType = taskModel.TaskSchedule?.ScheduleType ?? 0,
                ScheduledTime = taskModel.TaskSchedule?.ScheduledTime,
                ScheduledDaysOfWeek = taskModel.TaskSchedule?.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = taskModel.TaskSchedule?.ScheduledDayOfMonth,
                StartDate = ConvertDateTime.ConvertShamsiToMiladi(taskModel.TaskSchedule?.StartDatePersian),
                EndDate = ConvertDateTime.ConvertShamsiToMiladi(taskModel.TaskSchedule?.EndDatePersian),
                MaxOccurrences = taskModel.TaskSchedule?.MaxOccurrences,
                IsRecurring = taskModel.TaskSchedule?.IsRecurring ?? false,
                IsActive = true,
                IsScheduleEnabled = true,
                IsExecuted = false,
                ExecutionCount = 0,
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow,
                BranchId = taskModel.BranchIdSelected,
                Notes = null
            };

            // محاسبه اولین NextExecutionDate
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            schedule.NextExecutionDate = CalculateNextExecutionDate(schedule, nowIran);

            _context.ScheduledTaskCreation_Tbl.Add(schedule);
            await _context.SaveChangesAsync();

            return schedule.Id;
        }

        public async Task<List<ScheduledTaskCreation>> GetUserScheduledTasksAsync(string userId, bool includeInactive = false)
        {
            var query = _context.ScheduledTaskCreation_Tbl
                .Where(s => s.CreatedByUserId == userId);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && s.IsScheduleEnabled);
            }

            return await query
                .OrderBy(s => s.NextExecutionDate)
                .ToListAsync();
        }

        public async Task<List<ScheduledTaskCreation>> GetDueScheduledTasksAsync()
        {
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            var nowUtc = DateTime.UtcNow;

            return await _context.ScheduledTaskCreation_Tbl
                .Where(s =>
                    s.IsActive &&
                    s.IsScheduleEnabled &&
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value <= nowUtc &&
                    // ⭐ جلوگیری از اجرای مکرر: حداقل 1 دقیقه فاصله
                    (!s.LastExecutionDate.HasValue ||
                     EF.Functions.DateDiffMinute(s.LastExecutionDate.Value, nowUtc) >= 1))
                .ToListAsync();
        }

        public async Task UpdateExecutionStatusAsync(int scheduleId, bool success, string? notes = null)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            var nowUtc = DateTime.UtcNow;
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, IranTimeZone);

            schedule.LastExecutionDate = nowUtc;
            schedule.ExecutionCount++;
            schedule.Notes = notes;

            // بررسی اتمام زمان‌بندی
            if (schedule.ScheduleType == 0) // OneTime
            {
                schedule.IsExecuted = true;
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else if (schedule.MaxOccurrences.HasValue && schedule.ExecutionCount >= schedule.MaxOccurrences.Value)
            {
                // تعداد دفعات به حد نصاب رسیده
                schedule.IsExecuted = true;
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else if (schedule.EndDate.HasValue && nowUtc >= schedule.EndDate.Value)
            {
                // تاریخ پایان رسیده
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else
            {
                // محاسبه اجرای بعدی
                schedule.NextExecutionDate = CalculateNextExecutionDate(schedule, nowIran);
            }

            await _context.SaveChangesAsync();
        }

        public DateTime? CalculateNextExecutionDate(ScheduledTaskCreation schedule, DateTime baseTime)
        {
            if (string.IsNullOrEmpty(schedule.ScheduledTime))
                return null;

            if (!TimeSpan.TryParse(schedule.ScheduledTime, out var timeOfDay))
                return null;

            DateTime nextExecution;

            switch (schedule.ScheduleType)
            {
                case 0: // OneTime
                    if (schedule.StartDate.HasValue)
                    {
                        nextExecution = schedule.StartDate.Value.Date.Add(timeOfDay);
                        return nextExecution;
                    }
                    return null;

                case 1: // Daily
                    nextExecution = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);
                    if (nextExecution <= baseTime)
                    {
                        nextExecution = nextExecution.AddDays(1);
                    }
                    break;

                case 2: // Weekly
                    if (string.IsNullOrEmpty(schedule.ScheduledDaysOfWeek))
                        return null;

                    var selectedDays = schedule.ScheduledDaysOfWeek.Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
                        .Where(d => d >= 0 && d <= 6)
                        .OrderBy(d => d)
                        .ToList();

                    if (!selectedDays.Any())
                        return null;

                    nextExecution = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);
                    
                    for (int i = 0; i < 7; i++)
                    {
                        var checkDate = baseTime.AddDays(i);
                        var checkDateTime = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);
                        
                        if (checkDateTime > baseTime && selectedDays.Contains((int)checkDate.DayOfWeek))
                        {
                            nextExecution = checkDateTime;
                            break;
                        }
                    }
                    break;

                case 3: // Monthly
                    if (!schedule.ScheduledDayOfMonth.HasValue)
                        return null;

                    var dayOfMonth = schedule.ScheduledDayOfMonth.Value;
                    var currentMonth = baseTime;
                    var daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                    var actualDay = Math.Min(dayOfMonth, daysInMonth);

                    nextExecution = new DateTime(currentMonth.Year, currentMonth.Month, actualDay, timeOfDay.Hours, timeOfDay.Minutes, 0);

                    if (nextExecution <= baseTime)
                    {
                        var nextMonth = currentMonth.AddMonths(1);
                        daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                        actualDay = Math.Min(dayOfMonth, daysInMonth);
                        nextExecution = new DateTime(nextMonth.Year, nextMonth.Month, actualDay, timeOfDay.Hours, timeOfDay.Minutes, 0);
                    }
                    break;

                default:
                    return null;
            }

            // تبدیل به UTC
            return TimeZoneInfo.ConvertTimeToUtc(nextExecution, IranTimeZone);
        }

        public TaskViewModel? DeserializeTaskData(string taskDataJson)
        {
            try
            {
                return JsonSerializer.Deserialize<TaskViewModel>(taskDataJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task ToggleScheduleAsync(int scheduleId, bool isEnabled)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsScheduleEnabled = isEnabled;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsActive = false;
            schedule.IsScheduleEnabled = false;
            await _context.SaveChangesAsync();
        }

        public async Task<ScheduledTaskCreation?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست تسک‌های زمان‌بندی شده برای نمایش
        /// </summary>
        public async Task<ScheduledTaskListViewModel> GetScheduledTasksListAsync(string userId)
        {
            return await GetScheduledTasksListAsync(userId, false);
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست تسک‌های زمان‌بندی شده با پشتیبانی از Admin
        /// </summary>
        public async Task<ScheduledTaskListViewModel> GetScheduledTasksListAsync(string userId, bool isAdmin)
        {
            var query = _context.ScheduledTaskCreation_Tbl
                .Include(s => s.CreatedByUser)
                .Include(s => s.Branch)
                .AsQueryable();

            // اگر کاربر Admin نیست، فقط تسک‌های خودش را ببیند
            if (!isAdmin)
            {
                query = query.Where(s => s.CreatedByUserId == userId);
            }

            var schedules = await query
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            var cards = schedules.Select(s => MapToCardViewModel(s)).ToList();
            var stats = CalculateStats(schedules);

            return new ScheduledTaskListViewModel
            {
                ScheduledTasks = cards,
                Stats = stats
            };
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت تسک زمان‌بندی شده برای ویرایش
        /// </summary>
        public async Task<TaskViewModel?> GetScheduledTaskForEditAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return null;

            var taskModel = DeserializeTaskData(schedule.TaskDataJson);
            if (taskModel == null) return null;

            // بازیابی اطلاعات زمان‌بندی
            taskModel.TaskSchedule = new TaskScheduleViewModel
            {
                ScheduleTitle = schedule.ScheduleTitle,
                ScheduleDescription = schedule.ScheduleDescription,
                ScheduleType = schedule.ScheduleType,
                ScheduledTime = schedule.ScheduledTime,
                ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                StartDatePersian = schedule.StartDate.HasValue 
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.StartDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                EndDatePersian = schedule.EndDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.EndDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                MaxOccurrences = schedule.MaxOccurrences,
                IsRecurring = schedule.IsRecurring
            };

            return taskModel;
        }

        /// <summary>
        /// ⭐⭐⭐ بروزرسانی زمان‌بندی موجود
        /// </summary>
        public async Task<bool> UpdateScheduledTaskAsync(int scheduleId, TaskViewModel taskModel, string userId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return false;

            // بروزرسانی JSON
            var taskDataJson = JsonSerializer.Serialize(taskModel, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = false
            });

            schedule.ScheduleTitle = taskModel.TaskSchedule?.ScheduleTitle ?? taskModel.Title;
            schedule.ScheduleDescription = taskModel.TaskSchedule?.ScheduleDescription;
            schedule.TaskDataJson = taskDataJson;
            schedule.ScheduleType = taskModel.TaskSchedule?.ScheduleType ?? 0;
            schedule.ScheduledTime = taskModel.TaskSchedule?.ScheduledTime;
            schedule.ScheduledDaysOfWeek = taskModel.TaskSchedule?.ScheduledDaysOfWeek;
            schedule.ScheduledDayOfMonth = taskModel.TaskSchedule?.ScheduledDayOfMonth;
            schedule.StartDate = ConvertDateTime.ConvertShamsiToMiladi(taskModel.TaskSchedule?.StartDatePersian);
            schedule.EndDate = ConvertDateTime.ConvertShamsiToMiladi(taskModel.TaskSchedule?.EndDatePersian);
            schedule.MaxOccurrences = taskModel.TaskSchedule?.MaxOccurrences;
            schedule.IsRecurring = taskModel.TaskSchedule?.IsRecurring ?? false;
            schedule.BranchId = taskModel.BranchIdSelected;

            // محاسبه مجدد NextExecutionDate
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            schedule.NextExecutionDate = CalculateNextExecutionDate(schedule, nowIran);

            await _context.SaveChangesAsync();
            return true;
        }

        // ⭐⭐⭐ Helper Methods
        private ScheduledTaskCardViewModel MapToCardViewModel(ScheduledTaskCreation schedule)
        {
            var taskModel = DeserializeTaskData(schedule.TaskDataJson);

            return new ScheduledTaskCardViewModel
            {
                Id = schedule.Id,
                ScheduleTitle = schedule.ScheduleTitle,
                ScheduleDescription = schedule.ScheduleDescription,
                TaskTitle = taskModel?.Title ?? "نامشخص",
                ScheduleType = schedule.ScheduleType,
                ScheduledTime = schedule.ScheduledTime,
                ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                StartDate = schedule.StartDate,
                StartDatePersian = schedule.StartDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.StartDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                EndDate = schedule.EndDate,
                EndDatePersian = schedule.EndDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.EndDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                NextExecutionDate = schedule.NextExecutionDate,
                NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.NextExecutionDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                LastExecutionDate = schedule.LastExecutionDate,
                LastExecutionDatePersian = schedule.LastExecutionDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(schedule.LastExecutionDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                ExecutionCount = schedule.ExecutionCount,
                MaxOccurrences = schedule.MaxOccurrences,
                IsRecurring = schedule.IsRecurring,
                IsActive = schedule.IsActive,
                IsScheduleEnabled = schedule.IsScheduleEnabled,
                IsExecuted = schedule.IsExecuted,
                BranchId = schedule.BranchId,
                BranchName = schedule.Branch?.Name,
                CreatedByUserName = schedule.CreatedByUser != null
                    ? $"{schedule.CreatedByUser.FirstName} {schedule.CreatedByUser.LastName}"
                    : "نامشخص",
                CreatedDate = schedule.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(schedule.CreatedDate, "yyyy/MM/dd HH:mm"),
                TaskCode = taskModel?.TaskCode,
                Priority = taskModel?.Priority ?? 0,
                Important = taskModel?.Important ?? false,
                TaskType = taskModel?.TaskType ?? 0
            };
        }

        private ScheduledTaskStatsViewModel CalculateStats(List<ScheduledTaskCreation> schedules)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            return new ScheduledTaskStatsViewModel
            {
                TotalScheduled = schedules.Count,
                ActiveCount = schedules.Count(s => s.IsActive && s.IsScheduleEnabled),
                InactiveCount = schedules.Count(s => !s.IsActive || !s.IsScheduleEnabled),
                CompletedCount = schedules.Count(s => s.IsExecuted),
                PendingCount = schedules.Count(s => !s.IsExecuted && s.IsActive && s.IsScheduleEnabled),
                TodayCount = schedules.Count(s => 
                    s.NextExecutionDate.HasValue && 
                    s.NextExecutionDate.Value.Date == today),
                ThisWeekCount = schedules.Count(s =>
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value >= weekStart &&
                    s.NextExecutionDate.Value < weekEnd)
            };
        }
    }
}
