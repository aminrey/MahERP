using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت یادآوری‌های تسک (Task Reminders)
    /// شامل: ایجاد، حذف، غیرفعال‌سازی، دریافت یادآوری‌ها
    /// </summary>
    public partial class TaskRepository 
    {
        #region Reminder Schedule Management

        public void SaveTaskReminders(int taskId, List<TaskReminderViewModel> reminders)
        {
            try
            {
                // حذف یادآوری‌های قدیمی
                var existingReminders = _context.TaskReminderSchedule_Tbl.Where(r => r.TaskId == taskId).ToList();
                _context.TaskReminderSchedule_Tbl.RemoveRange(existingReminders);

                // اضافه کردن یادآوری‌های جدید
                foreach (var reminder in reminders)
                {
                    var taskReminder = new TaskReminderSchedule
                    {
                        TaskId = taskId,
                        Title = reminder.Title,
                        Description = reminder.Description,
                        ReminderType = reminder.ReminderType,
                        IntervalDays = reminder.IntervalDays,
                        DaysBeforeDeadline = reminder.DaysBeforeDeadline,
                        StartDate = reminder.StartDate,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    _context.TaskReminderSchedule_Tbl.Add(taskReminder);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ذخیره یادآوری‌های تسک: {ex.Message}", ex);
            }
        }

        public async Task<List<TaskReminderViewModel>> GetTaskRemindersListAsync(int taskId)
        {
            try
            {
                var reminders = await _context.TaskReminderSchedule_Tbl
                    .Include(r => r.Creator)
                    .Where(r => r.TaskId == taskId && r.IsActive) // ⭐⭐⭐ نمایش همه (حتی منقضی‌ها)
                    .OrderByDescending(r => r.IsExpired) // ⭐ منقضی‌ها در انتها
                    .ThenByDescending(r => r.CreatedDate)
                    .Select(r => new TaskReminderViewModel
                    {
                        Id = r.Id,
                        TaskId = r.TaskId,
                        Title = r.Title,
                        Description = r.Description,
                        ReminderType = r.ReminderType,
                        IntervalDays = r.IntervalDays,
                        DaysBeforeDeadline = r.DaysBeforeDeadline,
                        ScheduledDaysOfMonth = r.ScheduledDaysOfMonth,
                        StartDatePersian = r.StartDate.HasValue
                            ? ConvertDateTime.ConvertMiladiToShamsi(r.StartDate.Value, "yyyy/MM/dd")
                            : null,
                        EndDatePersian = r.EndDate.HasValue
                            ? ConvertDateTime.ConvertMiladiToShamsi(r.EndDate.Value, "yyyy/MM/dd")
                            : null,
                        NotificationTime = r.NotificationTime,
                        IsSystemDefault = r.IsSystemDefault,
                        IsActive = r.IsActive,
                        // ⭐⭐⭐ فیلدهای منقضی
                        IsExpired = r.IsExpired,
                        ExpiredReason = r.ExpiredReason,
                        ExpiredDatePersian = r.ExpiredDate.HasValue
                            ? ConvertDateTime.ConvertMiladiToShamsi(r.ExpiredDate.Value, "yyyy/MM/dd HH:mm")
                            : null,
                        CreatedDate = r.CreatedDate,
                        CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.CreatedDate, "yyyy/MM/dd HH:mm"),
                        CreatorName = r.Creator != null ? $"{r.Creator.FirstName} {r.Creator.LastName}" : "سیستم"
                    })
                    .ToListAsync();

                return reminders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskRemindersListAsync: {ex.Message}");
                return new List<TaskReminderViewModel>();
            }
        }

        public async Task<TaskReminderSchedule> GetReminderByIdAsync(int reminderId)
        {
            try
            {
                return await _context.TaskReminderSchedule_Tbl
                    .Include(r => r.Task)
                    .Include(r => r.Creator)
                    .FirstOrDefaultAsync(r => r.Id == reminderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetReminderByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> CreateReminderAsync(TaskReminderViewModel model, string userId)
        {
            try
            {
                var reminder = _mapper.Map<TaskReminderSchedule>(model);
                reminder.IsSystemDefault = false;
                reminder.CreatedDate = DateTime.Now;
                reminder.CreatorUserId = userId;

                // تبدیل تاریخ شمسی به میلادی
                if (!string.IsNullOrEmpty(model.StartDatePersian))
                {
                    reminder.StartDate = ConvertDateTime.ConvertShamsiToMiladi(model.StartDatePersian);
                }

                if (!string.IsNullOrEmpty(model.EndDatePersian))
                {
                    reminder.EndDate = ConvertDateTime.ConvertShamsiToMiladi(model.EndDatePersian);
                }

                // ⭐⭐⭐ Copy ScheduledDaysOfMonth 🆕
                if (!string.IsNullOrEmpty(model.ScheduledDaysOfMonth))
                {
                    reminder.ScheduledDaysOfMonth = model.ScheduledDaysOfMonth;
                }

                _context.TaskReminderSchedule_Tbl.Add(reminder);
                await _context.SaveChangesAsync();

                return reminder.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CreateReminderAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> DeactivateReminderAsync(int reminderId)
        {
            try
            {
                var reminder = await _context.TaskReminderSchedule_Tbl.FindAsync(reminderId);
                if (reminder == null)
                {
                    return false;
                }

                reminder.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DeactivateReminderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleReminderActiveStatusAsync(int reminderId)
        {
            try
            {
                var reminder = await _context.TaskReminderSchedule_Tbl.FindAsync(reminderId);
                if (reminder == null)
                {
                    return false;
                }

                reminder.IsActive = !reminder.IsActive;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ToggleReminderActiveStatusAsync: {ex.Message}");
                return false;
            }
        }

        private async Task DeactivateTaskRemindersAsync(int taskId)
        {
            try
            {
                var activeReminders = await _context.TaskReminderSchedule_Tbl
                    .Where(r => r.TaskId == taskId && r.IsActive && !r.IsExpired) // ⭐⭐⭐ فقط یادآورهای غیر منقضی
                    .ToListAsync();

                if (!activeReminders.Any())
                {
                    Console.WriteLine($"ℹ️ No active reminders found for task {taskId}");
                    return;
                }

                // ⭐⭐⭐ منقضی کردن به جای غیرفعال کردن
                foreach (var reminder in activeReminders)
                {
                    reminder.IsExpired = true;
                    reminder.ExpiredReason = "تسک تکمیل شده";
                    reminder.ExpiredDate = DateTime.Now;
                    _context.TaskReminderSchedule_Tbl.Update(reminder);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Expired {activeReminders.Count} reminders for task {taskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error expiring reminders for task {taskId}: {ex.Message}");
            }
        }

        public async Task<bool> DeactivateAllTaskRemindersAsync(int taskId)
        {
            try
            {
                await DeactivateTaskRemindersAsync(taskId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DeactivateAllTaskRemindersAsync: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Reminder Events Management

        public async Task<TaskRemindersViewModel> GetTaskRemindersAsync(string userId, TaskReminderFilterViewModel filters)
        {
            try
            {
                var now = DateTime.Now;
                var today = now.Date;

                var query = _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId)
                    .Include(r => r.Task)
                    .AsQueryable();

                if (filters != null)
                {
                    // فیلتر قدیمی (برای سازگاری)
                    if (!string.IsNullOrEmpty(filters.FilterType) && filters.FilterType != "all")
                    {
                        switch (filters.FilterType.ToLower())
                        {
                            case "pending":
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime <= now);
                                break;
                            case "sent":
                                query = query.Where(r => r.IsSent);
                                break;
                            case "overdue":
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime < now);
                                break;
                            case "today":
                                query = query.Where(r => r.ScheduledDateTime.Date == today);
                                break;
                            case "upcoming":
                                var maxDate = now.AddDays(filters.DaysAhead ?? 1);
                                query = query.Where(r => !r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= maxDate);
                                break;
                            case "unread":
                                query = query.Where(r => r.IsSent && !r.IsRead);
                                break;
                        }
                    }

                    // فیلترهای زمانی
                    if (filters.FromDate.HasValue)
                    {
                        query = query.Where(r => r.ScheduledDateTime >= filters.FromDate.Value);
                    }

                    if (filters.ToDate.HasValue)
                    {
                        query = query.Where(r => r.ScheduledDateTime <= filters.ToDate.Value);
                    }

                    // فیلترهای وضعیت
                    if (filters.IsSent.HasValue)
                    {
                        query = query.Where(r => r.IsSent == filters.IsSent.Value);
                    }

                    if (filters.IsRead.HasValue)
                    {
                        query = query.Where(r => r.IsRead == filters.IsRead.Value);
                    }

                    if (filters.Priority.HasValue)
                    {
                        query = query.Where(r => r.Priority == filters.Priority.Value);
                    }

                    // فیلترهای تسک
                    if (filters.TaskId.HasValue)
                    {
                        query = query.Where(r => r.TaskId == filters.TaskId.Value);
                    }

                    if (filters.TaskIds?.Any() == true)
                    {
                        query = query.Where(r => filters.TaskIds.Contains(r.TaskId));
                    }

                    if (!string.IsNullOrEmpty(filters.TaskTitle))
                    {
                        query = query.Where(r => r.Task != null && r.Task.Title.Contains(filters.TaskTitle));
                    }
                }

                // مرتب‌سازی
                query = ApplyReminderSorting(query, filters);

                // صفحه‌بندی یا محدودیت داشبورد
                if (filters?.ForDashboard == true && filters.DashboardLimit.HasValue)
                {
                    query = query.Take(filters.DashboardLimit.Value);
                }
                else
                {
                    var skip = ((filters?.Page ?? 1) - 1) * (filters?.PageSize ?? 20);
                    query = query.Skip(skip).Take(filters?.PageSize ?? 20);
                }

                var reminders = await query.ToListAsync();

                var reminderViewModels = reminders.Select(r => new TaskReminderItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Message = r.Message,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title,
                    TaskCode = r.Task?.TaskCode,
                    ScheduledDateTime = r.ScheduledDateTime,
                    ScheduledDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.ScheduledDateTime, "yyyy/MM/dd HH:mm"),
                    IsSent = r.IsSent,
                    IsRead = r.IsRead,
                    Priority = r.Priority,
                }).ToList();

                // محاسبه آمار
                var statsQuery = _context.TaskReminderEvent_Tbl.Where(r => r.RecipientUserId == userId);
                var stats = new TaskRemindersStatsViewModel
                {
                    PendingCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime <= now),
                    SentCount = await statsQuery.CountAsync(r => r.IsSent),
                    OverdueCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime < now),
                    TodayCount = await statsQuery.CountAsync(r => r.ScheduledDateTime.Date == today),
                    UnreadCount = await statsQuery.CountAsync(r => r.IsSent && !r.IsRead),
                    UpcomingCount = await statsQuery.CountAsync(r => !r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= now.AddDays(1))
                };

                return new TaskRemindersViewModel
                {
                    Reminders = reminderViewModels,
                    Stats = stats,
                    Filters = filters ?? new TaskReminderFilterViewModel { FilterType = "all" },
                    TotalCount = reminderViewModels.Count,
                    CurrentPage = filters?.Page ?? 1,
                    PageSize = filters?.PageSize ?? 20
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت یادآوری‌ها: {ex.Message}", ex);
            }
        }

        public async Task<List<TaskReminderItemViewModel>> GetDashboardRemindersAsync(string userId, int maxResults = 10, int daysAhead = 1)
        {
            try
            {
                var now = DateTime.Now;
                var maxDate = now.AddDays(daysAhead);

                var reminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId &&
                               (
                                   (!r.IsSent && r.ScheduledDateTime < now) ||
                                   (!r.IsSent && r.ScheduledDateTime >= now && r.ScheduledDateTime <= maxDate) ||
                                   (r.IsSent && !r.IsRead)
                               ))
                    .Include(r => r.Task)
                    .OrderBy(r => r.ScheduledDateTime)
                    .Take(maxResults)
                    .ToListAsync();

                return reminders.Select(r => new TaskReminderItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Message = r.Message,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title,
                    TaskCode = r.Task?.TaskCode,
                    ScheduledDateTime = r.ScheduledDateTime,
                    ScheduledDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.ScheduledDateTime, "yyyy/MM/dd HH:mm"),
                    IsSent = r.IsSent,
                    IsRead = r.IsRead,
                    Priority = r.Priority,
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت یادآوری‌های داشبورد: {ex.Message}", ex);
            }
        }

        public async Task MarkReminderAsReadAsync(int reminderId, string userId)
        {
            try
            {
                var reminder = await _context.TaskReminderEvent_Tbl
                    .FirstOrDefaultAsync(r => r.Id == reminderId && r.RecipientUserId == userId);

                if (reminder != null)
                {
                    reminder.IsRead = true;
                    reminder.ReadDateTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در علامت‌گذاری یادآوری: {ex.Message}", ex);
            }
        }

        public async Task MarkAllRemindersAsReadAsync(string userId)
        {
            try
            {
                var reminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && !r.IsRead)
                    .ToListAsync();

                foreach (var reminder in reminders)
                {
                    reminder.IsRead = true;
                    reminder.ReadDateTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در علامت‌گذاری همه یادآوری‌ها: {ex.Message}", ex);
            }
        }

        public async Task DeleteReminderAsync(int reminderId, string userId)
        {
            try
            {
                var reminder = await _context.TaskReminderEvent_Tbl
                    .FirstOrDefaultAsync(r => r.Id == reminderId && r.RecipientUserId == userId);

                if (reminder != null)
                {
                    _context.TaskReminderEvent_Tbl.Remove(reminder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در حذف یادآوری: {ex.Message}", ex);
            }
        }

        public async Task DeleteReadRemindersAsync(string userId)
        {
            try
            {
                var readReminders = await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && r.IsRead)
                    .ToListAsync();

                _context.TaskReminderEvent_Tbl.RemoveRange(readReminders);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در حذف یادآوری‌های خوانده شده: {ex.Message}", ex);
            }
        }

        private IQueryable<TaskReminderEvent> ApplyReminderSorting(IQueryable<TaskReminderEvent> query, TaskReminderFilterViewModel filters)
        {
            if (filters == null) return query.OrderByDescending(r => r.ScheduledDateTime);

            var sortBy = filters.SortBy?.ToLower() ?? "scheduleddatetime";
            var isDescending = filters.SortDirection?.ToLower() == "desc";

            return sortBy switch
            {
                "scheduleddatetime" => isDescending ? query.OrderByDescending(r => r.ScheduledDateTime) : query.OrderBy(r => r.ScheduledDateTime),
                "title" => isDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "priority" => isDescending ? query.OrderByDescending(r => r.Priority) : query.OrderBy(r => r.Priority),
                "issent" => isDescending ? query.OrderByDescending(r => r.IsSent) : query.OrderBy(r => r.IsSent),
                "isread" => isDescending ? query.OrderByDescending(r => r.IsRead) : query.OrderBy(r => r.IsRead),
                "tasktitle" => isDescending ? query.OrderByDescending(r => r.Task.Title) : query.OrderBy(r => r.Task.Title),
                _ => isDescending ? query.OrderByDescending(r => r.ScheduledDateTime) : query.OrderBy(r => r.ScheduledDateTime)
            };
        }

        #endregion
    }
}
