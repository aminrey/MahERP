using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Ajax Helpers & Focus Management - متدهای کمکی AJAX و فوکوس تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Focus Management

        /// <summary>
        /// تنظیم فوکوس روی تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetTaskFocus(int taskId, bool fromList = false)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var previousFocusedTaskId = await _taskRepository.GetUserFocusedTaskIdAsync(currentUserId);

            var result = await _taskRepository.SetTaskFocusAsync(taskId, currentUserId);

            if (result.Success)
            {
                if (fromList)
                {
                    var viewList = new List<object>();

                    var newFocusedTaskCard = await _taskRepository.GetTaskCardViewModelAsync(taskId, currentUserId);
                    if (newFocusedTaskCard != null)
                    {
                        newFocusedTaskCard.IsFocused = true;
                        var newPartialView = await this.RenderViewToStringAsync("_TaskRowPartial", newFocusedTaskCard);

                        viewList.Add(new
                        {
                            elementId = $"task-row-{taskId}",
                            view = new { result = newPartialView },
                            appendMode = false
                        });
                    }

                    if (previousFocusedTaskId.HasValue && previousFocusedTaskId.Value != taskId)
                    {
                        var previousTaskCard = await _taskRepository.GetTaskCardViewModelAsync(previousFocusedTaskId.Value, currentUserId);
                        if (previousTaskCard != null)
                        {
                            previousTaskCard.IsFocused = false;
                            var previousPartialView = await this.RenderViewToStringAsync("_TaskRowPartial", previousTaskCard);

                            viewList.Add(new
                            {
                                elementId = $"task-row-{previousFocusedTaskId.Value}",
                                view = new { result = previousPartialView },
                                appendMode = false
                            });
                        }
                    }

                    return Json(new
                    {
                        status = "update-view",
                        viewList = viewList.ToArray(),
                        message = new[] { new { status = "success", text = result.Message } }
                    });
                }
            }

            return Json(new { success = result.Success, message = result.Message });
        }

        /// <summary>
        /// حذف فوکوس از تسک
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveTaskFocus(int taskId, bool fromList = false)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskRepository.RemoveTaskFocusAsync(taskId, currentUserId);

            if (result.Success)
            {
                if (fromList)
                {
                    var taskCard = await _taskRepository.GetTaskCardViewModelAsync(taskId, currentUserId);

                    if (taskCard != null)
                    {
                        taskCard.IsFocused = false;

                        var partialView = await this.RenderViewToStringAsync("_TaskRowPartial", taskCard);

                        return Json(new
                        {
                            status = "update-view",
                            viewList = new[]
                            {
                                new
                                {
                                    elementId = $"task-row-{taskId}",
                                    view = new { result = partialView },
                                    appendMode = false
                                }
                            },
                            message = new[] { new { status = "success", text = result.Message } }
                        });
                    }
                }
            }

            return Json(new { success = result.Success, message = result.Message });
        }

        #endregion

        #region Ajax Helpers

        /// <summary>
        /// دریافت تاریخچه تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskHistory(int taskId)
        {
            try
            {
                var history = await _taskRepository.GetTaskHistoryAsync(taskId);
                return PartialView("_TaskHistoryTimeline", history);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetTaskHistory", "خطا در دریافت تاریخچه", ex);
                return PartialView("_TaskHistoryTimeline", new List<TaskHistoryViewModel>());
            }
        }

        /// <summary>
        /// بروزرسانی لیست‌های Contact و Organization بر اساس شعبه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BranchTriggerSelectForStakeholders(int branchId)
        {
            try
            {
                if (branchId <= 0)
                {
                    return Json(new { success = false, message = "شعبه نامعتبر است" });
                }

                var contacts = await _taskRepository.GetBranchContactsAsync(branchId);
                var organizations = await _taskRepository.GetBranchOrganizationsAsync(branchId);

                var contactsHtml = await this.RenderViewToStringAsync("_ContactsDropdown", contacts);
                var organizationsHtml = await this.RenderViewToStringAsync("_OrganizationsDropdown", organizations);

                return Json(new
                {
                    success = true,
                    status = "update-view",
                    viewList = new[]
                    {
                        new { elementId = "ContactSelectionDiv", view = new { result = contactsHtml } },
                        new { elementId = "OrganizationSelectionDiv", view = new { result = organizationsHtml } }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "BranchTriggerSelectForStakeholders",
                    "خطا در بارگذاری Contact/Organization",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new { success = false, message = $"خطا در بارگذاری: {ex.Message}" });
            }
        }

        /// <summary>
        /// دریافت سازمان‌های مرتبط با Contact انتخاب شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ContactTriggerSelect(int contactId)
        {
            try
            {
                if (contactId <= 0)
                {
                    return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
                }

                var organizations = await _taskRepository.GetContactOrganizationsAsync(contactId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "ContactTriggerSelect",
                    $"دریافت سازمان‌های Contact {contactId}",
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", organizations);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ContactTriggerSelect",
                    "خطا در دریافت سازمان‌های Contact",
                    ex,
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
            }
        }

        /// <summary>
        /// بارگذاری افراد مرتبط با Organization انتخاب شده
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OrganizationTriggerSelect(int organizationId)
        {
            try
            {
                var contacts = await _taskRepository.GetOrganizationContactsAsync(organizationId);

                var model = new
                {
                    Contacts = contacts,
                    OrganizationId = organizationId
                };

                return PartialView("_OrganizationContactsPartial", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "OrganizationTriggerSelect",
                    "خطا در بارگذاری افراد سازمان",
                    ex
                );

                return PartialView("_OrganizationContactsPartial", new
                {
                    Contacts = new List<ContactViewModel>(),
                    OrganizationId = organizationId
                });
            }
        }

        /// <summary>
        /// دریافت تیم‌های یک کاربر در شعبه مشخص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserTeams(string userId, int branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || branchId <= 0)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "اطلاعات ورودی نامعتبر است"
                    });
                }

                var userTeams = await _taskRepository.GetUserTeamsByBranchAsync(userId, branchId);

                var partialHtml = await this.RenderViewToStringAsync("_UserTeamsSelect", userTeams);

                if (string.IsNullOrWhiteSpace(partialHtml))
                {
                    throw new Exception("Partial view rendering failed");
                }

                var viewList = new List<object>
                {
                    new {
                        elementId = "team-select-container",
                        view = new { result = partialHtml }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View, "Tasks", "GetUserTeams",
                    $"بارگذاری تیم‌های کاربر {userId} در شعبه {branchId} - تعداد: {userTeams.Count}");

                return Json(new
                {
                    status = "update-view",
                    viewList = viewList,
                    teamsCount = userTeams.Count,
                    hasNoTeam = !userTeams.Any() || userTeams.All(t => t.Id == 0)
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "GetUserTeams", "خطا در دریافت تیم‌های کاربر", ex);

                var errorHtml = @"<select class='form-select form-select-sm team-select' disabled>
                                    <option value='0'>بدون تیم (خطا در بارگذاری)</option>
                                  </select>
                                  <small class='form-text text-danger mt-1'>
                                    <i class='fa fa-times-circle me-1'></i>
                                    خطا در بارگذاری تیم‌ها
                                  </small>";

                return Json(new
                {
                    status = "update-view",
                    viewList = new List<object>
                    {
                        new {
                            elementId = "team-select-container",
                            view = new { result = errorHtml }
                        }
                    },
                    message = $"خطا: {ex.Message}",
                    hasNoTeam = true
                });
            }
        }

        #endregion
    }
}
