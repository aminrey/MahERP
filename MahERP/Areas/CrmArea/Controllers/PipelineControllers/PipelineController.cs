using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.Services.CoreServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ⭐ برای رفع Ambiguity
using MvcSelectListItem = Microsoft.AspNetCore.Mvc.Rendering.SelectListItem;

namespace MahERP.Areas.CrmArea.Controllers.PipelineControllers
{
    /// <summary>
    /// ⭐⭐⭐ کنترلر مدیریت Pipeline و فرصت‌های فروش
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM.PIPELINE")]
    public class PipelineController : BaseController
    {
        private readonly ICrmOpportunityRepository _opportunityRepo;
        private readonly ICrmPipelineStageRepository _stageRepo;
        private readonly ICrmLeadRepository _leadRepo;
        private readonly ICoreIntegrationService _coreIntegrationService;

        public PipelineController(
            ICrmOpportunityRepository opportunityRepo,
            ICrmPipelineStageRepository stageRepo,
            ICrmLeadRepository leadRepo,
            ICoreIntegrationService coreIntegrationService,
            IUnitOfWork uow,
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
            _opportunityRepo = opportunityRepo;
            _stageRepo = stageRepo;
            _leadRepo = leadRepo;
            _coreIntegrationService = coreIntegrationService;
        }

        // ========================================
        // ⭐⭐⭐ PIPELINE BOARD (Kanban View)
        // ========================================

        /// <summary>
        /// نمای اصلی Pipeline (Kanban Board)
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.VIEW")]
        public async Task<IActionResult> Index(int? branchId = null, string? userId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // تعیین شعبه
            if (!branchId.HasValue)
            {
                var userBranches = _uow.BranchUW.Get(b => b.IsActive).ToList();
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;
            }

            // اطمینان از وجود مراحل پیش‌فرض
            await _stageRepo.EnsureDefaultStagesAsync(branchId.Value, currentUser.Id);

            // فیلتر
            var filter = new CrmOpportunityFilterViewModel
            {
                BranchId = branchId,
                AssignedUserId = userId,
                IncludeClosed = false
            };

            var board = await _opportunityRepo.GetPipelineBoardAsync(branchId.Value, filter);

            // آماده‌سازی Dropdowns
            await PrepareDropdownsAsync(branchId.Value);

            return View(board);
        }

        /// <summary>
        /// دریافت داده‌های Pipeline به صورت JSON (برای بروزرسانی AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBoardData(int branchId, string? userId = null, bool includeClosed = false)
        {
            var filter = new CrmOpportunityFilterViewModel
            {
                BranchId = branchId,
                AssignedUserId = userId,
                IncludeClosed = includeClosed
            };

            var board = await _opportunityRepo.GetPipelineBoardAsync(branchId, filter);

            return Json(new
            {
                success = true,
                columns = board.Columns.Select(c => new
                {
                    stageId = c.StageId,
                    stageName = c.StageName,
                    stageColor = c.StageColor,
                    stageIcon = c.StageIcon,
                    winProbability = c.WinProbability,
                    isWonStage = c.IsWonStage,
                    isLostStage = c.IsLostStage,
                    count = c.Count,
                    totalValue = c.TotalValue,
                    totalValueFormatted = c.TotalValueFormatted,
                    opportunities = c.Opportunities
                }),
                statistics = board.Statistics
            });
        }

        // ========================================
        // ⭐⭐⭐ OPPORTUNITY CRUD
        // ========================================

        /// <summary>
        /// صفحه ایجاد فرصت جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.CREATE")]
        public async Task<IActionResult> Create(int? branchId = null, int? contactId = null, int? organizationId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!branchId.HasValue)
            {
                var userBranches = _uow.BranchUW.Get(b => b.IsActive).ToList();
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;
            }

            var model = new CrmOpportunityCreateViewModel
            {
                BranchId = branchId.Value,
                ContactId = contactId,
                OrganizationId = organizationId,
                ExpectedCloseDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.AddMonths(1), "yyyy/MM/dd"),
                NextActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.AddDays(1), "yyyy/MM/dd"),
                NextActionTime = "09:00",
                NextActionType = CrmNextActionType.Call,
                CreateTaskForNextAction = true
            };

            await PrepareDropdownsAsync(branchId.Value);

            return View(model);
        }

        /// <summary>
        /// ذخیره فرصت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.PIPELINE.CREATE")]
        public async Task<IActionResult> Create(CrmOpportunityCreateViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // اعتبارسنجی
                if (!model.ContactId.HasValue && !model.OrganizationId.HasValue)
                {
                    ModelState.AddModelError("", "لطفاً یک فرد یا سازمان انتخاب کنید");
                    await PrepareDropdownsAsync(model.BranchId);
                    return View(model);
                }

                // دریافت مرحله پیش‌فرض
                if (!model.StageId.HasValue)
                {
                    var defaultStage = await _stageRepo.GetDefaultStageAsync(model.BranchId);
                    model.StageId = defaultStage?.Id;
                }

                var opportunity = new CrmOpportunity
                {
                    Title = model.Title,
                    Description = model.Description,
                    BranchId = model.BranchId,
                    StageId = model.StageId ?? 0,
                    ContactId = model.ContactId,
                    OrganizationId = model.OrganizationId,
                    AssignedUserId = model.AssignedUserId ?? currentUser.Id,
                    Value = model.Value,
                    Source = model.Source,
                    Notes = model.Notes,
                    NextActionType = model.NextActionType,
                    NextActionNote = model.NextActionNote,
                    CreatorUserId = currentUser.Id
                };

                // تاریخ پیش‌بینی بسته شدن
                if (!string.IsNullOrEmpty(model.ExpectedCloseDatePersian))
                {
                    opportunity.ExpectedCloseDate = ConvertDateTime.ConvertShamsiToMiladi(model.ExpectedCloseDatePersian);
                }

                // تاریخ اقدام بعدی
                if (!string.IsNullOrEmpty(model.NextActionDatePersian))
                {
                    var date = ConvertDateTime.ConvertShamsiToMiladi(model.NextActionDatePersian);
                    if (!string.IsNullOrEmpty(model.NextActionTime) && TimeSpan.TryParse(model.NextActionTime, out var time))
                    {
                        date = date.Date.Add(time);
                    }
                    opportunity.NextActionDate = date;
                }

                var created = await _opportunityRepo.CreateAsync(opportunity);

                // ایجاد تسک برای اقدام بعدی
                if (model.CreateTaskForNextAction && model.NextActionType.HasValue)
                {
                    var taskResult = await _coreIntegrationService.CreateTaskFromCrmOpportunityAsync(
                        new CrmOpportunityTaskRequest
                        {
                            OpportunityId = created.Id,
                            ActionType = model.NextActionType.Value,
                            DueDate = opportunity.NextActionDate ?? DateTime.Now.AddDays(1),
                            Description = model.NextActionNote,
                            CreatorUserId = currentUser.Id,
                            AssignedUserId = opportunity.AssignedUserId
                        });

                    if (taskResult.Success)
                    {
                        created.NextActionTaskId = taskResult.TaskId;
                        await _opportunityRepo.UpdateAsync(created);
                    }
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmOpportunity",
                    "ایجاد opportunity",
                    $"فرصت «{created.Title}» ایجاد شد",
                    recordId: created.Id.ToString(),
                    entityType: "CrmOpportunity"
                );

                TempData["SuccessMessage"] = "فرصت با موفقیت ایجاد شد";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "فرصت با موفقیت ایجاد شد",
                        opportunityId = created.Id,
                        redirectUrl = Url.Action("Details", new { id = created.Id })
                    });
                }

                return RedirectToAction("Details", new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                await PrepareDropdownsAsync(model.BranchId);
                return View(model);
            }
        }

        /// <summary>
        /// جزئیات فرصت
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            var opportunity = await _opportunityRepo.GetByIdAsync(id, includeDetails: true);
            if (opportunity == null)
                return NotFound();

            var viewModel = MapToViewModel(opportunity);

            // مراحل Pipeline
            var stages = await _stageRepo.GetByBranchAsync(opportunity.BranchId);
            ViewBag.Stages = new SelectList(stages, "Id", "Name", opportunity.StageId);

            return View(viewModel);
        }

        /// <summary>
        /// ویرایش فرصت
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            var opportunity = await _opportunityRepo.GetByIdAsync(id, includeDetails: true);
            if (opportunity == null)
                return NotFound();

            var viewModel = MapToViewModel(opportunity);
            await PrepareDropdownsAsync(opportunity.BranchId);

            return View(viewModel);
        }

        /// <summary>
        /// ذخیره تغییرات فرصت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> Edit(int id, CrmOpportunityViewModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                var currentUser = await _userManager.GetUserAsync(User);
                var opportunity = await _opportunityRepo.GetByIdAsync(id);
                if (opportunity == null)
                    return NotFound();

                // بروزرسانی
                opportunity.Title = model.Title;
                opportunity.Description = model.Description;
                opportunity.Value = model.Value;
                opportunity.Source = model.Source;
                opportunity.Tags = model.Tags;
                opportunity.Notes = model.Notes;
                opportunity.NextActionType = model.NextActionType;
                opportunity.NextActionNote = model.NextActionNote;
                opportunity.AssignedUserId = model.AssignedUserId;
                opportunity.LastUpdaterUserId = currentUser.Id;

                // تاریخ‌ها
                if (!string.IsNullOrEmpty(model.ExpectedCloseDatePersian))
                {
                    opportunity.ExpectedCloseDate = ConvertDateTime.ConvertShamsiToMiladi(model.ExpectedCloseDatePersian);
                }

                if (!string.IsNullOrEmpty(model.NextActionDatePersian))
                {
                    opportunity.NextActionDate = ConvertDateTime.ConvertShamsiToMiladi(model.NextActionDatePersian);
                }

                var result = await _opportunityRepo.UpdateAsync(opportunity);

                if (!result)
                {
                    ModelState.AddModelError("", "خطا در بروزرسانی فرصت");
                    await PrepareDropdownsAsync(opportunity.BranchId);
                    return View(model);
                }

                TempData["SuccessMessage"] = "فرصت با موفقیت بروزرسانی شد";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                await PrepareDropdownsAsync(model.BranchId);
                return View(model);
            }
        }

        // ========================================
        // ⭐⭐⭐ STAGE OPERATIONS (Drag & Drop)
        // ========================================

        /// <summary>
        /// تغییر مرحله فرصت (Drag & Drop)
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> MoveToStage(int opportunityId, int newStageId, string? note = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _opportunityRepo.MoveToStageAsync(opportunityId, newStageId, currentUser.Id, note);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در تغییر مرحله" });
                }

                var opportunity = await _opportunityRepo.GetByIdAsync(opportunityId, includeDetails: true);

                return Json(new
                {
                    success = true,
                    message = $"فرصت به مرحله «{opportunity?.Stage?.Name}» منتقل شد",
                    opportunity = opportunity != null ? new
                    {
                        id = opportunity.Id,
                        title = opportunity.Title,
                        stageId = opportunity.StageId,
                        stageName = opportunity.Stage?.Name,
                        probability = opportunity.Probability,
                        weightedValue = opportunity.WeightedValue,
                        isWon = opportunity.IsWon,
                        isLost = opportunity.IsLost
                    } : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان برنده
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> MarkAsWon(int id, string? note = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _opportunityRepo.MarkAsWonAsync(id, currentUser.Id, note);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در ثبت برنده شدن" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmOpportunity",
                    "برنده شدن فرصت",
                    $"فرصت #{id} برنده شد",
                    recordId: id.ToString(),
                    entityType: "CrmOpportunity"
                );

                return Json(new { success = true, message = "🎉 تبریک! فرصت برنده شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// علامت‌گذاری به عنوان از دست رفته
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> MarkAsLost(int id, string? lostReason = null, string? competitor = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _opportunityRepo.MarkAsLostAsync(id, currentUser.Id, lostReason, competitor);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در ثبت از دست رفتن" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmOpportunity",
                    "از دست رفتن فرصت",
                    $"فرصت #{id} از دست رفت. دلیل: {lostReason}",
                    recordId: id.ToString(),
                    entityType: "CrmOpportunity"
                );

                return Json(new { success = true, message = "فرصت به عنوان از دست رفته ثبت شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// بازگشایی فرصت بسته شده
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> Reopen(int id, int stageId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _opportunityRepo.ReopenAsync(id, stageId, currentUser.Id);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در بازگشایی فرصت" });
                }

                return Json(new { success = true, message = "فرصت مجدداً باز شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========================================
        // ⭐⭐⭐ LEAD CONVERSION
        // ========================================

        /// <summary>
        /// صفحه تبدیل Lead به Opportunity
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.CREATE")]
        public async Task<IActionResult> ConvertLead(int leadId)
        {
            var lead = await _leadRepo.GetByIdAsync(leadId, includeDetails: true);
            if (lead == null)
                return NotFound();

            // بررسی اینکه قبلاً تبدیل نشده باشد
            var existingOpportunity = await _opportunityRepo.GetByLeadAsync(leadId);
            if (existingOpportunity != null)
            {
                TempData["WarningMessage"] = "این سرنخ قبلاً به فرصت تبدیل شده است";
                return RedirectToAction("Details", new { id = existingOpportunity.Id });
            }

            var model = new ConvertLeadToOpportunityViewModel
            {
                LeadId = leadId,
                LeadName = lead.DisplayName,
                Title = $"فرصت فروش - {lead.DisplayName}",
                Value = lead.EstimatedValue,
                ExpectedCloseDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.AddMonths(1), "yyyy/MM/dd"),
                NextActionType = CrmNextActionType.Call,
                NextActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now.AddDays(1), "yyyy/MM/dd"),
                NextActionTime = "09:00",
                CreateTaskForNextAction = true,
                CloseLead = true,
                TransferInteractions = true,
                TransferFollowUps = true
            };

            // مراحل Pipeline
            var stages = await _stageRepo.GetByBranchAsync(lead.BranchId);
            ViewBag.Stages = new SelectList(stages.Where(s => !s.IsWonStage && !s.IsLostStage), "Id", "Name");

            return View(model);
        }

        /// <summary>
        /// اجرای تبدیل Lead به Opportunity
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.PIPELINE.CREATE")]
        public async Task<IActionResult> ConvertLead(ConvertLeadToOpportunityViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // اعتبارسنجی
                if (string.IsNullOrEmpty(model.NextActionDatePersian))
                {
                    ModelState.AddModelError("NextActionDatePersian", "تاریخ اقدام بعدی الزامی است");
                }

                if (!ModelState.IsValid)
                {
                    var lead = await _leadRepo.GetByIdAsync(model.LeadId, includeDetails: true);
                    var stages = await _stageRepo.GetByBranchAsync(lead?.BranchId ?? 0);
                    ViewBag.Stages = new SelectList(stages.Where(s => !s.IsWonStage && !s.IsLostStage), "Id", "Name");
                    return View(model);
                }

                var opportunity = await _opportunityRepo.CreateFromLeadAsync(model.LeadId, model, currentUser.Id);

                // ایجاد تسک برای اقدام بعدی
                if (model.CreateTaskForNextAction)
                {
                    var taskResult = await _coreIntegrationService.CreateTaskFromCrmOpportunityAsync(
                        new CrmOpportunityTaskRequest
                        {
                            OpportunityId = opportunity.Id,
                            ActionType = model.NextActionType,
                            DueDate = opportunity.NextActionDate ?? DateTime.Now.AddDays(1),
                            Description = model.NextActionNote,
                            CreatorUserId = currentUser.Id,
                            AssignedUserId = opportunity.AssignedUserId
                        });

                    if (taskResult.Success)
                    {
                        opportunity.NextActionTaskId = taskResult.TaskId;
                        await _opportunityRepo.UpdateAsync(opportunity);
                    }
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmOpportunity",
                    "تبدیل سرنخ به فرصت",
                    $"سرنخ #{model.LeadId} به فرصت #{opportunity.Id} تبدیل شد",
                    recordId: opportunity.Id.ToString(),
                    entityType: "CrmOpportunity"
                );

                TempData["SuccessMessage"] = "سرنخ با موفقیت به فرصت تبدیل شد";
                return RedirectToAction("Details", new { id = opportunity.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                var lead = await _leadRepo.GetByIdAsync(model.LeadId, includeDetails: true);
                var stages = await _stageRepo.GetByBranchAsync(lead?.BranchId ?? 0);
                ViewBag.Stages = new SelectList(stages.Where(s => !s.IsWonStage && !s.IsLostStage), "Id", "Name");
                return View(model);
            }
        }

        // ========================================
        // ⭐⭐⭐ PRODUCTS
        // ========================================

        /// <summary>
        /// افزودن محصول به فرصت
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> AddProduct(CrmOpportunityProductViewModel model)
        {
            try
            {
                var product = new CrmOpportunityProduct
                {
                    OpportunityId = model.OpportunityId,
                    ProductId = model.ProductId,
                    ProductName = model.ProductName,
                    Description = model.Description,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice,
                    DiscountPercent = model.DiscountPercent
                };

                await _opportunityRepo.AddProductAsync(product);

                return Json(new
                {
                    success = true,
                    message = "محصول اضافه شد",
                    product = new
                    {
                        id = product.Id,
                        productName = product.ProductName,
                        quantity = product.Quantity,
                        unitPrice = product.UnitPrice,
                        discountPercent = product.DiscountPercent,
                        totalAmount = product.TotalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// حذف محصول از فرصت
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.EDIT")]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            try
            {
                var result = await _opportunityRepo.RemoveProductAsync(productId);
                if (!result)
                {
                    return Json(new { success = false, message = "محصول یافت نشد" });
                }

                return Json(new { success = true, message = "محصول حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========================================
        // ⭐⭐⭐ STAGE MANAGEMENT
        // ========================================

        /// <summary>
        /// صفحه مدیریت مراحل Pipeline
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.PIPELINE.SETTINGS")]
        public async Task<IActionResult> ManageStages(int? branchId = null)
        {
            if (!branchId.HasValue)
            {
                var userBranches = _uow.BranchUW.Get(b => b.IsActive).ToList();
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;
            }

            var stages = await _stageRepo.GetByBranchAsync(branchId.Value, includeInactive: true);
            var viewModels = stages.Select(s => new CrmPipelineStageViewModel
            {
                Id = s.Id,
                BranchId = s.BranchId,
                Name = s.Name,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                ColorCode = s.ColorCode,
                Icon = s.Icon,
                WinProbability = s.WinProbability,
                IsWonStage = s.IsWonStage,
                IsLostStage = s.IsLostStage,
                IsDefault = s.IsDefault,
                IsActive = s.IsActive
            }).ToList();

            // لیست شعبه‌ها
            var branches = _uow.BranchUW.Get(b => b.IsActive).ToList();
            ViewBag.Branches = new SelectList(branches, "Id", "Name", branchId);
            ViewBag.CurrentBranchId = branchId;

            return View(viewModels);
        }

        /// <summary>
        /// ذخیره مرحله جدید
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.SETTINGS")]
        public async Task<IActionResult> SaveStage(CrmPipelineStageViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (model.Id == 0)
                {
                    // ایجاد جدید
                    var stage = new CrmPipelineStage
                    {
                        BranchId = model.BranchId,
                        Name = model.Name,
                        Description = model.Description,
                        ColorCode = model.ColorCode,
                        Icon = model.Icon,
                        WinProbability = model.WinProbability,
                        IsWonStage = model.IsWonStage,
                        IsLostStage = model.IsLostStage,
                        IsDefault = model.IsDefault,
                        CreatorUserId = currentUser.Id
                    };

                    await _stageRepo.CreateAsync(stage);
                    return Json(new { success = true, message = "مرحله ایجاد شد", stageId = stage.Id });
                }
                else
                {
                    // بروزرسانی
                    var stage = await _stageRepo.GetByIdAsync(model.Id);
                    if (stage == null)
                        return Json(new { success = false, message = "مرحله یافت نشد" });

                    stage.Name = model.Name;
                    stage.Description = model.Description;
                    stage.ColorCode = model.ColorCode;
                    stage.Icon = model.Icon;
                    stage.WinProbability = model.WinProbability;
                    stage.IsWonStage = model.IsWonStage;
                    stage.IsLostStage = model.IsLostStage;
                    stage.IsDefault = model.IsDefault;
                    stage.IsActive = model.IsActive;

                    await _stageRepo.UpdateAsync(stage);
                    return Json(new { success = true, message = "مرحله بروزرسانی شد" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// ترتیب‌بندی مجدد مراحل
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.PIPELINE.SETTINGS")]
        public async Task<IActionResult> ReorderStages(int branchId, [FromBody] List<int> stageIds)
        {
            try
            {
                var result = await _stageRepo.ReorderAsync(branchId, stageIds);
                if (!result)
                {
                    return Json(new { success = false, message = "خطا در ترتیب‌بندی" });
                }

                return Json(new { success = true, message = "ترتیب مراحل ذخیره شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========================================
        // ⭐⭐⭐ STATISTICS & REPORTS
        // ========================================

        /// <summary>
        /// آمار Pipeline
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics(int? branchId = null, string? userId = null)
        {
            var stats = await _opportunityRepo.GetStatisticsAsync(branchId, userId);
            return Json(stats);
        }

        /// <summary>
        /// لیست فرصت‌های نیازمند اقدام
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNeedingAction(string? userId = null, int? branchId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            userId ??= currentUser.Id;

            var opportunities = await _opportunityRepo.GetNeedingActionAsync(userId, branchId);
            return Json(opportunities.Select(o => new
            {
                id = o.Id,
                title = o.Title,
                customerName = o.CustomerName,
                value = o.Value,
                valueFormatted = o.ValueFormatted,
                nextActionType = o.NextActionType,
                nextActionDate = o.NextActionDate,
                nextActionDatePersian = o.NextActionDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.NextActionDate.Value, "yyyy/MM/dd HH:mm")
                    : null
            }));
        }

        // ========================================
        // ⭐⭐⭐ SEARCH
        // ========================================

        /// <summary>
        /// جستجوی فرصت‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string term, int? branchId = null)
        {
            var opportunities = await _opportunityRepo.SearchAsync(term, branchId);
            return Json(opportunities.Select(o => new
            {
                id = o.Id,
                text = o.Title,
                customerName = o.CustomerName,
                value = o.ValueFormatted,
                stage = o.Stage?.Name,
                stageColor = o.Stage?.ColorCode
            }));
        }

        // ========================================
        // ⭐⭐⭐ HELPERS
        // ========================================

        private CrmOpportunityViewModel MapToViewModel(CrmOpportunity o)
        {
            return new CrmOpportunityViewModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                BranchId = o.BranchId,
                BranchName = o.Branch?.Name,
                StageId = o.StageId,
                StageName = o.Stage?.Name,
                StageColor = o.Stage?.ColorCode,
                StageProbability = o.Stage?.WinProbability ?? 0,
                SourceLeadId = o.SourceLeadId,
                ContactId = o.ContactId,
                ContactName = o.Contact?.FullName,
                OrganizationId = o.OrganizationId,
                OrganizationName = o.Organization?.DisplayName,
                CustomerName = o.CustomerName,
                CustomerType = o.CustomerType,
                AssignedUserId = o.AssignedUserId,
                AssignedUserName = o.AssignedUser != null
                    ? $"{o.AssignedUser.FirstName} {o.AssignedUser.LastName}"
                    : null,
                Value = o.Value,
                ValueFormatted = o.ValueFormatted,
                Currency = o.Currency,
                Probability = o.Probability,
                WeightedValue = o.WeightedValue,
                WeightedValueFormatted = o.WeightedValue?.ToString("N0"),
                ExpectedCloseDate = o.ExpectedCloseDate,
                ExpectedCloseDatePersian = o.ExpectedCloseDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.ExpectedCloseDate.Value, "yyyy/MM/dd")
                    : null,
                ActualCloseDate = o.ActualCloseDate,
                ActualCloseDatePersian = o.ActualCloseDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.ActualCloseDate.Value, "yyyy/MM/dd")
                    : null,
                DaysToClose = o.DaysToClose,
                LostReason = o.LostReason,
                WinningCompetitor = o.WinningCompetitor,
                Source = o.Source,
                Tags = o.Tags,
                TagsList = o.TagsList,
                Notes = o.Notes,
                NextActionType = o.NextActionType,
                NextActionTypeText = GetNextActionTypeText(o.NextActionType),
                NextActionDate = o.NextActionDate,
                NextActionDatePersian = o.NextActionDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(o.NextActionDate.Value, "yyyy/MM/dd HH:mm")
                    : null,
                NextActionNote = o.NextActionNote,
                NextActionTaskId = o.NextActionTaskId,
                IsActive = o.IsActive,
                IsWon = o.IsWon,
                IsLost = o.IsLost,
                IsOpen = o.IsOpen,
                CreatedDate = o.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(o.CreatedDate, "yyyy/MM/dd"),
                CreatorUserId = o.CreatorUserId,
                Products = o.Products?.Where(p => p.IsActive).Select(p => new CrmOpportunityProductViewModel
                {
                    Id = p.Id,
                    OpportunityId = p.OpportunityId,
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    DiscountPercent = p.DiscountPercent,
                    TotalAmount = p.TotalAmount
                }).ToList() ?? new List<CrmOpportunityProductViewModel>(),
                ProductsCount = o.Products?.Count(p => p.IsActive) ?? 0,
                ProductsTotal = o.Products?.Where(p => p.IsActive).Sum(p => p.TotalAmount) ?? 0,
                RecentActivities = o.Activities?.OrderByDescending(a => a.ActivityDate).Take(10)
                    .Select(a => new CrmOpportunityActivityViewModel
                    {
                        Id = a.Id,
                        OpportunityId = a.OpportunityId,
                        ActivityType = a.ActivityType,
                        Title = a.Title,
                        Description = a.Description,
                        ActivityDate = a.ActivityDate,
                        ActivityDatePersian = ConvertDateTime.ConvertMiladiToShamsi(a.ActivityDate, "yyyy/MM/dd HH:mm"),
                        UserId = a.UserId,
                        UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null
                    }).ToList() ?? new List<CrmOpportunityActivityViewModel>()
            };
        }

        private async Task PrepareDropdownsAsync(int branchId)
        {
            // شعبه‌ها
            var branches = _uow.BranchUW.Get(b => b.IsActive).ToList();
            ViewBag.Branches = new SelectList(branches, "Id", "Name", branchId);

            // مراحل Pipeline
            var stages = await _stageRepo.GetByBranchAsync(branchId);
            ViewBag.Stages = new SelectList(stages.Where(s => !s.IsWonStage && !s.IsLostStage), "Id", "Name");

            // کاربران - از UserRepository استفاده می‌کنیم (0 = همه کاربران)
            var users = _userRepository.GetUserListBybranchId(0);
            ViewBag.Users = new SelectList(users.Select(u => new
            {
                Id = u.Id,
                Name = u.FullNamesString ?? $"{u.FirstName} {u.LastName}"
            }), "Id", "Name");

            // منابع
            ViewBag.Sources = DataModelLayer.StaticClasses.StaticCrmLeadStatusSeedData.SuggestedSources;

            // انواع اقدام بعدی
            ViewBag.NextActionTypes = Enum.GetValues<CrmNextActionType>()
                .Select(t => new MvcSelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = GetNextActionTypeText(t)
                }).ToList();
        }

        private string? GetNextActionTypeText(CrmNextActionType? type)
        {
            if (!type.HasValue) return null;

            return type.Value switch
            {
                CrmNextActionType.Call => "📞 تماس تلفنی",
                CrmNextActionType.Meeting => "🤝 جلسه حضوری",
                CrmNextActionType.Email => "📧 ارسال ایمیل",
                CrmNextActionType.Sms => "💬 ارسال پیامک",
                CrmNextActionType.SendQuote => "📋 ارسال پیشنهاد قیمت",
                CrmNextActionType.FollowUpQuote => "🔄 پیگیری پیشنهاد",
                CrmNextActionType.Visit => "🏢 بازدید",
                CrmNextActionType.Demo => "💻 دمو محصول",
                CrmNextActionType.Other => "📝 سایر",
                _ => type.Value.ToString()
            };
        }
    }
}
