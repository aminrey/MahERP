using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using MahERP.Attributes;

namespace MahERP.Areas.AdminArea.Controllers.ComunicationControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
    public class SmsProviderController : BaseController
    {
        private readonly ISmsProviderRepository _providerRepo;

        public SmsProviderController(
            ISmsProviderRepository providerRepo,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _providerRepo = providerRepo;
        }

        // ========== لیست Providers ==========

        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public IActionResult Index()
        {
            var providers = _providerRepo.GetAllProviders();
            var defaultProvider = _providerRepo.GetDefaultProvider();

            var viewModel = new SmsProviderListViewModel
            {
                Providers = providers.Select(p => new SmsProviderViewModel
                {
                    Id = p.Id,
                    ProviderCode = p.ProviderCode,
                    ProviderName = p.ProviderName,
                    Username = p.Username,
                    IsActive = p.IsActive,
                    IsDefault = p.IsDefault,
                    RemainingCredit = p.RemainingCredit
                 
                }).ToList(),
                CurrentDefaultProvider = defaultProvider?.ProviderName ?? "تنظیم نشده"
            };

            return View(viewModel);
        }
        // ========== ایجاد Provider جدید ==========

        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public IActionResult Create()
        {
            return View(new SmsProvider());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public async Task<IActionResult> Create(SmsProvider model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                model.ProviderCode = model.ProviderCode.ToUpper();
                model.CreatedDate = DateTime.Now;
                model.CreatorUserId = currentUser.Id;

                _providerRepo.CreateProvider(model);

                if (model.IsDefault)
                    _providerRepo.SetAsDefaultProvider(model.Id);

                TempData["SuccessMessage"] = "خدمات‌دهنده با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View(model);
            }
        }

        // ========== ویرایش Provider ==========

        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public IActionResult Edit(int id)
        {
            var providers = _providerRepo.GetAllProviders();
            var provider = providers.Find(p => p.Id == id);

            if (provider == null)
                return NotFound();

            return View(provider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public async Task<IActionResult> Edit(int id, SmsProvider model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                model.LastUpdateDate = DateTime.Now;
                model.LastUpdaterUserId = currentUser.Id;

                _providerRepo.UpdateProvider(model);

                if (model.IsDefault)
                    _providerRepo.SetAsDefaultProvider(model.Id);

                TempData["SuccessMessage"] = "خدمات‌دهنده با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View(model);
            }
        }

        // ========== حذف Provider ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public IActionResult Delete(int id)
        {
            try
            {
                var provider = _providerRepo.GetAllProviders().Find(p => p.Id == id);
                if (provider == null)
                    return Json(new { success = false, message = "خدمات‌دهنده یافت نشد" });

                if (provider.IsDefault)
                    return Json(new { success = false, message = "نمی‌توانید خدمات‌دهنده پیش‌فرض را حذف کنید" });

                _providerRepo.DeleteProvider(id);
                return Json(new { success = true, message = "خدمات‌دهنده حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== تنظیم پیش‌فرض ==========

        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public IActionResult SetAsDefault(int id)
        {
            try
            {
                _providerRepo.SetAsDefaultProvider(id);
                return Json(new { success = true, message = "خدمات‌دهنده به عنوان پیش‌فرض تنظیم شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== تست اتصال ==========

        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public async Task<IActionResult> TestConnection(int id)
        {
            try
            {
                var providers = _providerRepo.GetAllProviders();
                var provider = providers.Find(p => p.Id == id);

                if (provider == null)
                    return Json(new { success = false, message = "خدمات‌دهنده یافت نشد" });

                var instance = _providerRepo.CreateProviderInstance(provider);
                bool isConnected = await instance.TestConnectionAsync();

                if (isConnected)
                {
                    long credit = await instance.GetCreditAsync();
                    return Json(new
                    {
                        success = true,
                        message = "اتصال با موفقیت برقرار شد",
                        credit = credit.ToString("N0")
                    });
                }

                return Json(new { success = false, message = "خطا در برقراری اتصال" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== بروزرسانی اعتبار ==========

        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public async Task<IActionResult> UpdateCredit(int id)
        {
            try
            {
                await _providerRepo.UpdateProviderCredit(id);
                var providers = _providerRepo.GetAllProviders();
                var provider = providers.Find(p => p.Id == id);

                return Json(new
                {
                    success = true,
                    message = "اعتبار بروزرسانی شد",
                    credit = provider?.RemainingCredit?.ToString("N0") ?? "نامشخص"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== بروزرسانی اعتبار همه ==========

        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.PROVIDER")]
        public async Task<IActionResult> UpdateAllCredits()
        {
            try
            {
                await _providerRepo.UpdateAllActiveProvidersCredit();
                return Json(new { success = true, message = "اعتبار تمام خدمات‌دهندگان بروزرسانی شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }
    }
}