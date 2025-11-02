using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.CommunicationControllers
{
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE")]
    public class EmailTemplateController : BaseController
    {
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly UserManager<AppUsers> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _uow;

        public EmailTemplateController(
            IEmailTemplateRepository templateRepo,
            IUnitOfWork uow,
                        UserManager<AppUsers> userManager,

            IWebHostEnvironment environment,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
        {
            _templateRepo = templateRepo;
            _userManager = userManager;
            _environment = environment;
            _uow = uow;
        }

        // ========== GET: Index ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.VIEW")]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepo.GetAllTemplatesAsync();
            return View(templates);
        }

        // ========== GET: Create ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public IActionResult Create()
        {
            return View(new EmailTemplate());
        }

        // ========== POST: Create ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> Create(EmailTemplate model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                model.CreatorUserId = currentUser.Id;
                model.CreatedDate = DateTime.Now;

                var templateId = await _templateRepo.CreateTemplateAsync(model);

                TempData["SuccessMessage"] = "قالب ایمیل با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Details), new { id = templateId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View(model);
            }
        }

        // ========== GET: Edit ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateRepo.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return View("Create", template);
        }

        // ========== POST: Edit ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> Edit(int id, EmailTemplate model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View("Create", model);

            try
            {
                var exists = await _templateRepo.TemplateExistsAsync(id);
                if (!exists)
                    return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);

                await _templateRepo.UpdateTemplateFieldsAsync(
                    id,
                    model.Title,
                    model.SubjectTemplate,
                    model.BodyHtml,
                    model.BodyPlainText,
                    model.Description,
                    model.Category,
                    model.IsActive,
                    currentUser.Id
                );

                TempData["SuccessMessage"] = "قالب با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                return View("Create", model);
            }
        }

        // ========== POST: Upload Image (برای TinyMCE) ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { error = "فایل انتخاب نشده است" });

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "email-images");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/email-images/{uniqueFileName}";

                return Json(new { location = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"خطا در آپلود تصویر: {ex.Message}" });
            }
        }

        // ========== GET: Details ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            var template = await _templateRepo.GetTemplateWithDetailsAsync(id);
            if (template == null)
                return NotFound();

            return View(template);
        }

        // ========== POST: Delete ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exists = await _templateRepo.TemplateExistsAsync(id);
                if (!exists)
                    return Json(new { success = false, message = "قالب یافت نشد" });

                await _templateRepo.DeleteTemplateAsync(id);

                return Json(new { success = true, message = "قالب حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== جستجوی Contacts ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.VIEW")]
        public async Task<IActionResult> SearchContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsAsync(search);
            return Json(contacts);
        }

        // ========== جستجوی Organizations ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.VIEW")]
        public async Task<IActionResult> SearchOrganizations(string search = "")
        {
            var organizations = await _templateRepo.SearchOrganizationsAsync(search);
            return Json(organizations);
        }

        // ========== افزودن مخاطبین ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
        public async Task<IActionResult> AddRecipients(
            int templateId,
            List<int> contactIds,
            List<int> organizationIds)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                int count = await _templateRepo.AddRecipientsAsync(
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

        // ========== حذف مخاطب ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.TEMPLATE.CREATE")]
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
    }
}