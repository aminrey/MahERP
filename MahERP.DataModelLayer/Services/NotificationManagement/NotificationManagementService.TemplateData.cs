using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore; // ⭐⭐⭐ FIX: تصحیح using
using Microsoft.Extensions.Logging;
using System.Text;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// ساخت و مدیریت داده‌های قالب (Template Data)
    /// </summary>
    public partial class NotificationManagementService
    {
        #region 📝 ساخت داده‌های قالب - Template Data

        /// <summary>
        /// ساخت دیکشنری کامل داده‌ها برای جایگزینی در قالب
        /// </summary>
        private async Task<Dictionary<string, string>> BuildTemplateDataAsync(
            NotificationEventType eventType,
            string recipientUserId,
            string title,
            string message,
            string actionUrl,
            int systemNotificationId)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Title", title },
                { "Message", message },
                { "Description", message },
                { "ActionUrl", actionUrl },
                { "Date", DateTime.Now.ToString("yyyy/MM/dd") },
                { "Time", DateTime.Now.ToString("HH:mm") }
            };

            try
            {
                var recipient = await _context.Users
                    .Where(u => u.Id == recipientUserId)
                    .Select(u => new { u.FirstName, u.LastName, u.UserName, u.Email, u.PhoneNumber })
                    .FirstOrDefaultAsync();

                if (recipient != null)
                {
                    var fullName = $"{recipient.FirstName} {recipient.LastName}".Trim();
                    
                    data["RecipientFirstName"] = recipient.FirstName ?? "";
                    data["RecipientLastName"] = recipient.LastName ?? "";
                    data["RecipientFullName"] = fullName;
                    data["RecipientUserName"] = recipient.UserName ?? "";
                    data["RecipientEmail"] = recipient.Email ?? "";
                    data["RecipientPhone"] = recipient.PhoneNumber ?? "";
                    
                    data["FirstName"] = recipient.FirstName ?? "";
                    data["LastName"] = recipient.LastName ?? "";
                    data["UserName"] = fullName;
                    data["Email"] = recipient.Email ?? "";
                    data["PhoneNumber"] = recipient.PhoneNumber ?? "";
                }

                if (eventType == NotificationEventType.DailyTaskDigest)
                {
                    var pendingTasksList = await BuildPendingTasksListAsync(recipientUserId);
                    data["PendingTasks"] = pendingTasksList;
                    return data;
                }

                if (IsTaskRelatedEvent(eventType))
                {
                    var coreNotification = await _context.CoreNotification_Tbl
                        .Where(n => n.Id == systemNotificationId)
                        .Select(n => new { n.RelatedRecordId, n.SenderUserId })
                        .FirstOrDefaultAsync();

                    if (coreNotification != null && !string.IsNullOrEmpty(coreNotification.RelatedRecordId))
                    {
                        if (int.TryParse(coreNotification.RelatedRecordId, out int taskId))
                        {
                            await PopulateTaskDataAsync(data, taskId, coreNotification.SenderUserId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ خطا در ساخت داده‌های قالب");
            }

            return data;
        }

        /// <summary>
        /// پر کردن اطلاعات تسک در Dictionary
        /// </summary>
        private async Task PopulateTaskDataAsync(Dictionary<string, string> data, int taskId, string senderUserId)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Select(t => new
                    {
                        t.Title, t.TaskCode, t.Description, t.StartDate, t.DueDate, t.Priority, t.CreatorUserId,
                        CategoryTitle = t.TaskCategory != null ? t.TaskCategory.Title : "",
                        StakeholderName = t.Contact != null 
                            ? $"{t.Contact.FirstName} {t.Contact.LastName}" 
                            : (t.Organization != null ? t.Organization.DisplayName : ""),
                        BranchName = t.Branch != null ? t.Branch.Name : ""
                    })
                    .FirstOrDefaultAsync();

                if (task == null) return;

                data["TaskTitle"] = task.Title ?? "";
                data["TaskCode"] = task.TaskCode ?? "";
                data["TaskDescription"] = task.Description ?? "";
                data["TaskStartDate"] = task.StartDate.HasValue 
                    ? ConvertDateTime.ConvertMiladiToShamsi(task.StartDate.Value, "yyyy/MM/dd") : "";
                data["TaskDueDate"] = task.DueDate.HasValue 
                    ? ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd") : "";
                data["TaskPriority"] = task.Priority switch { 0 => "عادی", 1 => "متوسط", 2 => "بالا", 3 => "فوری", _ => "نامشخص" };
                data["TaskCategory"] = task.CategoryTitle;
                data["TaskStakeholder"] = task.StakeholderName;
                data["TaskBranch"] = task.BranchName;
                data["DueDate"] = data["TaskDueDate"];

                // ⭐⭐⭐ FIX: دریافت اطلاعات سازنده تسک
                if (!string.IsNullOrEmpty(task.CreatorUserId))
                {
                    var creator = await _context.Users
                        .Where(u => u.Id == task.CreatorUserId)
                        .Select(u => new { u.FirstName, u.LastName })
                        .FirstOrDefaultAsync();

                    if (creator != null)
                    {
                        var creatorName = $"{creator.FirstName} {creator.LastName}".Trim();
                        data["TaskCreatorName"] = creatorName;
                        // ⭐⭐⭐ FIX: اضافه کردن هم به عنوان CreatorName برای سازگاری با قالب‌ها
                        data["CreatorName"] = creatorName;
                    }
                }

                // ⭐⭐⭐ FIX: دریافت اطلاعات فرستنده (SenderName)
                if (!string.IsNullOrEmpty(senderUserId) && senderUserId != "SYSTEM")
                {
                    var sender = await _context.Users
                        .Where(u => u.Id == senderUserId)
                        .Select(u => new { u.FirstName, u.LastName })
                        .FirstOrDefaultAsync();

                    data["SenderName"] = sender != null 
                        ? $"{sender.FirstName} {sender.LastName}".Trim() 
                        : "سیستم";
                }
                else
                {
                    data["SenderName"] = "سیستم";
                }

                await PopulateTaskCommentsAsync(data, taskId);
                await PopulateTaskWorkLogsAsync(data, taskId);
                await PopulateTaskCompletionAsync(data, taskId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ خطا در دریافت اطلاعات تسک #{taskId}");
            }
        }

        /// <summary>
        /// پر کردن اطلاعات کامنت‌ها
        /// </summary>
        private async Task PopulateTaskCommentsAsync(Dictionary<string, string> data, int taskId)
        {
            var lastComment = await _context.TaskComment_Tbl
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new
                {
                    c.CommentText,
                    AuthorName = c.Creator != null 
                        ? $"{c.Creator.FirstName} {c.Creator.LastName}" 
                        : "نامشخص",
                    c.CreateDate
                })
                .FirstOrDefaultAsync();

            if (lastComment != null)
            {
                data["CommentText"] = lastComment.CommentText ?? "";
                data["CommentAuthor"] = lastComment.AuthorName;
                data["CommentDate"] = ConvertDateTime.ConvertMiladiToShamsi(lastComment.CreateDate, "yyyy/MM/dd HH:mm");
            }
            else
            {
                data["CommentText"] = "";
                data["CommentAuthor"] = "";
                data["CommentDate"] = "";
            }
        }

        /// <summary>
        /// پر کردن اطلاعات گزارش کار
        /// </summary>
        private async Task PopulateTaskWorkLogsAsync(Dictionary<string, string> data, int taskId)
        {
            var lastWorkLog = await _context.TaskWorkLog_Tbl
                .Where(w => w.TaskId == taskId && !w.IsDeleted)
                .OrderByDescending(w => w.WorkDate)
                .Select(w => new
                {
                    w.WorkDescription, w.DurationMinutes,
                    AuthorName = w.User != null 
                        ? $"{w.User.FirstName} {w.User.LastName}" 
                        : "نامشخص",
                    w.WorkDate
                })
                .FirstOrDefaultAsync();

            if (lastWorkLog != null)
            {
                data["WorkLogText"] = lastWorkLog.WorkDescription ?? "";
                data["WorkLogHours"] = lastWorkLog.DurationMinutes.HasValue 
                    ? $"{lastWorkLog.DurationMinutes.Value / 60.0:F1} ساعت" : "";
                data["WorkLogAuthor"] = lastWorkLog.AuthorName;
                data["WorkLogDate"] = ConvertDateTime.ConvertMiladiToShamsi(lastWorkLog.WorkDate, "yyyy/MM/dd");
            }
            else
            {
                data["WorkLogText"] = "";
                data["WorkLogHours"] = "";
                data["WorkLogAuthor"] = "";
                data["WorkLogDate"] = "";
            }
        }

        /// <summary>
        /// پر کردن اطلاعات تکمیل
        /// </summary>
        private async Task PopulateTaskCompletionAsync(Dictionary<string, string> data, int taskId)
        {
            var lastCompletion = await _context.TaskAssignment_Tbl
                .Where(a => a.TaskId == taskId && a.CompletionDate.HasValue)
                .OrderByDescending(a => a.CompletionDate)
                .Select(a => new
                {
                    CompletionText = a.UserReport,
                    CompletedByName = a.AssignedUser != null 
                        ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" 
                        : "نامشخص",
                    a.CompletionDate
                })
                .FirstOrDefaultAsync();

            if (lastCompletion != null)
            {
                data["CompletionText"] = lastCompletion.CompletionText ?? "";
                data["CompletedBy"] = lastCompletion.CompletedByName;
                data["CompletionDate"] = ConvertDateTime.ConvertMiladiToShamsi(lastCompletion.CompletionDate.Value, "yyyy/MM/dd HH:mm");
            }
            else
            {
                data["CompletionText"] = "";
                data["CompletedBy"] = "";
                data["CompletionDate"] = "";
            }

            data["OldPriority"] = "";
            data["NewPriority"] = data.GetValueOrDefault("TaskPriority", "");
            data["OldStatus"] = "";
            data["NewStatus"] = "";
        }

        /// <summary>
        /// ساخت لیست پیام‌های تسک‌های انجام نشده (هر پیام 10 تسک)
        /// ⭐⭐⭐ FIX: ساختار بهینه - هدر فقط در اولین پیام، فوتر فقط در آخرین پیام
        /// ⭐⭐⭐ FIX: شماره‌گذاری با ایموجی کامل
        /// </summary>
        private async Task<List<string>> BuildPendingTasksMessagesAsync(string userId)
        {
            try
            {
                var messages = new List<string>();

                var pendingTasks = await _context.TaskAssignment_Tbl
                    .Where(a => a.AssignedUserId == userId && !a.CompletionDate.HasValue && !a.Task.IsDeleted)
                    .Include(a => a.Task).ThenInclude(t => t.TaskOperations)
                    .Include(a => a.Task.Creator)
                    .OrderBy(a => a.Task.DueDate).ThenByDescending(a => a.Task.Priority)
                    .Select(a => new
                    {
                        a.Task.Id, a.Task.Title, a.Task.Description, a.Task.StartDate, a.Task.DueDate, a.Task.Priority,
                        CreatorName = a.Task.Creator != null 
                            ? $"{a.Task.Creator.FirstName} {a.Task.Creator.LastName}" : "نامشخص",
                        TotalOperations = a.Task.TaskOperations.Count(o => !o.IsDeleted),
                        CompletedOperations = a.Task.TaskOperations.Count(o => !o.IsDeleted && o.IsCompleted)
                    })
                    .ToListAsync();

                if (!pendingTasks.Any())
                {
                    messages.Add("✅ همه تسک‌های شما تکمیل شده است!");
                    return messages;
                }

                int pageSize = 10;
                int totalPages = (int)Math.Ceiling(pendingTasks.Count / (double)pageSize);

                // ⭐ دریافت اطلاعات کاربر برای سلام
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.FirstName, u.LastName })
                    .FirstOrDefaultAsync();
                
                var userName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "کاربر عزیز";

                for (int page = 0; page < totalPages; page++)
                {
                    var tasksInPage = pendingTasks.Skip(page * pageSize).Take(pageSize).ToList();
                    var result = new StringBuilder();

                    // ⭐⭐⭐ هدر فقط در صفحه اول
                    if (page == 0)
                    {
                        result.AppendLine($"👋 سلام {userName}،");
                        result.AppendLine($"📌 شما {pendingTasks.Count} تسک در حال انجام دارید:");
                        result.AppendLine();
                    }

                    // ⭐ لیست تسک‌ها
                    int startCounter = (page * pageSize) + 1;
                    for (int i = 0; i < tasksInPage.Count; i++)
                    {
                        var task = tasksInPage[i];
                        int counter = startCounter + i;

                        int progressPercentage = task.TotalOperations > 0 
                            ? (task.CompletedOperations * 100) / task.TotalOperations : 0;

                        string priorityEmoji = task.Priority switch { 3 => "🔴", 2 => "🟠", 1 => "🟡", _ => "🟢" };
                        string priorityText = task.Priority switch { 3 => "فوری", 2 => "بالا", 1 => "متوسط", _ => "عادی" };

                        string shortDescription = string.IsNullOrEmpty(task.Description) 
                            ? "بدون توضیحات" 
                            : (task.Description.Length > 30 ? task.Description.Substring(0, 30) + "..." : task.Description);

                        string startDatePersian = task.StartDate.HasValue 
                            ? ConvertDateTime.ConvertMiladiToShamsi(task.StartDate.Value, "yyyy/MM/dd") : "---";
                        string dueDatePersian = task.DueDate.HasValue 
                            ? ConvertDateTime.ConvertMiladiToShamsi(task.DueDate.Value, "yyyy/MM/dd") : "---";

                        // ⭐⭐⭐ NEW: تبدیل شماره به ایموجی کامل
                        string counterEmoji = GetNumberEmoji(counter);

                        result.AppendLine($"{counterEmoji} {task.Title}");
                        result.AppendLine($"   📝 {shortDescription}");
                        result.AppendLine($"   📅 شروع: {startDatePersian} | 🔚 پایان: {dueDatePersian}");
                        result.AppendLine($"   👤 سازنده: {task.CreatorName} | {priorityEmoji} اولویت: {priorityText}");
                        result.AppendLine($"   📊 پیشرفت: {progressPercentage}% ({task.CompletedOperations}/{task.TotalOperations} عملیات)");
                        result.AppendLine();
                    }

                    // ⭐⭐⭐ فوتر فقط در صفحه آخر
                    if (page == totalPages - 1)
                    {
                        result.AppendLine("✅ برای مشاهده جزئیات، به بخش تسک‌ها مراجعه کنید.");
                        result.AppendLine($"📊 جمع: {pendingTasks.Count} تسک");
                    }

                    messages.Add(result.ToString());
                }

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطا در ساخت لیست تسک‌های انجام نشده");
                return new List<string> { "خطا در دریافت لیست تسک‌ها" };
            }
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: تبدیل شماره به ایموجی (با پشتیبانی از 1-99)
        /// ⚠️ FIX: ترتیب درست برای نمایش فارسی (راست به چپ)
        /// </summary>
        private string GetNumberEmoji(int number)
        {
            // ⭐ ایموجی‌های اعداد
            var emojiMap = new Dictionary<int, string>
            {
                {0, "0️⃣"}, {1, "1️⃣"}, {2, "2️⃣"}, {3, "3️⃣"}, {4, "4️⃣"},
                {5, "5️⃣"}, {6, "6️⃣"}, {7, "7️⃣"}, {8, "8️⃣"}, {9, "9️⃣"}
            };

            if (number < 0 || number > 99)
                return number.ToString(); // Fallback برای اعداد خارج از محدوده

            if (number <= 9)
            {
                // ⭐ اعداد یک رقمی
                return emojiMap[number];
            }
            else
            {
                // ⭐⭐⭐ FIX: اعداد دو رقمی - ترتیب درست برای فارسی
                // مثال: 21 → 1️⃣2️⃣ (یکان + دهگان)
                int tens = number / 10;
                int ones = number % 10;
                return emojiMap[ones] + emojiMap[tens]; // ⚠️ ترتیب عوض شد: یکان + دهگان
            }
        }

        /// <summary>
        /// نسخه قدیمی - فقط اولین پیام را برمی‌گرداند
        /// </summary>
        private async Task<string> BuildPendingTasksListAsync(string userId)
        {
            var messages = await BuildPendingTasksMessagesAsync(userId);
            return messages.FirstOrDefault() ?? "";
        }

        #endregion
    }
}
