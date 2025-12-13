using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Services;
using MahERP.CommonLayer.Repository;
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
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly TelegramBotSendNotification _telegramService;

        public SettingsController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository, 
            IModuleTrackingService moduleTracking, 
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _telegramService = new TelegramBotSendNotification();
        }

        // ============== تنظیمات ماژول‌ها ==============
        public IActionResult ModuleSettings()
        {
            var settings = _baseRepository.GetSystemSettings();
            return View(settings);
        }

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
                _baseRepository.ClearSettingsCache();

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

        // ============== تنظیمات سیستم اعلان‌رسانی ==============
        
        /// <summary>
        /// صفحه تنظیمات سیستم (شامل تلگرام، SMTP، SMS)
        /// </summary>
        [HttpGet]
        public IActionResult SystemSettings()
        {
            var settings = _uow.SettingsUW.Get().FirstOrDefault() ?? new Settings();
            return View(settings);
        }

        /// <summary>
        /// بروزرسانی تنظیمات سیستم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSystemSettings(Settings model)
        {
            try
            {
                var settings = _uow.SettingsUW.Get().FirstOrDefault();

                if (settings == null)
                {
                    settings = new Settings();
                    _uow.SettingsUW.Create(settings);
                }

                // ============== TELEGRAM SETTINGS ==============
                settings.TelegramBotToken = model.TelegramBotToken;
                settings.IsTelegramEnabled = model.IsTelegramEnabled;
                settings.TelegramSystemLogGroupId = model.TelegramSystemLogGroupId;

                // ============== SMTP SETTINGS ==============
                settings.SmtpHost = model.SmtpHost;
                settings.SmtpPort = model.SmtpPort;
                settings.SmtpEnableSsl = model.SmtpEnableSsl;
                settings.SmtpUsername = model.SmtpUsername;
                
                // فقط اگر رمز جدید وارد شده باشد
                if (!string.IsNullOrWhiteSpace(model.SmtpPassword))
                {
                    settings.SmtpPassword = model.SmtpPassword;
                }

                settings.SmtpFromEmail = model.SmtpFromEmail;
                settings.SmtpFromName = model.SmtpFromName;
                settings.MaxAttachmentSizeMB = model.MaxAttachmentSizeMB;

                // ============== AUDIT ==============
                settings.LastModified = DateTime.Now;
                settings.LastModifiedByUserId = GetUserId();

                _uow.Save();
                _baseRepository.ClearSettingsCache();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Settings,
                    "UpdateSystemSettings",
                    "بروزرسانی تنظیمات سیستم اعلان‌رسانی",
                    GetUserId()
                );

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "تنظیمات با موفقیت ذخیره شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Settings",
                    "UpdateSystemSettings",
                    "خطا در بروزرسانی تنظیمات",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }

        /// <summary>
        /// تست کانکشن تلگرام
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestTelegramConnection(string botToken, long? testChatId)
        {
            if (string.IsNullOrWhiteSpace(botToken))
            {
                return Json(new
                {
                    success = false,
                    message = "لطفاً توکن ربات را وارد کنید"
                });
            }

            try
            {
                // اگر Chat ID وارد شده باشد، به آن ارسال کن
                if (testChatId.HasValue)
                {
                    // ارسال پیام تستی
                    string testMessage = $"✅ *تست کانکشن موفق*\n\n" +
                                       $"🤖 سیستم MahERP\n" +
                                       $"📅 {DateTime.Now:yyyy/MM/dd HH:mm}\n" +
                                       $"👤 توسط: {User.Identity.Name}";

                    await _telegramService.SendNotificationAsync(
                        testMessage,
                        testChatId.Value,
                        botToken
                    );

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Settings,
                        "TestTelegramConnection",
                        "تست موفق کانکشن تلگرام",
                        GetUserId()
                    );

                    return Json(new
                    {
                        success = true,
                        message = "✅ پیام تستی با موفقیت ارسال شد!"
                    });
                }
                else
                {
                    // برای تست بدون Chat ID، می‌خواهیم فقط توکن را validate کنیم
                    // ولی چون API محدودیت دارد، پیام هشدار می‌دهیم
                    return Json(new
                    {
                        success = false,
                        message = "⚠️ برای تست کانکشن، لطفاً Chat ID خود را وارد کنید"
                    });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Settings",
                    "TestTelegramConnection",
                    "خطا در تست کانکشن تلگرام",
                    ex
                );

                return Json(new
                {
                    success = false,
                    message = $"❌ خطا در اتصال: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// تست کانکشن SMTP
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestSmtpConnection(
            string smtpHost,
            int smtpPort,
            bool smtpEnableSsl,
            string smtpUsername,
            string smtpPassword,
            string smtpFromEmail,
            string testEmail)
        {
            if (string.IsNullOrWhiteSpace(smtpHost) || 
                string.IsNullOrWhiteSpace(smtpUsername) ||
                string.IsNullOrWhiteSpace(testEmail))
            {
                return Json(new
                {
                    success = false,
                    message = "لطفاً تمام فیلدهای الزامی را پر کنید"
                });
            }

            try
            {
                using var smtpClient = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = smtpEnableSsl,
                    Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                    Timeout = 10000
                };

                var message = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(smtpFromEmail ?? smtpUsername, "MahERP"),
                    Subject = "تست کانکشن SMTP - MahERP",
                    Body = $"<h3>✅ تست کانکشن موفق!</h3>" +
                          $"<p>این ایمیل از سیستم MahERP ارسال شده است.</p>" +
                          $"<p>📅 {DateTime.Now:yyyy/MM/dd HH:mm}</p>" +
                          $"<p>👤 توسط: {User.Identity.Name}</p>",
                    IsBodyHtml = true
                };

                message.To.Add(testEmail);

                await smtpClient.SendMailAsync(message);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Settings,
                    "TestSmtpConnection",
                    $"تست موفق کانکشن SMTP به {testEmail}",
                    GetUserId()
                );

                return Json(new
                {
                    success = true,
                    message = $"✅ ایمیل تستی با موفقیت به {testEmail} ارسال شد!"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Settings",
                    "TestSmtpConnection",
                    "خطا در تست SMTP",
                    ex
                );

                return Json(new
                {
                    success = false,
                    message = $"❌ خطا: {ex.Message}"
                });
            }
        }
    }
}