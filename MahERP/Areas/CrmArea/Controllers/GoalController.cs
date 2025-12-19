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
using MahERP.Extentions;
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
        private readonly IBranchRepository _branchRepo;

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
            IOrganizationRepository organizationRepo,
            IBranchRepository branchRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _goalRepo = goalRepo;
            _leadStageStatusRepo = leadStageStatusRepo;
            _contactRepo = contactRepo;
            _organizationRepo = organizationRepo;
            _branchRepo = branchRepo;
        }

        /// <summary>
        /// صفحه اصلی لیست اهداف با فیلتر و صفحه‌بندی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userBranches = _branchRepo.GetBrnachListByUserId(userId!);

            ViewBag.UserBranches = userBranches;
            ViewBag.HasSingleBranch = userBranches.Count == 1;
            ViewBag.DefaultBranchId = userBranches.Count == 1 ? userBranches.First().Id : (int?)null;
            ViewBag.DefaultBranchName = userBranches.Count == 1 ? userBranches.First().Name : null;

            // دریافت 10 هدف اخیر که تغییر کرده‌اند
            var recentGoals = await _goalRepo.GetRecentlyChangedGoalsAsync(10);
            ViewBag.RecentGoals = recentGoals;

            return View();
        }

        /// <summary>
        /// بارگذاری محتوای اهداف یک Stakeholder (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadStakeholderGoals(int? contactId, int? organizationId, int? branchId)
        {
            var viewModel = new StakeholderGoalsPageViewModel
            {
                BranchId = branchId,
                ContactId = contactId,
                OrganizationId = organizationId
            };

            if (contactId.HasValue)
            {
                var contact = await _contactRepo.GetByIdAsync(contactId.Value);
                if (contact != null)
                {
                    viewModel.StakeholderType = StakeholderType.Contact;
                    viewModel.ContactName = contact.FullName;
                    viewModel.ContactType = contact.ContactType;
                    viewModel.ContactTypeName = GetContactTypeName(contact.ContactType);

                    // دریافت اهداف
                    var goals = await _goalRepo.GetByContactAndBranchAsync(contactId.Value, branchId);
                    viewModel.Goals = goals.Select(g => MapToViewModel(g)).ToList();
                    viewModel.TotalGoals = viewModel.Goals.Count;
                    viewModel.ActiveGoals = viewModel.Goals.Count(g => g.IsActive && !g.IsConverted);
                    viewModel.ConvertedGoals = viewModel.Goals.Count(g => g.IsConverted);
                }
            }
            else if (organizationId.HasValue)
            {
                var org = await _organizationRepo.GetOrganizationByIdAsync(organizationId.Value);
                if (org != null)
                {
                    viewModel.StakeholderType = StakeholderType.Organization;
                    viewModel.OrganizationName = org.Name;

                    // دریافت اهداف
                    var goals = await _goalRepo.GetByOrganizationAndBranchAsync(organizationId.Value, branchId);
                    viewModel.Goals = goals.Select(g => MapToViewModel(g)).ToList();
                    viewModel.TotalGoals = viewModel.Goals.Count;
                    viewModel.ActiveGoals = viewModel.Goals.Count(g => g.IsActive && !g.IsConverted);
                    viewModel.ConvertedGoals = viewModel.Goals.Count(g => g.IsConverted);
                }
            }

            return PartialView("_StakeholderGoalsContent", viewModel);
        }

        /// <summary>
        /// دریافت 10 هدف اخیر (Sidebar)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentGoals()
        {
            var recentGoals = await _goalRepo.GetRecentlyChangedGoalsAsync(10);
            return PartialView("_RecentGoalsSidebar", recentGoals);
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
            // اگر هیچ‌کدام انتخاب نشده، به صفحه انتخاب هدف هدایت می‌شود
            if (!contactId.HasValue && !organizationId.HasValue)
            {
                // دریافت شعبه‌های کاربر جاری
                var userId = _userManager.GetUserId(User);
                var userBranches = _branchRepo.GetBrnachListByUserId(userId!);
                
                ViewBag.UserBranches = userBranches;
                ViewBag.HasSingleBranch = userBranches.Count == 1;
                ViewBag.DefaultBranchId = userBranches.Count == 1 ? userBranches.First().Id : (int?)null;
                ViewBag.DefaultBranchName = userBranches.Count == 1 ? userBranches.First().Name : null;
                
                return View("SelectTarget");
            }

            var viewModel = new GoalCreateViewModel
            {
                ContactId = contactId,
                OrganizationId = organizationId
            };

            if (contactId.HasValue)
            {
                var contact = await _contactRepo.GetByIdAsync(contactId.Value);
                if (contact == null)
                {
                    TempData["ErrorMessage"] = "فرد یافت نشد";
                    return RedirectToAction("Index");
                }
                ViewBag.TargetName = contact.FullName;
                ViewBag.TargetType = "Contact";
            }
            else if (organizationId.HasValue)
            {
                var org = await _organizationRepo.GetOrganizationByIdAsync(organizationId.Value);
                if (org == null)
                {
                    TempData["ErrorMessage"] = "سازمان یافت نشد";
                    return RedirectToAction("Index");
                }
                ViewBag.TargetName = org.Name;
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

        /// <summary>
        /// مودال انتخاب Contact
        /// </summary>
        [HttpGet]
        public IActionResult SelectContactModal(int branchId)
        {
            // دریافت افراد شعبه مشخص شده
            var branchContacts = _branchRepo.GetBranchContacts(branchId, includeInactive: false);
            var contacts = branchContacts.Select(bc => bc.Contact).ToList();
            
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = _branchRepo.GetBranchById(branchId)?.Name;
            
            return PartialView("_SelectContactModal", contacts);
        }

        /// <summary>
        /// مودال انتخاب Organization
        /// </summary>
        [HttpGet]
        public IActionResult SelectOrganizationModal(int branchId)
        {
            // دریافت سازمان‌های شعبه مشخص شده
            var branchOrganizations = _branchRepo.GetBranchOrganizations(branchId);
            var organizations = branchOrganizations.Select(bo => bo.Organization).ToList();
            
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = _branchRepo.GetBranchById(branchId)?.Name;
            
            return PartialView("_SelectOrganizationModal", organizations);
        }

        /// <summary>
        /// مودال ایجاد هدف سریع (Quick Add) - برای استفاده در جاهای مختلف
        /// </summary>
        /// <param name="contactId">شناسه فرد (اختیاری)</param>
        /// <param name="organizationId">شناسه سازمان (اختیاری)</param>
        /// <param name="returnUrl">URL بازگشت بعد از ثبت موفق</param>
        [HttpGet]
        public IActionResult CreateQuickModal(int? contactId = null, int? organizationId = null, string? returnUrl = null)
        {
            var viewModel = new GoalCreateViewModel
            {
                ContactId = contactId,
                OrganizationId = organizationId
            };

            ViewBag.ContactId = contactId;
            ViewBag.OrganizationId = organizationId;
            ViewBag.ReturnUrl = returnUrl;

            return PartialView("_CreateGoalQuickModal", viewModel);
        }

        /// <summary>
        /// ثبت هدف سریع (AJAX) - با امکان استفاده مجدد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick(GoalCreateViewModel model, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "عنوان هدف الزامی است" } }
                });
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                var goal = new Goal
                {
                    Title = model.Title,
                    Description = model.Description,
                    ProductName = model.ProductName,
                    ContactId = model.ContactId,
                    OrganizationId = model.OrganizationId,
                    EstimatedValue = model.EstimatedValue,
                    CreatorUserId = userId!,
                    IsActive = true
                };

                var createdGoal = await _goalRepo.CreateAsync(goal);

                // دریافت لیست جدید اهداف برای بروزرسانی
                List<Goal> goals;
                if (model.ContactId.HasValue)
                {
                    goals = await _goalRepo.GetByContactAsync(model.ContactId.Value);
                }
                else if (model.OrganizationId.HasValue)
                {
                    goals = await _goalRepo.GetByOrganizationAsync(model.OrganizationId.Value);
                }
                else
                {
                    goals = new List<Goal>();
                }

                var goalsViewModel = goals.Select(g => new GoalViewModel
                {
                    Id = g.Id,
                    Title = g.Title,
                    ProductName = g.ProductName,
                    CurrentLeadStageStatusTitle = g.CurrentLeadStageStatus?.Title,
                    CurrentLeadStageStatusColor = g.CurrentLeadStageStatus?.ColorCode
                }).ToList();

                // Render partial view for goals list
                var goalsHtml = await this.RenderViewToStringAsync("_GoalCheckboxList", goalsViewModel);

                return Json(new
                {
                    status = "success",
                    message = new[] { new { status = "success", text = "هدف با موفقیت ایجاد شد" } },
                    goalId = createdGoal.Id,
                    goalsHtml = goalsHtml,
                    returnUrl = returnUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = $"خطا در ایجاد هدف: {ex.Message}" } }
                });
            }
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
                EstimatedValueFormatted = g.EstimatedValue.HasValue ? (g.EstimatedValue.Value / 10).ToString("N0") : null, // تبدیل به تومان
                ActualValue = g.ActualValue,
                ActualValueFormatted = g.ActualValue.HasValue ? (g.ActualValue.Value / 10).ToString("N0") : null, // تبدیل به تومان
                IsActive = g.IsActive,
                CreatedDate = g.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(g.CreatedDate, "yyyy/MM/dd"),
                CreatorName = g.Creator?.UserName,
                TargetName = g.TargetName,
                TargetType = g.TargetType
            };
        }

        private string GetContactTypeName(DataModelLayer.Enums.ContactType? type)
        {
            return type switch
            {
                DataModelLayer.Enums.ContactType.Lead => "سرنخ",
                DataModelLayer.Enums.ContactType.Customer => "مشتری",
                DataModelLayer.Enums.ContactType.Partner => "شریک تجاری",
                DataModelLayer.Enums.ContactType.Other => "سایر",
                _ => "نامشخص"
            };
        }

        private async Task<GoalStatisticsViewModel> CalculateGoalStatistics(GoalFilterViewModel filters)
        {
            // دریافت همه اهداف بر اساس فیلترهای انتخاب شده (بدون صفحه‌بندی)
            var (allGoals, _) = await _goalRepo.GetListAsync(filters, 1, int.MaxValue);

            var statistics = new GoalStatisticsViewModel
            {
                TotalGoals = allGoals.Count,
                ActiveGoals = allGoals.Count(g => g.IsActive && !g.IsConverted),
                ConvertedGoals = allGoals.Count(g => g.IsConverted),
                InactiveGoals = allGoals.Count(g => !g.IsActive),
                ConversionRate = allGoals.Count > 0 
                    ? (decimal)Math.Round((double)allGoals.Count(g => g.IsConverted) / allGoals.Count * 100, 1) 
                    : 0,
                TotalEstimatedValue = allGoals.Sum(g => g.EstimatedValue ?? 0),
                TotalActualValue = allGoals.Where(g => g.IsConverted).Sum(g => g.ActualValue ?? 0)
            };

            return statistics;
        }

        #endregion
    }
}
