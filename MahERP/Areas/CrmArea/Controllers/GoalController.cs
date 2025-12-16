using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
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
    /// کنترلر مدیریت اهداف فروش (Goal)
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM")]
    public class GoalController : BaseController
    {
        private readonly IGoalRepository _goalRepo;
        private readonly ILeadStageStatusRepository _leadStageStatusRepo;
        private readonly IContactRepository _contactRepo;
        private readonly IOrganizationRepository _organizationRepo;

        public GoalController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IGoalRepository goalRepo,
            ILeadStageStatusRepository leadStageStatusRepo,
            IContactRepository contactRepo,
            IOrganizationRepository organizationRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _goalRepo = goalRepo;
            _leadStageStatusRepo = leadStageStatusRepo;
            _contactRepo = contactRepo;
            _organizationRepo = organizationRepo;
        }

        /// <summary>
        /// لیست اهداف یک Contact
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByContact(int contactId)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "فرد یافت نشد";
                return RedirectToAction("Index", "Contact", new { area = "ContactArea" });
            }

            var goals = await _goalRepo.GetByContactAsync(contactId, activeOnly: false);

            ViewBag.Contact = contact;
            ViewBag.ContactId = contactId;
            ViewBag.ContactName = contact.FullName;

            var viewModel = goals.Select(g => MapToViewModel(g)).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// لیست اهداف یک Organization
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByOrganization(int organizationId)
        {
            var organization = await _organizationRepo.GetOrganizationByIdAsync(organizationId);
            if (organization == null)
            {
                TempData["ErrorMessage"] = "سازمان یافت نشد";
                return RedirectToAction("Index", "Organization", new { area = "ContactArea" });
            }

            var goals = await _goalRepo.GetByOrganizationAsync(organizationId, activeOnly: false);

            ViewBag.Organization = organization;
            ViewBag.OrganizationId = organizationId;
            ViewBag.OrganizationName = organization.Name;

            var viewModel = goals.Select(g => MapToViewModel(g)).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// صفحه ایجاد هدف جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int? contactId = null, int? organizationId = null)
        {
            var viewModel = new GoalCreateViewModel
            {
                ContactId = contactId,
                OrganizationId = organizationId
            };

            if (contactId.HasValue)
            {
                var contact = await _contactRepo.GetByIdAsync(contactId.Value);
                ViewBag.TargetName = contact?.FullName;
                ViewBag.TargetType = "Contact";
            }
            else if (organizationId.HasValue)
            {
                var org = await _organizationRepo.GetOrganizationByIdAsync(organizationId.Value);
                ViewBag.TargetName = org?.Name;
                ViewBag.TargetType = "Organization";
            }

            return View(viewModel);
        }

        /// <summary>
        /// ایجاد هدف جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoalCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.ContactId.HasValue && !model.OrganizationId.HasValue)
            {
                ModelState.AddModelError("", "هدف باید به یک فرد یا سازمان متصل باشد");
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var goal = new Goal
            {
                Title = model.Title,
                Description = model.Description,
                ProductName = model.ProductName,
                ContactId = model.ContactId,
                OrganizationId = model.OrganizationId,
                EstimatedValue = model.EstimatedValue,
                CreatorUserId = userId!
            };

            await _goalRepo.CreateAsync(goal);

            TempData["SuccessMessage"] = "هدف با موفقیت ایجاد شد";

            if (model.ContactId.HasValue)
                return RedirectToAction(nameof(ByContact), new { contactId = model.ContactId });
            else
                return RedirectToAction(nameof(ByOrganization), new { organizationId = model.OrganizationId });
        }

        /// <summary>
        /// جزئیات هدف
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var goal = await _goalRepo.GetByIdAsync(id, includeInteractions: true);
            if (goal == null)
            {
                TempData["ErrorMessage"] = "هدف یافت نشد";
                return RedirectToAction("Index", "Dashboard");
            }

            var viewModel = MapToViewModel(goal);
            viewModel.InteractionsCount = goal.InteractionGoals?.Count ?? 0;

            return View(viewModel);
        }

        /// <summary>
        /// تبدیل هدف به خرید (Conversion)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsConverted(int id, decimal? actualValue = null)
        {
            var success = await _goalRepo.MarkAsConvertedAsync(id, actualValue);

            if (success)
                TempData["SuccessMessage"] = "هدف به خرید تبدیل شد";
            else
                TempData["ErrorMessage"] = "خطا در تبدیل هدف";

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// حذف هدف (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? contactId = null, int? organizationId = null)
        {
            var success = await _goalRepo.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = "هدف با موفقیت غیرفعال شد";
            else
                TempData["ErrorMessage"] = "خطا در حذف هدف";

            if (contactId.HasValue)
                return RedirectToAction(nameof(ByContact), new { contactId });
            else if (organizationId.HasValue)
                return RedirectToAction(nameof(ByOrganization), new { organizationId });
            
            return RedirectToAction("Index", "Dashboard");
        }

        #region Private Methods

        private GoalViewModel MapToViewModel(Goal g)
        {
            return new GoalViewModel
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                ProductName = g.ProductName,
                ContactId = g.ContactId,
                ContactName = g.Contact?.FullName,
                OrganizationId = g.OrganizationId,
                OrganizationName = g.Organization?.Name,
                CurrentLeadStageStatusId = g.CurrentLeadStageStatusId,
                CurrentLeadStageStatusTitle = g.CurrentLeadStageStatus?.Title,
                CurrentLeadStageStatusColor = g.CurrentLeadStageStatus?.ColorCode,
                IsConverted = g.IsConverted,
                ConversionDate = g.ConversionDate,
                ConversionDatePersian = g.ConversionDate.HasValue ? ConvertDateTime.ConvertMiladiToShamsi(g.ConversionDate.Value, "yyyy/MM/dd") : null,
                EstimatedValue = g.EstimatedValue,
                EstimatedValueFormatted = g.EstimatedValue?.ToString("N0"),
                ActualValue = g.ActualValue,
                ActualValueFormatted = g.ActualValue?.ToString("N0"),
                IsActive = g.IsActive,
                CreatedDate = g.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(g.CreatedDate, "yyyy/MM/dd"),
                CreatorName = g.Creator?.UserName,
                TargetName = g.TargetName,
                TargetType = g.TargetType
            };
        }

        #endregion
    }
}
