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
        /// ثبت تکمیل تسک در تاریخچه - نسخه اصلاح شده
        /// </summary>
        public async Task LogTaskCompletedAsync(int taskId, string userId, string taskTitle, string taskCode)
        {
            try
            {
                // دریافت اطلاعات کاربر
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var userName = user != null ? $"{user.FirstName} {user.LastName}" : "نامشخص";

                // ⭐ ایجاد رکورد تاریخچه با فیلدهای صحیح
                var history = new TaskHistory
                {
                    TaskId = taskId,
                    UserId = userId,                    // ✅ اصلاح شده: UserId
                    ActionType = 2,                     // ✅ StatusChanged (تغییر وضعیت)
                    Title = "تکمیل تسک",
                    Description = $"تسک «{taskTitle}» ({taskCode}) توسط {userName} تکمیل شد",
                    OldValue = "در حال انجام",
                    NewValue = "تکمیل شده - منتظر تایید",
                    ActionDate = DateTime.Now,
                    // ⭐ فیلدهای اختیاری:
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent()
                };

                await _context.TaskHistory_Tbl.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LogTaskCompletedAsync: {ex.Message}");
            }
        }
        #region Helper Methods

        /// <summary>
        /// دریافت آیکون برای نوع تغییر
        /// </summary>
        public string GetHistoryIcon(byte actionType)
        {
            return actionType switch
            {
                0 => "fa-plus",              // ایجاد
                1 => "fa-edit",              // ویرایش
                2 => "fa-sync",              // تغییر وضعیت
                3 => "fa-user-plus",         // اضافه کاربر
                4 => "fa-user-minus",        // حذف کاربر
                5 => "fa-plus-circle",       // افزودن عملیات
                6 => "fa-edit",              // ویرایش عملیات
                7 => "fa-check-circle",      // تکمیل عملیات
                8 => "fa-trash",             // حذف عملیات
                9 => "fa-clipboard-check",   // ثبت گزارش کار
                10 => "fa-bell-plus",        // افزودن یادآوری
                11 => "fa-bell",             // ویرایش یادآوری
                12 => "fa-bell-slash",       // حذف یادآوری
                15 => "fa-user-check",       // تایید سرپرست
                16 => "fa-award",            // تایید مدیر
                17 => "fa-times-circle",     // رد تسک
                _ => "fa-circle"
            };
        }

        /// <summary>
        /// دریافت رنگ Badge برای نوع تغییر
        /// </summary>
        public string GetHistoryBadgeClass(byte actionType)
        {
            return actionType switch
            {
                0 => "bg-primary",           // ایجاد
                1 => "bg-warning",           // ویرایش
                2 => "bg-info",              // تغییر وضعیت
                3 => "bg-info",              // اضافه کاربر
                4 => "bg-warning",           // حذف کاربر
                5 => "bg-primary",           // افزودن عملیات
                6 => "bg-warning",           // ویرایش عملیات
                7 => "bg-success",           // تکمیل عملیات
                8 => "bg-danger",            // حذف
                9 => "bg-info",              // گزارش کار
                10 => "bg-primary",          // افزودن یادآوری
                11 => "bg-warning",          // ویرایش یادآوری
                12 => "bg-danger",           // حذف یادآوری
                15 => "bg-info",             // تایید سرپرست
                16 => "bg-success",          // تایید مدیر
                17 => "bg-danger",           // رد تسک
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