using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MahERP.Attributes;

namespace MahERP.Areas.AdminArea.Controllers.CommunicationControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("COMMUNICATION.EMAIL")]
    public class EmailSendController : BaseController
    {
        private readonly IEmailRepository _emailRepo;
        private readonly IEmailQueueRepository _queueRepo;
        private readonly IEmailTemplateRepository _templateRepo;
        private readonly ILogger<EmailSendController> _logger;

        public EmailSendController(
            IEmailRepository emailRepo,
            IEmailQueueRepository queueRepo,
            IEmailTemplateRepository templateRepo,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            ILogger<EmailSendController> logger)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _emailRepo = emailRepo;
            _queueRepo = queueRepo;
            _templateRepo = templateRepo;
            _logger = logger;
        }

        // ========== صفحه اصلی ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Templates = await _templateRepo.GetActiveTemplatesAsync();
            return View();
        }

        // ========== ارسال به افراد ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> SendToContacts(
            List<int> contactIds,
            string subject,
            string body,
            bool isHtml = true,
            bool useQueue = true)
        {
            if (!contactIds.Any() || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                return Json(new { success = false, message = "لطفا تمام فیلدها را پر کنید" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (useQueue)
                {
                    // افزودن به صف
                    var queueItems = new List<EmailQueue>();

                    foreach (var contactId in contactIds)
                    {
                        var contact =  _uow.ContactUW.GetById(contactId);
                        if (contact != null && !string.IsNullOrEmpty(contact.PrimaryEmail))
                        {
                            queueItems.Add(new EmailQueue
                            {
                                ToEmail = contact.PrimaryEmail,
                                ToName = contact.FullName,
                                Subject = subject,
                                Body = body,
                                IsHtml = isHtml,
                                RequestedByUserId = currentUser.Id
                            });
                        }
                    }

                    int count = await _queueRepo.EnqueueBulkAsync(queueItems);
                    return Json(new { success = true, message = $"{count} ایمیل به صف اضافه شد" });
                }
                else
                {
                    // ارسال مستقیم
                    var logs = await _emailRepo.SendToMultipleContactsAsync(
                        contactIds,
                        subject,
                        body,
                        currentUser.Id,
                        isHtml
                    );

                    int successCount = logs.Count(l => l.IsSuccess);
                    return Json(new 
                    { 
                        success = true, 
                        message = $"{successCount} از {logs.Count} ایمیل ارسال شد" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل به افراد");
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== ارسال به سازمان ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> SendToOrganization(
            int organizationId,
            string subject,
            string body,
            bool sendToContacts = false,
            bool isHtml = true)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (sendToContacts)
                {
                    var logs = await _emailRepo.SendToOrganizationContactsAsync(
                        organizationId,
                        subject,
                        body,
                        currentUser.Id,
                        isHtml
                    );

                    int successCount = logs.Count(l => l.IsSuccess);
                    return Json(new { success = true, message = $"{successCount} ایمیل ارسال شد" });
                }
                else
                {
                    var log = await _emailRepo.SendToOrganizationAsync(
                        organizationId,
                        subject,
                        body,
                        currentUser.Id,
                        isHtml
                    );

                    return Json(new 
                    { 
                        success = log.IsSuccess, 
                        message = log.IsSuccess ? "ایمیل ارسال شد" : log.ErrorMessage 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== جستجوی مخاطبین ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> GetContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsAsync(search);
            return Json(contacts);
        }

        // ========== آمار ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> Statistics()
        {
            var stats = await _emailRepo.GetStatisticsAsync();
            var queueStats = await _queueRepo.GetStatisticsAsync();

            return Json(new
            {
                sent = stats,
                queue = queueStats
            });
        }

        // ========== ارسال به گیرندگان قالب ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.EMAIL.SEND")]
        public async Task<IActionResult> SendToTemplateRecipients(
            int templateId,
            bool useQueue = true)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var template = await _templateRepo.GetTemplateWithDetailsAsync(templateId);

                if (template == null)
                    return Json(new { success = false, message = "قالب یافت نشد" });

                if (!template.Recipients.Any())
                    return Json(new { success = false, message = "مخاطبی برای ارسال یافت نشد" });

                int sentCount = 0;

                foreach (var recipient in template.Recipients)
                {
                    string email = null;
                    string name = null;

                    if (recipient.RecipientType == 0 && recipient.Contact != null)
                    {
                        email = recipient.Contact.PrimaryEmail;
                        name = recipient.Contact.FullName;
                    }
                    else if (recipient.RecipientType == 1 && recipient.Organization != null)
                    {
                        email = recipient.Organization.Email;
                        name = recipient.Organization.DisplayName;
                    }

                    if (string.IsNullOrEmpty(email))
                        continue;

                    // جایگزینی Placeholders
                    var subject = template.SubjectTemplate
                        .Replace("{Name}", name)
                        .Replace("{Date}", DateTime.Now.ToString("yyyy/MM/dd"));

                    var body = template.BodyHtml
                        .Replace("{Name}", name)
                        .Replace("{Email}", email)
                        .Replace("{Date}", DateTime.Now.ToString("yyyy/MM/dd"));

                    if (useQueue)
                    {
                        await _queueRepo.EnqueueAsync(new EmailQueue
                        {
                            ToEmail = email,
                            ToName = name,
                            Subject = subject,
                            Body = body,
                            IsHtml = true,
                            RequestedByUserId = currentUser.Id
                        });
                        sentCount++;
                    }
                    else
                    {
                        var log = await _emailRepo.SendToContactAsync(
                            recipient.ContactId ?? 0,
                            subject,
                            body,
                            currentUser.Id,
                            true
                        );

                        if (log.IsSuccess)
                            sentCount++;
                    }
                }

                // بروزرسانی تعداد استفاده
                await _templateRepo.IncrementUsageCountAsync(templateId);

                return Json(new
                {
                    success = true,
                    message = $"{sentCount} ایمیل {(useQueue ? "به صف اضافه شد" : "ارسال شد")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل به مخاطبین قالب");
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }
    }
}