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
        private readonly IBackgroundJobRepository _backgroundJobRepo; // ⭐ تغییر نام
        private readonly IBackgroundJobNotificationService _backgroundJobNotificationService; // ⭐ تغییر نام

        public SmsSendController(
            ISmsService smsService,
            ISmsProviderRepository providerRepo,
            ISmsQueueRepository queueRepo,
            ISmsTemplateRepository templateRepo,
            IBackgroundJobRepository backgroundJobRepo, // ⭐ تغییر نام
            IBackgroundJobNotificationService backgroundJobNotificationService, // ⭐ تغییر نام
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, 
            IBaseRepository BaseRepository,
            ILogger<SmsSendController> logger, 
            IModuleTrackingService moduleTracking, 
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _smsService = smsService;
            _providerRepo = providerRepo;
            _queueRepo = queueRepo;
            _templateRepo = templateRepo;
            _logger = logger;
            _backgroundJobRepo = backgroundJobRepo; // ⭐ تغییر نام
            _backgroundJobNotificationService = backgroundJobNotificationService; // ⭐ تغییر نام
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

        // ========== جستجوی Contacts برای Select2 ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SearchContacts(string search = "")
        {
            var contacts = await _templateRepo.SearchContactsSimpleAsync(search);
            return Json(contacts);
        }

        // ========== جستجوی Organizations ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SearchOrganizations(string search = "")
        {
            var organizations = await _templateRepo.SearchOrganizationsAsync(search);
            return Json(organizations);
        }

        // ========== دریافت شماره‌های یک Contact ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> GetContactPhones(int contactId)
        {
            var phones = await _templateRepo.GetContactPhonesAsync(contactId);
            return Json(phones);
        }

        // ========== دریافت افراد یک Organization ==========
        [HttpGet]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> GetOrganizationContacts(int organizationId)
        {
            var contacts = await _templateRepo.GetOrganizationContactsAsync(organizationId);
            return Json(contacts);
        }

        // ========== ارسال به افراد انتخابی ==========
        [HttpPost]
        [PermissionRequired("COMMUNICATION.SMS.SEND")]
        public async Task<IActionResult> SendToContacts(
            List<string> selectedContacts,
            string message,
            int? providerId = null,
            bool useQueue = false)
        {
            if ((selectedContacts == null || !selectedContacts.Any()) || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "لطفا افراد و متن پیام را وارد کنید" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                // ⭐ Parse selectedContacts
                var contactPhoneData = new List<(int contactId, int? phoneId, string phoneNumber)>();
                
                foreach (var item in selectedContacts)
                {
                    var parts = item.Split('_');
                    if (parts.Length == 2)
                    {
                        var contactId = int.Parse(parts[0].Replace("c", ""));
                        var phoneIdStr = parts[1].Replace("p", "");
                        var phoneId = phoneIdStr == "0" ? (int?)null : int.Parse(phoneIdStr);
                        
                        string phoneNumber = null;
                        if (phoneId.HasValue)
                        {
                            var phone = _uow.ContactPhoneUW.GetById(phoneId.Value);
                            phoneNumber = phone?.PhoneNumber;
                        }
                        else
                        {
                            var contact = _uow.ContactUW.GetById(contactId);
                            phoneNumber = contact?.DefaultPhone?.PhoneNumber;
                        }
                        
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            contactPhoneData.Add((contactId, phoneId, phoneNumber));
                        }
                    }
                }

                if (!contactPhoneData.Any())
                {
                    return Json(new { success = false, message = "هیچ شماره معتبری یافت نشد" });
                }

                // ⭐ ایجاد Background Job
                var job = new MahERP.DataModelLayer.Entities.BackgroundJobs.BackgroundJob
                {
                    JobType = 0, // SMS Bulk Send
                    Title = $"ارسال پیامک به {contactPhoneData.Count} شماره",
                    Description = message.Length > 100 ? message.Substring(0, 100) + "..." : message,
                    Status = 0, // Pending
                    TotalItems = contactPhoneData.Count,
                    CreatedByUserId = currentUser.Id,
                    StartDate = DateTime.Now
                };

                var jobId = await _backgroundJobRepo.CreateJobAsync(job);

                // ⭐ اطلاع‌رسانی شروع Job
                await _backgroundJobNotificationService.NotifyJobStarted(currentUser.Id, jobId, job.Title);

                // ⭐ ارسال در Background (Task.Run)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _backgroundJobRepo.UpdateJobAsync(new MahERP.DataModelLayer.Entities.BackgroundJobs.BackgroundJob
                        {
                            Id = jobId,
                            Status = 1 // Running
                        });

                        int successCount = 0;
                        int failCount = 0;
                        int processed = 0;

                        if (useQueue)
                        {
                            var queueItems = new List<SmsQueue>();
                            
                            foreach (var (contactId, phoneId, phoneNumber) in contactPhoneData)
                            {
                                queueItems.Add(new SmsQueue
                                {
                                    PhoneNumber = phoneNumber,
                                    MessageText = message,
                                    RecipientType = 0,
                                    ContactId = contactId,
                                    ProviderId = providerId,
                                    Priority = 0,
                                    RequestedByUserId = currentUser.Id,
                                    CreatedDate = DateTime.Now
                                });
                                
                                processed++;
                                var progress = (int)((processed / (double)contactPhoneData.Count) * 100);
                                await _backgroundJobRepo.UpdateProgressAsync(jobId, progress, processed, processed, 0);
                                
                                // ⭐ اطلاع‌رسانی پیشرفت
                                await _backgroundJobNotificationService.NotifyJobProgress(currentUser.Id, jobId, progress, processed, processed, 0);
                            }

                            successCount = await _queueRepo.EnqueueBulkAsync(queueItems);
                        }
                        else
                        {
                            foreach (var (contactId, phoneId, phoneNumber) in contactPhoneData)
                            {
                                try
                                {
                                    var log = await _smsService.SendToContactAsync(
                                        contactId,
                                        message,
                                        currentUser.Id,
                                        providerId);
                                    
                                    if (log.IsSuccess)
                                        successCount++;
                                    else
                                        failCount++;
                                }
                                catch
                                {
                                    failCount++;
                                }
                                
                                processed++;
                                var progress = (int)((processed / (double)contactPhoneData.Count) * 100);
                                await _backgroundJobRepo.UpdateProgressAsync(jobId, progress, processed, successCount, failCount);
                                
                                // ⭐ اطلاع‌رسانی پیشرفت
                                await _backgroundJobNotificationService.NotifyJobProgress(currentUser.Id, jobId, progress, processed, successCount, failCount);
                                
                                await Task.Delay(200); // Rate limiting
                            }
                        }

                        await _backgroundJobRepo.CompleteJobAsync(jobId, true);
                        
                        // ⭐ اطلاع‌رسانی تکمیل
                        await _backgroundJobNotificationService.NotifyJobCompleted(currentUser.Id, jobId, true);
                    }
                    catch (Exception ex)
                    {
                        await _backgroundJobRepo.CompleteJobAsync(jobId, false, ex.Message);
                        
                        // ⭐ اطلاع‌رسانی خطا
                        await _backgroundJobNotificationService.NotifyJobCompleted(currentUser.Id, jobId, false, ex.Message);
                        
                        _logger.LogError(ex, "خطا در پردازش Background Job {JobId}", jobId);
                    }
                });

                return Json(new
                {
                    success = true,
                    message = $"ارسال پیامک شروع شد. پیشرفت را در بخش کارهای پس‌زمینه مشاهده کنید.",
                    jobId = jobId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال پیامک به افراد");
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

                // ⭐ ایجاد BackgroundJob برای ارسال انبوه
                var backgroundJob = new MahERP.DataModelLayer.Entities.BackgroundJobs.BackgroundJob
                {
                    JobType = 0, // SMS Bulk Send
                    Title = $"ارسال انبوه پیامک - قالب: {template.Title}",
                    Description = $"ارسال {recipients.Count} پیامک از قالب {template.Title}",
                    Status = 0, // Pending
                    Progress = 0,
                    TotalItems = recipients.Count,
                    ProcessedItems = 0,
                    SuccessCount = 0,
                    FailedCount = 0,
                    StartDate = DateTime.Now,
                    CreatedByUserId = currentUser.Id,
                    Metadata = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        TemplateId = templateId,
                        TemplateName = template.Title,
                        UseQueue = useQueue
                    })
                };

                var jobId = await _backgroundJobRepo.CreateJobAsync(backgroundJob);

                // ⭐ اطلاع‌رسانی شروع Job
                await _backgroundJobNotificationService.NotifyJobStarted(
                    currentUser.Id, 
                    jobId, 
                    backgroundJob.Title
                );

                // ⭐ شروع پردازش در بک‌گراند (استفاده از Task.Run)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessBulkSmsAsync(jobId, recipients, template, useQueue, currentUser.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"خطا در پردازش BackgroundJob {jobId}");
                        await _backgroundJobRepo.CompleteJobAsync(jobId, false, ex.Message);
                        await _backgroundJobNotificationService.NotifyJobCompleted(
                            currentUser.Id, jobId, false, ex.Message);
                    }
                });

                return Json(new
                {
                    success = true,
                    message = $"عملیات ارسال {recipients.Count} پیامک آغاز شد",
                    jobId = jobId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال به مخاطبین قالب");
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ⭐⭐⭐ متد پردازش در بک‌گراند
        private async Task ProcessBulkSmsAsync(
            int jobId,
            List<SmsTemplateRecipient> recipients,
            SmsTemplate template,
            bool useQueue,
            string userId)
        {
            int processed = 0;
            int success = 0;
            int failed = 0;

            foreach (var recipient in recipients)
            {
                try
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

                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        if (useQueue)
                        {
                            await _queueRepo.EnqueueAsync(new SmsQueue
                            {
                                PhoneNumber = phoneNumber,
                                MessageText = template.MessageTemplate,
                                RecipientType = recipient.RecipientType,
                                ContactId = recipient.ContactId,
                                OrganizationId = recipient.OrganizationId,
                                RequestedByUserId = userId
                            });
                            success++;
                        }
                        else
                        {
                            // ارسال مستقیم
                            success++;
                        }
                    }
                    else
                    {
                        failed++;
                    }

                    processed++;

                    // بروزرسانی Progress هر 10 آیتم
                    if (processed % 10 == 0 || processed == recipients.Count)
                    {
                        int progress = (processed * 100) / recipients.Count;
                        await _backgroundJobRepo.UpdateProgressAsync(jobId, progress, processed, success, failed);
                        
                        await _backgroundJobNotificationService.NotifyJobProgress(
                            userId, jobId, progress, processed, success, failed);
                    }

                    // تاخیر کوچک برای جلوگیری از فشار بیش از حد
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"خطا در پردازش مخاطب {recipient.Id}");
                    failed++;
                    processed++;
                }
            }

            // بروزرسانی تعداد استفاده
            await _templateRepo.IncrementUsageCountAsync(template.Id);

            // تکمیل Job
            await _backgroundJobRepo.CompleteJobAsync(jobId, true);
            
            await _backgroundJobNotificationService.NotifyJobCompleted(
                userId, jobId, true, $"{success} موفق، {failed} ناموفق");
        }
    }
}