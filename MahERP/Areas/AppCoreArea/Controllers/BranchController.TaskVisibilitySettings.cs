using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.AppCoreArea.Controllers
{
    /// <summary>
    /// کنترلر برای مدیریت تنظیمات نمایش تسک در شعبه
    /// </summary>
    [Authorize]
    public partial class BranchController
    {
        #region Task Visibility Settings

        /// <summary>
        /// ⭐⭐⭐ مودال تنظیمات نمایش تسک (پیش‌فرض شعبه)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TaskVisibilitySettingsModal(int branchId)
        {
            try
            {
                var settings = await _visibilitySettingsRepo.GetSettingsViewModelAsync(branchId);
                return PartialView("_BranchTaskVisibilitySettingsModal", settings);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"خطا در بارگذاری تنظیمات: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ مودال تنظیمات شخصی مدیر
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TaskVisibilitySettingsPersonalModal(int branchId, string managerId)
        {
            try
            {
                var settings = await _visibilitySettingsRepo.GetSettingsViewModelAsync(branchId, managerId);
                return PartialView("_BranchTaskVisibilitySettingsModal", settings);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"خطا در بارگذاری تنظیمات: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ ذخیره تنظیمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTaskVisibilitySettings(BranchTaskVisibilitySettingsViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _visibilitySettingsRepo.SaveSettingsAsync(model, userId);

                if (result)
                {
                    return Json(new
                    {
                        status = "success",
                        message = "تنظیمات با موفقیت ذخیره شد"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "error",
                        message = "خطا در ذخیره تنظیمات"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ حذف تنظیمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaskVisibilitySettings(int settingsId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _visibilitySettingsRepo.DeleteSettingsAsync(settingsId, userId);

                if (result)
                {
                    return Json(new
                    {
                        status = "success",
                        message = "تنظیمات حذف شد"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "error",
                        message = "خطا در حذف تنظیمات"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ⭐⭐⭐ لیست تنظیمات شعبه (JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBranchTaskVisibilitySettings(int branchId)
        {
            try
            {
                var settings = await _visibilitySettingsRepo.GetAllBranchSettingsAsync(branchId);

                var result = settings.Select(s => new
                {
                    id = s.Id,
                    managerUserId = s.ManagerUserId,
                    managerName = s.Manager != null
                        ? $"{s.Manager.FirstName} {s.Manager.LastName}"
                        : "پیش‌فرض شعبه",
                    showAllSubTeams = s.ShowAllSubTeamsByDefault,
                    maxTasks = s.MaxTasksToShow,
                    teamsCount = s.GetVisibleTeamIds().Count,
                    isActive = s.IsActive
                }).ToList();

                return Json(new
                {
                    status = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        #endregion
    }
}
