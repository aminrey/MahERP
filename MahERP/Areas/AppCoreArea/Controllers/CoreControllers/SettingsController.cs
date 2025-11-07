using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.CoreControllers
{
    [Area("AppCoreArea")]

    [Authorize(Roles = "Admin")]
    public class SettingsController : BaseController
    {
        public SettingsController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking)
        {
        }

        // GET: Settings/ModuleSettings
        public IActionResult ModuleSettings()
        {
            var settings = _baseRepository.GetSystemSettings();
            return View(settings);
        }

        // POST: Settings/UpdateModuleSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateModuleSettings(Settings model)
        {
            try
            {
                var settings = _uow.SettingsUW.Get().FirstOrDefault();

                if (settings == null)
                {
                    settings = new Settings();
                    _uow.SettingsUW.Create(settings);
                }

                settings.IsTaskingModuleEnabled = model.IsTaskingModuleEnabled;
                settings.IsCrmModuleEnabled = model.IsCrmModuleEnabled;
                settings.LastModified = DateTime.Now;
                settings.LastModifiedByUserId = GetUserId();

                _uow.Save();

                // پاک کردن کش
                _baseRepository.ClearSettingsCache();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Settings,
                    "UpdateModuleSettings",
                    $"بروزرسانی تنظیمات ماژول‌ها - Tasking: {model.IsTaskingModuleEnabled}, CRM: {model.IsCrmModuleEnabled}",
                    GetUserId()
                );

                TempData["SuccessMessage"] = "تنظیمات ماژول‌ها با موفقیت بروزرسانی شد.";
                return RedirectToAction(nameof(ModuleSettings));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در بروزرسانی تنظیمات: {ex.Message}";
                return View("ModuleSettings", model);
            }
        }
    }
}