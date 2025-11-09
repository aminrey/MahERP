using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactGroupRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.CommunicationControllers
{
    /// <summary>
    /// کنترلر ارتباطات - ارسال دسته‌جمعی SMS و Email
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    public class CommunicationController : BaseController
    {
        private readonly IEmailRepository _emailRepository;
        private readonly ISmsService _smsService;
        private readonly IContactGroupRepository _groupRepository; // ⭐ اضافه شده
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly AppDbContext _context; // ⭐ برای دسترسی مستقیم به Contact

        public CommunicationController(
            IEmailRepository emailRepository,
            ISmsService smsService,
            IContactGroupRepository groupRepository, // ⭐ اضافه شده
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            AppDbContext context, // ⭐ اضافه شده
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _emailRepository = emailRepository;
            _smsService = smsService;
            _groupRepository = groupRepository; // ⭐ اضافه شده
            _uow = uow;
            _userManager = userManager;
            _context = context; // ⭐ اضافه شده
        }

        // ==================== BULK SMS ====================

        /// <summary>
        /// ارسال پیامک دسته‌جمعی به افراد (از طریق Contact IDs)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBulkSms(
            List<int> contactIds,
            string message,
            int? branchId = null,
            int? providerId = null)
        {
            try
            {
                if (contactIds == null || !contactIds.Any())
                    return Json(new { success = false, message = "لیست افراد خالی است" });

                if (string.IsNullOrWhiteSpace(message))
                    return Json(new { success = false, message = "متن پیام الزامی است" });

                var userId = GetUserId();
                var logs = await _smsService.SendToMultipleContactsAsync(contactIds, message, userId, providerId);

                var successCount = logs.Count(l => l.IsSuccess);
                var failedCount = logs.Count - successCount;

                // ⭐ تصحیح: استفاده از Create
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create, // ⭐ تغییر از Action به Create
                    "Communication",
                    "SendBulkSms",
                    $"درخواست ارسال پیامک به {contactIds.Count} نفر - موفق: {successCount}, ناموفق: {failedCount}",
                    recordId: branchId?.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount,
                    failedCount,
                    totalSent = logs.Count,
                    message = $"پیامک به {successCount} نفر ارسال شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendBulkSms", "خطا در درخواست ارسال", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ⭐ ارسال پیامک به یک گروه کامل (System Level) - ساده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSmsToGroup(int groupId, string message, int? providerId = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _smsService.SendToContactGroupAsync(groupId, message, userId, providerId);

                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                // ⭐ تصحیح
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Communication",
                    "SendSmsToGroup",
                    $"ارسال پیامک به گروه '{result.GroupTitle}' - {result.SuccessCount}/{result.TotalSent} موفق",
                    recordId: groupId.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    totalSent = result.TotalSent,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendSmsToGroup", "خطا", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ⭐ ارسال پیامک به گروه شعبه (Branch Level) - ساده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSmsToBranchGroup(int branchGroupId, string message, int? providerId = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _smsService.SendToBranchContactGroupAsync(branchGroupId, message, userId, providerId);

                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Communication",
                    "SendSmsToBranchGroup",
                    $"ارسال پیامک به گروه شعبه '{result.GroupTitle}' - {result.SuccessCount}/{result.TotalSent} موفق",
                    recordId: branchGroupId.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    totalSent = result.TotalSent,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendSmsToBranchGroup", "خطا", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ==================== BULK EMAIL ====================

        /// <summary>
        /// ارسال ایمیل دسته‌جمعی به افراد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBulkEmail(
            List<int> contactIds,
            string subject,
            string body,
            int? branchId = null,
            bool isHtml = true)
        {
            try
            {
                if (contactIds == null || !contactIds.Any())
                    return Json(new { success = false, message = "لیست افراد خالی است" });

                if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
                    return Json(new { success = false, message = "موضوع و متن ایمیل الزامی است" });

                var userId = GetUserId();
                var logs = await _emailRepository.SendToMultipleContactsAsync(contactIds, subject, body, userId, isHtml);

                var successCount = logs.Count(l => l.IsSuccess);
                var failedCount = logs.Count - successCount;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Communication",
                    "SendBulkEmail",
                    $"درخواست ارسال ایمیل به {contactIds.Count} نفر - موفق: {successCount}, ناموفق: {failedCount}",
                    recordId: branchId?.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount,
                    failedCount,
                    totalSent = logs.Count,
                    message = $"ایمیل به {successCount} نفر ارسال شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendBulkEmail", "خطا", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ⭐ ارسال ایمیل به یک گروه کامل (System Level) - ساده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailToGroup(int groupId, string subject, string body, bool isHtml = true)
        {
            try
            {
                var userId = GetUserId();
                var result = await _emailRepository.SendToContactGroupAsync(groupId, subject, body, userId, isHtml);

                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Communication",
                    "SendEmailToGroup",
                    $"ارسال ایمیل به گروه '{result.GroupTitle}' - {result.SuccessCount}/{result.TotalSent} موفق",
                    recordId: groupId.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    totalSent = result.TotalSent,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendEmailToGroup", "خطا", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ⭐ ارسال ایمیل به گروه شعبه (Branch Level) - ساده شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailToBranchGroup(int branchGroupId, string subject, string body, bool isHtml = true)
        {
            try
            {
                var userId = GetUserId();
                var result = await _emailRepository.SendToBranchContactGroupAsync(branchGroupId, subject, body, userId, isHtml);

                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Communication",
                    "SendEmailToBranchGroup",
                    $"ارسال ایمیل به گروه شعبه '{result.GroupTitle}' - {result.SuccessCount}/{result.TotalSent} موفق",
                    recordId: branchGroupId.ToString()
                );

                return Json(new
                {
                    success = true,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    totalSent = result.TotalSent,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Communication", "SendEmailToBranchGroup", "خطا", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// دریافت تعداد افراد قابل دسترسی در یک گروه
        /// </summary>
        [HttpGet]
        public IActionResult GetGroupContactsCount(int groupId, bool isBranchGroup = false)
        {
            try
            {
                int count;
                
                if (isBranchGroup)
                {
                    var branchContacts = _groupRepository.GetBranchGroupContacts(
                        groupId, 
                        includeInactive: false
                    );
                    count = branchContacts.Count;
                }
                else
                {
                    var contacts = _groupRepository.GetGroupContacts(
                        groupId, 
                        includeInactive: false
                    );
                    count = contacts.Count;
                }

                return Json(new { success = true, count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// بررسی اعتبار افراد برای ارسال SMS
        /// </summary>
        [HttpPost]
        public IActionResult ValidateContactsForSms(List<int> contactIds)
        {
            try
          
            {
                // ⭐ تصحیح: استفاده مستقیم از DbContext
                var contacts = _context.Contact_Tbl
                    .Include(c => c.Phones.Where(p => p.IsDefault && p.IsActive))
                    .Where(c => contactIds.Contains(c.Id))
                    .Select(c => new
                    {
                        c.Id,
                        HasPhone = c.Phones.Any(p => p.IsDefault && p.IsActive),
                        DefaultPhone = c.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault()
                    })
                    .ToList();

                var validCount = contacts.Count(c => c.HasPhone);
                var invalidCount = contacts.Count - validCount;

                return Json(new
                {
                    success = true,
                    totalContacts = contacts.Count,
                    validCount,
                    invalidCount,
                    invalidContacts = contacts.Where(c => !c.HasPhone).Select(c => c.Id).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// بررسی اعتبار افراد برای ارسال Email
        /// </summary>
        [HttpPost]
        public IActionResult ValidateContactsForEmail(List<int> contactIds)
        {
            try
            {
                // ⭐ تصحیح: استفاده مستقیم از DbContext
                var contacts = _context.Contact_Tbl
                    .Where(c => contactIds.Contains(c.Id))
                    .Select(c => new
                    {
                        c.Id,
                        HasEmail = !string.IsNullOrEmpty(c.PrimaryEmail),
                        c.PrimaryEmail
                    })
                    .ToList();

                var validCount = contacts.Count(c => c.HasEmail);
                var invalidCount = contacts.Count - validCount;

                return Json(new
                {
                    success = true,
                    totalContacts = contacts.Count,
                    validCount,
                    invalidCount,
                    invalidContacts = contacts.Where(c => !c.HasEmail).Select(c => c.Id).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}