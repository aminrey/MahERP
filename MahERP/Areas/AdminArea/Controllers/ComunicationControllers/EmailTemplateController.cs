using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MahERP.DataModelLayer;
using MahERP.Services;
using MahERP.WebApp.Services;
using MahERP.DataModelLayer.Services;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.Areas.AdminArea.Controllers.ComunicationControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class EmailTemplateController : BaseController
    {
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly UserManager<AppUsers> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _uow;

        public EmailTemplateController(
            IEmailTemplateRepository templateRepo,
            UserManager<AppUsers> userManager,
            IUnitOfWork uow,
            IWebHostEnvironment environment,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _templateRepo = templateRepo;
            _userManager = userManager;
            _environment = environment;
            _uow = uow;
        }

        // ========== GET: Index ==========
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepo.GetAllTemplatesAsync();
            return View(templates);
        }

        // ========== GET: Create ==========
        
        [HttpGet]
        public IActionResult Create()
        {
            return View(new EmailTemplate());
        }

        // ========== POST: Create ==========
        
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> SearchContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsAsync(search);
            return Json(contacts);
        }

        // ========== جستجوی Organizations ==========
        
        [HttpGet]
        public async Task<IActionResult> SearchOrganizations(string search = "")
        {
            var organizations = await _templateRepo.SearchOrganizationsAsync(search);
            return Json(organizations);
        }

        // ========== افزودن مخاطبین ==========
        
        [HttpPost]
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