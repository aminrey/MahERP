using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.AppCoreArea.Controllers
{
    /// <summary>
    /// ⭐⭐⭐ BranchController Partial - مدیریت تنظیمات پیش‌فرض تسک شعبه
    /// </summary>
    public partial class BranchController
    {
        #region Branch Task Settings Modal

        /// <summary>
        /// نمایش مودال تنظیمات پیش‌فرض تسک شعبه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BranchTaskSettingsModal(int branchId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی به شعبه
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این شعبه دسترسی ندارید" }
                        }
                    });
                }

                var branch = userBranches.FirstOrDefault(b => b.Id == branchId);
                if (branch == null)
                {
                    return NotFound();
                }

                // دریافت تنظیمات موجود یا ایجاد پیش‌فرض
                var settings = await _taskRepository.GetBranchDefaultSettingsAsync(branchId);
                var hasSettings = settings != null;

                if (settings == null)
                {
                    // ایجاد تنظیمات پیش‌فرض
                    settings = new BranchDefaultTaskSettings
                    {
                        BranchId = branchId,
                        CanCommentRoles = "a,b,c,d,e",
                        CanAddMembersRoles = "a,b",
                        CanRemoveMembersRoles = "a,b",
                        CanEditAfterCompletionRoles = "a,b",
                        CreatorCanEditDelete = false
                    };
                }

                var viewModel = new BranchDefaultSettingsViewModel
                {
                    BranchId = branchId,
                    BranchName = branch.Name,
                    HasSettings = hasSettings
                };

                // تنظیم 1: کامنت
                viewModel.CommentSetting = CreateBranchSettingItem(
                    1,
                    "کامنت‌گذاری",
                    "تنظیم پیش‌فرض برای تمام تسک‌های جدید این شعبه",
                    settings.CanCommentRoles);

                // تنظیم 2: افزودن عضو
                viewModel.AddMembersSetting = CreateBranchSettingItem(
                    2,
                    "افزودن عضو جدید",
                    "چه کسانی می‌توانند افراد جدید را به تسک اضافه کنند",
                    settings.CanAddMembersRoles);

                // تنظیم 3: حذف عضو
                viewModel.RemoveMembersSetting = CreateBranchSettingItem(
                    3,
                    "حذف عضو",
                    "چه کسانی می‌توانند اعضا را از تسک حذف کنند",
                    settings.CanRemoveMembersRoles);

                // تنظیم 4: ویرایش پس از اتمام
                viewModel.EditAfterCompletionSetting = CreateBranchSettingItem(
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

                // دریافت آمار
                viewModel.Statistics = await _taskRepository.GetSettingsStatisticsAsync(branchId: branchId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "BranchTaskSettingsModal",
                    $"مشاهده تنظیمات پیش‌فرض تسک شعبه: {branch.Name}",
                    recordId: branchId.ToString(),
                    entityType: "BranchDefaultTaskSettings"
                );

                return PartialView("_BranchTaskSettingsModal", viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "BranchTaskSettingsModal",
                    "خطا در بارگذاری تنظیمات شعبه",
                    ex,
                    recordId: branchId.ToString()
                );

                return BadRequest("خطا در بارگذاری تنظیمات: " + ex.Message);
            }
        }

        #endregion

        #region Save Branch Settings

        /// <summary>
        /// ذخیره تنظیمات پیش‌فرض شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBranchTaskSettings(
            int branchId,
            string canCommentRoles,
            string canAddMembersRoles,
            string canRemoveMembersRoles,
            string canEditAfterCompletionRoles,
            bool creatorCanEditDelete)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این شعبه دسترسی ندارید" }
                        }
                    });
                }

                var settings = new BranchDefaultTaskSettings
                {
                    BranchId = branchId,
                    CanCommentRoles = NormalizeBranchRoles(canCommentRoles),
                    CanAddMembersRoles = NormalizeBranchRoles(canAddMembersRoles),
                    CanRemoveMembersRoles = NormalizeBranchRoles(canRemoveMembersRoles),
                    CanEditAfterCompletionRoles = NormalizeBranchRoles(canEditAfterCompletionRoles),
                    CreatorCanEditDelete = creatorCanEditDelete
                };

                await _taskRepository.SaveBranchDefaultSettingsAsync(settings, currentUserId);

                var branch = userBranches.FirstOrDefault(b => b.Id == branchId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Branch",
                    "SaveBranchTaskSettings",
                    $"ویرایش تنظیمات پیش‌فرض تسک شعبه: {branch?.Name}",
                    recordId: branchId.ToString(),
                    entityType: "BranchDefaultTaskSettings"
                );

                return Json(new
                {
                    success = true,
                    message = new[] {
                        new { status = "success", text = "تنظیمات پیش‌فرض شعبه با موفقیت ذخیره شد" }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "SaveBranchTaskSettings",
                    "خطا در ذخیره تنظیمات شعبه",
                    ex,
                    recordId: branchId.ToString()
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
        /// اعمال تنظیمات شعبه به تمام تسک‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyBranchSettingsToAllTasks(int branchId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی دسترسی
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "شما به این شعبه دسترسی ندارید" }
                        }
                    });
                }

                // اعمال تنظیمات
                var updatedCount = await _taskRepository.ApplyBranchSettingsToAllTasksAsync(branchId, currentUserId);

                var branch = userBranches.FirstOrDefault(b => b.Id == branchId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Branch",
                    "ApplyBranchSettingsToAllTasks",
                    $"اعمال تنظیمات به {updatedCount} تسک در شعبه: {branch?.Name}",
                    recordId: branchId.ToString(),
                    entityType: "BranchDefaultTaskSettings"
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
                    "Branch",
                    "ApplyBranchSettingsToAllTasks",
                    "خطا در اعمال تنظیمات",
                    ex,
                    recordId: branchId.ToString()
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
        /// ایجاد آیتم تنظیم برای شعبه
        /// </summary>
        private SettingItemViewModel CreateBranchSettingItem(
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
        private string NormalizeBranchRoles(string roles)
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
