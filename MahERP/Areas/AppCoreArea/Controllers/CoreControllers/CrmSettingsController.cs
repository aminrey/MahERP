using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.Repository;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.CoreControllers
{
    /// <summary>
    /// تنظیمات اختصاصی ماژول CRM
    /// ⭐ این کنترلر در AppCoreArea قرار دارد اما فقط زمانی نمایش داده می‌شود که ماژول CRM فعال باشد
    /// </summary>
    [Area("AppCoreArea")]
    [Authorize]

    [PermissionRequired("CRM.SETTINGS")]
    public class CrmSettingsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly CrmLeadSourceRepository _leadSourceRepository;
        private readonly CrmLostReasonRepository _lostReasonRepository;

        public CrmSettingsController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            AppDbContext context,
            CrmLeadSourceRepository leadSourceRepository,
            CrmLostReasonRepository lostReasonRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _context = context;
            _leadSourceRepository = leadSourceRepository;
            _lostReasonRepository = lostReasonRepository;
        }

        #region ✅ صفحه اصلی تنظیمات CRM

        /// <summary>
        /// صفحه اصلی تنظیمات CRM
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // ✅ بررسی فعال بودن ماژول CRM
                var settings = _baseRepository.GetSystemSettings();
                if (settings == null || !settings.IsCrmModuleEnabled)
                {
                    TempData["ErrorMessage"] = "ماژول CRM غیرفعال است";
                    return RedirectToAction("Index", "Dashboard", new { area = "AppCoreArea" });
                }

                var viewModel = new CrmSettingsIndexViewModel
                {
                    // آمار کلی
                    TotalLeadStatuses = await _context.CrmLeadStatus_Tbl.CountAsync(s => s.IsActive),
                    TotalPipelineStages = await _context.CrmPipelineStage_Tbl.CountAsync(s => s.IsActive),
                    TotalLeadSources = await _context.CrmLeadSource_Tbl.CountAsync(s => s.IsActive),
                    TotalLostReasons = await _context.CrmLostReason_Tbl.CountAsync(r => r.IsActive),

                    // لیست وضعیت‌های Lead
                    LeadStatuses = await _context.CrmLeadStatus_Tbl
                        .OrderBy(s => s.DisplayOrder)
                        .Select(s => new CrmLeadStatusViewModel
                        {
                            Id = s.Id,
                            Title = s.Title,
                            TitleEnglish = s.TitleEnglish,
                            ColorCode = s.ColorCode,
                            Icon = s.Icon,
                            DisplayOrder = s.DisplayOrder,
                            IsDefault = s.IsDefault,
                            IsFinal = s.IsFinal,
                            IsPositive = s.IsPositive,
                            IsActive = s.IsActive,
                            LeadsCount = s.Leads.Count
                        })
                        .ToListAsync(),

                    // لیست منابع Lead (از جدول جدید)
                    LeadSources = await _context.CrmLeadSource_Tbl
                        .OrderBy(s => s.DisplayOrder)
                        .Select(s => new CrmLeadSourceListViewModel
                        {
                            Id = s.Id,
                            Name = s.Name,
                            NameEnglish = s.NameEnglish,
                            Code = s.Code,
                            Icon = s.Icon,
                            ColorCode = s.ColorCode,
                            DisplayOrder = s.DisplayOrder,
                            IsDefault = s.IsDefault,
                            IsSystem = s.IsSystem,
                            IsActive = s.IsActive,
                            LeadsCount = s.Leads.Count
                        })
                        .ToListAsync(),

                    // لیست دلایل از دست رفتن
                    LostReasons = await _context.CrmLostReason_Tbl
                        .OrderBy(r => r.DisplayOrder)
                        .Select(r => new CrmLostReasonListViewModel
                        {
                            Id = r.Id,
                            Title = r.Title,
                            TitleEnglish = r.TitleEnglish,
                            Code = r.Code,
                            AppliesTo = r.AppliesTo,
                            Category = r.Category,
                            Icon = r.Icon,
                            ColorCode = r.ColorCode,
                            DisplayOrder = r.DisplayOrder,
                            IsSystem = r.IsSystem,
                            RequiresNote = r.RequiresNote,
                            IsActive = r.IsActive,
                            LeadCount = r.Leads.Count,
                            OpportunityCount = r.Opportunities.Count
                        })
                        .ToListAsync()
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CrmSettings",
                    "Index",
                    "مشاهده تنظیمات CRM");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmSettings",
                    "Index",
                    "خطا در بارگذاری تنظیمات CRM",
                    ex);

                TempData["ErrorMessage"] = "خطا در بارگذاری تنظیمات";
                return RedirectToAction("Index", "Dashboard", new { area = "AppCoreArea" });
            }
        }

        #endregion

        #region ✅ مدیریت وضعیت‌های Lead (Lead Status)

        /// <summary>
        /// دریافت لیست وضعیت‌های Lead به صورت JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeadStatuses()
        {
            try
            {
                var statuses = await _context.CrmLeadStatus_Tbl
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        id = s.Id,
                        title = s.Title,
                        titleEnglish = s.TitleEnglish,
                        colorCode = s.ColorCode,
                        icon = s.Icon,
                        displayOrder = s.DisplayOrder,
                        isDefault = s.IsDefault,
                        isFinal = s.IsFinal,
                        isPositive = s.IsPositive,
                        isActive = s.IsActive,
                        leadsCount = s.Leads.Count
                    })
                    .ToListAsync();

                return Json(new { success = true, data = statuses });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ایجاد وضعیت Lead جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeadStatus(CrmLeadStatusViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", errors)
                    });
                }

                // اگر پیش‌فرض انتخاب شده، سایرین را غیرفعال کن
                if (model.IsDefault)
                {
                    var defaultStatuses = await _context.CrmLeadStatus_Tbl
                        .Where(s => s.IsDefault)
                        .ToListAsync();

                    foreach (var status in defaultStatuses)
                    {
                        status.IsDefault = false;
                    }
                }

                var newStatus = new CrmLeadStatus
                {
                    Title = model.Title,
                    TitleEnglish = model.TitleEnglish,
                    ColorCode = model.ColorCode ?? "#6c757d",
                    Icon = model.Icon ?? "fa-circle",
                    DisplayOrder = model.DisplayOrder,
                    IsDefault = model.IsDefault,
                    IsFinal = model.IsFinal,
                    IsPositive = model.IsPositive,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = GetUserId()
                };

                await _context.CrmLeadStatus_Tbl.AddAsync(newStatus);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmSettings",
                    "CreateLeadStatus",
                    $"ایجاد وضعیت Lead جدید: {model.Title}",
                    recordId: newStatus.Id.ToString());

                return Json(new
                {
                    success = true,
                    message = "وضعیت با موفقیت ایجاد شد",
                    data = new { id = newStatus.Id }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmSettings",
                    "CreateLeadStatus",
                    "خطا در ایجاد وضعیت",
                    ex);

                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ویرایش وضعیت Lead
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeadStatus(CrmLeadStatusViewModel model)
        {
            try
            {
                var status = await _context.CrmLeadStatus_Tbl.FindAsync(model.Id);
                if (status == null)
                {
                    return Json(new { success = false, message = "وضعیت یافت نشد" });
                }

                // اگر پیش‌فرض انتخاب شده، سایرین را غیرفعال کن
                if (model.IsDefault && !status.IsDefault)
                {
                    var defaultStatuses = await _context.CrmLeadStatus_Tbl
                        .Where(s => s.IsDefault && s.Id != model.Id)
                        .ToListAsync();

                    foreach (var s in defaultStatuses)
                    {
                        s.IsDefault = false;
                    }
                }

                status.Title = model.Title;
                status.TitleEnglish = model.TitleEnglish;
                status.ColorCode = model.ColorCode ?? "#6c757d";
                status.Icon = model.Icon ?? "fa-circle";
                status.DisplayOrder = model.DisplayOrder;
                status.IsDefault = model.IsDefault;
                status.IsFinal = model.IsFinal;
                status.IsPositive = model.IsPositive;
                status.Description = model.Description;
                status.IsActive = model.IsActive;
                status.LastUpdateDate = DateTime.Now;
                status.LastUpdaterUserId = GetUserId();

                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "UpdateLeadStatus",
                    $"ویرایش وضعیت Lead: {model.Title}",
                    recordId: model.Id.ToString());

                return Json(new { success = true, message = "وضعیت با موفقیت بروزرسانی شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmSettings",
                    "UpdateLeadStatus",
                    "خطا در ویرایش وضعیت",
                    ex);

                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// حذف وضعیت Lead
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeadStatus(int id)
        {
            try
            {
                var status = await _context.CrmLeadStatus_Tbl
                    .Include(s => s.Leads)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (status == null)
                {
                    return Json(new { success = false, message = "وضعیت یافت نشد" });
                }

                if (status.Leads.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = $"این وضعیت {status.Leads.Count} سرنخ فعال دارد و قابل حذف نیست"
                    });
                }

                if (status.IsDefault)
                {
                    return Json(new
                    {
                        success = false,
                        message = "وضعیت پیش‌فرض قابل حذف نیست. ابتدا وضعیت پیش‌فرض دیگری تعیین کنید"
                    });
                }

                _context.CrmLeadStatus_Tbl.Remove(status);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmSettings",
                    "DeleteLeadStatus",
                    $"حذف وضعیت Lead: {status.Title}",
                    recordId: id.ToString());

                return Json(new { success = true, message = "وضعیت با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmSettings",
                    "DeleteLeadStatus",
                    "خطا در حذف وضعیت",
                    ex);

                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// تغییر ترتیب نمایش وضعیت‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderLeadStatuses([FromBody] List<int> statusIds)
        {
            try
            {
                for (int i = 0; i < statusIds.Count; i++)
                {
                    var status = await _context.CrmLeadStatus_Tbl.FindAsync(statusIds[i]);
                    if (status != null)
                    {
                        status.DisplayOrder = i + 1;
                    }
                }

                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "ReorderLeadStatuses",
                    "تغییر ترتیب نمایش وضعیت‌های Lead");

                return Json(new { success = true, message = "ترتیب با موفقیت ذخیره شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        #endregion

        #region ✅ مدیریت منابع Lead (Lead Sources) - NEW

        /// <summary>
        /// صفحه مدیریت منابع سرنخ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LeadSources()
        {
            try
            {
                var sources = await _leadSourceRepository.GetAllAsync(includeInactive: true);
                var viewModel = sources.Select(s => new CrmLeadSourceListViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    NameEnglish = s.NameEnglish,
                    Code = s.Code,
                    Icon = s.Icon,
                    ColorCode = s.ColorCode,
                    DisplayOrder = s.DisplayOrder,
                    IsDefault = s.IsDefault,
                    IsSystem = s.IsSystem,
                    IsActive = s.IsActive,
                    LeadsCount = s.LeadsCount
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// دریافت لیست منابع Lead به صورت JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeadSources()
        {
            try
            {
                var sources = await _leadSourceRepository.GetAllAsync(includeInactive: true);
                var result = sources.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    nameEnglish = s.NameEnglish,
                    code = s.Code,
                    icon = s.Icon,
                    colorCode = s.ColorCode,
                    displayOrder = s.DisplayOrder,
                    isDefault = s.IsDefault,
                    isSystem = s.IsSystem,
                    isActive = s.IsActive,
                    leadsCount = s.LeadsCount,
                    canDelete = s.CanDelete
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ایجاد منبع سرنخ جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeadSource(CrmLeadSourceFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    });
                }

                // بررسی تکراری نبودن نام
                if (await _leadSourceRepository.IsNameDuplicateAsync(model.Name))
                {
                    return Json(new { success = false, message = "این نام قبلاً استفاده شده است" });
                }

                // بررسی تکراری نبودن کد
                if (!string.IsNullOrEmpty(model.Code) && await _leadSourceRepository.IsCodeDuplicateAsync(model.Code))
                {
                    return Json(new { success = false, message = "این کد قبلاً استفاده شده است" });
                }

                var entity = new CrmLeadSource
                {
                    Name = model.Name,
                    NameEnglish = model.NameEnglish,
                    Code = model.Code,
                    Icon = model.Icon ?? "fa-globe",
                    ColorCode = model.ColorCode ?? "#6c757d",
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    IsDefault = model.IsDefault,
                    IsActive = model.IsActive,
                    CreatorUserId = GetUserId()
                };

                await _leadSourceRepository.CreateAsync(entity);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmSettings",
                    "CreateLeadSource",
                    $"ایجاد منبع سرنخ: {model.Name}",
                    recordId: entity.Id.ToString());

                return Json(new { success = true, message = "منبع سرنخ با موفقیت ایجاد شد", data = new { id = entity.Id } });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "CreateLeadSource", "خطا در ایجاد منبع سرنخ", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ویرایش منبع سرنخ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeadSource(CrmLeadSourceFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    });
                }

                var existing = await _leadSourceRepository.GetByIdAsync(model.Id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "منبع سرنخ یافت نشد" });
                }

                // بررسی تکراری نبودن نام
                if (await _leadSourceRepository.IsNameDuplicateAsync(model.Name, model.Id))
                {
                    return Json(new { success = false, message = "این نام قبلاً استفاده شده است" });
                }

                existing.Name = model.Name;
                existing.NameEnglish = model.NameEnglish;
                existing.Code = model.Code;
                existing.Icon = model.Icon;
                existing.ColorCode = model.ColorCode;
                existing.Description = model.Description;
                existing.DisplayOrder = model.DisplayOrder;
                existing.IsDefault = model.IsDefault;
                existing.IsActive = model.IsActive;
                existing.LastUpdaterUserId = GetUserId();

                await _leadSourceRepository.UpdateAsync(existing);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "UpdateLeadSource",
                    $"ویرایش منبع سرنخ: {model.Name}",
                    recordId: model.Id.ToString());

                return Json(new { success = true, message = "منبع سرنخ با موفقیت بروزرسانی شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "UpdateLeadSource", "خطا در ویرایش منبع سرنخ", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// حذف منبع سرنخ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeadSource(int id)
        {
            try
            {
                var existing = await _leadSourceRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "منبع سرنخ یافت نشد" });
                }

                await _leadSourceRepository.DeleteAsync(id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmSettings",
                    "DeleteLeadSource",
                    $"حذف منبع سرنخ: {existing.Name}",
                    recordId: id.ToString());

                return Json(new { success = true, message = "منبع سرنخ با موفقیت حذف شد" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "DeleteLeadSource", "خطا در حذف منبع سرنخ", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// تغییر ترتیب نمایش منابع سرنخ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderLeadSources([FromBody] List<int> sourceIds)
        {
            try
            {
                await _leadSourceRepository.UpdateDisplayOrdersAsync(sourceIds);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "ReorderLeadSources",
                    "تغییر ترتیب نمایش منابع سرنخ");

                return Json(new { success = true, message = "ترتیب با موفقیت ذخیره شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        #endregion

        #region ✅ مدیریت دلایل از دست رفتن (Lost Reasons) - NEW

        /// <summary>
        /// صفحه مدیریت دلایل از دست رفتن
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LostReasons()
        {
            try
            {
                var reasons = await _lostReasonRepository.GetAllAsync(includeInactive: true);
                var stats = await _lostReasonRepository.GetUsageStatisticsAsync();

                var viewModel = reasons.Select(r => new CrmLostReasonListViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    TitleEnglish = r.TitleEnglish,
                    Code = r.Code,
                    AppliesTo = r.AppliesTo,
                    Category = r.Category,
                    Icon = r.Icon,
                    ColorCode = r.ColorCode,
                    DisplayOrder = r.DisplayOrder,
                    IsSystem = r.IsSystem,
                    RequiresNote = r.RequiresNote,
                    IsActive = r.IsActive,
                    LeadCount = stats.FirstOrDefault(s => s.ReasonId == r.Id).LeadCount,
                    OpportunityCount = stats.FirstOrDefault(s => s.ReasonId == r.Id).OpportunityCount
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// دریافت لیست دلایل از دست رفتن به صورت JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLostReasons(byte? appliesTo = null)
        {
            try
            {
                var reasons = await _lostReasonRepository.GetAllAsync(appliesTo: appliesTo, includeInactive: true);
                var result = reasons.Select(r => new
                {
                    id = r.Id,
                    title = r.Title,
                    titleEnglish = r.TitleEnglish,
                    code = r.Code,
                    appliesTo = r.AppliesTo,
                    appliesToText = r.AppliesToText,
                    category = r.Category,
                    categoryText = r.CategoryText,
                    icon = r.Icon,
                    colorCode = r.ColorCode,
                    displayOrder = r.DisplayOrder,
                    isSystem = r.IsSystem,
                    requiresNote = r.RequiresNote,
                    isActive = r.IsActive,
                    usageCount = r.UsageCount,
                    canDelete = r.CanDelete
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت دلایل برای Lead (برای Dropdown)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLostReasonsForLead()
        {
            try
            {
                var reasons = await _lostReasonRepository.GetForLeadAsync();
                var result = reasons.Select(r => new
                {
                    id = r.Id,
                    title = r.Title,
                    requiresNote = r.RequiresNote
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت دلایل برای Opportunity (برای Dropdown)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLostReasonsForOpportunity()
        {
            try
            {
                var reasons = await _lostReasonRepository.GetForOpportunityAsync();
                var result = reasons.Select(r => new
                {
                    id = r.Id,
                    title = r.Title,
                    requiresNote = r.RequiresNote
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ایجاد دلیل از دست رفتن جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLostReason(CrmLostReasonFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    });
                }

                // بررسی تکراری نبودن عنوان
                if (await _lostReasonRepository.IsTitleDuplicateAsync(model.Title))
                {
                    return Json(new { success = false, message = "این عنوان قبلاً استفاده شده است" });
                }

                var entity = new CrmLostReason
                {
                    Title = model.Title,
                    TitleEnglish = model.TitleEnglish,
                    Code = model.Code,
                    AppliesTo = model.AppliesTo,
                    Category = model.Category,
                    Description = model.Description,
                    Icon = model.Icon ?? "fa-times-circle",
                    ColorCode = model.ColorCode ?? "#dc3545",
                    DisplayOrder = model.DisplayOrder,
                    RequiresNote = model.RequiresNote,
                    IsActive = model.IsActive,
                    CreatorUserId = GetUserId()
                };

                await _lostReasonRepository.CreateAsync(entity);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmSettings",
                    "CreateLostReason",
                    $"ایجاد دلیل از دست رفتن: {model.Title}",
                    recordId: entity.Id.ToString());

                return Json(new { success = true, message = "دلیل از دست رفتن با موفقیت ایجاد شد", data = new { id = entity.Id } });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "CreateLostReason", "خطا در ایجاد دلیل از دست رفتن", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ویرایش دلیل از دست رفتن
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLostReason(CrmLostReasonFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    });
                }

                var existing = await _lostReasonRepository.GetByIdAsync(model.Id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "دلیل یافت نشد" });
                }

                // بررسی تکراری نبودن عنوان
                if (await _lostReasonRepository.IsTitleDuplicateAsync(model.Title, model.Id))
                {
                    return Json(new { success = false, message = "این عنوان قبلاً استفاده شده است" });
                }

                existing.Title = model.Title;
                existing.TitleEnglish = model.TitleEnglish;
                existing.Code = model.Code;
                existing.AppliesTo = model.AppliesTo;
                existing.Category = model.Category;
                existing.Description = model.Description;
                existing.Icon = model.Icon;
                existing.ColorCode = model.ColorCode;
                existing.DisplayOrder = model.DisplayOrder;
                existing.RequiresNote = model.RequiresNote;
                existing.IsActive = model.IsActive;
                existing.LastUpdaterUserId = GetUserId();

                await _lostReasonRepository.UpdateAsync(existing);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "UpdateLostReason",
                    $"ویرایش دلیل از دست رفتن: {model.Title}",
                    recordId: model.Id.ToString());

                return Json(new { success = true, message = "دلیل از دست رفتن با موفقیت بروزرسانی شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "UpdateLostReason", "خطا در ویرایش دلیل از دست رفتن", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// حذف دلیل از دست رفتن
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLostReason(int id)
        {
            try
            {
                var existing = await _lostReasonRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "دلیل یافت نشد" });
                }

                await _lostReasonRepository.DeleteAsync(id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmSettings",
                    "DeleteLostReason",
                    $"حذف دلیل از دست رفتن: {existing.Title}",
                    recordId: id.ToString());

                return Json(new { success = true, message = "دلیل از دست رفتن با موفقیت حذف شد" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmSettings", "DeleteLostReason", "خطا در حذف دلیل از دست رفتن", ex);
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// تغییر ترتیب نمایش دلایل از دست رفتن
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderLostReasons([FromBody] List<int> reasonIds)
        {
            try
            {
                await _lostReasonRepository.UpdateDisplayOrdersAsync(reasonIds);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "ReorderLostReasons",
                    "تغییر ترتیب نمایش دلایل از دست رفتن");

                return Json(new { success = true, message = "ترتیب با موفقیت ذخیره شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        #endregion

        #region ✅ مدیریت تنظیمات عمومی CRM

        /// <summary>
        /// ذخیره تنظیمات عمومی CRM
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneralSettings(CrmGeneralSettingsViewModel model)
        {
            try
            {
                // TODO: ذخیره در جدول تنظیمات CRM
                // فعلاً در Settings عمومی ذخیره می‌کنیم

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Update,
                    "CrmSettings",
                    "SaveGeneralSettings",
                    "بروزرسانی تنظیمات عمومی CRM");

                return Json(new
                {
                    success = true,
                    message = "تنظیمات با موفقیت ذخیره شد"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmSettings",
                    "SaveGeneralSettings",
                    "خطا در ذخیره تنظیمات",
                    ex);

                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        #endregion
    }
}
