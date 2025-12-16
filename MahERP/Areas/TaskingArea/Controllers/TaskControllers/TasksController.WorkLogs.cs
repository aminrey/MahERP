using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// WorkLogs Management - مدیریت گزارش کارهای انجام شده روی تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Work Log Management

        /// <summary>
        /// مودال ثبت کار انجام شده روی تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SubmitTaskWorkLogModal(int taskId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var model = await _taskRepository.PrepareLogTaskWorkModalAsync(taskId, currentUserId);

                if (model == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "شما عضو این تسک نیستید یا تسک یافت نشد"
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "LogTaskWorkModal",
                    $"نمایش مودال ثبت کار برای تسک {model.TaskCode}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks",
                    recordTitle: model.TaskTitle);

                return PartialView("_SubmitTaskWorkLogModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "SubmitTaskWorkLogModal", "خطا در نمایش مودال ثبت کار", ex);
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری مودال"
                });
            }
        }

        /// <summary>
        /// ثبت کار انجام شده روی تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogTaskWork(TaskWorkLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    status = "validation-error",
                    message = new[]
                    {
                        new
                        {
                            status = "error",
                            text = "اطلاعات وارد شده معتبر نیست"
                        }
                    }
                });
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskRepository.AddTaskWorkLogAsync(
                model.TaskId,
                currentUserId,
                model.WorkDescription,
                model.DurationMinutes,
                model.ProgressPercentage
            );

            if (result.Success)
            {
                NotificationProcessingBackgroundService.EnqueueTaskNotification(
                    model.TaskId,
                    currentUserId,
                    NotificationEventType.TaskWorkLog,
                    priority: 1
                );

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Tasks",
                    "LogTaskWork",
                    $"ثبت گزارش کار برای تسک {model.TaskId}",
                    recordId: model.TaskId.ToString(),
                    entityType: "Tasks");

                // ⭐⭐⭐ دریافت لیست به‌روز Work Logs
                var workLogs = await _taskRepository.GetTaskWorkLogsAsync(model.TaskId);
                
                // ⭐⭐⭐ بررسی اینکه آیا کاربر هنوز عضو تسک است
                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);
                var isStillAssigned = task?.TaskAssignments?.Any(a => a.AssignedUserId == currentUserId) ?? false;

                // ⭐⭐⭐ رندر Partial View
                var partialHtml = await this.RenderViewToStringAsync(
                    "_WorkLogsList",
                    workLogs,
                    viewBag: new
                    {
                        TaskId = model.TaskId,
                        IsAssignedToCurrentUser = isStillAssigned
                    });

                // ⭐⭐⭐ بروزرسانی badge count در tab
                var workLogsCount = workLogs?.Count ?? 0;

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "work-logs-container",
                            view = new { result = partialHtml }
                        }
                    },
                    updateBadge = new
                    {
                        tabId = "work-logs-tab",
                        count = workLogsCount
                    },
                    message = new[]
                    {
                        new
                        {
                            status = "success",
                            text = result.Message
                        }
                    }
                });
            }

            return Json(new
            {
                success = false,
                status = "error",
                message = new[]
                {
                    new
                    {
                        status = "error",
                        text = result.Message
                    }
                }
            });
        }

        /// <summary>
        /// مودال نمایش لیست گزارش کارهای یک تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewTaskWorkLogsModal(int taskId)
        {
            try
            {
                var workLogs = await _taskRepository.GetTaskWorkLogsAsync(taskId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "ViewTaskWorkLogsModal",
                    $"نمایش لیست گزارش کارهای تسک {taskId}",
                    recordId: taskId.ToString(),
                    entityType: "Tasks");

                return PartialView("_TaskWorkLogsModal", workLogs);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ViewTaskWorkLogsModal", "خطا در نمایش لیست گزارش کارها", ex);
                return PartialView("_TaskWorkLogsModal", new List<TaskWorkLogViewModel>());
            }
        }

        #endregion
    }
}
