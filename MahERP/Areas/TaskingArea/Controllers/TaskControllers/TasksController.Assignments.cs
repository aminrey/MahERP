using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// Assignments Management - مدیریت تخصیص کاربران به تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Assignments Management

        /// <summary>
        /// نمایش مودال افزودن کاربر به تسک
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AssignUserToTaskModal(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = await _taskRepository.GetTaskByIdAsync(taskId);

                if (task == null)
                    return NotFound();

                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Forbid();

                var model = new AssignUserToTaskViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    BranchId = task.BranchId ?? 0
                };

                if (model.BranchId > 0)
                {
                    var branchData = await _taskRepository.GetBranchTriggeredDataAsync(model.BranchId);
                    model.AvailableUsers = branchData.Users;
                    model.AvailableTeams = branchData.Teams;
                }

                return PartialView("_AssignUserToTaskModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AssignUserToTaskModal", "خطا در نمایش مودال", ex);
                return StatusCode(500, "خطا در بارگذاری مودال");
            }
        }

        /// <summary>
        /// ثبت تخصیص کاربر جدید به تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUserToTask(AssignUserToTaskViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);

                if (task == null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تسک یافت نشد" } }
                    });

                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما دسترسی لازم را ندارید" } }
                    });

                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(userId, model.TaskId);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما این تسک را تکمیل کرده‌اید و نمی‌توانید کاربر جدید اضافه کنید" } }
                    });

                var existingAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(
                    model.SelectedUserId,
                    model.TaskId);

                if (existingAssignment != null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "این کاربر قبلاً به تسک اختصاص داده شده است" } }
                    });

                var result = await _taskRepository.AssignUserToTaskAsync(
                    model.TaskId,
                    model.SelectedUserId,
                    userId,
                    model.SelectedTeamId,
                    model.Description);

                if (result)
                {
                    var assignedUser = await _userManager.FindByIdAsync(model.SelectedUserId);
                    var assignedUserName = assignedUser != null ? $"{assignedUser.FirstName} {assignedUser.LastName}" : "نامشخص";

                    await _taskHistoryRepository.LogUserAssignedAsync(model.TaskId, userId, assignedUserName);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Tasks",
                        "AssignUserToTask",
                        $"تخصیص کاربر {assignedUserName} به تسک {task.Title}",
                        recordId: model.TaskId.ToString());

                    var updatedTask = _taskRepository.GetTaskById(model.TaskId, includeAssignments: true);

                    var assignments = updatedTask.TaskAssignments
                        .Select(a => new TaskAssignmentViewModel
                        {
                            Id = a.Id,
                            TaskId = a.TaskId,
                            AssignedUserId = a.AssignedUserId,
                            AssignedUserName = a.AssignedUser != null
                                ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                                : "نامشخص",
                            AssignDate = a.AssignmentDate,
                            CompletionDate = a.CompletionDate,
                            Description = a.Description
                        })
                        .ToList();

                    var partialHtml = await this.RenderViewToStringAsync(
                        "_TaskMembersList",
                        new
                        {
                            Assignments = assignments,
                            TaskId = task.Id,
                            IsCreator = isCreator,
                            IsManager = isAdmin,
                            IsTaskCompleted = isTaskCompletedForCurrentUser
                        });

                    return Json(new
                    {
                        status = "update-view",
                        viewList = new[]
                        {
                            new
                            {
                                elementId = "task-members-container",
                                view = new { result = partialHtml }
                            }
                        },
                        message = new[] { new { status = "success", text = "کاربر با موفقیت به تسک اختصاص داده شد" } }
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تخصیص کاربر" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "AssignUserToTask", "خطا در تخصیص کاربر", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }
        /// <summary>
        /// نمایش مودال تأیید حذف Assignment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveAssignmentModal(int assignmentId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var assignment = await _taskRepository.GetTaskAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                    return NotFound();

                var task = assignment.Task;

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Forbid();

                var model = new RemoveAssignmentViewModel
                {
                    AssignmentId = assignmentId,
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    UserName = assignment.AssignedUser != null
                        ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}"
                        : "نامشخص"
                };

                return PartialView("_RemoveAssignmentModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveAssignmentModal", "خطا در نمایش مودال", ex);
                return StatusCode(500, "خطا در بارگذاری مودال");
            }
        }
        /// <summary>
        /// حذف Assignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int assignmentId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var assignment = await _taskRepository.GetTaskAssignmentByIdAsync(assignmentId);

                if (assignment == null)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "تخصیص یافت نشد" } }
                    });

                var task = assignment.Task;

                // بررسی دسترسی
                var isCreator = task.CreatorUserId == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isAdmin)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما دسترسی لازم را ندارید" } }
                    });

                // ⭐ بررسی اینکه آیا تسک برای کاربر جاری تکمیل شده؟
                var currentUserAssignment = await _taskRepository.GetTaskAssignmentByUserAndTaskAsync(userId, task.Id);
                var isTaskCompletedForCurrentUser = currentUserAssignment?.CompletionDate.HasValue ?? false;

                if (isTaskCompletedForCurrentUser)
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما این تسک را تکمیل کرده‌اید و نمی‌توانید کاربر حذف کنید" } }
                    });

                // حذف Assignment
                var removedUserName = assignment.AssignedUser != null
                    ? $"{assignment.AssignedUser.FirstName} {assignment.AssignedUser.LastName}"
                    : "نامشخص";

                var result = await _taskRepository.RemoveTaskAssignmentAsync(assignmentId);

                if (result)
                {
                    // ثبت در تاریخچه
                    await _taskHistoryRepository.LogUserRemovedAsync(
                        task.Id,
                        userId,
                        removedUserName);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Tasks",
                        "RemoveAssignment",
                        $"حذف {removedUserName} از تسک {task.Title}",
                        recordId: task.Id.ToString());

                    // ⭐⭐⭐ دریافت لیست به‌روزرسانی شده اعضا
                    var updatedTask = _taskRepository.GetTaskById(
                        task.Id,
                        includeAssignments: true);

                    var assignments = updatedTask.TaskAssignments
                        .Select(a => new TaskAssignmentViewModel
                        {
                            Id = a.Id,
                            TaskId = a.TaskId,
                            AssignedUserId = a.AssignedUserId,
                            AssignedUserName = a.AssignedUser != null
                                ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                                : "نامشخص",
                            AssignDate = a.AssignmentDate,
                            CompletionDate = a.CompletionDate, // ⭐⭐⭐ از Assignment
                            Description = a.Description
                        })
                        .ToList();

                    // ⭐⭐⭐ رندر Partial View
                    var partialHtml = await this.RenderViewToStringAsync(
                        "_TaskMembersList",
                        new
                        {
                            Assignments = assignments,
                            TaskId = task.Id,
                            IsCreator = isCreator,
                            IsManager = isAdmin,
                            IsTaskCompleted = isTaskCompletedForCurrentUser // ⭐⭐⭐ برای کاربر جاری
                        });

                    // ⭐⭐⭐ برگرداندن JSON با ساختار update-view
                    return Json(new
                    {
                        status = "update-view",
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "task-members-container",
                        view = new { result = partialHtml }
                    }
                },
                        message = new[] { new { status = "success", text = $"{removedUserName} با موفقیت از تسک حذف شد" } }
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف کاربر" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Tasks", "RemoveAssignment", "خطا در حذف کاربر", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }
        /// <summary>
        /// دریافت تیم‌های کاربر برای Assignment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetUserTeamsForAssignment(string userId, int branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || branchId <= 0)
                {
                    return Json(new { status = "error", message = "پارامترهای نامعتبر" });
                }

                var userTeams = await _taskRepository.GetUserTeamsByBranchAsync(userId, branchId);

                var html = "";

                if (!userTeams.Any())
                {
                    html = @"<select class='form-select team-select' name='SelectedTeamId' required disabled>
                        <option value='0'>بدون تیم</option>
                     </select>
                     <small class='form-text text-warning mt-1'>
                        <i class='fa fa-exclamation-triangle me-1'></i>
                        این کاربر در هیچ تیمی عضو نیست
                     </small>";
                }
                else if (userTeams.Count == 1)
                {
                    var team = userTeams.First();
                    var managerInfo = !string.IsNullOrEmpty(team.ManagerName) ? $" (مدیر: {team.ManagerName})" : "";

                    html = $@"<select class='form-select team-select' name='SelectedTeamId' required>
                        <option value='{team.Id}' 
                                data-manager-id='{team.ManagerUserId}' 
                                data-manager-name='{team.ManagerName}' 
                                data-member-count='{team.MemberCount}' 
                                selected>
                            {team.Title}{managerInfo}
                        </option>
                      </select>
                      <small class='form-text text-success mt-1'>
                        <i class='fa fa-check me-1'></i>
                        تیم به صورت خودکار انتخاب شد
                      </small>";
                }
                else
                {
                    html = "<select class='form-select team-select' name='SelectedTeamId' required>";
                    html += "<option value=''>انتخاب تیم...</option>";

                    foreach (var team in userTeams)
                    {
                        var managerInfo = !string.IsNullOrEmpty(team.ManagerName) ? $" (مدیر: {team.ManagerName})" : "";

                        html += $@"<option value='{team.Id}' 
                                      data-manager-id='{team.ManagerUserId ?? ""}' 
                                      data-manager-name='{team.ManagerName ?? ""}' 
                                      data-member-count='{team.MemberCount}'>
                                  {team.Title}{managerInfo}
                              </option>";
                    }

                    html += "</select>";
                    html += @"<small class='form-text text-muted mt-1'>
                        <i class='fa fa-info-circle me-1'></i>
                        لطفاً تیم مربوطه را انتخاب کنید
                      </small>";
                }

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "TeamSelectDiv",
                            view = new { result = html }
                        }
                    },
                    hasNoTeam = !userTeams.Any(),
                    teamCount = userTeams.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserTeamsForAssignment: {ex.Message}");
                return Json(new { status = "error", message = "خطا در دریافت تیم‌ها" });
            }
        }

        #endregion
    }
}
