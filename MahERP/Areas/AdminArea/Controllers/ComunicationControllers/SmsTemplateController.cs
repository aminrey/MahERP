using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.ComunicationControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class SmsTemplateController : BaseController
    {
        private readonly ISmsTemplateRepository _templateRepo;

        public SmsTemplateController(
            ISmsTemplateRepository templateRepo,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _templateRepo = templateRepo;
        }

        // ========== لیست قالب‌ها ==========

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepo.GetAllTemplatesAsync();
            return View(templates);
        }

        // ========== جزئیات قالب ==========

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _templateRepo.GetTemplateDetailAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // ========== ایجاد قالب ==========

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SmsTemplate model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                model.CreatorUserId = currentUser.Id;
                model.CreatedDate = DateTime.Now;

                await _templateRepo.CreateTemplateAsync(model);

                TempData["SuccessMessage"] = "قالب با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View(model);
            }
        }

        // ========== ویرایش قالب ==========

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateRepo.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SmsTemplate model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);
                if (template == null)
                    return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);

                template.Title = model.Title;
                template.MessageTemplate = model.MessageTemplate;
                template.Description = model.Description;
                template.TemplateType = model.TemplateType;
                template.IsActive = model.IsActive;
                template.LastUpdateDate = DateTime.Now;
                template.LastUpdaterUserId = currentUser.Id;

                await _templateRepo.UpdateTemplateAsync(template);

                TempData["SuccessMessage"] = "قالب با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View(model);
            }
        }

        // ========== حذف قالب ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _templateRepo.DeleteTemplateAsync(id);
                return Json(new { success = true, message = "قالب حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== مدیریت مخاطبین ==========

        [HttpPost]
        public async Task<IActionResult> AddRecipients(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                int count = await _templateRepo.AddMultipleRecipientsAsync(
                    templateId,
                    contactIds,
                    organizationIds,
                    currentUser.Id
                );

                return Json(new { success = true, message = $"{count} مخاطب اضافه شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRecipient(int recipientId)
        {
            try
            {
                await _templateRepo.RemoveRecipientAsync(recipientId);
                return Json(new { success = true, message = "مخاطب حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== جستجو ==========

        [HttpGet]
        public async Task<IActionResult> SearchContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsAsync(search);
            return Json(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> SearchOrganizations(string search = "")
        {
            var organizations = await _templateRepo.SearchOrganizationsAsync(search);
            return Json(organizations);
        }
    }
}