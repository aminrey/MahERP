using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// کنترلر AJAX برای عملیات CRM
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    public class CrmAjaxController : BaseController
    {
        private readonly IContactRepository _contactRepo;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IBranchRepository _branchRepo;

        public CrmAjaxController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IContactRepository contactRepo,
            IOrganizationRepository organizationRepo,
            IBranchRepository branchRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _contactRepo = contactRepo;
            _organizationRepo = organizationRepo;
            _branchRepo = branchRepo;
        }

        /// <summary>
        /// جستجوی Contacts برای Select2 (با فیلتر شعبه)
        /// </summary>
        [HttpGet]
        public IActionResult SearchContacts(string? term, int? branchId)
        {
            try
            {
                IEnumerable<dynamic> contacts;

                if (branchId.HasValue && branchId.Value > 0)
                {
                    // فیلتر بر اساس شعبه
                    var branchContacts = _branchRepo.GetBranchContacts(branchId.Value, includeInactive: false);
                    
                    contacts = branchContacts
                        .Where(bc => bc.Contact != null && bc.Contact.IsActive)
                        .Where(bc => string.IsNullOrEmpty(term) || 
                                     bc.Contact!.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                     (bc.Contact.NationalCode != null && bc.Contact.NationalCode.Contains(term)))
                        .Take(20)
                        .Select(bc => new
                        {
                            id = bc.Contact!.Id,
                            text = bc.Contact.FullName + (bc.Contact.NationalCode != null ? $" ({bc.Contact.NationalCode})" : ""),
                            contactType = (int)bc.Contact.ContactType
                        })
                        .ToList();
                }
                else
                {
                    // جستجوی عمومی (بدون فیلتر شعبه)
                    var allContacts = _contactRepo.SearchContactsAsync(term ?? "", 20).Result;
                    
                    contacts = allContacts.Select(c => new
                    {
                        id = c.Id,
                        text = c.FullName + (c.NationalCode != null ? $" ({c.NationalCode})" : ""),
                        contactType = (int)c.ContactType
                    }).ToList();
                }

                return Json(new { results = contacts });
            }
            catch (Exception ex)
            {
                return Json(new { results = new List<object>(), error = ex.Message });
            }
        }

        /// <summary>
        /// جستجوی Organizations برای Select2 (با فیلتر شعبه)
        /// </summary>
        [HttpGet]
        public IActionResult SearchOrganizations(string? term, int? branchId)
        {
            try
            {
                IEnumerable<dynamic> organizations;

                if (branchId.HasValue && branchId.Value > 0)
                {
                    // فیلتر بر اساس شعبه
                    var branchOrganizations = _branchRepo.GetBranchOrganizations(branchId.Value);
                    
                    organizations = branchOrganizations
                        .Where(bo => bo.Organization != null && bo.Organization.IsActive)
                        .Where(bo => string.IsNullOrEmpty(term) || 
                                     bo.Organization!.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                     (bo.Organization.RegistrationNumber != null && bo.Organization.RegistrationNumber.Contains(term)))
                        .Take(20)
                        .Select(bo => new
                        {
                            id = bo.Organization!.Id,
                            text = bo.Organization.Name + (bo.Organization.RegistrationNumber != null ? $" ({bo.Organization.RegistrationNumber})" : "")
                        })
                        .ToList();
                }
                else
                {
                    // جستجوی عمومی (بدون فیلتر شعبه)
                    var allOrganizations = _organizationRepo.SearchOrganizationsAsync(term ?? "", 20).Result;
                    
                    organizations = allOrganizations.Select(o => new
                    {
                        id = o.Id,
                        text = o.Name + (o.RegistrationNumber != null ? $" ({o.RegistrationNumber})" : "")
                    }).ToList();
                }

                return Json(new { results = organizations });
            }
            catch (Exception ex)
            {
                return Json(new { results = new List<object>(), error = ex.Message });
            }
        }

        /// <summary>
        /// دریافت Contacts یک شعبه
        /// </summary>
        [HttpGet]
        public IActionResult GetBranchContacts(int branchId)
        {
            var branchContacts = _branchRepo.GetBranchContacts(branchId, includeInactive: false);
            
            var contacts = branchContacts
                .Where(bc => bc.Contact != null && bc.Contact.IsActive)
                .Select(bc => new
                {
                    id = bc.Contact!.Id,
                    fullName = bc.Contact.FullName,
                    contactType = (int)bc.Contact.ContactType,
                    nationalCode = bc.Contact.NationalCode
                })
                .OrderBy(c => c.fullName)
                .ToList();

            return Json(contacts);
        }

        /// <summary>
        /// دریافت Organizations یک شعبه
        /// </summary>
        [HttpGet]
        public IActionResult GetBranchOrganizations(int branchId)
        {
            var branchOrganizations = _branchRepo.GetBranchOrganizations(branchId);
            
            var organizations = branchOrganizations
                .Where(bo => bo.Organization != null && bo.Organization.IsActive)
                .Select(bo => new
                {
                    id = bo.Organization!.Id,
                    name = bo.Organization.Name,
                    registrationNumber = bo.Organization.RegistrationNumber
                })
                .OrderBy(o => o.name)
                .ToList();

            return Json(organizations);
        }

        /// <summary>
        /// دریافت Goals یک Contact
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGoalsByContact(int contactId)
        {
            // TODO: Implement with GoalRepository
            return Json(new List<object>());
        }

        /// <summary>
        /// دریافت نوع Contact
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContactType(int contactId)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId);
            if (contact == null)
            {
                return Json(new { isCustomer = false });
            }

            return Json(new
            {
                isCustomer = contact.ContactType == DataModelLayer.Enums.ContactType.Customer,
                contactType = (int)contact.ContactType
            });
        }
    }
}
