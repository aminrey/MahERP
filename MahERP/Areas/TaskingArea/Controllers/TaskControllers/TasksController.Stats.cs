using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Stats Management - مدیریت آمار و رفرش تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Stats

        /// <summary>
        /// دریافت آمار Hero Section
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskHeroStats(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true);

            if (task == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                progressPercentage = task.TaskOperations.Any()
                    ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                    : 0,
                completedOperations = task.TaskOperations.Count(o => o.IsCompleted),
                totalOperations = task.TaskOperations.Count
            });
        }

        /// <summary>
        /// دریافت درصد پیشرفت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskProgress(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true);

            if (task == null)
                return Json(new { success = false });

            var percentage = task.TaskOperations.Any()
                ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                : 0;

            return Json(new { success = true, percentage });
        }

        /// <summary>
        /// دریافت آمار Sidebar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskSidebarStats(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId, includeOperations: true, includeAssignments: true);

            if (task == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                completedOps = task.TaskOperations.Count(o => o.IsCompleted),
                pendingOps = task.TaskOperations.Count(o => !o.IsCompleted),
                teamMembers = task.TaskAssignments.Count,
                progress = task.TaskOperations.Any()
                    ? (task.TaskOperations.Count(o => o.IsCompleted) * 100 / task.TaskOperations.Count)
                    : 0
            });
        }

        /// <summary>
        /// دریافت آمار بروز شده Hero Section
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskHeroStatsPartial(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                var html = await this.RenderViewToStringAsync("_TaskHeroStats", viewModel);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskHeroStatsPartial", "خطا در دریافت آمار", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// دریافت آمار بروز شده Sidebar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskSidebarStatsPartial(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                var html = await this.RenderViewToStringAsync("_TaskSidebarStats", viewModel);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskSidebarStatsPartial", "خطا در دریافت آمار Sidebar", ex);
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// بروزرسانی تمام آمارها با یک درخواست
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RefreshAllTaskStats(int taskId)
        {
            try
            {
                var task = _taskRepository.GetTaskById(
                    taskId,
                    includeOperations: true,
                    includeAssignments: true);

                if (task == null)
                    return Json(new { success = false, message = "تسک یافت نشد" });

                var viewModel = _mapper.Map<TaskViewModel>(task);

                var heroHtml = await this.RenderViewToStringAsync("_TaskHeroStats", viewModel);
                var sidebarHtml = await this.RenderViewToStringAsync("_TaskSidebarStats", viewModel);

                return Json(new
                {
                    success = true,
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "hero-stats-container",
                            view = new { result = heroHtml }
                        },
                        new
                        {
                            elementId = "sidebar-stats-container",
                            view = new { result = sidebarHtml }
                        }
                    },
                    totalOperations = viewModel.Operations?.Count ?? 0,
                    completedOperations = viewModel.Operations?.Count(o => o.IsCompleted) ?? 0
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RefreshAllTaskStats", "خطا در بروزرسانی آمار", ex);
                return Json(new { success = false, message = "خطا در بروزرسانی آمار" });
            }
        }

        #endregion
    }
}
