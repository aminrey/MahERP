using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.CommunicationControllers
{
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("COMMUNICATION.SMS.TEMPLATE")]
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
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _templateRepo = templateRepo;
        }

        // ========== لیست قالب‌ها ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepo.GetAllTemplatesAsync();
            return View(templates);
        }

        // ========== جزئیات قالب ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _templateRepo.GetTemplateDetailAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // ========== ایجاد قالب ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
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
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateRepo.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
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
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
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
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
        public IActionResult ShowAddRecipientsModal(int id)
        {
            return PartialView("_AddRecipientsModal", id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.CREATE")]
        public async Task<IActionResult> AddRecipients(
            int templateId,
            List<string> selectedContacts,
            List<int> organizationIds)
        {
            try
            {
                if ((selectedContacts == null || selectedContacts.Count == 0) && 
                    (organizationIds == null || organizationIds.Count == 0))
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[]
                        {
                            new { status = "warning", text = "لطفا حداقل یک مخاطب انتخاب کنید" }
                        }
                    });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                
                // Parse selectedContacts
                var contactData = new List<(int contactId, int? phoneId)>();
                if (selectedContacts != null)
                {
                    foreach (var item in selectedContacts)
                    {
                        // فرمت: "c123_p456" یا "c123_p0"
                        var parts = item.Split('_');
                        if (parts.Length == 2)
                        {
                            var contactId = int.Parse(parts[0].Replace("c", ""));
                            var phoneIdStr = parts[1].Replace("p", "");
                            var phoneId = phoneIdStr == "0" ? (int?)null : int.Parse(phoneIdStr);
                            
                            contactData.Add((contactId, phoneId));
                        }
                    }
                }
                
                int count = await _templateRepo.AddMultipleRecipientsWithPhonesAsync(
                    templateId,
                    contactData,
                    organizationIds ?? new List<int>(),
                    currentUser.Id
                );

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Details", new { id = templateId }),
                    message = new[]
                    {
                        new { status = "success", text = $"{count} مخاطب با موفقیت اضافه شد" }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[]
                    {
                        new { status = "error", text = $"خطا: {ex.Message}" }
                    }
                });
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
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> SearchContactsSimple(string search = "")
        {
            try
            {
                var contacts = await _templateRepo.SearchContactsSimpleAsync(search);
                return Json(contacts);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// دریافت شماره‌های یک Contact
        /// </summary>
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> GetContactPhones(int contactId)
        {
            try
            {
                var phones = await _templateRepo.GetContactPhonesAsync(contactId);
                return Json(phones);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// دریافت افراد یک Organization
        /// </summary>
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> GetOrganizationContacts(int organizationId)
        {
            try
            {
                var contacts = await _templateRepo.GetOrganizationContactsAsync(organizationId);
                return Json(contacts);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> SearchContacts(string search = "")
        {
            try
            {
                var contacts = await _templateRepo.SearchContactsAsync(search);
                return Json(contacts);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.TEMPLATE.VIEW")]
        public async Task<IActionResult> SearchOrganizations(string search = "")
        {
            try
            {
                var organizations = await _templateRepo.SearchOrganizationsAsync(search);
                return Json(organizations);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }
    }
}