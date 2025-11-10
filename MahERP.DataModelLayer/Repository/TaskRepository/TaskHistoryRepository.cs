using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    /// <summary>
    /// Repository برای مدیریت تاریخچه تسک‌ها
    /// </summary>
    public class TaskHistoryRepository : ITaskHistoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskHistoryRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Core Methods

        /// <summary>
        /// ثبت تغییر در تاریخچه
        /// </summary>
        public async Task<int> LogHistoryAsync(
            int taskId,
            string userId,
            TaskHistoryActionType actionType,
            string title,
            string description = null,
            int? relatedItemId = null,
            string relatedItemType = null,
            object oldValue = null,
            object newValue = null)
        {
            try
            {
                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = (byte)actionType,
                    Title = title,
                    Description = description,
                    RelatedItemId = relatedItemId,
                    RelatedItemType = relatedItemType,
                    OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                    NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                    ActionDate = DateTime.Now,
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent()
                };

                _context.TaskHistory_Tbl.Add(history);
                await _context.SaveChangesAsync();

                return history.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging task history: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// دریافت تاریخچه کامل تسک
        /// </summary>
        public async Task<List<TaskHistoryViewModel>> GetTaskHistoryAsync(int taskId)
        {
            try
            {
                var histories = await _context.TaskHistory_Tbl
                    .Include(h => h.User)
                    .Where(h => h.TaskId == taskId)
                    .OrderByDescending(h => h.ActionDate)
                    .ToListAsync();

                return histories.Select(h => new TaskHistoryViewModel
                {
                    Id = h.Id,
                    ActionType = h.ActionType,
                    Title = h.Title,
                    Description = h.Description,
                    UserName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "سیستم",
                    ActionDate = h.ActionDate,
                    ActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(h.ActionDate, "yyyy/MM/dd HH:mm"),
                    RelatedItemId = h.RelatedItemId,
                    RelatedItemType = h.RelatedItemType,
                    IconClass = GetHistoryIcon(h.ActionType),
                    BadgeClass = GetHistoryBadgeClass(h.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting task history: {ex.Message}");
                return new List<TaskHistoryViewModel>();
            }
        }

        /// <summary>
        /// دریافت آخرین تغییرات تسک
        /// </summary>
        public async Task<List<TaskHistoryViewModel>> GetRecentTaskHistoryAsync(int taskId, int take = 10)
        {
            try
            {
                var histories = await _context.TaskHistory_Tbl
                    .Include(h => h.User)
                    .Where(h => h.TaskId == taskId)
                    .OrderByDescending(h => h.ActionDate)
                    .Take(take)
                    .ToListAsync();

                return histories.Select(h => new TaskHistoryViewModel
                {
                    Id = h.Id,
                    ActionType = h.ActionType,
                    Title = h.Title,
                    Description = h.Description,
                    UserName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "سیستم",
                    ActionDate = h.ActionDate,
                    ActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(h.ActionDate, "yyyy/MM/dd HH:mm"),
                    IconClass = GetHistoryIcon(h.ActionType),
                    BadgeClass = GetHistoryBadgeClass(h.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting recent task history: {ex.Message}");
                return new List<TaskHistoryViewModel>();
            }
        }

        /// <summary>
        /// حذف تاریخچه تسک
        /// </summary>
        public async Task<bool> DeleteTaskHistoryAsync(int taskId)
        {
            try
            {
                var histories = await _context.TaskHistory_Tbl
                    .Where(h => h.TaskId == taskId)
                    .ToListAsync();

                _context.TaskHistory_Tbl.RemoveRange(histories);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting task history: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Specific History Methods

        /// <summary>
        /// ثبت ایجاد تسک
        /// </summary>
        public async Task LogTaskCreatedAsync(int taskId, string userId, string taskTitle, string taskCode)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.TaskCreated,
                "ایجاد تسک",
                $"تسک «{taskTitle}» با کد «{taskCode}» ایجاد شد"
            );
        }

        /// <summary>
        /// ثبت تکمیل تسک
        /// </summary>
        public async Task LogTaskCompletedAsync(int taskId, string userId, string taskTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.StatusChanged,
                "تکمیل تسک",
                $"تسک «{taskTitle}» تکمیل شد"
            );
        }

        /// <summary>
        /// ثبت ویرایش تسک
        /// </summary>
        public async Task LogTaskEditedAsync(int taskId, string userId, string taskTitle, object oldValues, object newValues)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.TaskEdited,
                "ویرایش تسک",
                $"تسک «{taskTitle}» ویرایش شد",
                null,
                null,
                oldValues,
                newValues
            );
        }

        /// <summary>
        /// ثبت افزودن عملیات
        /// </summary>
        public async Task LogOperationAddedAsync(int taskId, string userId, int operationId, string operationTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.OperationAdded,
                "افزودن عملیات",
                $"عملیات «{operationTitle}» اضافه شد",
                operationId,
                "TaskOperation"
            );
        }

        /// <summary>
        /// ثبت ویرایش عملیات
        /// </summary>
        public async Task LogOperationEditedAsync(int taskId, string userId, int operationId, string operationTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.OperationEdited,
                "ویرایش عملیات",
                $"عملیات «{operationTitle}» ویرایش شد",
                operationId,
                "TaskOperation"
            );
        }

        /// <summary>
        /// ثبت تکمیل عملیات
        /// </summary>
        public async Task LogOperationCompletedAsync(int taskId, string userId, int operationId, string operationTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.OperationCompleted,
                "تکمیل عملیات",
                $"عملیات «{operationTitle}» تکمیل شد",
                operationId,
                "TaskOperation"
            );
        }

        /// <summary>
        /// ثبت حذف عملیات
        /// </summary>
        public async Task LogOperationDeletedAsync(int taskId, string userId, int operationId, string operationTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.OperationDeleted,
                "حذف عملیات",
                $"عملیات «{operationTitle}» حذف شد",
                operationId,
                "TaskOperation"
            );
        }

        /// <summary>
        /// ثبت گزارش کار روی عملیات
        /// </summary>
        public async Task LogWorkLogAddedAsync(
            int taskId,
            string userId,
            int operationId,
            string operationTitle,
            int workLogId,
            string workDescription,
            int? durationMinutes = null)
        {
            var durationText = durationMinutes.HasValue ? $" ({durationMinutes} دقیقه)" : "";

            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.WorkLogAdded,
                "ثبت گزارش کار",
                $"گزارش کار برای عملیات «{operationTitle}» ثبت شد{durationText}",
                workLogId,
                "TaskOperationWorkLog",
                null,
                new { OperationId = operationId, Description = workDescription, DurationMinutes = durationMinutes }
            );
        }

        /// <summary>
        /// ثبت افزودن یادآوری
        /// </summary>
        public async Task LogReminderAddedAsync(
            int taskId,
            string userId,
            int reminderId,
            string reminderTitle,
            byte reminderType)
        {
            var typeText = GetReminderTypeText(reminderType);

            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.ReminderAdded,
                "افزودن یادآوری",
                $"یادآوری «{reminderTitle}» ({typeText}) اضافه شد",
                reminderId,
                "TaskReminderSchedule"
            );
        }

        /// <summary>
        /// ثبت حذف یادآوری
        /// </summary>
        public async Task LogReminderDeletedAsync(int taskId, string userId, int reminderId, string reminderTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.ReminderDeleted,
                "حذف یادآوری",
                $"یادآوری «{reminderTitle}» حذف شد",
                reminderId,
                "TaskReminderSchedule"
            );
        }

        /// <summary>
        /// ثبت تایید سرپرست
        /// </summary>
        public async Task LogSupervisorApprovedAsync(int taskId, string userId, string taskTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.SupervisorApproved,
                "تایید سرپرست",
                $"تسک «{taskTitle}» توسط سرپرست تایید شد"
            );
        }

        /// <summary>
        /// ثبت تایید مدیر
        /// </summary>
        public async Task LogManagerApprovedAsync(int taskId, string userId, string taskTitle)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.ManagerApproved,
                "تایید مدیر",
                $"تسک «{taskTitle}» توسط مدیر تایید و نهایی شد"
            );
        }

        /// <summary>
        /// ثبت رد تسک
        /// </summary>
        public async Task LogTaskRejectedAsync(int taskId, string userId, string taskTitle, string reason)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.TaskRejected,
                "رد تسک",
                $"تسک «{taskTitle}» رد شد. دلیل: {reason}"
            );
        }

        /// <summary>
        /// ثبت افزودن کاربر به تسک
        /// </summary>
        public async Task LogUserAssignedAsync(int taskId, string userId, string assignedUserName)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.UserAssigned,
                "اضافه کردن کاربر",
                $"کاربر «{assignedUserName}» به تسک اضافه شد"
            );
        }

        /// <summary>
        /// ثبت حذف کاربر از تسک
        /// </summary>
        public async Task LogUserRemovedAsync(int taskId, string userId, string removedUserName)
        {
            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.UserRemoved,
                "حذف کاربر",
                $"کاربر «{removedUserName}» از تسک حذف شد"
            );
        }
        /// <summary>
        /// ثبت افزودن تسک به "روز من"
        /// </summary>
        public async Task LogTaskAddedToMyDayAsync(int taskId, string userId, string taskTitle, string taskCode, DateTime plannedDate)
        {
            try
            {
                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = 20, // نوع جدید: افزودن به روز من
                    Title = "افزودن به روز من",
                    Description = $"تسک \"{taskTitle}\" ({taskCode}) به روز من اضافه شد - تاریخ: {ConvertDateTime.ConvertMiladiToShamsi(plannedDate, "yyyy/MM/dd")}",
                    ActionDate = DateTime.Now,
                    RelatedItemType = "MyDay",
                    RelatedItemId = taskId
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging MyDay addition: {ex.Message}");
            }
        }

        /// <summary>
        /// ثبت حذف تسک از "روز من"
        /// </summary>
        public async Task LogTaskRemovedFromMyDayAsync(int taskId, string userId, string taskTitle, string taskCode)
        {
            try
            {
                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = 21, // نوع جدید: حذف از روز من
                    Title = "حذف از روز من",
                    Description = $"تسک \"{taskTitle}\" ({taskCode}) از روز من حذف شد",
                    ActionDate = DateTime.Now,
                    RelatedItemType = "MyDay",
                    RelatedItemId = taskId
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging MyDay removal: {ex.Message}");
            }
        }
        #endregion
        /// <summary>
        /// ثبت تکمیل تسک در تاریخچه - با پشتیبانی از تکمیل مستقل/مشترک
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="userId">شناسه کاربر تکمیل کننده</param>
        /// <param name="taskTitle">عنوان تسک</param>
        /// <param name="taskCode">کد تسک</param>
        /// <param name="isFullyCompleted">آیا کل تسک تکمیل شد؟ (برای نمایش متمایز)</param>
        public async Task LogTaskCompletedAsync(
            int taskId,
            string userId,
            string taskTitle,
            string taskCode,
            bool isFullyCompleted = false)
        {
            try
            {
                // ⭐ دریافت اطلاعات کاربر و تسک
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var userName = user != null ? $"{user.FirstName} {user.LastName}" : "نامشخص";

                // ⭐ دریافت اطلاعات تسک
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    Console.WriteLine($"❌ Task {taskId} not found in LogTaskCompletedAsync");
                    return;
                }

                // ⭐⭐⭐ تعیین متن و وضعیت بر اساس نوع تکمیل
                string description;
                string oldValue;
                string newValue;

                if (task.IsIndependentCompletion)
                {
                    // ========================================
                    // ⭐⭐⭐ تکمیل مستقل (Independent)
                    // ========================================

                    var totalMembers = task.TaskAssignments.Count;
                    var completedMembers = task.TaskAssignments.Count(a => a.CompletionDate.HasValue);

                    if (isFullyCompleted)
                    {
                        // ⭐ همه افراد تکمیل کردند
                        description = $"تسک «{taskTitle}» ({taskCode}) توسط {userName} تکمیل شد. " +
                                     $"✅ همه اعضا ({completedMembers}/{totalMembers}) تسک را تکمیل کردند - تسک به پایان رسید";
                        oldValue = $"مستقل - {completedMembers - 1}/{totalMembers} تکمیل شده";
                        newValue = $"مستقل - همه ({completedMembers}/{totalMembers}) تکمیل شدند ✅";
                    }
                    else
                    {
                        // ⭐ هنوز برخی افراد تکمیل نکرده‌اند
                        description = $"تسک «{taskTitle}» ({taskCode}) توسط {userName} تکمیل شد. " +
                                     $"⏳ {completedMembers}/{totalMembers} نفر تکمیل کردند - منتظر تکمیل سایرین";
                        oldValue = $"مستقل - {completedMembers - 1}/{totalMembers} تکمیل شده";
                        newValue = $"مستقل - {completedMembers}/{totalMembers} تکمیل شده";
                    }
                }
                else
                {
                    // ========================================
                    // ⭐⭐⭐ تکمیل مشترک (Shared)
                    // ========================================

                    var totalMembers = task.TaskAssignments.Count;
                    description = $"تسک «{taskTitle}» ({taskCode}) توسط {userName} تکمیل شد. " +
                                 $"✅ تسک برای همه ({totalMembers} نفر) قفل شد";
                    oldValue = "در حال انجام";
                    newValue = "تکمیل شده - برای همه قفل شد ✅";
                }

                // ⭐ ایجاد رکورد تاریخچه
                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = 2, // StatusChanged
                    Title = isFullyCompleted ? "تکمیل نهایی تسک" : "تکمیل تسک",
                    Description = description,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ActionDate = DateTime.Now,
                    RelatedItemType = task.IsIndependentCompletion ? "IndependentCompletion" : "SharedCompletion",
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent()
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Logged task completion: {description}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogTaskCompletedAsync: {ex.Message}");
            }
        }
        /// <summary>
        /// ثبت شروع کار روی تسک توسط یک عضو (مخصوص تکمیل مستقل)
        /// </summary>
        public async Task LogTaskStartedByMemberAsync(
            int taskId,
            string userId,
            string taskTitle,
            string taskCode)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var userName = user != null ? $"{user.FirstName} {user.LastName}" : "نامشخص";

                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = 2, // StatusChanged
                    Title = "شروع کار روی تسک",
                    Description = $"{userName} شروع به کار روی تسک «{taskTitle}» ({taskCode}) کرد",
                    OldValue = "اختصاص داده شده",
                    NewValue = "در حال انجام",
                    ActionDate = DateTime.Now,
                    RelatedItemType = "TaskStart",
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent()
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogTaskStartedByMemberAsync: {ex.Message}");
            }
        }
        /// <summary>
        /// ثبت بروزرسانی گزارش تکمیل (زمانی که کاربر قبلاً تکمیل کرده و فقط گزارش را ویرایش می‌کند)
        /// </summary>
        public async Task LogCompletionReportUpdatedAsync(
            int taskId,
            string userId,
            string taskTitle,
            string taskCode)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var userName = user != null ? $"{user.FirstName} {user.LastName}" : "نامشخص";

                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = 1, // TaskEdited
                    Title = "بروزرسانی گزارش تکمیل",
                    Description = $"{userName} گزارش تکمیل تسک «{taskTitle}» ({taskCode}) را بروزرسانی کرد",
                    OldValue = "گزارش قبلی",
                    NewValue = "گزارش جدید",
                    ActionDate = DateTime.Now,
                    RelatedItemType = "CompletionReportUpdate",
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent()
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogCompletionReportUpdatedAsync: {ex.Message}");
            }
        }
        /// <summary>
        /// ثبت غیرفعال شدن یادآوری‌ها هنگام تکمیل تسک
        /// </summary>
        public async Task LogRemindersDeactivatedOnCompletionAsync(
            int taskId,
            string userId,
            string taskTitle,
            string taskCode)
        {
            try
            {
                var historyEntry = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,
                    ActionType = (byte)TaskHistoryActionType.RemindersDeactivatedOnCompletion, // ⭐ نیاز به اضافه کردن enum
                    Title = "غیرفعال شدن یادآوری‌ها",
                    Description = $"یادآوری‌های تسک '{taskTitle}' ({taskCode}) به دلیل تکمیل تسک، خودکار غیرفعال شدند",
                    ActionDate = DateTime.Now,
                    RelatedItemType = "TaskReminder"
                };

                await _context.TaskHistory_Tbl.AddAsync(historyEntry);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Logged reminder deactivation for task {taskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging reminder deactivation: {ex.Message}");
            }
        }

        public async Task LogCommentAddedAsync(int taskId, string userId, int commentId, string commentPreview)
        {
            var history = new TaskHistory
            {
                TaskId = taskId,
                Title="ثبت نظر در تسک",
                UserId = userId,
                ActionType = 24,
                ActionDate = DateTime.Now,
                Description = $"افزودن کامنت: {commentPreview}...",
                OldValue = null,
                NewValue = commentId.ToString()
            };

            await _context.TaskHistory_Tbl.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task LogCommentDeletedAsync(int taskId, string userId, int commentId)
        {
            var history = new TaskHistory
            {
                TaskId = taskId,
                Title = "حذف نظر در تسک",

                UserId = userId,
                ActionType = 25,
                ActionDate = DateTime.Now,
                Description = "حذف کامنت",
                OldValue = commentId.ToString(),
                NewValue = null
            };

            await _context.TaskHistory_Tbl.AddAsync(history);
            await _context.SaveChangesAsync();
        }
        #region Helper Methods
        /// <summary>
        /// ثبت گزارش کار روی تسک (سطح کلی)
        /// </summary>
        public async Task LogTaskWorkLogAddedAsync(
            int taskId,
            string userId,
            int workLogId,
            string workDescription,
            int? durationMinutes = null)
        {
            var durationText = durationMinutes.HasValue ? $" ({durationMinutes} دقیقه)" : "";

            await LogHistoryAsync(
                taskId,
                userId,
                TaskHistoryActionType.TaskWorkLogAdded, // ⭐ استفاده از enum جدید
                "ثبت گزارش کار روی تسک",
                $"کاربر گزارش کار جدیدی ثبت کرد: {workDescription}{durationText}",
                workLogId,
                "TaskWorkLog"
            );
        }
        /// <summary>
        /// دریافت آیکون برای نوع تغییر - بروزرسانی شده
        /// </summary>
        public string GetHistoryIcon(byte actionType)
        {
            return actionType switch
            {
                0 => "fa-plus",                    // ایجاد
                1 => "fa-edit",                    // ویرایش
                2 => "fa-check-circle",            // ⭐ تغییر: تکمیل/تغییر وضعیت
                3 => "fa-user-plus",               // اضافه کاربر
                4 => "fa-user-minus",              // حذف کاربر
                5 => "fa-plus-circle",             // افزودن عملیات
                6 => "fa-edit",                    // ویرایش عملیات
                7 => "fa-check-square",            // تکمیل عملیات
                8 => "fa-trash",                   // حذف عملیات
                9 => "fa-clipboard-check",         // ثبت گزارش کار
                10 => "fa-bell-plus",              // افزودن یادآوری
                15 => "fa-user-check",             // تایید سرپرست
                16 => "fa-award",                  // تایید مدیر
                20 => "fa-calendar-plus",          // افزودن به روز من
                21 => "fa-calendar-minus",         // حذف از روز من
                _ => "fa-circle"
            };
        }

        /// <summary>
        /// دریافت رنگ Badge - بروزرسانی شده
        /// </summary>
        public string GetHistoryBadgeClass(byte actionType)
        {
            return actionType switch
            {
                0 => "bg-primary",                 // ایجاد
                1 => "bg-warning",                 // ویرایش
                2 => "bg-success",                 // ⭐ تغییر: تکمیل/تغییر وضعیت
                3 => "bg-info",                    // اضافه کاربر
                4 => "bg-warning",                 // حذف کاربر
                5 => "bg-primary",                 // افزودن عملیات
                7 => "bg-success",                 // تکمیل عملیات
                8 => "bg-danger",                  // حذف
                9 => "bg-info",                    // گزارش کار
                10 => "bg-primary",                // افزودن یادآوری
                15 => "bg-info",                   // تایید سرپرست
                16 => "bg-success",                // تایید مدیر
                20 => "bg-info",                   // افزودن به روز من
                21 => "bg-secondary",              // حذف از روز من
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// دریافت متن نوع تغییر
        /// </summary>
        public string GetActionTypeText(byte actionType)
        {
            return actionType switch
            {
                0 => "ایجاد تسک",
                1 => "ویرایش تسک",
                2 => "تغییر وضعیت",
                3 => "افزودن کاربر",
                4 => "حذف کاربر",
                5 => "افزودن عملیات",
                6 => "ویرایش عملیات",
                7 => "تکمیل عملیات",
                8 => "حذف عملیات",
                9 => "ثبت گزارش کار",
                10 => "افزودن یادآوری",
                11 => "ویرایش یادآوری",
                12 => "حذف یادآوری",
                15 => "تایید سرپرست",
                16 => "تایید مدیر",
                17 => "رد تسک",
                _ => "نامشخص"
            };
        }

        private string GetUserIp()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GetUserAgent()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GetReminderTypeText(byte reminderType)
        {
            return reminderType switch
            {
                0 => "یکبار",
                1 => "تکراری",
                2 => "قبل از مهلت",
                3 => "روز شروع",
                4 => "روز پایان",
                _ => "نامشخص"
            };
        }

        #endregion
    }
}