using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.LeadControllers
{
    /// <summary>
    /// کنترلر مدیریت وضعیت‌های سرنخ CRM
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM.LEAD.STATUS")]
    public class LeadStatusController : BaseController
    {
        private readonly ICrmLeadStatusRepository _statusRepo;

        public LeadStatusController(
            ICrmLeadStatusRepository statusRepo,
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
            _statusRepo = statusRepo;
        }

        // ========== لیست وضعیت‌ها ==========

        /// <summary>
        /// صفحه اصلی مدیریت وضعیت‌ها
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.STATUS.VIEW")]
        public async Task<IActionResult> Index()
        {
            var statuses = await _statusRepo.GetAllAsync(includeInactive: true);
            var leadsCount = await _statusRepo.GetLeadsCountByStatusAsync();

            var viewModel = new CrmLeadStatusListViewModel
            {
                Statuses = statuses.Select(s => new CrmLeadStatusViewModel
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
                    Description = s.Description,
                    IsActive = s.IsActive,
                    LeadsCount = leadsCount.ContainsKey(s.Id) ? leadsCount[s.Id] : 0,
                    BadgeClass = s.BadgeClass,
                    CreatedDate = s.CreatedDate
                }).ToList()
            };

            return View(viewModel);
        }

        // ========== ایجاد وضعیت ==========

        /// <summary>
        /// مودال ایجاد وضعیت جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.STATUS.CREATE")]
        public IActionResult Create()
        {
            var model = new CrmLeadStatusViewModel
            {
                ColorCode = "#6c757d",
                Icon = "fa-circle",
                DisplayOrder = 1,
                IsActive = true
            };
            return PartialView("_CreateStatusModal", model);
        }

        /// <summary>
        /// ذخیره وضعیت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.STATUS.CREATE")]
        public async Task<IActionResult> Create(CrmLeadStatusViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "warning", text = "اطلاعات وارد شده معتبر نیست" } }
                    });
                }

                // بررسی یکتا بودن عنوان
                if (!await _statusRepo.IsTitleUniqueAsync(model.Title))
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "warning", text = "وضعیتی با این عنوان قبلاً ثبت شده است" } }
                    });
                }

                var currentUser = await _userManager.GetUserAsync(User);

                var status = new CrmLeadStatus
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
                    CreatorUserId = currentUser.Id
                };

                await _statusRepo.CreateAsync(status);

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmLeadStatus",
                    "ایجاد وضعیت سرنخ",
                    $"وضعیت «{status.Title}» ایجاد شد",
                    recordId: status.Id.ToString(),
                    entityType: "CrmLeadStatus",
                    recordTitle: status.Title
                );

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index"),
                    message = new[] { new { status = "success", text = "وضعیت با موفقیت ایجاد شد" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }

        // ========== ویرایش وضعیت ==========

        /// <summary>
        /// مودال ویرایش وضعیت
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.STATUS.EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            var status = await _statusRepo.GetByIdAsync(id);
            if (status == null)
                return NotFound();

            var model = new CrmLeadStatusViewModel
            {
                Id = status.Id,
                Title = status.Title,
                TitleEnglish = status.TitleEnglish,
                ColorCode = status.ColorCode,
                Icon = status.Icon,
                DisplayOrder = status.DisplayOrder,
                IsDefault = status.IsDefault,
                IsFinal = status.IsFinal,
                IsPositive = status.IsPositive,
                Description = status.Description,
                IsActive = status.IsActive
            };

            return PartialView("_EditStatusModal", model);
        }

        /// <summary>
        /// ذخیره تغییرات وضعیت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.STATUS.EDIT")]
        public async Task<IActionResult> Edit(int id, CrmLeadStatusViewModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "warning", text = "اطلاعات وارد شده معتبر نیست" } }
                    });
                }

                // بررسی یکتا بودن عنوان
                if (!await _statusRepo.IsTitleUniqueAsync(model.Title, model.Id))
                {
                    return Json(new
                    {
                        status = "validation-error",
                        message = new[] { new { status = "warning", text = "وضعیتی با این عنوان قبلاً ثبت شده است" } }
                    });
                }

                var currentUser = await _userManager.GetUserAsync(User);

                var status = new CrmLeadStatus
                {
                    Id = model.Id,
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
                    LastUpdaterUserId = currentUser.Id
                };

                var result = await _statusRepo.UpdateAsync(status);

                if (!result)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در بروزرسانی وضعیت" } }
                    });
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmLeadStatus",
                    "ویرایش وضعیت سرنخ",
                    $"وضعیت «{status.Title}» ویرایش شد",
                    recordId: status.Id.ToString(),
                    entityType: "CrmLeadStatus",
                    recordTitle: status.Title
                );

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index"),
                    message = new[] { new { status = "success", text = "وضعیت با موفقیت بروزرسانی شد" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا: {ex.Message}" } }
                });
            }
        }

        // ========== حذف وضعیت ==========

        /// <summary>
        /// حذف وضعیت (غیرفعال کردن)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.STATUS.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // بررسی استفاده از وضعیت
                if (await _statusRepo.IsStatusInUseAsync(id))
                {
                    return Json(new
                    {
                        success = false,
                        message = "این وضعیت در سرنخ‌ها استفاده شده و قابل حذف نیست. ابتدا سرنخ‌ها را به وضعیت دیگری منتقل کنید."
                    });
                }

                var status = await _statusRepo.GetByIdAsync(id);
                if (status == null)
                {
                    return Json(new { success = false, message = "وضعیت یافت نشد" });
                }

                var statusTitle = status.Title;
                var result = await _statusRepo.DeleteAsync(id);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در حذف وضعیت" });
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmLeadStatus",
                    "حذف وضعیت سرنخ",
                    $"وضعیت «{statusTitle}» حذف شد",
                    recordId: id.ToString(),
                    entityType: "CrmLeadStatus",
                    recordTitle: statusTitle
                );

                return Json(new { success = true, message = "وضعیت با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== عملیات ویژه ==========

        /// <summary>
        /// تنظیم وضعیت پیش‌فرض
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.LEAD.STATUS.EDIT")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _statusRepo.SetAsDefaultAsync(id, currentUser.Id);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در تنظیم وضعیت پیش‌فرض" });
                }

                return Json(new { success = true, message = "وضعیت پیش‌فرض تنظیم شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// دریافت لیست وضعیت‌ها برای Dropdown
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatusesDropdown()
        {
            var statuses = await _statusRepo.GetAllAsync();
            return Json(statuses.Select(s => new
            {
                id = s.Id,
                text = s.Title,
                color = s.ColorCode,
                icon = s.Icon,
                isDefault = s.IsDefault,
                isFinal = s.IsFinal
            }));
        }
    }
}
