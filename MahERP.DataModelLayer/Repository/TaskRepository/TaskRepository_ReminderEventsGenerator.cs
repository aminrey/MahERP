using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// ⭐⭐⭐ Partial Class برای مدیریت تولید Event های یادآوری تسک
    /// این کلاس مسئول تولید اولیه TaskReminderEvent ها هنگام ایجاد/بروزرسانی یادآوری است
    /// </summary>
    public partial class TaskRepository
    {
        /// <summary>
        /// تولید Event های اولیه برای یادآوری‌های یک تسک
        /// این متد بلافاصله بعد از ذخیره یادآوری‌ها در SaveTaskReminders فراخوانی می‌شود
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        public async Task GenerateInitialReminderEventsAsync(int taskId)
        {
            try
            {
                Console.WriteLine($"🔔 GenerateInitialReminderEventsAsync started for Task {taskId}");

                // ⭐ دریافت تسک با اطلاعات کامل
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    Console.WriteLine($"❌ Task {taskId} not found");
                    return;
                }

                // ⭐ دریافت یادآوری‌های فعال تسک
                var reminders = await _context.TaskReminderSchedule_Tbl
                    .Where(r => r.TaskId == taskId && r.IsActive)
                    .ToListAsync();

                if (!reminders.Any())
                {
                    Console.WriteLine($"ℹ️ No active reminders found for Task {taskId}");
                    return;
                }

                Console.WriteLine($"📋 Found {reminders.Count} active reminders");

                // ⭐⭐⭐ دریافت لیست کاربران اختصاص داده شده که تسک را تکمیل نکرده‌اند
                var assignedUserIds = task.TaskAssignments
                    .Where(a => !a.CompletionDate.HasValue) // ⭐ فقط کاربرانی که تسک را تکمیل نکرده‌اند
                    .Select(a => a.AssignedUserId)
                    .Distinct()
                    .ToList();

                if (!assignedUserIds.Any())
                {
                    Console.WriteLine($"⚠️ No uncompleted users assigned to Task {taskId}");
                    return;
                }

                Console.WriteLine($"👥 Found {assignedUserIds.Count} uncompleted assigned users");

                var now = DateTime.Now;
                var eventsToAdd = new List<TaskReminderEvent>();

                // ⭐ تولید Event برای هر یادآوری
                foreach (var reminder in reminders)
                {
                    DateTime? nextEventDate = null;

                    // 🔹 محاسبه تاریخ Event بر اساس نوع یادآوری
                    switch (reminder.ReminderType)
                    {
                        case 0: // یکبار
                            nextEventDate = reminder.StartDate;
                            break;

                        case 1: // تکراری
                            if (reminder.IntervalDays.HasValue && reminder.IntervalDays > 0)
                            {
                                var startDate = reminder.StartDate ?? now;
                                nextEventDate = startDate;

                                // اگر تاریخ شروع گذشته، اولین تاریخ آینده را محاسبه کن
                                if (nextEventDate < now)
                                {
                                    var daysPassed = (now - nextEventDate.Value).Days;
                                    var cyclesPassed = daysPassed / reminder.IntervalDays.Value;
                                    nextEventDate = nextEventDate.Value.AddDays((cyclesPassed + 1) * reminder.IntervalDays.Value);
                                }
                            }
                            break;

                        case 2: // قبل از مهلت
                            if (task.DueDate.HasValue && reminder.DaysBeforeDeadline.HasValue && reminder.DaysBeforeDeadline > 0)
                            {
                                nextEventDate = task.DueDate.Value.AddDays(-reminder.DaysBeforeDeadline.Value);

                                // ⭐⭐⭐ اگر مهلت گذشته، Event ایجاد نکن
                                if (nextEventDate < now)
                                {
                                    Console.WriteLine($"⏳ Reminder '{reminder.Title}' skipped - Due date passed");
                                    continue;
                                }
                            }
                            break;

                        case 4: // ⭐⭐⭐ NEW: ماهانه (چند روز)
                            if (!string.IsNullOrEmpty(reminder.ScheduledDaysOfMonth))
                            {
                                var daysOfMonth = reminder.ScheduledDaysOfMonth
                                    .Split(',')
                                    .Select(d => int.TryParse(d.Trim(), out var day) ? day : (int?)null)
                                    .Where(d => d.HasValue && d.Value >= 1 && d.Value <= 31)
                                    .Select(d => d.Value)
                                    .OrderBy(d => d)
                                    .ToList();

                                if (daysOfMonth.Any())
                                {
                                    // پیدا کردن اولین روز آینده
                                    var currentDay = now.Day;
                                    var currentMonth = now.Month;
                                    var currentYear = now.Year;

                                    // بررسی ماه جاری
                                    var daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                                    var upcomingDaysThisMonth = daysOfMonth
                                        .Where(d => d >= currentDay && d <= daysInCurrentMonth)
                                        .ToList();

                                    if (upcomingDaysThisMonth.Any())
                                    {
                                        // اولین روز در ماه جاری
                                        var nextDay = upcomingDaysThisMonth.First();
                                        nextEventDate = new DateTime(currentYear, currentMonth, nextDay);
                                    }
                                    else
                                    {
                                        // ماه بعد
                                        var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
                                        var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
                                        var daysInNextMonth = DateTime.DaysInMonth(nextYear, nextMonth);

                                        // اولین روز موجود در ماه بعد
                                        var firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextMonth);
                                        if (firstAvailableDay > 0)
                                        {
                                            nextEventDate = new DateTime(nextYear, nextMonth, firstAvailableDay);
                                        }
                                    }

                                    Console.WriteLine($"📅 Monthly reminder (days: {reminder.ScheduledDaysOfMonth}) - Next: {nextEventDate:yyyy-MM-dd}");
                                }
                            }
                            break;
                    }

                    // ⭐ اگر تاریخ محاسبه شد، Event ایجاد کن
                    if (nextEventDate.HasValue)
                    {
                        // اعمال ساعت اعلان (اگر تنظیم شده)
                        if (reminder.NotificationTime != TimeSpan.Zero)
                        {
                            nextEventDate = nextEventDate.Value.Date.Add(reminder.NotificationTime);
                        }

                        // ⭐ ایجاد Event برای هر کاربر (فقط کاربران تکمیل نکرده)
                        foreach (var userId in assignedUserIds)
                        {
                            var reminderEvent = new TaskReminderEvent
                            {
                                TaskId = taskId,
                                RecipientUserId = userId,
                                ScheduleId = reminder.Id,
                                Title = reminder.Title ?? "یادآوری تسک",
                                Message = reminder.Description ?? $"یادآوری برای تسک {task.TaskCode}",
                                ScheduledDateTime = nextEventDate.Value,
                                Priority = DeterminePriority(reminder.ReminderType, nextEventDate.Value, task.DueDate),
                                IsSent = false,
                                IsRead = false,
                                CreateDate = now
                            };

                            eventsToAdd.Add(reminderEvent);

                            Console.WriteLine($"✅ Created event for user {userId} at {nextEventDate:yyyy-MM-dd HH:mm}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Could not calculate event date for reminder '{reminder.Title}'");
                    }
                }

                // ⭐ ذخیره Event ها در دیتابیس
                if (eventsToAdd.Any())
                {
                    await _context.TaskReminderEvent_Tbl.AddRangeAsync(eventsToAdd);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"💾 Successfully saved {eventsToAdd.Count} reminder events");
                }
                else
                {
                    Console.WriteLine($"ℹ️ No events to save");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GenerateInitialReminderEventsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // ⚠️ عدم throw کردن exception برای جلوگیری از rollback کل transaction
            }
        }

        /// <summary>
        /// تعیین اولویت Event بر اساس نوع یادآوری و تاریخ مهلت
        /// </summary>
        private byte DeterminePriority(byte reminderType, DateTime eventDate, DateTime? dueDate)
        {
            if (!dueDate.HasValue)
                return 1; // عادی

            var daysUntilDue = (dueDate.Value - eventDate).Days;

            if (daysUntilDue <= 1)
                return 3; // فوری

            if (daysUntilDue <= 3)
                return 2; // مهم

            return 1; // عادی
        }
    }
}
