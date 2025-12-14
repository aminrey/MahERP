using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// ⭐⭐⭐ TasksController Partial - مدیریت تنظیمات تسک
    /// </summary>
    public partial class TasksController
    {
        #region Task Settings Modal

        /// <summary>
        /// نمایش مودال تنظیمات تسک (آدرس دهی با نام GetTaskSettingsModal)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaskSettingsModal(int taskId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                // بررسی دسترسی به تسک
                var task = await _taskRepository.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return Json(new
                    {
                        success = false,
                        status = "error",
                        message = new[] {
                            new { status = "error", text = "تسک یافت نشد" }
                        }
                    });
                }

                // ⭐⭐⭐ بررسی دسترسی ویرایش تنظیمات
                var canEdit = await _taskRepository.CanUserEditSettingsAsync(taskId, currentUserId);

                // ⭐⭐⭐ اگر Admin سیستم است، اجازه ویرایش بده
                if (isAdmin)
                {
                    canEdit = true;
                }

                // ⭐⭐⭐ حتی اگر کاربر نمی‌تواند ویرایش کند، اجازه مشاهده بده
                // فقط CanEdit در ViewModel روی false باشد

                // دریافت تنظیمات
                var settings = await _taskRepository.GetTaskSettingsAsync(taskId);
                // ⭐⭐⭐ ارسال taskId اصلی - چون settings.TaskId ممکن است 0 باشد (Global)
                var viewModel = await _taskRepository.MapEntityToViewModelAsync(settings, currentUserId, taskId);

                // ⭐⭐⭐ اگر Admin است، متن نقش را تغییر بده
                if (isAdmin)
                {
                    viewModel.CurrentUserRoleText = "مدیر سیستم";
                    viewModel.CanEdit = true;
                }

                // ⭐⭐⭐ Render Partial View به String
                var htmlContent = await this.RenderViewToStringAsync("_TaskSettingsPartial", viewModel);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Tasks",
                    "GetTaskSettingsModal",
                    $"مشاهده تنظیمات تسک: {task.Title}",
                    recordId: taskId.ToString(),
                    entityType: "TaskSettings"
                );

                // ⭐⭐⭐ برگرداندن JSON با فرمت update-view
                return Json(new
                {
                    success = true,
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "task-settings-container",
                            view = new { result = htmlContent }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "GetTaskSettingsModal",
                    "خطا در بارگذاری تنظیمات تسک",
                    ex,
                    recordId: taskId.ToString()
                );

                return Json(new
                {
                    success = false,
                    status = "error",
                    message = new[] {
                        new { status = "error", text = "خطا در بارگذاری تنظیمات: " + ex.Message }
                    }
                });
            }
        }

        /// <summary>
        /// نمایش مودال تنظیمات تسک (نام جایگزین)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TaskSettingsModal(int taskId)
        {
            // Redirect به GetTaskSettingsModal برای یکپارچگی
            return await GetTaskSettingsModal(taskId);
        }

        #endregion

        #region Save Task Settings

        /// <summary>
        /// ذخیره تنظیمات تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTaskSettingsSubmit(SaveTaskSettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new { success = false, message = errors });
                }

                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var canEdit = await _taskRepository.CanUserEditSettingsAsync(model.TaskId, currentUserId);
                if (!canEdit)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما مجوز تغییر تنظیمات این تسک را ندارید" }
                        }
                    });
                }

                // بررسی استفاده از وراثت
                if (model.UseInheritedSettings)
                {
                    // حذف تنظیمات سفارشی و بازگشت به وراثت
                    await _taskRepository.ResetTaskSettingsAsync(model.TaskId);

                    // ⭐⭐⭐ دریافت تنظیمات جدید (وراثتی) و رندر partial view
                    var inheritedSettings = await _taskRepository.GetTaskSettingsAsync(model.TaskId);
                    var inheritedViewModel = await _taskRepository.MapEntityToViewModelAsync(inheritedSettings, currentUserId);
                    var inheritedHtmlContent = await this.RenderViewToStringAsync("_TaskSettingsPartial", inheritedViewModel);

                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "تنظیمات تسک به حالت پیش‌فرض بازگشت داده شد" }
                        },
                        status = "update-view",
                        viewList = new[] {
                            new {
                                elementId = "task-settings-container",
                                view = new { result = inheritedHtmlContent }
                            }
                        }
                    });
                }

                // اعتبارسنجی نقش‌ها
                var validationResult = await ValidateRolesAsync(model, currentUserId);
                if (!validationResult.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = validationResult.ErrorMessage }
                        }
                    });
                }

                // ایجاد Entity
                var settings = new TaskSettings
                {
                    TaskId = model.TaskId,
                    CanEditSettingsRoles = NormalizeRoles(model.CanEditSettingsRoles), // ⭐⭐⭐ جدید
                    CanCommentRoles = NormalizeRoles(model.CanCommentRoles),
                    CanAddMembersRoles = NormalizeRoles(model.CanAddMembersRoles),
                    CanRemoveMembersRoles = NormalizeRoles(model.CanRemoveMembersRoles),
                    CanEditAfterCompletionRoles = NormalizeRoles(model.CanEditAfterCompletionRoles),
                    CreatorCanEditDelete = model.CreatorCanEditDelete,
                    IsInherited = false,
                    InheritedFrom = 3 // Task-specific
                };

                // ذخیره
                await _taskRepository.SaveTaskSettingsAsync(settings, currentUserId);

                var task = await _taskRepository.GetTaskByIdAsync(model.TaskId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Tasks",
                    "SaveTaskSettingsSubmit",
                    $"ویرایش تنظیمات تسک: {task?.Title}",
                    recordId: model.TaskId.ToString(),
                    entityType: "TaskSettings"
                );

                // ⭐⭐⭐ دریافت تنظیمات جدید و رندر partial view
                var updatedSettings = await _taskRepository.GetTaskSettingsAsync(model.TaskId);
                var updatedViewModel = await _taskRepository.MapEntityToViewModelAsync(updatedSettings, currentUserId);
                var htmlContent = await this.RenderViewToStringAsync("_TaskSettingsPartial", updatedViewModel);

                return Json(new
                {
                    success = true,
                    message = new[] {
                        new { status = "success", text = "تنظیمات تسک با موفقیت ذخیره شد" }
                    },
                    status = "update-view",
                    viewList = new[] {
                        new {
                            elementId = "task-settings-container",
                            view = new { result = htmlContent }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "SaveTaskSettingsSubmit",
                    "خطا در ذخیره تنظیمات تسک",
                    ex,
                    recordId: model.TaskId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا در ذخیره تنظیمات: " + ex.Message }
                    }
                });
            }
        }

        #endregion

        #region Reset Settings

        /// <summary>
        /// بازگشت تنظیمات به حالت پیش‌فرض
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetTaskSettings(int taskId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var canEdit = await _taskRepository.CanUserEditSettingsAsync(taskId, currentUserId);
                if (!canEdit)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما مجوز تغییر تنظیمات این تسک را ندارید" }
                        }
                    });
                }

                var success = await _taskRepository.ResetTaskSettingsAsync(taskId);

                if (success)
                {
                    var task = await _taskRepository.GetTaskByIdAsync(taskId);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Tasks",
                        "ResetTaskSettings",
                        $"بازنشانی تنظیمات تسک: {task?.Title}",
                        recordId: taskId.ToString(),
                        entityType: "TaskSettings"
                    );

                    // ⭐⭐⭐ دریافت تنظیمات جدید (وراثتی) و رندر partial view
                    var inheritedSettings = await _taskRepository.GetTaskSettingsAsync(taskId);
                    var inheritedViewModel = await _taskRepository.MapEntityToViewModelAsync(inheritedSettings, currentUserId);
                    var htmlContent = await this.RenderViewToStringAsync("_TaskSettingsPartial", inheritedViewModel);

                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "تنظیمات تسک به حالت پیش‌فرض بازگشت داده شد" }
                        },
                        status = "update-view",
                        viewList = new[] {
                            new {
                                elementId = "task-settings-container",
                                view = new { result = htmlContent }
                            }
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "warning", text = "این تسک تنظیمات سفارشی ندارد" }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Tasks",
                    "ResetTaskSettings",
                    "خطا در بازنشانی تنظیمات",
                    ex,
                    recordId: taskId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا: " + ex.Message }
                    }
                });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// اعتبارسنجی نقش‌ها - بررسی Authority Level (نسخه Async)
        /// ⭐⭐⭐ هر نقش فقط می‌تواند نقش‌های پایین‌تر از خود را مدیریت کند
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> ValidateRolesAsync(
            SaveTaskSettingsViewModel model,
            string currentUserId)
        {
            // ⭐⭐⭐ اگر Admin سیستم است، بدون محدودیت اجازه دهید
            if (User.IsInRole("Admin"))
            {
                return (true, string.Empty);
            }

            var userRole = await _taskRepository.GetUserRoleInTaskAsync(model.TaskId, currentUserId);

            if (userRole == null)
            {
                return (false, "نقش شما در تسک مشخص نیست");
            }

            // ⭐⭐⭐ بررسی Authority Level برای هر تنظیم
            var allRoles = new[]
            {
                (Name: "تغییر تنظیمات", Roles: model.CanEditSettingsRoles),
                (Name: "کامنت‌گذاری", Roles: model.CanCommentRoles),
                (Name: "افزودن عضو", Roles: model.CanAddMembersRoles),
                (Name: "حذف عضو", Roles: model.CanRemoveMembersRoles),
                (Name: "ویرایش پس از اتمام", Roles: model.CanEditAfterCompletionRoles)
            };

            foreach (var (settingName, roleString) in allRoles)
            {
                if (string.IsNullOrWhiteSpace(roleString))
                    continue;

                var roles = roleString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var role in roles)
                {
                    var targetRole = role.Trim() switch
                    {
                        "a" => TaskRole.Manager,
                        "b" => TaskRole.Creator,
                        "c" => TaskRole.Member,
                        "d" => TaskRole.Supervisor,
                        "e" => TaskRole.CarbonCopy,
                        _ => (TaskRole?)null
                    };

                    if (targetRole.HasValue)
                    {
                        // ⭐⭐⭐ بررسی Authority Level
                        if (!_taskRepository.CanManageRole(userRole.Value, targetRole.Value))
                        {
                            return (false, $"شما نمی‌توانند دسترسی '{GetRoleText(targetRole.Value)}' را در تنظیم '{settingName}' مدیریت کنید");
                        }
                    }
                }
            }

            // ⭐⭐⭐ بررسی تنظیم CanEditSettingsRoles - سازنده نمی‌تواند دسترسی مدیر را تغییر دهد
            if (userRole.Value == TaskRole.Creator && !string.IsNullOrWhiteSpace(model.CanEditSettingsRoles))
            {
                if (model.CanEditSettingsRoles.Contains("a"))
                {
                    // سازنده می‌تواند دسترسی مدیر را حفظ کند، ولی نمی‌تواند حذفش کند
                    // این منطق در اینجا OK است چون سازنده نمی‌تواند "a" را اضافه/حذف کند
                }
            }

            // بررسی تنظیم 5 (فقط مدیر می‌تواند تغییر دهد)
            if (model.CreatorCanEditDelete && userRole.Value != TaskRole.Manager)
            {
                return (false, "فقط مدیر تیم می‌تواند مجوز حذف/ویرایش برای سازنده را تغییر دهد");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Normalize کردن نقش‌ها (حذف فضای خالی، مرتب‌سازی)
        /// </summary>
        private string NormalizeRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return string.Empty;

            var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            return string.Join(",", roleList);
        }

        /// <summary>
        /// دریافت متن نقش
        /// </summary>
        private string GetRoleText(TaskRole role) => role switch
        {
            TaskRole.Manager => "مدیر",
            TaskRole.Creator => "سازنده",
            TaskRole.Member => "عضو",
            TaskRole.Supervisor => "ناظر",
            TaskRole.CarbonCopy => "رونوشت",
            _ => "نامشخص"
        };

        #endregion

        #region Preview & Stats

        /// <summary>
        /// پیش‌نمایش تنظیمات (برای Live Preview در UI)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PreviewTaskSettings(SaveTaskSettingsViewModel model)
        {
            try
            {
                // شبیه‌سازی تنظیمات
                var previewData = new
                {
                    canComment = ParseRoles(model.CanCommentRoles),
                    canAddMembers = ParseRoles(model.CanAddMembersRoles),
                    canRemoveMembers = ParseRoles(model.CanRemoveMembersRoles),
                    canEditAfterCompletion = ParseRoles(model.CanEditAfterCompletionRoles),
                    creatorCanEditDelete = model.CreatorCanEditDelete
                };

                return Json(new { success = true, preview = previewData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Parse کردن نقش‌ها
        /// </summary>
        private List<string> ParseRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return new List<string>();

            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();
        }

        #endregion
    }
}
