using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.OrganizationGroupRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("ORG")]
    public class OrganizationsController : BaseController
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IOrganizationGroupRepository _organizationGroupRepository;


        public OrganizationsController(
            IOrganizationRepository organizationRepository,
            IContactRepository contactRepository,
            IUnitOfWork uow,
                IOrganizationGroupRepository organizationGroupRepository, 

            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _organizationRepository = organizationRepository;
            _contactRepository = contactRepository;
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
            _organizationGroupRepository = organizationGroupRepository; 

        }

        // ==================== INDEX ====================

        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> Index(string searchTerm = null, byte? organizationType = null, int? groupId = null)
        {
            try
            {
                List<Organization> organizations;

                // اگر گروه انتخاب شده
                if (groupId.HasValue)
                {
                    organizations = _organizationGroupRepository.GetGroupOrganizations(groupId.Value, includeInactive: false);
                    ViewBag.SelectedGroupId = groupId.Value;
                }
                // جستجو
                else if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    organizations = _organizationRepository.SearchOrganizations(searchTerm, organizationType, true);
                }
                // همه سازمان‌ها
                else
                {
                    organizations = _organizationRepository.GetAllOrganizations(true, organizationType);
                }

                var viewModels = _mapper.Map<List<OrganizationViewModel>>(organizations);

                // ⭐ دریافت لیست گروه‌ها برای فیلتر
                var allGroups = _organizationGroupRepository.GetAllGroups(includeInactive: false);
                ViewBag.OrganizationGroups = allGroups;

                // ⭐ دریافت گروه‌های هر سازمان (برای نمایش در جدول)
                var organizationIds = organizations.Select(o => o.Id).ToList();
                var organizationGroupsDict = new Dictionary<int, List<OrganizationGroup>>();
                foreach (var orgId in organizationIds)
                {
                    organizationGroupsDict[orgId] = _organizationGroupRepository.GetOrganizationGroups(orgId);
                }
                ViewBag.OrganizationGroupsDict = organizationGroupsDict;

                ViewBag.SearchTerm = searchTerm;
                ViewBag.OrganizationType = organizationType;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Index",
                    "مشاهده لیست سازمان‌ها"
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// جزئیات سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(id,
                    includeDepartments: true,
                    includeContacts: true);

                if (organization == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Organizations",
                        "Details",
                        "تلاش برای مشاهده سازمان غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<OrganizationViewModel>(organization);

                // آمار
                var stats = await _organizationRepository.GetOrganizationStatisticsAsync(id);
                ViewBag.Statistics = stats;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Details",
                    $"مشاهده جزئیات سازمان: {organization.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Details", "خطا در دریافت جزئیات", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// نمایش فرم افزودن سازمان جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create()
        {
            try
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Create",
                    "مشاهده فرم افزودن سازمان جدید"
                );

                return View(new OrganizationViewModel { IsActive = true, OrganizationType = 0 });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Create", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره سازمان جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create(OrganizationViewModel model)
        {
            // اعتبارسنجی
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", "نام سازمان الزامی است");
            }

            // بررسی یکتا بودن شماره ثبت
            if (!string.IsNullOrEmpty(model.RegistrationNumber))
            {
                if (!_organizationRepository.IsRegistrationNumberUnique(model.RegistrationNumber))
                {
                    ModelState.AddModelError("RegistrationNumber", "این شماره ثبت قبلاً استفاده شده است");
                }
            }

            // بررسی یکتا بودن کد اقتصادی
            if (!string.IsNullOrEmpty(model.EconomicCode))
            {
                if (!_organizationRepository.IsEconomicCodeUnique(model.EconomicCode))
                {
                    ModelState.AddModelError("EconomicCode", "این کد اقتصادی قبلاً استفاده شده است");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var organization = _mapper.Map<Organization>(model);
                    organization.CreatedDate = DateTime.Now;
                    organization.CreatorUserId = GetUserId();
                    organization.IsActive = true;

                    _uow.OrganizationUW.Create(organization);
                    _uow.Save();

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "Create",
                        $"ایجاد سازمان جدید: {organization.DisplayName}",
                        recordId: organization.Id.ToString(),
                        entityType: "Organization",
                        recordTitle: organization.DisplayName
                    );

                    TempData["SuccessMessage"] = "سازمان با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "Create", "خطا در ایجاد", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== EDIT ====================

        /// <summary>
        /// نمایش فرم ویرایش سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(id);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var viewModel = _mapper.Map<OrganizationViewModel>(organization);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Edit",
                    $"مشاهده فرم ویرایش سازمان: {organization.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Edit", "خطا در نمایش فرم", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره ویرایش سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(OrganizationViewModel model)
        {
            // اعتبارسنجی
            if (!string.IsNullOrEmpty(model.RegistrationNumber))
            {
                if (!_organizationRepository.IsRegistrationNumberUnique(model.RegistrationNumber, model.Id))
                {
                    ModelState.AddModelError("RegistrationNumber", "این شماره ثبت قبلاً استفاده شده است");
                }
            }

            if (!string.IsNullOrEmpty(model.EconomicCode))
            {
                if (!_organizationRepository.IsEconomicCodeUnique(model.EconomicCode, model.Id))
                {
                    ModelState.AddModelError("EconomicCode", "این کد اقتصادی قبلاً استفاده شده است");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var organization = _uow.OrganizationUW.GetById(model.Id);
                    if (organization == null)
                        return RedirectToAction("ErrorView", "Home");

                    var oldValues = new
                    {
                        organization.Name,
                        organization.RegistrationNumber,
                        organization.EconomicCode,
                        organization.IsActive
                    };

                    var originalCreated = organization.CreatedDate;
                    var originalCreatorId = organization.CreatorUserId;

                    _mapper.Map(model, organization);

                    organization.CreatedDate = originalCreated;
                    organization.CreatorUserId = originalCreatorId;
                    organization.LastUpdateDate = DateTime.Now;
                    organization.LastUpdaterUserId = GetUserId();

                    _uow.OrganizationUW.Update(organization);
                    _uow.Save();

                    var newValues = new
                    {
                        organization.Name,
                        organization.RegistrationNumber,
                        organization.EconomicCode,
                        organization.IsActive
                    };

                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Organizations",
                        "Edit",
                        $"ویرایش سازمان: {organization.DisplayName}",
                        oldValues,
                        newValues,
                        recordId: organization.Id.ToString(),
                        entityType: "Organization",
                        recordTitle: organization.DisplayName
                    );

                    TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Details), new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "Edit", "خطا در ویرایش", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// نمایش مودال تایید حذف
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.themeclass = "bg-danger";
                ViewBag.ViewTitle = "حذف سازمان";

                return PartialView("_DeleteOrganization", organization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Delete", "خطا در نمایش فرم حذف", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// حذف سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.DELETE")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var organizationName = organization.DisplayName;

                // حذف نرم
                organization.IsActive = false;
                organization.LastUpdateDate = DateTime.Now;
                organization.LastUpdaterUserId = GetUserId();

                _uow.OrganizationUW.Update(organization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Organizations",
                    "Delete",
                    $"حذف سازمان: {organizationName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organizationName
                );

                TempData["SuccessMessage"] = "سازمان با موفقیت حذف شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Delete", "خطا در حذف", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در حذف سازمان";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== ORGANIZATION CHART ====================

        /// <summary>
        /// نمایش چارت سازمانی
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> OrganizationChart(int organizationId)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId, includeDepartments: true);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var chartViewModel = await _organizationRepository.GetOrganizationChartAsync(organizationId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "OrganizationChart",
                    $"مشاهده چارت سازمانی: {organization.DisplayName}",
                    recordId: organizationId.ToString()
                );

                return View(chartViewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "OrganizationChart", "خطا در نمایش چارت", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== DEPARTMENT MANAGEMENT ====================

        /// <summary>
        /// افزودن بخش جدید - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddDepartment(int organizationId, int? parentDepartmentId = null)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                // دریافت لیست افراد برای انتخاب مدیر
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.ParentDepartmentId = parentDepartmentId;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

                var model = new OrganizationDepartmentViewModel
                {
                    OrganizationId = organizationId,
                    ParentDepartmentId = parentDepartmentId,
                    IsActive = true,
                    DisplayOrder = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddDepartment", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره بخش جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddDepartment(OrganizationDepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var department = _mapper.Map<OrganizationDepartment>(model);
                    department.CreatorUserId = GetUserId();

                    var createdDepartment = await _organizationRepository.CreateDepartmentAsync(department);
                    var departmentId = createdDepartment.Id;
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddDepartment",
                        $"ایجاد بخش جدید: {department.Title}",
                        recordId: departmentId.ToString()
                    );

                    TempData["SuccessMessage"] = "بخش با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(OrganizationChart), new { organizationId = model.OrganizationId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddDepartment", "خطا در ایجاد بخش", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // بازگشت به فرم در صورت خطا
            var organization = await _organizationRepository.GetOrganizationByIdAsync(model.OrganizationId);
            var contacts = _contactRepository.GetAllContacts(includeInactive: false);

            ViewBag.OrganizationId = model.OrganizationId;
            ViewBag.OrganizationName = organization?.DisplayName;
            ViewBag.ParentDepartmentId = model.ParentDepartmentId;
            ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

            return View(model);
        }

        // ==================== POSITION MANAGEMENT ====================

        /// <summary>
        /// مدیریت سمت‌ها
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> ManagePositions(int departmentId)
        {
            try
            {
                var department = _organizationRepository.GetDepartmentById(departmentId, includePositions: true);
                if (department == null)
                    return RedirectToAction("ErrorView", "Home");

                var positions = _organizationRepository.GetDepartmentPositions(departmentId, includeInactive: false);
                var viewModels = _mapper.Map<List<DepartmentPositionViewModel>>(positions);

                ViewBag.DepartmentId = departmentId;
                ViewBag.DepartmentTitle = department.Title;
                ViewBag.OrganizationId = department.OrganizationId;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "ManagePositions", "خطا در نمایش سمت‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// افزودن سمت جدید - Modal
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public IActionResult AddPositionModal(int departmentId)
        {
            var department = _organizationRepository.GetDepartmentById(departmentId);

            ViewBag.DepartmentTitle = department?.Title;

            var model = new DepartmentPositionViewModel
            {
                DepartmentId = departmentId,
                IsActive = true,
                DisplayOrder = 1,
                PowerLevel = 50
            };

            return PartialView("_AddPositionModal", model);
        }

        /// <summary>
        /// ذخیره سمت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddPosition(DepartmentPositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var position = _mapper.Map<DepartmentPosition>(model);
                    position.CreatorUserId = GetUserId();

                    var createdPosition = await _organizationRepository.CreatePositionAsync(position);
                    var positionId = createdPosition.Id;
                    // رندر لیست به‌روزرسانی شده
                    var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);
                    var viewModels = _mapper.Map<List<DepartmentPositionViewModel>>(positions);
                    var renderedView = await this.RenderViewToStringAsync("_PositionsTableRows", viewModels);

                    return Json(new
                    {
                        status = "update-view",
                        message = new[] { new { status = "success", text = "سمت با موفقیت اضافه شد" } },
                        viewList = new[]
                        {
                            new
                            {
                                elementId = "positionsTableBody",
                                view = new { result = renderedView }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddPosition", "خطا در افزودن سمت", ex);
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در ذخیره: " + ex.Message } }
                    });
                }
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => new { status = "error", text = e.ErrorMessage })
                .ToArray();

            return Json(new
            {
                status = "validation-error",
                message = errors
            });
        }

        // ==================== MEMBER MANAGEMENT ====================

        /// <summary>
        /// افزودن عضو به بخش - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddMember(int departmentId)
        {
            try
            {
                var department = _organizationRepository.GetDepartmentById(departmentId);
                if (department == null)
                    return RedirectToAction("ErrorView", "Home");

                // دریافت افراد موجود
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);

                // دریافت سمت‌های بخش
                var positions = _organizationRepository.GetDepartmentPositions(departmentId, includeInactive: false);

                ViewBag.DepartmentId = departmentId;
                ViewBag.DepartmentTitle = department.Title;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                ViewBag.Positions = new SelectList(positions, "Id", "Title");

                var model = new DepartmentMemberViewModel
                {
                    DepartmentId = departmentId,
                    IsActive = true,
                    JoinDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddMember", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره عضو جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddMember(DepartmentMemberViewModel model)
        {
            // ⭐ حذف الزام بودن PositionId از ModelState
            if (model.PositionId == 0)
            {
                ModelState.Remove(nameof(model.PositionId));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی عضویت قبلی
                    if (_organizationRepository.IsContactMemberOfDepartment(model.ContactId, model.DepartmentId))
                    {
                        ModelState.AddModelError("", "این شخص قبلاً به این بخش اضافه شده است");

                        // بازگشت به فرم
                        var department = _organizationRepository.GetDepartmentById(model.DepartmentId, includePositions: true);
                        var contacts = _contactRepository.GetAllContacts(includeInactive: false);
                        var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);

                        ViewBag.DepartmentId = model.DepartmentId;
                        ViewBag.DepartmentTitle = department?.Title;
                        ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                        ViewBag.Positions = new SelectList(positions, "Id", "Title");

                        return View(model);
                    }

                    var member = _mapper.Map<DepartmentMember>(model);
                    member.CreatorUserId = GetUserId();
                    
                    // ⭐ اگر سمت انتخاب نشده، null بگذار (Repository خودش رسیدگی می‌کند)
                    if (member.PositionId == 0)
                    {
                        member.PositionId = null;
                    }

                    var memberId = await _organizationRepository.AddMemberToDepartmentAsync(member);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddMember",
                        $"افزودن عضو به بخش",
                        recordId: memberId.ToString()
                    );

                    TempData["SuccessMessage"] = "عضو با موفقیت اضافه شد";

                    // بازگشت به چارت سازمانی
                    var dept = _organizationRepository.GetDepartmentById(model.DepartmentId);
                    return RedirectToAction(nameof(OrganizationChart), new { organizationId = dept.OrganizationId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddMember", "خطا در افزودن عضو", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // بازگشت به فرم در صورت خطا
            {
                var department = _organizationRepository.GetDepartmentById(model.DepartmentId, includePositions: true);
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);
                var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);

                ViewBag.DepartmentId = model.DepartmentId;
                ViewBag.DepartmentTitle = department?.Title;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                ViewBag.Positions = new SelectList(positions, "Id", "Title");
            }

            return View(model);
        }

        // ==================== TOGGLE ACTIVATION ====================

        /// <summary>
        /// نمایش مودال تایید فعال/غیرفعال کردن سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> ToggleActivation(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                {
                    return NotFound();
                }

                ViewBag.OrganizationId = id;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.IsActive = organization.IsActive;

                if (organization.IsActive)
                {
                    ViewBag.ModalTitle = "غیرفعال کردن سازمان";
                    ViewBag.ThemeClass = "bg-warning";
                    ViewBag.ButtonClass = "btn btn-warning";
                    ViewBag.ActionText = "غیرفعال";
                }
                else
                {
                    ViewBag.ModalTitle = "فعال کردن سازمان";
                    ViewBag.ThemeClass = "bg-success";
                    ViewBag.ButtonClass = "btn btn-success";
                    ViewBag.ActionText = "فعال";
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "ToggleActivation",
                    $"مشاهده فرم تغییر وضعیت سازمان: {organization.DisplayName}",
                    recordId: id.ToString()
                );

                return PartialView("_ToggleActivation", organization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Organizations",
                    "ToggleActivation",
                    "خطا در نمایش فرم",
                    ex,
                    recordId: id.ToString()
                );
                return StatusCode(500);
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن سازمان - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> ToggleActivationPost(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "سازمان یافت نشد" } }
                    });
                }

                var oldStatus = organization.IsActive;
                var organizationName = organization.DisplayName;

                // تغییر وضعیت
                organization.IsActive = !organization.IsActive;
                organization.LastUpdateDate = DateTime.Now;
                organization.LastUpdaterUserId = GetUserId();

                _uow.OrganizationUW.Update(organization);
                _uow.Save();

                var actionText = organization.IsActive ? "فعال" : "غیرفعال";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Organizations",
                    "ToggleActivation",
                    $"تغییر وضعیت سازمان {organizationName} به {actionText}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organizationName
                );

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "Organizations", new { area = "AdminArea" }),
                    message = new[]
                    {
                        new
                        {
                            status = "success",
                            text = $"سازمان با موفقیت {actionText} شد"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Organizations",
                    "ToggleActivation",
                    "خطا در تغییر وضعیت",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تغییر وضعیت: " + ex.Message } }
                });
            }
        }

        // ==================== ADD ORGANIZATION CONTACT ====================

        /// <summary>
        /// افزودن عضو/فرد به سازمان - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddOrganizationContact(int organizationId)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                // دریافت افراد موجود
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

                var model = new OrganizationContactViewModel
                {
                    OrganizationId = organizationId,
                    IsActive = true,
                    IsPrimary = false,
                    ImportanceLevel = 50,
                    RelationType = 0 // پیش‌فرض
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddOrganizationContact", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره عضو/فرد جدید به سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddOrganizationContact(OrganizationContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی وجود قبلی
                    var existingContact = _organizationRepository.GetOrganizationContacts(model.OrganizationId,false)
                        .FirstOrDefault(oc => oc.ContactId == model.ContactId && oc.IsActive);

                    if (existingContact != null)
                    {
                        ModelState.AddModelError("", "این شخص قبلاً به این سازمان اضافه شده است");

                        var organization = await _organizationRepository.GetOrganizationByIdAsync(model.OrganizationId);
                        var contacts = _contactRepository.GetAllContacts(includeInactive: false);

                        ViewBag.OrganizationId = model.OrganizationId;
                        ViewBag.OrganizationName = organization?.DisplayName;
                        ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

                        return View(model);
                    }

                    var organizationContact = _mapper.Map<OrganizationContact>(model);
                    organizationContact.CreatorUserId = GetUserId();

                    var contactId = await _organizationRepository.AddContactToOrganizationAsync(organizationContact);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddOrganizationContact",
                        $"افزودن فرد به سازمان",
                        recordId: contactId.ToString()
                    );

                    TempData["SuccessMessage"] = "فرد با موفقیت به سازمان اضافه شد";
                    return RedirectToAction(nameof(Details), new { id = model.OrganizationId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddOrganizationContact", "خطا در افزودن فرد", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // بازگشت به فرم در صورت خطا
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(model.OrganizationId);
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);

                ViewBag.OrganizationId = model.OrganizationId;
                ViewBag.OrganizationName = organization?.DisplayName;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
            }

            return View(model);
        }

        /// <summary>
        /// حذف عضو از سازمان
        /// </summary>
        [HttpPost]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> RemoveOrganizationContact(int id)
        {
            try
            {
                var organizationContact = _organizationRepository.GetOrganizationContactById(id);
                if (organizationContact == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "عضو یافت نشد"
                    });
                }

                var contactName = organizationContact.Contact.FullName;
                var organizationId = organizationContact.OrganizationId;

                var result = await _organizationRepository.RemoveContactFromOrganizationAsync(id);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Organizations",
                        "RemoveOrganizationContact",
                        $"حذف {contactName} از سازمان",
                        recordId: id.ToString()
                    );

                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = organizationId }),
                        message = new[]
                        {
                            new { status = "success", text = "عضو با موفقیت حذف شد" }
                        }
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = "خطا در حذف عضو"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "RemoveOrganizationContact", "خطا در حذف عضو", ex);
                return Json(new
                {
                    status = "error",
                    message = "خطا: " + ex.Message
                });
            }
        }
    }
}