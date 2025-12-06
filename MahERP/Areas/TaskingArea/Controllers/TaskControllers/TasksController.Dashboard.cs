using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Dashboard & Listing - صفحات لیست و داشبورد تسک‌ها
    /// </summary>
    public partial class TasksController
    {
        #region Dashboard & Index

        /// <summary>
        /// صفحه اصلی لیست تسک‌ها - نسخه جدید
        /// </summary>
        public async Task<IActionResult> Index(
            TaskViewType viewType = TaskViewType.MyTasks,
            TaskGroupingType grouping = TaskGroupingType.Team,
            QuickStatusFilter? statusFilter = null,
            TaskFilterViewModel advancedFilters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ اعمال فیلتر پیش‌فرض (فقط در حال انجام)
                if (!statusFilter.HasValue && advancedFilters == null)
                {
                    statusFilter = QuickStatusFilter.Pending;
                }

                // ⭐⭐⭐ ایجاد filters برای ارسال به Repository
                var filters = advancedFilters ?? new TaskFilterViewModel();

                // ⭐⭐⭐ ذخیره viewType و grouping در فیلتر
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                // اعمال فیلتر وضعیت سریع
                if (statusFilter.HasValue)
                {
                    filters.TaskStatus = statusFilter.Value switch
                    {
                        QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                        QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                        QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                        QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                        _ => TaskStatusFilter.InProgress
                    };
                }
                else
                {
                    filters.TaskStatus = TaskStatusFilter.InProgress;
                    statusFilter = QuickStatusFilter.Pending;
                }

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                // ⭐⭐⭐ مرتب‌سازی گروه‌ها از جدید به قدیم
                if (model.TaskGroups != null && model.TaskGroups.Any())
                {
                    model.TaskGroups = model.TaskGroups
                        .OrderByDescending(g =>
                        {
                            var allTasks = new List<TaskCardViewModel>();
                            allTasks.AddRange(g.PendingTasks);
                            allTasks.AddRange(g.CompletedTasks);
                            return allTasks.Any() ? allTasks.Max(t => t.CreateDate) : DateTime.MinValue;
                        })
                        .ToList();
                }

                ViewBag.CurrentViewType = viewType;
                ViewBag.CurrentGrouping = grouping;
                ViewBag.CurrentStatusFilter = statusFilter ?? QuickStatusFilter.Pending;
                ViewBag.UserId = userId;
                ViewBag.HasAdvancedFilters = advancedFilters != null;

                // ⭐⭐⭐ تنظیم URL بازگشت به Dashboard کنترلر
                SetBackUrlInViewBag("Index", "Dashboard", "TaskingArea");

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "Index",
                    $"مشاهده لیست تسک‌ها - نمایش: {viewType}, گروه‌بندی: {grouping}, فیلتر: {statusFilter}");

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "Index", "خطا در دریافت لیست تسک‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// تسک‌هایی که کاربر ناظر آن‌هاست
        /// </summary>
        public async Task<IActionResult> SupervisedTasks(TaskFilterViewModel filters = null)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // دریافت تسک‌های نظارتی
                var model = await _taskRepository.GetSupervisedTasksAsync(currentUserId, filters ?? new TaskFilterViewModel());

                // تنظیم ViewBag
                ViewBag.Title = "تسک‌های تحت نظارت";
                ViewBag.IsSupervisedTasks = true;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در دریافت تسک‌های نظارتی: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}
