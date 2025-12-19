using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
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
    /// کنترلر مدیریت تعاملات
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionRepository _interactionRepo;
        private readonly IInteractionTypeRepository _interactionTypeRepo;
        private readonly ILeadStageStatusRepository _leadStageStatusRepo;
        private readonly IPostPurchaseStageRepository _postPurchaseStageRepo;
        private readonly IGoalRepository _goalRepo;
        private readonly IContactRepository _contactRepo;
        private readonly IReferralRepository _referralRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IOrganizationRepository _organizationRepo;

        public InteractionController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IInteractionRepository interactionRepo,
            IInteractionTypeRepository interactionTypeRepo,
            ILeadStageStatusRepository leadStageStatusRepo,
            IPostPurchaseStageRepository postPurchaseStageRepo,
            IGoalRepository goalRepo,
            IContactRepository contactRepo,
            IReferralRepository referralRepo,
            IBranchRepository branchRepo,
            IOrganizationRepository organizationRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _interactionRepo = interactionRepo;
            _interactionTypeRepo = interactionTypeRepo;
            _leadStageStatusRepo = leadStageStatusRepo;
            _postPurchaseStageRepo = postPurchaseStageRepo;
            _goalRepo = goalRepo;
            _contactRepo = contactRepo;
            _referralRepo = referralRepo;
            _branchRepo = branchRepo;
            _organizationRepo = organizationRepo;
        }

        #region صفحه اصلی لیست تعاملات

        /// <summary>
        /// صفحه اصلی مشاهده تعاملات (با انتخاب شعبه و Stakeholder)
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

            // دریافت 10 تعامل recent برای Sidebar
            var recentInteractions = await _interactionRepo.GetRecentInteractionsAsync(10);
            ViewBag.RecentInteractions = recentInteractions;

            return View();
        }

        /// <summary>
        /// بارگذاری محتوای Stakeholder پس از انتخاب (AJAX Partial)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadStakeholderContent(int? contactId, int? organizationId, int? branchId)
        {
            var viewModel = new StakeholderInteractionPageViewModel
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
                    viewModel.ContactPhone = contact.PrimaryEmail; // TODO: Get from ContactPhones
                    viewModel.ContactEmail = contact.PrimaryEmail;

                    // آمار
                    viewModel.TotalInteractionsCount = await _interactionRepo.GetInteractionCountByContactAsync(contactId.Value);
                    var lastInteraction = await _interactionRepo.GetLastInteractionForContactAsync(contactId.Value);
                    viewModel.LastInteractionDatePersian = lastInteraction != null 
                        ? ConvertDateTime.ConvertMiladiToShamsi(lastInteraction.InteractionDate, "yyyy/MM/dd") 
                        : null;

                    // اهداف
                    viewModel.Goals = await _goalRepo.GetGoalSummariesForContactAsync(contactId.Value);
                    viewModel.ActiveGoalsCount = viewModel.Goals.Count(g => g.IsActive && !g.IsConverted);
                }
            }
            else if (organizationId.HasValue)
            {
                var org = await _organizationRepo.GetOrganizationByIdAsync(organizationId.Value);
                if (org != null)
                {
                    viewModel.StakeholderType = StakeholderType.Organization;
                    viewModel.OrganizationName = org.Name;
                    viewModel.OrganizationPhone = org.PrimaryPhone;
                    viewModel.OrganizationAddress = org.Address;

                    // اهداف سازمان
                    viewModel.Goals = await _goalRepo.GetGoalSummariesForOrganizationAsync(organizationId.Value);
                    viewModel.ActiveGoalsCount = viewModel.Goals.Count(g => g.IsActive && !g.IsConverted);
                }
            }

            return PartialView("_StakeholderInteractionContent", viewModel);
        }

        /// <summary>
        /// بارگذاری تعاملات یک هدف خاص (AJAX Partial با صفحه‌بندی)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGoalInteractions(int goalId, int page = 1, InteractionFilterViewModel? filter = null)
        {
            var goal = await _goalRepo.GetByIdAsync(goalId);
            if (goal == null)
                return PartialView("_GoalInteractionsList", new GoalInteractionsViewModel());

            var (interactions, totalCount) = await _interactionRepo.GetByGoalAsync(goalId, filter, page, 10);

            var viewModel = new GoalInteractionsViewModel
            {
                GoalId = goalId,
                GoalTitle = goal.Title,
                Interactions = interactions.Select(i => MapToViewModel(i)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = 10,
                Filters = filter
            };

            return PartialView("_GoalInteractionsList", viewModel);
        }

        /// <summary>
        /// بارگذاری تعاملات Contact (بدون گروه‌بندی بر اساس هدف)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadContactInteractions(int contactId, int page = 1, InteractionFilterViewModel? filter = null)
        {
            var (interactions, totalCount) = await _interactionRepo.GetByContactPagedAsync(contactId, filter, page, 10);

            var viewModel = new StakeholderInteractionsViewModel
            {
                ContactId = contactId,
                Interactions = interactions.Select(i => MapToViewModel(i)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = 10,
                Filters = filter
            };

            return PartialView("_StakeholderInteractionsList", viewModel);
        }

        /// <summary>
        /// بارگذاری تعاملات Organization
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadOrganizationInteractions(int organizationId, int page = 1, InteractionFilterViewModel? filter = null)
        {
            var (interactions, totalCount) = await _interactionRepo.GetByOrganizationPagedAsync(organizationId, filter, page, 10);

            var viewModel = new StakeholderInteractionsViewModel
            {
                OrganizationId = organizationId,
                Interactions = interactions.Select(i => MapToViewModel(i)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = 10,
                Filters = filter
            };

            return PartialView("_StakeholderInteractionsList", viewModel);
        }

        /// <summary>
        /// بارگذاری مجدد 10 تعامل recent (برای Sidebar)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentInteractions()
        {
            var recentInteractions = await _interactionRepo.GetRecentInteractionsAsync(10);
            return PartialView("_RecentInteractionsSidebar", new RecentInteractionsViewModel { Interactions = recentInteractions });
        }

        #endregion

        /// <summary>
        /// لیست تعاملات یک Contact خاص
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByContact(int contactId)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "فرد یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var interactions = await _interactionRepo.GetByContactAsync(contactId, includeGoals: true);

            ViewBag.Contact = contact;
            ViewBag.ContactName = contact.FullName;

            var viewModel = interactions.Select(i => MapToViewModel(i)).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// صفحه ایجاد تعامل جدید - مرحله 1: انتخاب شعبه و مخاطب
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int? contactId = null, int? organizationId = null, int? branchId = null)
        {
            var userId = _userManager.GetUserId(User);
            var userBranches = _branchRepo.GetBrnachListByUserId(userId!);

            ViewBag.UserBranches = userBranches;
            ViewBag.HasSingleBranch = userBranches.Count == 1;
            ViewBag.DefaultBranchId = userBranches.Count == 1 ? userBranches.First().Id : branchId;
            ViewBag.DefaultBranchName = userBranches.Count == 1 ? userBranches.First().Name : null;

            // اگر هیچ مخاطبی انتخاب نشده، به صفحه انتخاب مخاطب برو
            if (!contactId.HasValue)
            {
                return View("SelectContact");
            }

            var viewModel = new InteractionCreateViewModel
            {
                ContactId = contactId.Value,
                OrganizationId = organizationId,
                BranchId = branchId ?? ViewBag.DefaultBranchId,
                InteractionTypes = await GetInteractionTypesAsync(),
                PostPurchaseStages = await GetPostPurchaseStagesAsync()
            };

            var contact = await _contactRepo.GetByIdAsync(contactId.Value);
            if (contact != null)
            {
                ViewBag.ContactName = contact.FullName;
                ViewBag.ContactType = contact.ContactType;
                viewModel.AvailableGoals = await GetGoalsForContactAsync(contactId.Value);
            }

            // اگر سازمان انتخاب شده، نامش رو بگیر
            if (organizationId.HasValue)
            {
                var org = await _organizationRepo.GetOrganizationByIdAsync(organizationId.Value);
                if (org != null)
                {
                    ViewBag.OrganizationName = org.Name;
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// مودال انتخاب Contact بر اساس شعبه
        /// </summary>
        [HttpGet]
        public IActionResult SelectContactModal(int branchId)
        {
            var branchContacts = _branchRepo.GetBranchContacts(branchId, includeInactive: false);
            var contacts = branchContacts.Select(bc => bc.Contact).ToList();

            ViewBag.BranchId = branchId;
            ViewBag.BranchName = _branchRepo.GetBranchById(branchId)?.Name;

            return PartialView("_SelectContactModal", contacts);
        }

        /// <summary>
        /// مودال انتخاب Organization بر اساس شعبه
        /// </summary>
        [HttpGet]
        public IActionResult SelectOrganizationModal(int branchId)
        {
            var branchOrganizations = _branchRepo.GetBranchOrganizations(branchId);
            var organizations = branchOrganizations.Select(bo => bo.Organization).ToList();

            ViewBag.BranchId = branchId;
            ViewBag.BranchName = _branchRepo.GetBranchById(branchId)?.Name;

            return PartialView("_SelectOrganizationModal", organizations);
        }

        /// <summary>
        /// دریافت اعضای سازمان (AJAX) - شامل OrganizationContacts و DepartmentMembers
        /// </summary>
        [HttpGet]
        public IActionResult GetOrganizationMembers(int organizationId)
        {
            var allMembers = new List<dynamic>();
            
            // 1. دریافت OrganizationContacts (روابط فرد با سازمان)
            var orgContacts = _organizationRepo.GetOrganizationContacts(organizationId, relationType: null, includeInactive: false);
            foreach (var oc in orgContacts)
            {
                if (oc.Contact != null)
                {
                    allMembers.Add(new
                    {
                        contactId = oc.ContactId,
                        contactFullName = oc.Contact.FullName,
                        positionTitle = oc.JobTitle ?? GetRelationTypeName(oc.RelationType),
                        departmentName = (string?)null,
                        source = "OrganizationContact"
                    });
                }
            }
            
            // 2. دریافت DepartmentMembers (اعضای چارت سازمانی)
            var departments = _organizationRepo.GetOrganizationDepartments(organizationId, includeInactive: false);
            foreach (var dept in departments)
            {
                var members = _organizationRepo.GetDepartmentMembers(dept.Id, includeInactive: false);
                foreach (var m in members)
                {
                    if (m.Contact != null)
                    {
                        allMembers.Add(new
                        {
                            contactId = m.ContactId,
                            contactFullName = m.Contact.FullName,
                            positionTitle = m.Position?.Title,
                            departmentName = dept.Title,
                            source = "DepartmentMember"
                        });
                    }
                }
            }
            
            // حذف تکراری‌ها (یک فرد ممکنه هم در OrganizationContacts و هم در DepartmentMembers باشه)
            var uniqueMembers = allMembers
                .GroupBy(m => (int)m.contactId)
                .Select(g => {
                    // ترجیح DepartmentMember (چون سمت دقیق‌تری داره)
                    var deptMember = g.FirstOrDefault(x => (string)x.source == "DepartmentMember");
                    return deptMember ?? g.First();
                })
                .Select(m => new
                {
                    contactId = (int)m.contactId,
                    contactFullName = (string)m.contactFullName,
                    positionTitle = (string?)m.positionTitle,
                    departmentName = (string?)m.departmentName
                })
                .ToList();

            return Json(new
            {
                success = true,
                members = uniqueMembers,
                totalCount = uniqueMembers.Count
            });
        }
        
        /// <summary>
        /// دریافت نام نوع رابطه
        /// </summary>
        private string GetRelationTypeName(byte relationType)
        {
            return relationType switch
            {
                0 => "کارمند",
                1 => "سهامدار",
                2 => "مدیرعامل",
                3 => "رئیس هیئت مدیره",
                4 => "عضو هیئت مدیره",
                5 => "مشاور",
                6 => "پیمانکار",
                7 => "نماینده",
                _ => "سایر"
            };
        }

        /// <summary>
        /// ایجاد تعامل جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InteractionCreateViewModel model)
        {
            // بررسی الزامی بودن انتخاب حداقل یک هدف
            if (model.GoalIds == null || !model.GoalIds.Any())
            {
                ModelState.AddModelError("GoalIds", "لطفاً حداقل یک هدف انتخاب کنید");
                model.InteractionTypes = await GetInteractionTypesAsync();
                model.PostPurchaseStages = await GetPostPurchaseStagesAsync();
                if (model.ContactId > 0)
                    model.AvailableGoals = await GetGoalsForContactAsync(model.ContactId);
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.InteractionTypes = await GetInteractionTypesAsync();
                model.PostPurchaseStages = await GetPostPurchaseStagesAsync();
                if (model.ContactId > 0)
                    model.AvailableGoals = await GetGoalsForContactAsync(model.ContactId);
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // Parse date
            DateTime interactionDate = DateTime.Now;
            if (!string.IsNullOrEmpty(model.InteractionDatePersian))
            {
                interactionDate = ConvertDateTime.ConvertShamsiToMiladi(model.InteractionDatePersian);
                if (!string.IsNullOrEmpty(model.InteractionTime) && TimeSpan.TryParse(model.InteractionTime, out var time))
                {
                    interactionDate = interactionDate.Date.Add(time);
                }
            }

            DateTime? nextActionDate = null;
            if (!string.IsNullOrEmpty(model.NextActionDatePersian))
            {
                nextActionDate = ConvertDateTime.ConvertShamsiToMiladi(model.NextActionDatePersian);
            }

            var interaction = new Interaction
            {
                ContactId = model.ContactId,
                OrganizationId = model.OrganizationId, // ⭐ اضافه شد
                InteractionTypeId = model.InteractionTypeId,
                PostPurchaseStageId = model.PostPurchaseStageId,
                Subject = model.Subject,
                Description = model.Description,
                InteractionDate = interactionDate,
                DurationMinutes = model.DurationMinutes,
                Result = model.Result,
                NextAction = model.NextAction,
                NextActionDate = nextActionDate,
                HasReferral = model.HasReferral,
                IsReferred = model.IsReferred,
                CreatorUserId = userId!
            };

            var createdInteraction = await _interactionRepo.CreateAsync(interaction, model.GoalIds);

            // ثبت Referral اگر وجود داشته باشد
            if (model.HasReferral && model.ReferredContactId.HasValue)
            {
                // مشتری کسی رو معرفی کرده
                await CreateReferralAsync(model.ContactId, model.ReferredContactId.Value, createdInteraction.Id, null, userId!);
            }
            else if (model.IsReferred && model.ReferrerContactId.HasValue)
            {
                // این لید توسط کسی معرفی شده
                await CreateReferralAsync(model.ReferrerContactId.Value, model.ContactId, null, createdInteraction.Id, userId!);
            }

            TempData["SuccessMessage"] = "تعامل با موفقیت ثبت شد";
            
            // ریدایرکت بر اساس اینکه Organization داریم یا نه
            if (model.OrganizationId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { organizationId = model.OrganizationId });
            }
            
            return RedirectToAction(nameof(Index), new { contactId = model.ContactId });
        }

        /// <summary>
        /// جزئیات تعامل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var interaction = await _interactionRepo.GetByIdAsync(id, includeGoals: true);
            if (interaction == null)
            {
                TempData["ErrorMessage"] = "تعامل یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = MapToViewModel(interaction);
            return View(viewModel);
        }

        /// <summary>
        /// حذف تعامل (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? contactId = null)
        {
            var success = await _interactionRepo.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = "تعامل با موفقیت حذف شد";
            else
                TempData["ErrorMessage"] = "خطا در حذف تعامل";

            if (contactId.HasValue)
                return RedirectToAction(nameof(ByContact), new { contactId });

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private InteractionViewModel MapToViewModel(Interaction i)
        {
            return new InteractionViewModel
            {
                Id = i.Id,
                ContactId = i.ContactId,
                ContactName = i.Contact?.FullName,
                ContactType = i.Contact?.ContactType,
                ContactTypeName = GetContactTypeName(i.Contact?.ContactType),
                InteractionTypeId = i.InteractionTypeId,
                InteractionTypeName = i.InteractionType?.Title,
                InteractionTypeColor = i.InteractionType?.ColorCode,
                LeadStageName = i.InteractionType?.LeadStageStatus?.Title,
                LeadStageColor = i.InteractionType?.LeadStageStatus?.ColorCode,
                PostPurchaseStageId = i.PostPurchaseStageId,
                PostPurchaseStageName = i.PostPurchaseStage?.Title,
                PostPurchaseStageColor = i.PostPurchaseStage?.ColorCode,
                Subject = i.Subject,
                Description = i.Description,
                InteractionDate = i.InteractionDate,
                InteractionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(i.InteractionDate, "yyyy/MM/dd"),
                InteractionTime = i.InteractionDate.ToString("HH:mm"),
                DurationMinutes = i.DurationMinutes,
                Result = i.Result,
                NextAction = i.NextAction,
                NextActionDate = i.NextActionDate,
                NextActionDatePersian = i.NextActionDate.HasValue ? ConvertDateTime.ConvertMiladiToShamsi(i.NextActionDate.Value, "yyyy/MM/dd") : null,
                HasReferral = i.HasReferral,
                IsReferred = i.IsReferred,
                CreatedDate = i.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(i.CreatedDate, "yyyy/MM/dd"),
                CreatorName = i.Creator?.UserName,
                Goals = i.InteractionGoals?.Select(ig => new GoalViewModel
                {
                    Id = ig.Goal.Id,
                    Title = ig.Goal.Title,
                    ProductName = ig.Goal.ProductName
                }).ToList() ?? new List<GoalViewModel>()
            };
        }

        private string GetContactTypeName(ContactType? type)
        {
            return type switch
            {
                ContactType.Lead => "سرنخ",
                ContactType.Customer => "مشتری",
                ContactType.Partner => "شریک تجاری",
                ContactType.Other => "سایر",
                _ => "نامشخص"
            };
        }

        private async Task<List<InteractionTypeViewModel>> GetInteractionTypesAsync()
        {
            var types = await _interactionTypeRepo.GetAllAsync();
            return types.Select(t => new InteractionTypeViewModel
            {
                Id = t.Id,
                Title = t.Title,
                LeadStageStatusTitle = t.LeadStageStatus?.Title,
                LeadStageStatusColor = t.LeadStageStatus?.ColorCode,
                ColorCode = t.ColorCode,
                Icon = t.Icon
            }).ToList();
        }

        private async Task<List<PostPurchaseStageViewModel>> GetPostPurchaseStagesAsync()
        {
            var stages = await _postPurchaseStageRepo.GetAllAsync();
            return stages.Select(s => new PostPurchaseStageViewModel
            {
                Id = s.Id,
                Title = s.Title,
                ColorCode = s.ColorCode,
                Icon = s.Icon
            }).ToList();
        }

        private async Task<List<GoalViewModel>> GetGoalsForContactAsync(int contactId)
        {
            var goals = await _goalRepo.GetByContactAsync(contactId);
            return goals.Select(g => new GoalViewModel
            {
                Id = g.Id,
                Title = g.Title,
                ProductName = g.ProductName,
                IsConverted = g.IsConverted,
                CurrentLeadStageStatusTitle = g.CurrentLeadStageStatus?.Title
            }).ToList();
        }

        private async Task CreateReferralAsync(int referrerContactId, int referredContactId, int? referrerInteractionId, int? referredInteractionId, string userId)
        {
            try
            {
                // بررسی اینکه معرفی‌کننده مشتری باشد
                var referrer = await _contactRepo.GetByIdAsync(referrerContactId);
                if (referrer?.ContactType != ContactType.Customer)
                {
                    // فقط log می‌کنیم - خطا نمی‌دهیم
                    return;
                }

                var referral = new Referral
                {
                    ReferrerContactId = referrerContactId,
                    ReferredContactId = referredContactId,
                    ReferrerInteractionId = referrerInteractionId,
                    ReferredInteractionId = referredInteractionId,
                    CreatorUserId = userId
                };

                await _referralRepo.CreateAsync(referral);
            }
            catch
            {
                // خطا را نادیده بگیر - اصل عملیات تعامل مهم‌تر است
            }
        }

        #endregion
    }
}
