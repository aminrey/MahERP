using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// بخش مدیریت تسک‌های زمان‌بندی شده (Scheduled Tasks)
    /// </summary>
    public partial class TaskRepository
    {
        private static readonly TimeZoneInfo IranTimeZone = 
            TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        #region Scheduled Tasks CRUD

        /// <summary>
        /// ایجاد زمان‌بندی تسک (Schedule) و بررسی CreateImmediately
        /// </summary>
        public async Task<(int ScheduleId, Tasks? ImmediateTask)> CreateScheduledTaskAsync(
            TaskViewModel model, 
            string userId)
        {
            // ⭐ ساخت JSON Template
            var taskTemplateJson = CreateTaskTemplateJson(model);

            // ⭐ محاسبه NextExecutionDate
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            var nextExecution = CalculateNextExecutionDate(model.TaskSchedule, nowIran);

            // ⭐ ذخیره در ScheduledTaskCreation_Tbl
            var schedule = new ScheduledTaskCreation
            {
                ScheduleTitle = model.TaskSchedule?.ScheduleTitle ?? model.Title,
                ScheduleDescription = model.TaskSchedule?.ScheduleDescription,
                TaskDataJson = taskTemplateJson, // ⭐ تصحیح: TaskDataJson
                ScheduleType = model.TaskSchedule?.ScheduleType ?? 0,
                ScheduledTime = model.TaskSchedule?.ScheduledTime,
                ScheduledDaysOfWeek = model.TaskSchedule?.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = model.TaskSchedule?.ScheduledDayOfMonth,
                StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(model.TaskSchedule?.StartDatePersian),
                EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(model.TaskSchedule?.EndDatePersian),
                MaxOccurrences = model.TaskSchedule?.MaxOccurrences,
                IsScheduleEnabled = true,
                IsActive = true,
                ExecutionCount = 0,
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow,
                BranchId = model.BranchIdSelected,
                NextExecutionDate = nextExecution
            };

            _context.ScheduledTaskCreation_Tbl.Add(schedule);
            await _context.SaveChangesAsync();

            // ⭐⭐⭐ بررسی CreateImmediately
            Tasks? immediateTask = null;
            if (model.TaskSchedule?.CreateImmediately == true)
            {
                // ساخت تسک فوری (ViewModel از قبل آماده است)
                immediateTask = await CreateTaskEntityAsync(model, userId, _mapper);
                immediateTask.ScheduleId = schedule.Id;
                immediateTask.CreationMode = 1; // ⭐ خودکار (از Schedule)
                
                // ⭐⭐⭐ استفاده مستقیم از _context به جای _unitOfWork
                _context.Tasks_Tbl.Update(immediateTask);
                await _context.SaveChangesAsync();
            }

            return (schedule.Id, immediateTask);
        }

        /// <summary>
        /// محاسبه NextExecutionDate بر اساس نوع زمان‌بندی
        /// </summary>
        private DateTime? CalculateNextExecutionDate(
            TaskScheduleViewModel? schedule, 
            DateTime baseTime)
        {
            if (schedule == null || string.IsNullOrEmpty(schedule.ScheduledTime))
                return null;

            if (!TimeSpan.TryParse(schedule.ScheduledTime, out var timeOfDay))
                return null;

            // ⭐⭐⭐ baseTime در Iran TimeZone است
            DateTime nextExecution;

            switch (schedule.ScheduleType)
            {
                case 0: // OneTime
                    if (!string.IsNullOrEmpty(schedule.StartDatePersian))
                    {
                        var startDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(schedule.StartDatePersian);
                        // ⭐ ساخت DateTime در Iran TimeZone
                        nextExecution = new DateTime(
                            startDate.Year, startDate.Month, startDate.Day,
                            timeOfDay.Hours, timeOfDay.Minutes, 0);
                        return TimeZoneInfo.ConvertTimeToUtc(nextExecution, IranTimeZone);
                    }
                    return null;

                case 1: // Daily
                    // ⭐ ساخت DateTime در Iran TimeZone
                    nextExecution = new DateTime(
                        baseTime.Year, baseTime.Month, baseTime.Day, 
                        timeOfDay.Hours, timeOfDay.Minutes, 0);
                    
                    if (nextExecution <= baseTime)
                        nextExecution = nextExecution.AddDays(1);
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

                    nextExecution = baseTime;
                    for (int i = 0; i < 7; i++)
                    {
                        var checkDate = baseTime.AddDays(i);
                        var checkDateTime = new DateTime(
                            checkDate.Year, checkDate.Month, checkDate.Day,
                            timeOfDay.Hours, timeOfDay.Minutes, 0);

                        if (checkDateTime > baseTime && 
                            selectedDays.Contains((int)checkDate.DayOfWeek))
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
                    var daysInMonth = DateTime.DaysInMonth(baseTime.Year, baseTime.Month);
                    var actualDay = Math.Min(dayOfMonth, daysInMonth);

                    nextExecution = new DateTime(
                        baseTime.Year, baseTime.Month, actualDay,
                        timeOfDay.Hours, timeOfDay.Minutes, 0);

                    if (nextExecution <= baseTime)
                    {
                        var nextMonth = baseTime.AddMonths(1);
                        daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                        actualDay = Math.Min(dayOfMonth, daysInMonth);
                        nextExecution = new DateTime(
                            nextMonth.Year, nextMonth.Month, actualDay,
                            timeOfDay.Hours, timeOfDay.Minutes, 0);
                    }
                    break;

                default:
                    return null;
            }

            // ⭐⭐⭐ تبدیل Iran Time به UTC
            return TimeZoneInfo.ConvertTimeToUtc(nextExecution, IranTimeZone);
        }

        /// <summary>
        /// ساخت JSON Template از TaskViewModel
        /// </summary>
        private string CreateTaskTemplateJson(TaskViewModel model)
        {
            // ⭐⭐⭐ فقط اطلاعات مورد نیاز ذخیره می‌شود
            var template = new
            {
                model.Title,
                model.Description,
                model.BranchIdSelected,
                model.Priority,
                model.Important,
                model.TaskType,
                model.EstimatedHours,
                model.TaskCategoryIdSelected,
                model.SelectedOrganizationId,
                model.SelectedContactId,
                model.DueDatePersian,
                model.SuggestedStartDatePersian,
                model.IsHardDeadline,
                model.TimeNote,
                Operations = model.Operations?.Select(o => new
                {
                    o.Title,
                    o.Description,
                    o.OperationOrder,
                    o.EstimatedHours
                }).ToList(),
                Assignments = model.UserTeamAssignmentsJson, // ⭐ JSON پیش‌ساخته
                Reminders = model.TaskRemindersJson // ⭐ JSON پیش‌ساخته
            };

            return JsonSerializer.Serialize(template, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = false
            });
        }

        /// <summary>
        /// دریافت لیست زمان‌بندی‌های کاربر
        /// </summary>
        public async Task<ScheduledTaskListViewModel> GetUserScheduledTasksAsync(
            string userId, 
            bool isAdmin = false)
        {
            var query = _context.ScheduledTaskCreation_Tbl
                .Include(s => s.CreatedByUser)
                .Include(s => s.Branch)
                .Where(s => s.IsActive)
                .AsQueryable();

            // ⭐⭐⭐ Admin می‌تواند همه را ببیند
            if (!isAdmin)
            {
                query = query.Where(s => s.CreatedByUserId == userId);
            }

            var schedules = await query
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            var cards = schedules.Select(s => MapToScheduledTaskCard(s)).ToList();
            var stats = CalculateScheduledTaskStats(schedules);

            return new ScheduledTaskListViewModel
            {
                ScheduledTasks = cards,
                Stats = stats
            };
        }

        /// <summary>
        /// Map ScheduledTaskCreation به CardViewModel
        /// </summary>
        private ScheduledTaskCardViewModel MapToScheduledTaskCard(ScheduledTaskCreation schedule)
        {
            var taskModel = DeserializeTaskTemplate(schedule.TaskDataJson); // ⭐ تصحیح

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
                NextExecutionDate = schedule.NextExecutionDate,
                NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        schedule.NextExecutionDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                LastExecutionDate = schedule.LastExecutionDate,
                ExecutionCount = schedule.ExecutionCount,
                MaxOccurrences = schedule.MaxOccurrences,
                IsActive = schedule.IsActive,
                IsScheduleEnabled = schedule.IsScheduleEnabled,
                BranchId = schedule.BranchId,
                BranchName = schedule.Branch?.Name,
                CreatedByUserName = schedule.CreatedByUser != null
                    ? $"{schedule.CreatedByUser.FirstName} {schedule.CreatedByUser.LastName}"
                    : "نامشخص",
                CreatedDate = schedule.CreatedDate,
                CreatedDatePersian = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                    schedule.CreatedDate, "yyyy/MM/dd HH:mm")
            };
        }

        /// <summary>
        /// محاسبه آمار زمان‌بندی‌ها
        /// </summary>
        private ScheduledTaskStatsViewModel CalculateScheduledTaskStats(
            List<ScheduledTaskCreation> schedules)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            return new ScheduledTaskStatsViewModel
            {
                TotalScheduled = schedules.Count,
                ActiveCount = schedules.Count(s => s.IsScheduleEnabled),
                InactiveCount = schedules.Count(s => !s.IsScheduleEnabled),
                CompletedCount = schedules.Count(s => 
                    s.MaxOccurrences.HasValue && 
                    s.ExecutionCount >= s.MaxOccurrences.Value),
                PendingCount = schedules.Count(s => 
                    s.IsScheduleEnabled && 
                    (!s.MaxOccurrences.HasValue || 
                     s.ExecutionCount < s.MaxOccurrences.Value)),
                TodayCount = schedules.Count(s =>
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value.Date == today),
                ThisWeekCount = schedules.Count(s =>
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value >= today &&
                    s.NextExecutionDate.Value < today.AddDays(7))
            };
        }

        /// <summary>
        /// Deserialize کردن TaskTemplateJson
        /// </summary>
        private TaskViewModel? DeserializeTaskTemplate(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<TaskViewModel>(json, 
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ⭐ متد public برای Deserialize (برای استفاده در Controller)
        /// </summary>
        public TaskViewModel? DeserializeTaskData(string taskDataJson)
        {
            return DeserializeTaskTemplate(taskDataJson);
        }

        /// <summary>
        /// دریافت جزئیات یک زمان‌بندی
        /// </summary>
        public async Task<ScheduledTaskCreation?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.ScheduledTaskCreation_Tbl
                .Include(s => s.CreatedByUser)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);
        }

        /// <summary>
        /// فعال/غیرفعال کردن زمان‌بندی
        /// </summary>
        public async Task ToggleScheduleAsync(int scheduleId, bool isEnabled)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsScheduleEnabled = isEnabled;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// حذف زمان‌بندی (نرم)
        /// </summary>
        public async Task DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsActive = false;
            schedule.IsScheduleEnabled = false;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// دریافت تسک زمان‌بندی شده برای ویرایش
        /// </summary>
        public async Task<TaskViewModel?> GetScheduledTaskForEditAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return null;

            var taskModel = DeserializeTaskTemplate(schedule.TaskDataJson); // ⭐ تصحیح
            if (taskModel == null) return null;

            // بازیابی اطلاعات زمان‌بندی
            taskModel.TaskSchedule = new TaskScheduleViewModel
            {
                IsScheduled = true,
                ScheduleTitle = schedule.ScheduleTitle,
                ScheduleDescription = schedule.ScheduleDescription,
                ScheduleType = schedule.ScheduleType,
                ScheduledTime = schedule.ScheduledTime,
                ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                StartDatePersian = schedule.StartDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        schedule.StartDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                EndDatePersian = schedule.EndDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        schedule.EndDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                MaxOccurrences = schedule.MaxOccurrences
            };

            return taskModel;
        }

        /// <summary>
        /// بروزرسانی زمان‌بندی موجود
        /// </summary>
        public async Task<bool> UpdateScheduledTaskAsync(
            int scheduleId, 
            TaskViewModel taskModel, 
            string userId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return false;

            // بروزرسانی JSON
            var taskDataJson = CreateTaskTemplateJson(taskModel);

            schedule.ScheduleTitle = taskModel.TaskSchedule?.ScheduleTitle ?? taskModel.Title;
            schedule.ScheduleDescription = taskModel.TaskSchedule?.ScheduleDescription;
            schedule.TaskDataJson = taskDataJson; // ⭐ تصحیح
            schedule.ScheduleType = taskModel.TaskSchedule?.ScheduleType ?? 0;
            schedule.ScheduledTime = taskModel.TaskSchedule?.ScheduledTime;
            schedule.ScheduledDaysOfWeek = taskModel.TaskSchedule?.ScheduledDaysOfWeek;
            schedule.ScheduledDayOfMonth = taskModel.TaskSchedule?.ScheduledDayOfMonth;
            schedule.StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(
                taskModel.TaskSchedule?.StartDatePersian);
            schedule.EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(
                taskModel.TaskSchedule?.EndDatePersian);
            schedule.MaxOccurrences = taskModel.TaskSchedule?.MaxOccurrences;
            schedule.BranchId = taskModel.BranchIdSelected;

            // محاسبه مجدد NextExecutionDate
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            schedule.NextExecutionDate = CalculateNextExecutionDate(taskModel.TaskSchedule, nowIran);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// دریافت زمان‌بندی‌های آماده برای اجرا توسط Background Service
        /// </summary>
        public async Task<List<ScheduledTaskCreation>> GetDueScheduledTasksAsync()
        {
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

        /// <summary>
        /// بروزرسانی وضعیت اجرا پس از ساخت تسک توسط Background Service
        /// </summary>
        public async Task UpdateExecutionStatusAsync(
            int scheduleId, 
            bool success, 
            string? notes = null)
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
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else if (schedule.MaxOccurrences.HasValue && 
                     schedule.ExecutionCount >= schedule.MaxOccurrences.Value)
            {
                // تعداد دفعات به حد نصاب رسیده
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
                var taskSchedule = new TaskScheduleViewModel
                {
                    ScheduleType = schedule.ScheduleType,
                    ScheduledTime = schedule.ScheduledTime,
                    ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                    ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                    StartDatePersian = schedule.StartDate.HasValue
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                            schedule.StartDate.Value, "yyyy/MM/dd HH:mm")
                        : null
                };

                schedule.NextExecutionDate = CalculateNextExecutionDate(taskSchedule, nowIran);
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
