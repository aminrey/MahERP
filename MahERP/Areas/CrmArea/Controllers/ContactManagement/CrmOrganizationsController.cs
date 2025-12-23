using AutoMapper;
using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.ContactManagement
{
    /// <summary>
    /// مدیریت سازمان‌ها در CRM با محدودیت شعبه
    /// ⚠️ فقط کاربران CRM با دسترسی ORG
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("ORG.VIEW")]
    public class CrmOrganizationsController : BaseController
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly AppDbContext _context;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public CrmOrganizationsController(
            IOrganizationRepository organizationRepository,
            IBranchRepository branchRepository,
            AppDbContext context,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IUnitOfWork uow)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _organizationRepository = organizationRepository;
            _branchRepository = branchRepository;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX (انتخاب شعبه + لیست) ====================

        /// <summary>
        /// صفحه اصلی - انتخاب شعبه و نمایش لیست سازمان‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? branchId, string searchTerm = null, byte? organizationType = null)
        {
            try
            {
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                var userBranches = _branchRepository.GetBrnachListByUserId(userId);

                ViewBag.UserBranches = userBranches;
                ViewBag.HasSingleBranch = userBranches.Count == 1;

                // اگر فقط یک شعبه داره
                if (userBranches.Count == 1 && !branchId.HasValue)
                {
                    branchId = userBranches.First().Id;
                }

                ViewBag.SelectedBranchId = branchId;

                // اگر شعبه انتخاب نشده
                if (!branchId.HasValue)
                {
                    ViewBag.ShowBranchSelection = true;
                    return View(new List<OrganizationViewModel>());
                }

                // بررسی دسترسی به شعبه
                if (!userBranches.Any(b => b.Id == branchId.Value))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var selectedBranch = userBranches.First(b => b.Id == branchId.Value);
                ViewBag.SelectedBranchName = selectedBranch.Name;
                ViewBag.ShowBranchSelection = false;

                // دریافت سازمان‌های شعبه با Include صریح
                var branchOrganizations = _context.BranchOrganization_Tbl
                    .Include(bo => bo.Organization)
                    .Where(bo => bo.BranchId == branchId.Value && bo.IsActive && bo.Organization.IsActive)
                    .ToList();

                var organizations = branchOrganizations.Select(bo => bo.Organization).ToList();

                // فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    organizations = organizations.Where(o =>
                        o.Name.ToLower().Contains(searchTerm) ||
                        (o.Brand != null && o.Brand.ToLower().Contains(searchTerm)) ||
                        (o.RegistrationNumber != null && o.RegistrationNumber.Contains(searchTerm))
                    ).ToList();
                }

                // فیلتر نوع
                if (organizationType.HasValue)
                {
                    organizations = organizations.Where(o => o.OrganizationType == organizationType.Value).ToList();
                }

                // مرتب‌سازی
                organizations = organizations.OrderBy(o => o.Name).ToList();

                var viewModels = _mapper.Map<List<OrganizationViewModel>>(organizations);

                // گروه‌بندی بر اساس حرف اول
                var groupedOrganizations = viewModels
                    .GroupBy(o => string.IsNullOrEmpty(o.Name) ? "#" : o.Name.Substring(0, 1).ToUpper())
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.GroupedOrganizations = groupedOrganizations;
                ViewBag.TotalCount = organizations.Count;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CrmOrganizations",
                    "Index",
                    $"مشاهده لیست سازمان‌های شعبه {selectedBranch.Name}",
                    recordId: branchId.ToString()
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Index", "خطا در دریافت لیست", ex);
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// صفحه ایجاد سازمان جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create(int? branchId)
        {
            try
            {
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                var userBranches = _branchRepository.GetBrnachListByUserId(userId);

                ViewBag.UserBranches = userBranches;
                ViewBag.HasSingleBranch = userBranches.Count == 1;

                // اگر فقط یک شعبه داره
                if (userBranches.Count == 1)
                {
                    branchId = userBranches.First().Id;
                }

                ViewBag.SelectedBranchId = branchId;

                // اگر شعبه انتخاب نشده
                if (!branchId.HasValue)
                {
                    ViewBag.ShowBranchSelection = true;
                    return View(new OrganizationCreateViewModel());
                }

                // بررسی دسترسی به شعبه
                if (!userBranches.Any(b => b.Id == branchId.Value))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var selectedBranch = userBranches.First(b => b.Id == branchId.Value);
                ViewBag.SelectedBranchName = selectedBranch.Name;
                ViewBag.ShowBranchSelection = false;

                var model = new OrganizationCreateViewModel
                {
                    BranchId = branchId.Value
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Create", "خطا در بارگذاری فرم", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ثبت سازمان جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create(OrganizationCreateViewModel model)
        {
            try
            {
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                if (!ModelState.IsValid)
                {
                    var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                    ViewBag.UserBranches = userBranches;
                    ViewBag.SelectedBranchId = model.BranchId;
                    return View(model);
                }

                var userBranches2 = _branchRepository.GetBrnachListByUserId(userId);
                if (!userBranches2.Any(b => b.Id == model.BranchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                // 1️⃣ ایجاد Organization در دیتابیس اصلی
                var organization = _mapper.Map<Organization>(model);
                organization.CreatorUserId = userId;
                organization.CreatedDate = DateTime.Now;
                organization.IsActive = true;

                _context.Organization_Tbl.Add(organization);
                await _context.SaveChangesAsync();

                // 2️⃣ اتصال به شعبه (BranchOrganization)
                var branchOrganization = new BranchOrganization
                {
                    BranchId = model.BranchId,
                    OrganizationId = organization.Id,
                    AssignedByUserId = userId,
                    AssignDate = DateTime.Now,
                    IsActive = true
                };

                _context.BranchOrganization_Tbl.Add(branchOrganization);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmOrganizations",
                    "Create",
                    $"ایجاد سازمان جدید: {organization.Name}",
                    recordId: organization.Id.ToString()
                );

                TempData["SuccessMessage"] = $"سازمان '{organization.Name}' با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Details), new { id = organization.Id, branchId = model.BranchId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Create", "خطا در ایجاد سازمان", ex);
                ModelState.AddModelError("", "خطا در ذخیره اطلاعات: " + ex.Message);

                var userId = GetUserId();
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                ViewBag.UserBranches = userBranches;
                ViewBag.SelectedBranchId = model.BranchId;
                return View(model);
            }
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// جزئیات سازمان
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id, int branchId)
        {
            try
            {
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var organization = _organizationRepository.GetOrganizationById(id);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "سازمان یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var branchOrganization = _context.BranchOrganization_Tbl
                    .FirstOrDefault(bo => bo.OrganizationId == id && bo.BranchId == branchId && bo.IsActive);

                if (branchOrganization == null)
                {
                    TempData["ErrorMessage"] = "این سازمان در شعبه انتخاب شده وجود ندارد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var viewModel = _mapper.Map<OrganizationViewModel>(organization);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = userBranches.First(b => b.Id == branchId).Name;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CrmOrganizations",
                    "Details",
                    $"مشاهده جزئیات سازمان: {organization.Name}",
                    recordId: id.ToString()
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Details", "خطا در نمایش جزئیات", ex);
                TempData["ErrorMessage"] = "خطا در نمایش اطلاعات";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== EDIT ====================

        /// <summary>
        /// ویرایش سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(int id, int branchId)
        {
            try
            {
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var organization = _organizationRepository.GetOrganizationById(id);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "سازمان یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var branchOrganization = _context.BranchOrganization_Tbl
                    .FirstOrDefault(bo => bo.OrganizationId == id && bo.BranchId == branchId && bo.IsActive);

                if (branchOrganization == null)
                {
                    TempData["ErrorMessage"] = "این سازمان در شعبه انتخاب شده وجود ندارد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var viewModel = _mapper.Map<OrganizationEditViewModel>(organization);
                viewModel.BranchId = branchId;

                ViewBag.BranchName = userBranches.First(b => b.Id == branchId).Name;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Edit", "خطا در بارگذاری فرم", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ذخیره تغییرات سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(OrganizationEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                    ViewBag.BranchName = userBranches.First(b => b.Id == model.BranchId).Name;
                    return View(model);
                }

                var userBranches2 = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches2.Any(b => b.Id == model.BranchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var organization = _organizationRepository.GetOrganizationById(model.Id);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "سازمان یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId = model.BranchId });
                }

                // بروزرسانی
                _mapper.Map(model, organization);
                organization.LastUpdaterUserId = GetUserId();
                organization.LastUpdateDate = DateTime.Now;

                _context.Organization_Tbl.Update(organization);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmOrganizations",
                    "Edit",
                    $"ویرایش سازمان: {organization.Name}",
                    recordId: organization.Id.ToString()
                );

                TempData["SuccessMessage"] = "اطلاعات با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Details), new { id = organization.Id, branchId = model.BranchId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Edit", "خطا در ویرایش", ex);
                ModelState.AddModelError("", "خطا در ذخیره تغییرات");
                return View(model);
            }
        }

        // ==================== DELETE ====================

        /// <summary>
        /// حذف سازمان از شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.DELETE")]
        public async Task<IActionResult> Delete(int id, int branchId)
        {
            try
            {
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new { success = false, message = "شما به این شعبه دسترسی ندارید" });
                }

                var organization = _organizationRepository.GetOrganizationById(id);
                if (organization == null)
                {
                    return Json(new { success = false, message = "سازمان یافت نشد" });
                }

                var branchOrganization = _context.BranchOrganization_Tbl
                    .FirstOrDefault(bo => bo.OrganizationId == id && bo.BranchId == branchId);

                if (branchOrganization != null)
                {
                    branchOrganization.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmOrganizations",
                    "Delete",
                    $"حذف سازمان از شعبه: {organization.Name}",
                    recordId: id.ToString()
                );

                return Json(new { success = true, message = "سازمان از شعبه حذف شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmOrganizations", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف سازمان" });
            }
        }
    }
}
