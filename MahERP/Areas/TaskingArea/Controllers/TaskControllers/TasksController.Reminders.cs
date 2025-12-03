using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Reminders Management - مدیریت یادآوری‌های تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Reminders Management

        /// <summary>
        /// دریافت لیست یادآوری‌های تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskReminders(int taskId)
        {
            try
            {
                var currentUserId = GetUserId();
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new { status = "error", message = "تسک یافت نشد" });
                }

                var reminders = await _taskRepository.GetTaskRemindersListAsync(taskId);

                ViewBag.IsTaskCompleted = task.TaskAssignments?.Any(a => a.CompletionDate.HasValue && a.AssignedUserId == currentUserId) ?? false;

                return PartialView("_TaskRemindersList", new { TaskId = taskId, Reminders = reminders });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskReminders", "خطا در دریافت یادآوری‌ها", ex);
                return Json(new { status = "error", message = "خطا در دریافت یادآوری‌ها" });
            }
        }

        /// <summary>
        /// نمایش مودال افزودن یادآوری جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddReminderModal(int taskId)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new { status = "error", message = "تسک یافت نشد" });
                }

                var model = new TaskReminderViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    ReminderType = 2,
                    DaysBeforeDeadline = 3,
                    NotificationTime = new TimeSpan(9, 0, 0),
                    IsActive = true
                };

                return PartialView("_AddReminderModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddReminderModal", "خطا در نمایش مودال", ex);
                return Json(new { status = "error", message = "خطا در نمایش فرم" });
            }
        }

        /// <summary>
        /// ذخیره یادآوری جدید
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveReminder(TaskReminderViewModel model)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var reminderId = await _taskRepository.CreateReminderAsync(model, currentUserId);

                if (reminderId == 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری" } }
                    });
                }

                await _taskHistoryRepository.LogReminderAddedAsync(
                    model.TaskId,
                    currentUserId,
                    reminderId,
                    model.Title,
                    model.ReminderType
                );

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "reminders-list-container",
                            view = new
                            {
                                result = await this.RenderViewToStringAsync(
                                    "_TaskRemindersList",
                                    new {
                                        TaskId = model.TaskId,
                                        Reminders = await _taskRepository.GetTaskRemindersListAsync(model.TaskId)
                                    }
                                )
                            }
                        }
                    },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SaveReminder", "خطا در ذخیره یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری" } }
                });
            }
        }

        /// <summary>
        /// نمایش مودال تأیید حذف یادآوری
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteReminderConfirmModal(int reminderId)
        {
            try
            {
                var reminder = await _taskRepository.GetReminderByIdAsync(reminderId);
                if (reminder == null)
                {
                    return Json(new { status = "error", message = "یادآوری یافت نشد" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "DeleteReminderConfirmModal",
                    $"نمایش مودال تأیید حذف یادآوری {reminder.Title}");

                return PartialView("_DeleteReminderConfirmModal", reminderId);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteReminderConfirmModal", "خطا در نمایش مودال", ex);
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }

        /// <summary>
        /// حذف یادآوری تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteTaskReminder(int id)
        {
            try
            {
                var reminder = await _taskRepository.GetReminderByIdAsync(id);
                if (reminder == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "یادآوری یافت نشد" } }
                    });
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var taskId = reminder.TaskId;
                var reminderTitle = reminder.Title;

                var result = await _taskRepository.DeactivateReminderAsync(id);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در حذف یادآوری" } }
                    });
                }

                await _taskHistoryRepository.LogReminderDeletedAsync(taskId, currentUserId, id, reminderTitle);

                var updatedReminders = await _taskRepository.GetTaskRemindersListAsync(taskId);
                var partialHtml = await this.RenderViewToStringAsync(
                    "_TaskRemindersList",
                    new { TaskId = taskId, Reminders = updatedReminders }
                );

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "reminders-list-container",
                            view = new { result = partialHtml }
                        }
                    },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "DeleteTaskReminder", "خطا در حذف یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف یادآوری" } }
                });
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن یادآوری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReminderStatus(int id)
        {
            try
            {
                var reminder = await _taskRepository.GetReminderByIdAsync(id);
                if (reminder == null)
                {
                    return Json(new { status = "error", message = "یادآوری یافت نشد" });
                }

                var result = await _taskRepository.ToggleReminderActiveStatusAsync(id);

                if (!result)
                {
                    return Json(new { status = "error", message = "خطا در تغییر وضعیت" });
                }

                var updatedReminder = await _taskRepository.GetReminderByIdAsync(id);
                var statusText = updatedReminder.IsActive ? "فعال" : "غیرفعال";

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = $"یادآوری {statusText} شد" } },
                    updateTarget = "#reminders-list-container",
                    updateUrl = Url.Action("GetTaskReminders", new { taskId = reminder.TaskId })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ToggleReminderStatus", "خطا در تغییر وضعیت", ex);
                return Json(new { status = "error", message = "خطا در تغییر وضعیت" });
            }
        }

        /// <summary>
        /// ذخیره یادآوری سفارشی (Create Task)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCustomReminder(TaskReminderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToList();

                    return Json(new { status = "validation-error", message = errors });
                }

                switch (model.ReminderType)
                {
                    case 0:
                        if (string.IsNullOrEmpty(model.StartDatePersian))
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "تاریخ یادآوری الزامی است" } }
                            });
                        }
                        break;
                    case 1:
                        if (!model.IntervalDays.HasValue || model.IntervalDays <= 0)
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "فاصله تکرار یادآوری الزامی است" } }
                            });
                        }
                        break;
                    case 2:
                        if (!model.DaysBeforeDeadline.HasValue || model.DaysBeforeDeadline <= 0)
                        {
                            return Json(new
                            {
                                status = "validation-error",
                                message = new[] { new { status = "error", text = "تعداد روز قبل از مهلت الزامی است" } }
                            });
                        }
                        break;
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create, "Tasks", "SaveCustomReminder",
                    $"ایجاد یادآوری سفارشی: {model.Title}");

                ViewBag.ReminderId = DateTime.Now.Ticks;

                var partialViewHtml = await this.RenderViewToStringAsync("_ReminderItem", model, appendMode: true);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "customRemindersList",
                            appendMode = true,
                            view = new { result = partialViewHtml }
                        }
                    },
                    message = new[] { new { status = "success", text = "یادآوری با موفقیت اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SaveCustomReminder", "خطا در ذخیره یادآوری", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ذخیره یادآوری: " + ex.Message } }
                });
            }
        }

        /// <summary>
        /// نمایش مودال یادآوری سفارشی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddCustomReminderModal()
        {
            try
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "AddCustomReminderModal", "نمایش مودال یادآوری سفارشی");
                return PartialView("_AddCustomReminderModal");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AddCustomReminderModal", "خطا در نمایش مودال", ex);
                return BadRequest("خطا در بارگذاری مودال");
            }
        }

        #endregion
    }
}
