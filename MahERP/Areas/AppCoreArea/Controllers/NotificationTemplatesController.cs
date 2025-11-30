using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Notifications;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers
{
    [Area("AppCoreArea")]
    [Authorize(Roles = "Admin")]
    [PermissionRequired("NOTIFICATION.TEMPLATES")]
    public class NotificationTemplatesController : BaseController
    {
        private readonly INotificationTemplateRepository _templateRepo;

        public NotificationTemplatesController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            ModuleTrackingBackgroundService moduleTracking,
            INotificationTemplateRepository templateRepo,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _templateRepo = templateRepo;
        }

        #region لیست الگوها

        /// <summary>
        /// صفحه لیست الگوها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? notificationTypeId = null, byte? channelType = null)
        {
            try
            {
                var viewModel = await _templateRepo.GetTemplateListViewModelAsync(notificationTypeId, channelType);

                ViewBag.SelectedNotificationType = notificationTypeId;
                ViewBag.SelectedChannelType = channelType;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationTemplates",
                    "Index",
                    "مشاهده لیست الگوهای پیام");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Index",
                    "خطا در بارگذاری لیست",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری لیست الگوها";
                return RedirectToAction("Index", "NotificationSettings");
            }
        }

        #endregion

        #region ایجاد الگو

        /// <summary>
        /// نمایش فرم ایجاد الگو
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int? notificationTypeId = null, byte? channelType = null)
        {
            try
            {
                // ⭐⭐⭐ اصلاح: ارسال notificationTypeId به GetTemplateFormViewModelAsync
                var viewModel = await _templateRepo.GetTemplateFormViewModelAsync(
                    templateId: null, 
                    eventType: notificationTypeId.HasValue ? (byte)notificationTypeId.Value : (byte?)null
                );

                // ✅ اصلاح: NotificationEventType به جای NotificationTypeConfigId
                if (notificationTypeId.HasValue)
                    viewModel.NotificationEventType = (byte)notificationTypeId.Value;

                // ✅ اصلاح: Channel به جای ChannelType
                if (channelType.HasValue)
                    viewModel.Channel = channelType.Value;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationTemplates",
                    "Create",
                    "نمایش فرم ایجاد الگو");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Create",
                    "خطا در نمایش فرم",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ذخیره الگوی جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotificationTemplateFormViewModel model)
        {
            try
            {
                // ✅ اصلاح: Channel به جای ChannelType
                if (model.Channel == 1 && string.IsNullOrWhiteSpace(model.Subject))
                {
                    ModelState.AddModelError(nameof(model.Subject), "موضوع ایمیل الزامی است");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new { status = "validation-error", message = errors });
                }

                var currentUserId = GetUserId();

                // بررسی تکراری
                if (!string.IsNullOrEmpty(model.TemplateCode))
                {
                    var exists = await _templateRepo.GetTemplateByCodeAsync(model.TemplateCode);
                    if (exists != null)
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "error", text = "کد الگو تکراری است" } }
                        });
                    }
                }

                // ✅ اصلاح: تطابق کامل با Entity جدید
                var template = new NotificationTemplate
                {
                    TemplateName = model.TemplateName,
                    TemplateCode = model.TemplateCode,
                    Description = model.Description,

                    // ✅ فیلدهای جدید
                    NotificationEventType = model.NotificationEventType,
                    Channel = model.Channel,

                    Subject = model.Subject,
                    MessageTemplate = model.MessageTemplate,
                    BodyHtml = model.BodyHtml,

                    RecipientMode = model.RecipientMode,
                    IsActive = model.IsActive,
                    IsSystemTemplate = false,
                    
                    // ⭐⭐⭐ فیلدهای زمان‌بندی
                    IsScheduled = model.IsScheduled,
                    ScheduleType = model.ScheduleType,
                    ScheduledTime = model.ScheduledTime,
                    ScheduledDaysOfWeek = model.ScheduledDaysOfWeek,
                    ScheduledDayOfMonth = model.ScheduledDayOfMonth,
                    IsScheduleEnabled = model.IsScheduled,
                    
                    CreatedByUserId = currentUserId,
                    CreatedDate = DateTime.Now
                };

                var templateId = await _templateRepo.CreateTemplateAsync(template, currentUserId);

                // ✅ ذخیره دریافت‌کنندگان
                if (model.RecipientMode != 0 && model.SelectedUserIds?.Any() == true)
                {
                    foreach (var userId in model.SelectedUserIds)
                    {
                        await _templateRepo.AddRecipientAsync(
                            templateId,
                            recipientType: 2, // User
                            contactId: null,
                            organizationId: null,
                            userId: userId,
                            currentUserId: currentUserId // ✅ اصلاح: named parameter
                        );
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "NotificationTemplates",
                    "Create",
                    $"ایجاد الگوی جدید: {model.TemplateName}",
                    recordId: templateId.ToString());

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "NotificationTemplates", new { area = "AppCoreArea" }),
                    message = new[] { new { status = "success", text = "الگو با موفقیت ایجاد شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Create",
                    "خطا در ایجاد الگو",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا در ایجاد الگو: {ex.Message}" } }
                });
            }
        }

        #endregion

        #region ویرایش الگو

        /// <summary>
        /// نمایش فرم ویرایش الگو
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var viewModel = await _templateRepo.GetTemplateFormViewModelAsync(id);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "الگو یافت نشد";
                    return RedirectToAction("Index");
                }

                // ✅ بررسی اینکه الگوی سیستمی است یا خیر
                var template = await _templateRepo.GetTemplateByIdAsync(id);
                if (template != null && template.IsSystemTemplate)
                {
                    TempData["WarningMessage"] = "الگوهای سیستمی قابل ویرایش نیستند";
                    return RedirectToAction("Index");
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationTemplates",
                    "Edit",
                    $"نمایش فرم ویرایش: {viewModel.TemplateName}",
                    recordId: id.ToString());

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Edit",
                    "خطا در نمایش فرم",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ذخیره ویرایش الگو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NotificationTemplateFormViewModel model, string changeNote)
        {
            try
            {
                // ✅ اصلاح: Channel به جای ChannelType
                if (model.Channel == 1 && string.IsNullOrWhiteSpace(model.Subject))
                {
                    ModelState.AddModelError(nameof(model.Subject), "موضوع ایمیل الزامی است");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => new { status = "error", text = e.ErrorMessage })
                        .ToArray();

                    return Json(new { status = "validation-error", message = errors });
                }

                var currentUserId = GetUserId();

                // ✅ اصلاح: model.Id از نوع int است (nullable نیست)
                var template = new NotificationTemplate
                {
                    Id = model.Id, // ✅ حذف .Value
                    TemplateName = model.TemplateName,
                    TemplateCode = model.TemplateCode,
                    Description = model.Description,

                    // ✅ فیلدهای جدید
                    NotificationEventType = model.NotificationEventType,
                    Channel = model.Channel,

                    Subject = model.Subject,
                    MessageTemplate = model.MessageTemplate, // ✅ به جای Body
                    BodyHtml = model.BodyHtml,

                    RecipientMode = model.RecipientMode,
                    IsActive = model.IsActive,
                    
                    // ⭐⭐⭐ فیلدهای زمان‌بندی
                    IsScheduled = model.IsScheduled,
                    ScheduleType = model.ScheduleType,
                    ScheduledTime = model.ScheduledTime,
                    ScheduledDaysOfWeek = model.ScheduledDaysOfWeek,
                    ScheduledDayOfMonth = model.ScheduledDayOfMonth,
                    IsScheduleEnabled = model.IsScheduled, // وقتی IsScheduled فعال است، IsScheduleEnabled هم فعال است
                    
                    LastModifiedDate = DateTime.Now,
                    LastModifiedByUserId = currentUserId
                };

                var result = await _templateRepo.UpdateTemplateAsync(template, currentUserId, changeNote);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در بروزرسانی" } }
                    });
                }

                // بروزرسانی دریافت‌کنندگان
                await _templateRepo.UpdateRecipientsAsync(model.Id, model.SelectedUserIds, currentUserId); // ✅ حذف .Value

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationTemplates",
                    "Edit",
                    $"ویرایش الگو: {model.TemplateName}",
                    recordId: model.Id.ToString());

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "NotificationTemplates", new { area = "AppCoreArea" }),
                    message = new[] { new { status = "success", text = "الگو با موفقیت بروزرسانی شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Edit",
                    "خطا در ویرایش",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا در ویرایش الگو: {ex.Message}" } }
                });
            }
        }

        #endregion

        #region حذف و Toggle

        /// <summary>
        /// حذف الگو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);

                if (template == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "الگو یافت نشد" } }
                    });
                }

                if (template.IsSystemTemplate)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "الگوهای سیستمی قابل حذف نیستند" } }
                    });
                }

                var result = await _templateRepo.DeleteTemplateAsync(id);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در حذف" } }
                    });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "NotificationTemplates",
                    "Delete",
                    $"حذف الگو: {template.TemplateName}",
                    recordId: id.ToString());

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "الگو با موفقیت حذف شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Delete",
                    "خطا در حذف",
                    ex);

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف الگو" } }
                });
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن الگو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);

                if (template == null)
                {
                    return Json(new { success = false, message = "الگو یافت نشد" });
                }

                var newStatus = !template.IsActive;
                var result = await _templateRepo.ToggleTemplateStatusAsync(id, newStatus);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در تغییر وضعیت" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "NotificationTemplates",
                    "ToggleStatus",
                    $"{(newStatus ? "فعال" : "غیرفعال")} کردن: {template.TemplateName}",
                    recordId: id.ToString());

                return Json(new
                {
                    success = true,
                    isActive = newStatus,
                    message = $"الگو با موفقیت {(newStatus ? "فعال" : "غیرفعال")} شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "ToggleStatus",
                    "خطا در تغییر وضعیت",
                    ex);

                return Json(new { success = false, message = "خطا در تغییر وضعیت" });
            }
        }

        #endregion

        #region پیش‌نمایش

        /// <summary>
        /// پیش‌نمایش الگو
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);

                if (template == null)
                {
                    return NotFound();
                }

                var previewContent = await _templateRepo.PreviewTemplateAsync(id);

                var viewModel = new NotificationTemplatePreviewViewModel
                {
                    TemplateId = template.Id,
                    TemplateName = template.TemplateName,
                    ChannelType = template.Channel, // ✅ اصلاح
                    Subject = template.Subject,
                    Body = template.MessageTemplate, // ✅ اصلاح
                    BodyHtml = template.BodyHtml,
                    PreviewContent = previewContent,
                    SampleData = new Dictionary<string, string>
                    {
                        { "UserName", "احمد محمدی" },
                        { "TaskTitle", "تسک نمونه" },
                        { "TaskCode", "T-001" },
                        { "DueDate", "1403/08/20" },
                        { "Title", "اعلان نمونه" },
                        { "Message", "این یک پیام تستی است" },
                        { "ActionUrl", "/TaskingArea/Tasks/Details/1" },
                        { "Date", DateTime.Now.ToString("yyyy/MM/dd") },
                        { "Time", DateTime.Now.ToString("HH:mm") }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationTemplates",
                    "Preview",
                    $"پیش‌نمایش: {template.TemplateName}",
                    recordId: id.ToString());

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "Preview",
                    "خطا در پیش‌نمایش",
                    ex);

                TempData["ErrorMessage"] = "خطا در پیش‌نمایش الگو";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region تاریخچه

        /// <summary>
        /// نمایش تاریخچه تغییرات الگو
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    TempData["ErrorMessage"] = "الگو یافت نشد";
                    return RedirectToAction("Index");
                }

                var history = await _templateRepo.GetTemplateHistoryAsync(id);

                ViewBag.Template = template;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "NotificationTemplates",
                    "History",
                    $"مشاهده تاریخچه: {template.TemplateName}",
                    recordId: id.ToString());

                return View(history);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "History",
                    "خطا در نمایش تاریخچه",
                    ex);

                TempData["ErrorMessage"] = "خطا در نمایش تاریخچه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region ⭐⭐⭐ API: دریافت متغیرها بر اساس EventType

        /// <summary>
        /// API: دریافت متغیرهای فیلتر شده بر اساس EventType
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVariablesForEventType(byte eventType)
        {
            try
            {
                var variables = await _templateRepo.GetVariablesForEventTypeAsync(eventType);

                return Json(new
                {
                    success = true,
                    variables = variables
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "GetVariablesForEventType",
                    "خطا در دریافت متغیرها",
                    ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        #endregion

        #region ⭐⭐⭐ ارسال دستی قالب‌های زمان‌بندی شده

        /// <summary>
        /// دریافت لیست قالب‌های زمان‌بندی شده برای ارسال دستی
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ⭐⭐⭐ اضافه کردن برای تست - بعداً حذف کنید
        public async Task<IActionResult> GetScheduledTemplatesForManualSend()
        {
            try
            {
                var templates = await _templateRepo.GetAllTemplatesAsync();
                
                // ⭐ فقط قالب‌های زمان‌بندی شده و فعال
                var scheduledTemplates = templates
                    .Where(t => t.IsScheduled && t.IsActive)
                    .Select(t => new
                    {
                        id = t.Id,
                        templateName = t.TemplateName,
                        description = t.Description,
                        channel = t.Channel,
                        channelName = t.ChannelName,
                        scheduleType = t.ScheduleType,
                        scheduleTypeName = t.ScheduleTypeName,
                        scheduledTime = t.ScheduledTime,
                        subject = t.Subject,
                        messageTemplate = t.MessageTemplate
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    templates = scheduledTemplates
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "GetScheduledTemplatesForManualSend",
                    "خطا در دریافت قالب‌ها",
                    ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک قالب خاص
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ⭐⭐⭐ اضافه کردن برای تست - بعداً حذف کنید
        public async Task<IActionResult> GetTemplateById(int id)
        {
            try
            {
                var template = await _templateRepo.GetTemplateByIdAsync(id);

                if (template == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "قالب یافت نشد"
                    });
                }

                return Json(new
                {
                    success = true,
                    template = new
                    {
                        id = template.Id,
                        templateName = template.TemplateName,
                        description = template.Description,
                        channel = template.Channel,
                        channelName = template.ChannelName,
                        subject = template.Subject,
                        messageTemplate = template.MessageTemplate,
                        bodyHtml = template.BodyHtml
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "GetTemplateById",
                    "خطا در دریافت قالب",
                    ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        #endregion

        #region ⭐⭐⭐ تست و دیباگ زمان‌بندی

        /// <summary>
        /// صفحه تست سیستم زمان‌بندی
        /// </summary>
        [HttpGet]
        public IActionResult TestScheduling()
        {
            return View();
        }

        /// <summary>
        /// دریافت وضعیت قالب‌های زمان‌بندی شده
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetScheduledTemplatesStatus()
        {
            try
            {
                var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, iranTimeZone);

                var templates = await _templateRepo.GetAllTemplatesAsync();
                var scheduledTemplates = templates.Where(t => t.IsScheduled).ToList();

                var result = new
                {
                    currentTimeUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    currentTimeIran = nowIran.ToString("yyyy-MM-dd HH:mm:ss"),
                    templates = scheduledTemplates.Select(t => new
                    {
                        id = t.Id,
                        templateName = t.TemplateName,
                        scheduleType = t.ScheduleType,
                        scheduleTypeName = t.ScheduleTypeName,
                        scheduledTime = t.ScheduledTime,
                        scheduledDaysOfWeek = t.ScheduledDaysOfWeek,
                        scheduledDayOfMonth = t.ScheduledDayOfMonth,
                        lastExecutionDate = t.LastExecutionDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        nextExecutionDate = t.NextExecutionDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        isActive = t.IsActive,
                        isScheduleEnabled = t.IsScheduleEnabled,
                        isDue = t.IsDueForExecution
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "GetScheduledTemplatesStatus",
                    "خطا در دریافت وضعیت",
                    ex);

                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// اجرای دستی قالب‌های زمان‌بندی شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteScheduledTemplatesManually()
        {
            try
            {
                // ⚠️ فقط برای تست - در محیط واقعی از Hangfire یا Background Service استفاده شود
                var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, iranTimeZone);

                var templates = await _templateRepo.GetAllTemplatesAsync();
                var dueTemplates = templates.Where(t => t.IsDueForExecution).ToList();

                if (!dueTemplates.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "هیچ قالب آماده‌ای برای اجرا وجود ندارد"
                    });
                }

                var executedCount = 0;
                foreach (var template in dueTemplates)
                {
                    try
                    {
                        // ⚠️ اینجا باید NotificationManagementService صدا زده شود
                        // برای تست، فقط NextExecutionDate را بروزرسانی می‌کنیم
                        
                        template.LastExecutionDate = nowIran;
                        template.UsageCount++;
                        template.LastUsedDate = nowIran;

                        // محاسبه زمان بعدی (شبیه‌سازی)
                        template.NextExecutionDate = CalculateNextExecution(template, nowIran);

                        await _templateRepo.UpdateTemplateAsync(template, GetUserId(), "اجرای دستی");

                        executedCount++;
                    }
                    catch (Exception ex)
                    {
                        await _activityLogger.LogErrorAsync(
                            "NotificationTemplates",
                            "ExecuteScheduledTemplatesManually",
                            $"خطا در اجرای {template.TemplateName}",
                            ex);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"{executedCount} قالب با موفقیت اجرا شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "NotificationTemplates",
                    "ExecuteScheduledTemplatesManually",
                    "خطا در اجرای دستی",
                    ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// محاسبه زمان اجرای بعدی (helper method)
        /// </summary>
        private DateTime? CalculateNextExecution(NotificationTemplate template, DateTime now)
        {
            if (string.IsNullOrEmpty(template.ScheduledTime))
                return null;

            var timeParts = template.ScheduledTime.Split(':');
            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out int hour) ||
                !int.TryParse(timeParts[1], out int minute))
            {
                return null;
            }

            switch (template.ScheduleType)
            {
                case 1: // روزانه
                    var nextDaily = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
                    return nextDaily <= now ? nextDaily.AddDays(1) : nextDaily;

                case 2: // هفتگی
                    // ساده‌سازی: 7 روز بعد
                    return new DateTime(now.Year, now.Month, now.Day, hour, minute, 0).AddDays(7);

                case 3: // ماهانه
                    // ساده‌سازی: 30 روز بعد
                    return new DateTime(now.Year, now.Month, now.Day, hour, minute, 0).AddDays(30);

                default:
                    return null;
            }
        }

        #endregion
    }
}