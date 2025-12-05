using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// ⭐⭐⭐ TaskCategoryController Partial - مدیریت تنظیمات پیش‌فرض دسته‌بندی
    /// </summary>
    public partial class TaskCategoryController
    {
        #region Category Task Settings Modal

        /// <summary>
        /// نمایش مودال تنظیمات پیش‌فرض تسک دسته‌بندی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CategoryTaskSettingsModal(int categoryId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // دریافت دسته‌بندی از Repository
                var category = await _taskRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound();
                }

                // بررسی دسترسی: آیا این دسته‌بندی در هیچ‌یک از شعبه‌های کاربر تعریف شده است؟
                var userBranches = await _branchRepository.GetUserBranchIdsAsync(currentUserId);
                var categoryBranches = await _branchRepository.GetCategoryBranchIdsAsync(categoryId);
                
                if (!categoryBranches.Any(branchId => userBranches.Contains(branchId)))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این دسته‌بندی دسترسی ندارید" }
                        }
                    });
                }

                // دریافت تنظیمات موجود یا ایجاد پیش‌فرض
                var settings = await _taskRepository.GetCategoryDefaultSettingsAsync(categoryId);
                var hasSettings = settings != null;

                if (settings == null)
                {
                    // ایجاد تنظیمات پیش‌فرض
                    settings = new TaskCategoryDefaultSettings
                    {
                        TaskCategoryId = categoryId,
                        CanCommentRoles = "a,b,c,d,e",
                        CanAddMembersRoles = "a,b",
                        CanRemoveMembersRoles = "a,b",
                        CanEditAfterCompletionRoles = "a,b",
                        CreatorCanEditDelete = false
                    };
                }

                var viewModel = new CategoryDefaultSettingsViewModel
                {
                    CategoryId = categoryId,
                    CategoryName = category.Title,
                    HasSettings = hasSettings
                };

                // تنظیم 1: کامنت
                viewModel.CommentSetting = CreateCategorySettingItem(
                    1,
                    "کامنت‌گذاری",
                    "تنظیم پیش‌فرض برای تمام تسک‌های جدید این دسته‌بندی",
                    settings.CanCommentRoles);

                // تنظیم 2: افزودن عضو
                viewModel.AddMembersSetting = CreateCategorySettingItem(
                    2,
                    "افزودن عضو جدید",
                    "چه کسانی می‌توانند افراد جدید را به تسک اضافه کنند",
                    settings.CanAddMembersRoles);

                // تنظیم 3: حذف عضو
                viewModel.RemoveMembersSetting = CreateCategorySettingItem(
                    3,
                    "حذف عضو",
                    "چه کسانی می‌توانند اعضا را از تسک حذف کنند",
                    settings.CanRemoveMembersRoles);

                // تنظیم 4: ویرایش پس از اتمام
                viewModel.EditAfterCompletionSetting = CreateCategorySettingItem(
                    4,
                    "ویرایش پس از تکمیل",
                    "پس از تکمیل تسک، چه کسانی می‌توانند آن را ویرایش کنند",
                    settings.CanEditAfterCompletionRoles);

                // تنظیم 5: حذف/ویرایش سازنده
                viewModel.CreatorEditDeleteSetting = new CreatorEditDeleteSettingViewModel
                {
                    SettingId = 5,
                    Title = "مجوز حذف/ویرایش برای سازنده",
                    Description = "آیا سازنده تسک می‌تواند آن را حذف یا ویرایش کند؟",
                    IsEnabled = settings.CreatorCanEditDelete,
                    IsReadOnly = false
                };

                // دریافت آمار از Repository
                viewModel.Statistics = await _taskRepository.GetSettingsStatisticsAsync(categoryId: categoryId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "TaskCategory",
                    "CategoryTaskSettingsModal",
                    $"مشاهده تنظیمات پیش‌فرض دسته‌بندی: {category.Title}",
                    recordId: categoryId.ToString(),
                    entityType: "TaskCategoryDefaultSettings"
                );

                return PartialView("_CategoryTaskSettingsModal", viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskCategory",
                    "CategoryTaskSettingsModal",
                    "خطا در بارگذاری تنظیمات دسته‌بندی",
                    ex,
                    recordId: categoryId.ToString()
                );

                return BadRequest("خطا در بارگذاری تنظیمات: " + ex.Message);
            }
        }

        #endregion

        #region Save Category Settings

        /// <summary>
        /// ذخیره تنظیمات پیش‌فرض دسته‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategoryTaskSettings(
            int categoryId,
            string canCommentRoles,
            string canAddMembersRoles,
            string canRemoveMembersRoles,
            string canEditAfterCompletionRoles,
            bool creatorCanEditDelete)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // دریافت دسته‌بندی و بررسی دسترسی از Repository
                var category = await _taskRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "دسته‌بندی یافت نشد" }
                        }
                    });
                }

                var userBranches = await _branchRepository.GetUserBranchIdsAsync(currentUserId);
                var categoryBranches = await _branchRepository.GetCategoryBranchIdsAsync(categoryId);
                
                if (!categoryBranches.Any(branchId => userBranches.Contains(branchId)))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این دسته‌بندی دسترسی ندارید" }
                        }
                    });
                }

                var settings = new TaskCategoryDefaultSettings
                {
                    TaskCategoryId = categoryId,
                    CanCommentRoles = NormalizeCategoryRoles(canCommentRoles),
                    CanAddMembersRoles = NormalizeCategoryRoles(canAddMembersRoles),
                    CanRemoveMembersRoles = NormalizeCategoryRoles(canRemoveMembersRoles),
                    CanEditAfterCompletionRoles = NormalizeCategoryRoles(canEditAfterCompletionRoles),
                    CreatorCanEditDelete = creatorCanEditDelete
                };

                await _taskRepository.SaveCategoryDefaultSettingsAsync(settings, currentUserId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "TaskCategory",
                    "SaveCategoryTaskSettings",
                    $"ویرایش تنظیمات پیش‌فرض دسته‌بندی: {category.Title}",
                    recordId: categoryId.ToString(),
                    entityType: "TaskCategoryDefaultSettings"
                );

                return Json(new
                {
                    success = true,
                    message = new[] {
                        new { status = "success", text = "تنظیمات پیش‌فرض دسته‌بندی با موفقیت ذخیره شد" }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskCategory",
                    "SaveCategoryTaskSettings",
                    "خطا در ذخیره تنظیمات دسته‌بندی",
                    ex,
                    recordId: categoryId.ToString()
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

        #region Apply to All Tasks

        /// <summary>
        /// اعمال تنظیمات دسته‌بندی به تمام تسک‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCategorySettingsToAllTasks(int categoryId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی از Repository
                var category = await _taskRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "دسته‌بندی یافت نشد" }
                        }
                    });
                }

                var userBranches = await _branchRepository.GetUserBranchIdsAsync(currentUserId);
                var categoryBranches = await _branchRepository.GetCategoryBranchIdsAsync(categoryId);
                
                if (!categoryBranches.Any(branchId => userBranches.Contains(branchId)))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این دسته‌بندی دسترسی ندارید" }
                        }
                    });
                }

                // اعمال تنظیمات از Repository
                var updatedCount = await _taskRepository.ApplyCategorySettingsToAllTasksAsync(categoryId, currentUserId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "TaskCategory",
                    "ApplyCategorySettingsToAllTasks",
                    $"اعمال تنظیمات به {updatedCount} تسک در دسته‌بندی: {category.Title}",
                    recordId: categoryId.ToString(),
                    entityType: "TaskCategoryDefaultSettings"
                );

                return Json(new
                {
                    success = true,
                    message = new[] {
                        new { status = "success", text = $"تنظیمات به {updatedCount} تسک اعمال شد" }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "TaskCategory",
                    "ApplyCategorySettingsToAllTasks",
                    "خطا در اعمال تنظیمات",
                    ex,
                    recordId: categoryId.ToString()
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
        /// ایجاد آیتم تنظیم برای دسته‌بندی
        /// </summary>
        private SettingItemViewModel CreateCategorySettingItem(
            int settingId,
            string title,
            string description,
            string selectedRoles)
        {
            var allRoles = new List<RoleCheckboxItem>
            {
                new() { RoleCode = "a", RoleText = "مدیر", AuthorityLevel = 1, IsChecked = selectedRoles.Contains("a"), IsDisabled = false },
                new() { RoleCode = "b", RoleText = "سازنده", AuthorityLevel = 2, IsChecked = selectedRoles.Contains("b"), IsDisabled = false },
                new() { RoleCode = "c", RoleText = "عضو", AuthorityLevel = 3, IsChecked = selectedRoles.Contains("c"), IsDisabled = false },
                new() { RoleCode = "d", RoleText = "ناظر", AuthorityLevel = 4, IsChecked = selectedRoles.Contains("d"), IsDisabled = false },
                new() { RoleCode = "e", RoleText = "رونوشت", AuthorityLevel = 5, IsChecked = selectedRoles.Contains("e"), IsDisabled = false }
            };

            return new SettingItemViewModel
            {
                SettingId = settingId,
                Title = title,
                Description = description,
                AvailableRoles = allRoles,
                IsReadOnly = false,
                SelectedRoles = selectedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            };
        }

        /// <summary>
        /// Normalize کردن نقش‌ها
        /// </summary>
        private string NormalizeCategoryRoles(string roles)
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

        #endregion
    }
}
