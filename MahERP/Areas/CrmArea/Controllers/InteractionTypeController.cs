using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// کنترلر مدیریت انواع تعامل (CRUD توسط ادمین)
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM.SETTINGS")]
    public class InteractionTypeController : BaseController
    {
        private readonly IInteractionTypeRepository _interactionTypeRepo;
        private readonly ILeadStageStatusRepository _leadStageStatusRepo;

        public InteractionTypeController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IInteractionTypeRepository interactionTypeRepo,
            ILeadStageStatusRepository leadStageStatusRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _interactionTypeRepo = interactionTypeRepo;
            _leadStageStatusRepo = leadStageStatusRepo;
        }

        /// <summary>
        /// لیست انواع تعامل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var interactionTypes = await _interactionTypeRepo.GetAllAsync(activeOnly: false);
            
            var viewModel = interactionTypes.Select(t => new InteractionTypeViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                LeadStageStatusId = t.LeadStageStatusId,
                LeadStageStatusTitle = t.LeadStageStatus?.Title,
                LeadStageStatusColor = t.LeadStageStatus?.ColorCode,
                DisplayOrder = t.DisplayOrder,
                ColorCode = t.ColorCode,
                Icon = t.Icon,
                IsActive = t.IsActive
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// صفحه ایجاد نوع تعامل جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new InteractionTypeViewModel
            {
                IsActive = true,
                LeadStageStatuses = await GetLeadStageStatusesAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// ایجاد نوع تعامل جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InteractionTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.LeadStageStatuses = await GetLeadStageStatusesAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var interactionType = new InteractionType
            {
                Title = model.Title,
                Description = model.Description,
                LeadStageStatusId = model.LeadStageStatusId,
                DisplayOrder = model.DisplayOrder,
                ColorCode = model.ColorCode,
                Icon = model.Icon,
                IsActive = model.IsActive,
                CreatorUserId = userId!
            };

            await _interactionTypeRepo.CreateAsync(interactionType);

            TempData["SuccessMessage"] = "نوع تعامل با موفقیت ایجاد شد";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// صفحه ویرایش نوع تعامل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var interactionType = await _interactionTypeRepo.GetByIdAsync(id);
            if (interactionType == null)
            {
                TempData["ErrorMessage"] = "نوع تعامل یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new InteractionTypeViewModel
            {
                Id = interactionType.Id,
                Title = interactionType.Title,
                Description = interactionType.Description,
                LeadStageStatusId = interactionType.LeadStageStatusId,
                DisplayOrder = interactionType.DisplayOrder,
                ColorCode = interactionType.ColorCode,
                Icon = interactionType.Icon,
                IsActive = interactionType.IsActive,
                LeadStageStatuses = await GetLeadStageStatusesAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// ویرایش نوع تعامل
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InteractionTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.LeadStageStatuses = await GetLeadStageStatusesAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var interactionType = new InteractionType
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                LeadStageStatusId = model.LeadStageStatusId,
                DisplayOrder = model.DisplayOrder,
                ColorCode = model.ColorCode,
                Icon = model.Icon,
                IsActive = model.IsActive,
                LastUpdaterUserId = userId
            };

            var success = await _interactionTypeRepo.UpdateAsync(interactionType);

            if (success)
                TempData["SuccessMessage"] = "نوع تعامل با موفقیت بروزرسانی شد";
            else
                TempData["ErrorMessage"] = "خطا در بروزرسانی نوع تعامل";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// حذف نوع تعامل (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _interactionTypeRepo.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = "نوع تعامل با موفقیت غیرفعال شد";
            else
                TempData["ErrorMessage"] = "خطا در حذف نوع تعامل";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// دریافت لیست وضعیت‌های لید برای Dropdown
        /// </summary>
        private async Task<List<LeadStageStatusViewModel>> GetLeadStageStatusesAsync()
        {
            var stages = await _leadStageStatusRepo.GetAllAsync();
            return stages.Select(s => new LeadStageStatusViewModel
            {
                Id = s.Id,
                Title = s.Title,
                ColorCode = s.ColorCode,
                Icon = s.Icon
            }).ToList();
        }
    }
}
