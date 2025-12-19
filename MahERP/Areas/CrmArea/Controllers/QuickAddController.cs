using AutoMapper;
using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.Helpers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// کنترلر افزودن سریع Contact و Organization
    /// برای استفاده در فرم‌های Interaction و Goal
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM")]
    public partial class QuickAddController : BaseController
    {
        private readonly IContactRepository _contactRepo;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public QuickAddController(
            IContactRepository contactRepo,
            IOrganizationRepository organizationRepo,
            IBranchRepository branchRepo,
            IUnitOfWork uow,
            IMapper mapper,
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
            _contactRepo = contactRepo;
            _organizationRepo = organizationRepo;
            _branchRepo = branchRepo;
            _uow = uow;
            _mapper = mapper;
        }

        // ==================== MODALS ====================

        /// <summary>
        /// مودال انتخاب نوع (Contact یا Organization)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ⭐ موقتاً برای تست
        public IActionResult SelectTypeModal(int branchId, int? organizationId = null)
        {
            try
            {
                ViewBag.BranchId = branchId;
                ViewBag.OrganizationId = organizationId;
                
                // اگر سازمان انتخاب شده، فقط گزینه Contact نشون بده
                ViewBag.HasOrganization = organizationId.HasValue;
                
                return PartialView("_SelectTypeModal");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// مودال افزودن سریع Contact
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ⭐ موقتاً برای تست
        public IActionResult QuickAddContactModal(int branchId, int? organizationId = null)
        {
            try
            {
                var model = new QuickAddContactViewModel
                {
                    BranchId = branchId,
                    OrganizationId = organizationId
                };

                var branch = _branchRepo.GetBranchById(branchId);
                ViewBag.BranchName = branch?.Name ?? "نامشخص";
                
                if (organizationId.HasValue)
                {
                    var org = _organizationRepo.GetOrganizationById(organizationId.Value);
                    ViewBag.OrganizationName = org?.DisplayName ?? "نامشخص";
                }

                return PartialView("_QuickAddContactModal", model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// مودال افزودن سریع Organization
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ⭐ موقتاً برای تست
        public IActionResult QuickAddOrganizationModal(int branchId)
        {
            try
            {
                var model = new QuickAddOrganizationViewModel
                {
                    BranchId = branchId
                };

                var branch = _branchRepo.GetBranchById(branchId);
                ViewBag.BranchName = branch?.Name ?? "نامشخص";

                return PartialView("_QuickAddOrganizationModal", model);
            }
            catch (Exception ex)
            {
                // Log error
                return Content($"Error: {ex.Message}");
            }
        }

        // به فایل بعدی ادامه می‌دیم (CRUD Actions)
    }
}
