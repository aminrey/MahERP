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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.CommunicationControllers
{
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("COMMUNICATION.SMS")]
    public class SmsSendController : BaseController
    {
        private readonly ISmsService _smsService;
        private readonly ISmsProviderRepository _providerRepo;
        private readonly ISmsQueueRepository _queueRepo;
        private readonly ISmsTemplateRepository _templateRepo;
        private readonly ILogger<SmsSendController> _logger;

        public SmsSendController(
            ISmsService smsService,
            ISmsProviderRepository providerRepo,
            ISmsQueueRepository queueRepo,
            ISmsTemplateRepository templateRepo,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository,
            ILogger<SmsSendController> logger, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _smsService = smsService;
            _providerRepo = providerRepo;
            _queueRepo = queueRepo;
            _templateRepo = templateRepo;
            _logger = logger;
        }

        // ========== صفحه اصلی ارسال ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Providers = _providerRepo.GetActiveProviders();
            ViewBag.Templates = await _templateRepo.GetAllTemplatesAsync();

            return View();
        }

        // ========== ارسال به افراد انتخابی ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendToContacts(
            List<int> contactIds,
            string message,
            int? providerId = null)
        {
            if (!contactIds.Any() || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "لطفا افراد و متن پیام را وارد کنید" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var logs = await _smsService.SendToMultipleContactsAsync(
                    contactIds,
                    message,
                    currentUser.Id,
                    providerId);

                int successCount = logs.Count(l => l.IsSuccess);
                int failCount = logs.Count - successCount;

                return Json(new
                {
                    success = true,
                    message = $"ارسال به {successCount} نفر موفق و {failCount} نفر ناموفق بود",
                    successCount,
                    failCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== ارسال به سازمان ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendToOrganization(
            int organizationId,
            string message,
            bool sendToContacts = false,
            int? providerId = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "لطفا متن پیام را وارد کنید" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (sendToContacts)
                {
                    var logs = await _smsService.SendToOrganizationContactsAsync(
                        organizationId,
                        message,
                        currentUser.Id,
                        providerId);

                    int successCount = logs.Count(l => l.IsSuccess);
                    return Json(new
                    {
                        success = true,
                        message = $"ارسال به {successCount} نفر از سازمان موفق بود"
                    });
                }
                else
                {
                    var log = await _smsService.SendToOrganizationAsync(
                        organizationId,
                        message,
                        currentUser.Id,
                        providerId);

                    return Json(new
                    {
                        success = log.IsSuccess,
                        message = log.IsSuccess ? "پیامک با موفقیت ارسال شد" : log.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== ارسال با قالب ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendWithTemplate(
            int contactId,
            int templateId,
            Dictionary<string, string> parameters,
            int? providerId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var log = await _smsService.SendWithTemplateToContactAsync(
                    contactId,
                    templateId,
                    parameters,
                    currentUser.Id,
                    providerId);

                return Json(new
                {
                    success = log.IsSuccess,
                    message = log.IsSuccess ? "پیامک با موفقیت ارسال شد" : log.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== دریافت لیست مخاطبین برای انتخاب ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> GetContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsAsync(search);
            return Json(contacts);
        }

        // ========== افزودن به صف ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendToContactsQueued(
            List<int> contactIds,
            string message,
            int? providerId = null,
            byte priority = 0,
            DateTime? scheduledDate = null)
        {
            if (!contactIds.Any() || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "لطفا افراد و متن پیام را وارد کنید" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var queueItems = new List<SmsQueue>();

                foreach (var contactId in contactIds)
                {
                    var contact =  _uow.ContactUW.GetById(contactId);

                    if (contact?.DefaultPhone != null)
                    {
                        queueItems.Add(new SmsQueue
                        {
                            PhoneNumber = contact.DefaultPhone.PhoneNumber,
                            MessageText = message,
                            RecipientType = 0,
                            ContactId = contactId,
                            ProviderId = providerId,
                            Priority = priority,
                            ScheduledDate = scheduledDate,
                            RequestedByUserId = currentUser.Id
                        });
                    }
                }

                int enqueuedCount = await _queueRepo.EnqueueBulkAsync(queueItems);

                return Json(new
                {
                    success = true,
                    message = $"{enqueuedCount} پیامک به صف ارسال اضافه شد",
                    enqueuedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== ارسال به دریافت کنندگان قالب ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendToTemplateRecipients(
            int templateId,
            bool useQueue = true)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var template = await _templateRepo.GetTemplateByIdAsync(templateId);

                if (template == null)
                    return Json(new { success = false, message = "قالب یافت نشد" });

                var recipients = await _templateRepo.GetTemplateRecipientsAsync(templateId);

                if (!recipients.Any())
                    return Json(new { success = false, message = "مخاطبی برای ارسال یافت نشد" });

                int sentCount = 0;

                foreach (var recipient in recipients)
                {
                    string phoneNumber = null;

                    if (recipient.RecipientType == 0 && recipient.Contact != null)
                    {
                        phoneNumber = recipient.Contact.DefaultPhone?.PhoneNumber;
                    }
                    else if (recipient.RecipientType == 1 && recipient.Organization != null)
                    {
                        phoneNumber = recipient.Organization.PrimaryPhone;
                    }

                    if (string.IsNullOrEmpty(phoneNumber))
                        continue;

                    if (useQueue)
                    {
                        await _queueRepo.EnqueueAsync(new SmsQueue
                        {
                            PhoneNumber = phoneNumber,
                            MessageText = template.MessageTemplate,
                            RecipientType = recipient.RecipientType,
                            ContactId = recipient.ContactId,
                            OrganizationId = recipient.OrganizationId,
                            RequestedByUserId = currentUser.Id
                        });
                        sentCount++;
                    }
                    else
                    {
                        // ارسال مستقیم از طریق Service
                        sentCount++;
                    }
                }

                // بروزرسانی تعداد استفاده
                await _templateRepo.IncrementUsageCountAsync(templateId);

                return Json(new
                {
                    success = true,
                    message = $"{sentCount} پیامک {(useQueue ? "به صف اضافه شد" : "ارسال شد")}"

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال به مخاطبین قالب");
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }
    }
}