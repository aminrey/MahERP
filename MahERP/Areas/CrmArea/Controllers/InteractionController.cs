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
            IReferralRepository referralRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _interactionRepo = interactionRepo;
            _interactionTypeRepo = interactionTypeRepo;
            _leadStageStatusRepo = leadStageStatusRepo;
            _postPurchaseStageRepo = postPurchaseStageRepo;
            _goalRepo = goalRepo;
            _contactRepo = contactRepo;
            _referralRepo = referralRepo;
        }

        /// <summary>
        /// لیست تعاملات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(InteractionFilterViewModel? filter = null, int page = 1)
        {
            filter ??= new InteractionFilterViewModel();

            var (interactions, totalCount) = await _interactionRepo.GetListAsync(filter, page, 20);

            var viewModel = new InteractionListViewModel
            {
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = 20,
                Filters = filter,
                Interactions = interactions.Select(i => MapToViewModel(i)).ToList()
            };

            return View(viewModel);
        }

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
        /// صفحه ایجاد تعامل جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int? contactId = null)
        {
            var viewModel = new InteractionCreateViewModel
            {
                ContactId = contactId ?? 0,
                InteractionTypes = await GetInteractionTypesAsync(),
                PostPurchaseStages = await GetPostPurchaseStagesAsync()
            };

            // اگر Contact مشخص شده، اهداف آن را بگیر
            if (contactId.HasValue)
            {
                var contact = await _contactRepo.GetByIdAsync(contactId.Value);
                if (contact != null)
                {
                    ViewBag.ContactName = contact.FullName;
                    ViewBag.ContactType = contact.ContactType;
                    viewModel.AvailableGoals = await GetGoalsForContactAsync(contactId.Value);
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// ایجاد تعامل جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InteractionCreateViewModel model)
        {
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
            return RedirectToAction(nameof(ByContact), new { contactId = model.ContactId });
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
