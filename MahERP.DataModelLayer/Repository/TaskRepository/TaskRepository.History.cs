using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت تاریخچه تسک‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Task History Methods

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
        /// دریافت آیکون برای نوع تغییر
        /// </summary>
        private string GetHistoryIcon(byte actionType)
        {
            return actionType switch
            {
                0 => "fa-plus",              // ایجاد
                1 => "fa-edit",              // ویرایش
                2 => "fa-sync",              // تغییر وضعیت
                3 => "fa-user-plus",         // اضافه کاربر
                4 => "fa-user-minus",        // حذف کاربر
                5 => "fa-plus-circle",       // افزودن عملیات
                7 => "fa-check-circle",      // تکمیل عملیات
                8 => "fa-trash",             // حذف عملیات
                9 => "fa-clipboard-check",   // ثبت گزارش کار
                10 => "fa-bell-plus",        // افزودن یادآوری
                15 => "fa-user-check",       // تایید سرپرست
                16 => "fa-award",            // تایید مدیر
                _ => "fa-circle"
            };
        }

        /// <summary>
        /// دریافت رنگ Badge برای نوع تغییر
        /// </summary>
        private string GetHistoryBadgeClass(byte actionType)
        {
            return actionType switch
            {
                0 => "bg-primary",           // ایجاد
                1 => "bg-warning",           // ویرایش
                2 => "bg-info",              // تغییر وضعیت
                5 => "bg-primary",           // افزودن عملیات
                7 => "bg-success",           // تکمیل عملیات
                8 => "bg-danger",            // حذف
                9 => "bg-info",              // گزارش کار
                10 => "bg-warning",          // یادآوری
                15 => "bg-info",             // تایید سرپرست
                16 => "bg-success",          // تایید مدیر
                _ => "bg-secondary"
            };
        }

        #endregion
    }
}
