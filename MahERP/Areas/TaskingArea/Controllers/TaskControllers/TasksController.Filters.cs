using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Filters & Search - فیلترهای پیشرفته و جستجو
    /// </summary>
    public partial class TasksController
    {
        #region Advanced Filters

        /// <summary>
        /// اعمال فیلترهای پیشرفته - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ApplyAdvancedFilters(TaskFilterViewModel filters)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // ⭐⭐⭐ حفظ viewType و grouping از فیلتر
                var viewType = filters.ViewType ?? TaskViewType.MyTasks;
                var grouping = filters.Grouping ?? TaskGroupingType.Team;

                // ⭐ اعمال فیلترهای پیشرفته
                var model = await _taskRepository.GetTaskListAsync(
                    userId,
                    viewType,
                    grouping,
                    filters);

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

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "ApplyAdvancedFilters",
                    $"اعمال فیلترهای پیشرفته - نتایج: {model.Tasks.Count}");

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "task-groups-container",
                            view = new { result = html }
                        }
                    },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent,
                        total = model.Tasks.Count
                    },
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    message = new[] { new { status = "success", text = $"{model.Tasks.Count} تسک یافت شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ApplyAdvancedFilters", "خطا در اعمال فیلترها", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در اعمال فیلترها" } }
                });
            }
        }

        /// <summary>
        /// دریافت داده‌های اولیه برای فیلترهای پیشرفته - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetAdvancedFilterData()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // دریافت شعبه‌های کاربر
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                var branchIds = userBranches.Select(b => b.Id).ToList();

                // ⭐ دریافت کاربران شعبه‌ها
                var branchUsers = new List<UserViewModelFull>();
                foreach (var branchId in branchIds)
                {
                    var users = _branchRepository.GetBranchUsersByBranchId(branchId, includeInactive: false);
                    foreach (var branchUser in users)
                    {
                        if (!branchUsers.Any(u => u.Id == branchUser.UserId))
                        {
                            branchUsers.Add(new UserViewModelFull
                            {
                                Id = branchUser.UserId,
                                FullNamesString = branchUser.UserFullName
                            });
                        }
                    }
                }

                var uniqueUsers = branchUsers.OrderBy(u => u.FullNamesString).ToList();

                // ⭐⭐⭐ دریافت سازمان‌ها
                var organizations = new List<object>();
                var allOrganizations = await _organizationRepository.GetOrganizationsAsViewModelAsync(includeInactive: false);

                foreach (var org in allOrganizations)
                {
                    organizations.Add(new
                    {
                        Id = org.Id,
                        Name = org.DisplayName ?? org.Name,
                        Type = org.OrganizationType == 0 ? "شخص حقیقی" : "شخص حقوقی",
                        MemberCount = org.TotalMembers
                    });
                }

                var uniqueOrganizations = organizations.OrderBy(o => ((dynamic)o).Name).ToList();

                // ⭐⭐⭐ دریافت تیم‌های عضو کاربر
                var userTeams = new List<object>();
                foreach (var branchId in branchIds)
                {
                    var branchTeams = _teamRepository.GetTeamsByBranchId(branchId, includeInactive: false);

                    foreach (var team in branchTeams)
                    {
                        var isMember = _teamRepository.GetTeamMembers(team.Id, includeInactive: false)
                            .Any(tm => tm.UserId == userId);

                        if (isMember && !userTeams.Any(t => ((dynamic)t).Id == team.Id))
                        {
                            userTeams.Add(new
                            {
                                Id = team.Id,
                                Title = team.Title,
                                BranchId = team.BranchId,
                                ManagerName = team.ManagerFullName,
                                MemberCount = _teamRepository.GetTeamMembers(team.Id, includeInactive: false).Count
                            });
                        }
                    }
                }

                var uniqueTeams = userTeams.OrderBy(t => ((dynamic)t).Title).ToList();

                var categories = _taskRepository.GetAllCategories(activeOnly: true);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetAdvancedFilterData",
                    $"دریافت داده‌های فیلتر پیشرفته - {uniqueUsers.Count} کاربر، {uniqueOrganizations.Count} سازمان، {uniqueTeams.Count} تیم");

                return Json(new
                {
                    status = "success",
                    users = uniqueUsers.Select(u => new { Id = u.Id, FullName = u.FullNamesString }).ToList(),
                    organizations = uniqueOrganizations,
                    teams = uniqueTeams,
                    categories = categories.Select(c => new { c.Id, c.Title }).ToList()
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetAdvancedFilterData", "خطا در دریافت داده‌های فیلتر", ex);
                return Json(new { status = "error", message = "خطا در دریافت داده‌ها: " + ex.Message });
            }
        }

        /// <summary>
        /// پاک کردن فیلترهای پیشرفته
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearAdvancedFilters(
            TaskViewType viewType = TaskViewType.MyTasks,
            TaskGroupingType grouping = TaskGroupingType.Team,
            QuickStatusFilter statusFilter = QuickStatusFilter.Pending)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var filters = new TaskFilterViewModel
                {
                    ViewType = viewType,
                    Grouping = grouping,
                    TaskStatus = statusFilter switch
                    {
                        QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                        QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                        QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                        QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                        _ => TaskStatusFilter.All
                    }
                };

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

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

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "task-groups-container",
                            view = new { result = html }
                        }
                    },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    currentStatusFilter = (int)statusFilter,
                    message = new[] { new { status = "success", text = "فیلترها پاک شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ClearAdvancedFilters", "خطا در پاک کردن فیلترها", ex);
                return Json(new { status = "error", message = "خطا در پاک کردن فیلترها" });
            }
        }

        #endregion

        #region Quick Filters & Grouping

        /// <summary>
        /// تغییر گروه‌بندی - AJAX
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeGrouping(
            TaskViewType viewType,
            TaskGroupingType grouping,
            TaskFilterViewModel currentFilters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var filters = currentFilters ?? new TaskFilterViewModel();
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "task-groups-container",
                            view = new { result = html }
                        }
                    },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "خطا در تغییر گروه‌بندی" });
            }
        }

        /// <summary>
        /// تغییر فیلتر وضعیت سریع
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeQuickStatusFilter(
            TaskViewType viewType,
            TaskGroupingType grouping,
            int statusFilter,
            TaskFilterViewModel currentFilters = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var statusFilterEnum = (QuickStatusFilter)statusFilter;

                var filters = currentFilters ?? new TaskFilterViewModel();
                filters.ViewType = viewType;
                filters.Grouping = grouping;

                filters.TaskStatus = statusFilterEnum switch
                {
                    QuickStatusFilter.Pending => TaskStatusFilter.InProgress,
                    QuickStatusFilter.Completed => TaskStatusFilter.Completed,
                    QuickStatusFilter.Overdue => TaskStatusFilter.Overdue,
                    QuickStatusFilter.Urgent => TaskStatusFilter.InProgress,
                    _ => TaskStatusFilter.All
                };

                var model = await _taskRepository.GetTaskListAsync(userId, viewType, grouping, filters);

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

                var html = await this.RenderViewToStringAsync("_TaskListGroupsPartial", model);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "ChangeQuickStatusFilter",
                    $"تغییر فیلتر وضعیت به: {statusFilterEnum}");

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "task-groups-container",
                            view = new { result = html }
                        }
                    },
                    stats = new
                    {
                        pending = model.Stats.TotalPending,
                        completed = model.Stats.TotalCompleted,
                        overdue = model.Stats.TotalOverdue,
                        urgent = model.Stats.TotalUrgent
                    },
                    currentViewType = (int)viewType,
                    currentGrouping = (int)grouping,
                    currentStatusFilter = statusFilter
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "ChangeQuickStatusFilter", "خطا در تغییر فیلتر", ex);
                return Json(new { status = "error", message = "خطا در تغییر فیلتر" });
            }
        }

        #endregion
    }
}
