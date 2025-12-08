using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.OrganizationGroupRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// کنترلر اصلی مدیریت سازمان‌ها
    /// تقسیم شده به Partial Controllers برای مدیریت بهتر کد
    /// </summary>
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("ORG")]
    public partial class OrganizationsController : BaseController
    {
        protected readonly IOrganizationRepository _organizationRepository;
        protected readonly IContactRepository _contactRepository;
        protected readonly IPositionRepository _positionRepository; // ⭐⭐⭐ NEW
        protected readonly IUnitOfWork _uow;
        protected new readonly UserManager<AppUsers> _userManager;
        protected readonly IMapper _mapper;
        protected readonly IOrganizationGroupRepository _organizationGroupRepository;

        public OrganizationsController(
            IOrganizationRepository organizationRepository,
            IContactRepository contactRepository,
            IPositionRepository positionRepository, // ⭐⭐⭐ NEW
            IUnitOfWork uow,
            IOrganizationGroupRepository organizationGroupRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository BaseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _organizationRepository = organizationRepository;
            _contactRepository = contactRepository;
            _positionRepository = positionRepository; // ⭐⭐⭐ NEW
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
            _organizationGroupRepository = organizationGroupRepository;
        }

        // ==================== INDEX ====================

        /// <summary>
        /// لیست سازمان‌ها با فیلتر و گروه‌بندی
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> Index(string searchTerm = null, byte? organizationType = null, int? groupId = null)
        {
            try
            {
                List<DataModelLayer.Entities.Contacts.Organization> organizations;

                if (groupId.HasValue)
                {
                    organizations = _organizationGroupRepository.GetGroupOrganizations(groupId.Value, includeInactive: false);
                    ViewBag.SelectedGroupId = groupId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    organizations = _organizationRepository.SearchOrganizations(searchTerm, organizationType, true);
                }
                else
                {
                    organizations = _organizationRepository.GetAllOrganizations(true, organizationType);
                }

                organizations = organizations.OrderBy(o => o.Name).ToList();
                var viewModels = _mapper.Map<List<OrganizationViewModel>>(organizations);

                var groupedOrganizations = viewModels
                    .GroupBy(o =>
                    {
                        if (string.IsNullOrEmpty(o.Name)) return "#";
                        var firstChar = o.Name[0];
                        return char.IsLetter(firstChar) ? char.ToUpper(firstChar).ToString() : "#";
                    })
                    .OrderBy(g => g.Key == "#" ? "ي" : g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.GroupedOrganizations = groupedOrganizations;
                ViewBag.AlphabeticalIndex = groupedOrganizations.Keys.ToList();
                ViewBag.OrganizationGroups = _organizationGroupRepository.GetAllGroups(includeInactive: false);

                var organizationIds = organizations.Select(o => o.Id).ToList();
                var organizationGroupsDict = new Dictionary<int, List<OrganizationGroup>>();
                foreach (var orgId in organizationIds)
                {
                    organizationGroupsDict[orgId] = _organizationGroupRepository.GetOrganizationGroups(orgId);
                }
                ViewBag.OrganizationGroupsDict = organizationGroupsDict;

                ViewBag.SearchTerm = searchTerm;
                ViewBag.OrganizationType = organizationType;

                await _activityLogger.LogActivityAsync(ActivityTypeEnum.View, "Organizations", "Index", "مشاهده لیست سازمان‌ها");

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

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

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// دریافت حرف اول برای گروه‌بندی
        /// </summary>
        protected string GetFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text)) return "#";
            var firstChar = text[0];
            return char.IsLetter(firstChar) ? char.ToUpper(firstChar).ToString() : "#";
        }
    }
}
