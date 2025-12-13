using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
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

// ⭐⭐⭐ Alias برای جلوگیری از تداخل
using CrmSelectListItem = MahERP.DataModelLayer.ViewModels.CrmViewModels.CrmSelectListItem;

namespace MahERP.Areas.CrmArea.Controllers.LeadControllers
{
    /// <summary>
    /// کنترلر مدیریت سرنخ‌های CRM
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM.LEAD")]
    public class LeadsController : BaseController
    {
        private readonly ICrmLeadRepository _leadRepo;
        private readonly ICrmLeadStatusRepository _statusRepo;
        private readonly ICrmLeadInteractionRepository _interactionRepo;
        private readonly ICrmFollowUpRepository _followUpRepo;
        private readonly IContactRepository _contactRepo;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly ICoreIntegrationService _coreIntegrationService;

        public LeadsController(
            ICrmLeadRepository leadRepo,
            ICrmLeadStatusRepository statusRepo,
            ICrmLeadInteractionRepository interactionRepo,
            ICrmFollowUpRepository followUpRepo,
            IContactRepository contactRepo,
            IOrganizationRepository organizationRepo,
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
            _leadRepo = leadRepo;
            _statusRepo = statusRepo;
            _interactionRepo = interactionRepo;
            _followUpRepo = followUpRepo;
            _contactRepo = contactRepo;
            _organizationRepo = organizationRepo;
            _coreIntegrationService = coreIntegrationService;
        }

        // ========== لیست سرنخ‌ها ==========

        /// <summary>
        /// صفحه اصلی سرنخ‌ها
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.VIEW")]
        public async Task<IActionResult> Index(CrmLeadFilterViewModel filter, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            var (leads, totalCount) = await _leadRepo.GetListAsync(filter, page, 20);

            var viewModel = new CrmLeadListViewModel
            {
                Leads = leads.Select(l => MapToViewModel(l)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = 20,
                Filters = filter,
                Statistics = await _leadRepo.GetStatisticsAsync(filter.BranchId, filter.AssignedUserId)
            };

            // آماده‌سازی Dropdowns
            await PrepareDropdownsAsync();

            return View(viewModel);
        }

        // ========== جزئیات سرنخ ==========

        /// <summary>
        /// صفحه جزئیات سرنخ
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            var lead = await _leadRepo.GetByIdAsync(id, includeDetails: true);
            if (lead == null)
                return NotFound();

            var viewModel = MapToViewModel(lead);

            // آماده‌سازی Dropdowns
            var statuses = await _statusRepo.GetAllAsync();
            ViewBag.Statuses = new SelectList(statuses, "Id", "Title", lead.StatusId);

            return View(viewModel);
        }

        // ========== ایجاد سرنخ ==========

        /// <summary>
        /// صفحه ایجاد سرنخ جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.CREATE")]
        public async Task<IActionResult> Create()
        {
            var model = new CrmLeadCreateViewModel();

            await PrepareDropdownsAsync();

            return View(model);
        }

        /// <summary>
        /// ذخیره سرنخ جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.CREATE")]
        public async Task<IActionResult> Create(CrmLeadCreateViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // اعتبارسنجی ساده
                if (!model.ContactId.HasValue && !model.OrganizationId.HasValue)
                {
                    ModelState.AddModelError("", "لطفاً یک فرد یا سازمان انتخاب کنید");
                    await PrepareDropdownsAsync();
                    return View(model);
                }

                CrmLead lead;
                string assignedUserId = model.AssignedUserId ?? currentUser.Id;

                if (model.ContactId.HasValue)
                {
                    lead = await _leadRepo.CreateFromContactAsync(
                        model.ContactId.Value, model.BranchId, assignedUserId, currentUser.Id);
                }
                else
                {
                    lead = await _leadRepo.CreateFromOrganizationAsync(
                        model.OrganizationId.Value, model.BranchId, assignedUserId, currentUser.Id);
                }

                // بروزرسانی اطلاعات اضافی
                if (!string.IsNullOrEmpty(model.Source) || !string.IsNullOrEmpty(model.Notes))
                {
                    lead.Source = model.Source;
                    lead.Notes = model.Notes;
                    lead.Tags = model.Tags;
                    await _leadRepo.UpdateAsync(lead);
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmLead",
                    "ایجاد سرنخ",
                    $"سرنخ جدید ایجاد شد",
                    recordId: lead.Id.ToString(),
                    entityType: "CrmLead"
                );

                TempData["SuccessMessage"] = "سرنخ با موفقیت ایجاد شد";
                return RedirectToAction("Details", new { id = lead.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PrepareDropdownsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                await PrepareDropdownsAsync();
                return View(model);
            }
        }

        // ========== ویرایش سرنخ ==========

        /// <summary>
        /// صفحه ویرایش سرنخ
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _leadRepo.GetByIdAsync(id, includeDetails: true);
            if (lead == null)
                return NotFound();

            var viewModel = MapToViewModel(lead);
            await PrepareDropdownsAsync();

            var statuses = await _statusRepo.GetAllAsync();
            ViewBag.Statuses = new SelectList(statuses, "Id", "Title", lead.StatusId);

            return View(viewModel);
        }

        /// <summary>
        /// ذخیره تغییرات سرنخ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> Edit(int id, CrmLeadViewModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                var currentUser = await _userManager.GetUserAsync(User);
                var lead = await _leadRepo.GetByIdAsync(id);
                if (lead == null)
                    return NotFound();

                // بروزرسانی
                lead.StatusId = model.StatusId;
                lead.AssignedUserId = model.AssignedUserId;
                lead.Source = model.Source;
                lead.Score = model.Score;
                lead.Notes = model.Notes;
                lead.Tags = model.Tags;
                lead.EstimatedValue = model.EstimatedValue;
                lead.IsActive = model.IsActive;
                lead.LastUpdaterUserId = currentUser.Id;

                var result = await _leadRepo.UpdateAsync(lead);

                if (!result)
                {
                    ModelState.AddModelError("", "خطا در بروزرسانی سرنخ");
                    await PrepareDropdownsAsync();
                    return View(model);
                }

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmLead",
                    "ویرایش سرنخ",
                    $"سرنخ ویرایش شد",
                    recordId: lead.Id.ToString(),
                    entityType: "CrmLead"
                );

                TempData["SuccessMessage"] = "سرنخ با موفقیت بروزرسانی شد";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا: {ex.Message}");
                await PrepareDropdownsAsync();
                return View(model);
            }
        }

        // ========== عملیات سریع ==========

        /// <summary>
        /// تغییر وضعیت سرنخ
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> ChangeStatus(int id, int statusId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _leadRepo.ChangeStatusAsync(id, statusId, currentUser.Id);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در تغییر وضعیت" });
                }

                var status = await _statusRepo.GetByIdAsync(statusId);

                return Json(new
                {
                    success = true,
                    message = $"وضعیت به «{status?.Title}» تغییر کرد",
                    statusTitle = status?.Title,
                    statusColor = status?.ColorCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// تخصیص سرنخ به کاربر
        /// </summary>
        [HttpPost]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> AssignToUser(int id, string userId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _leadRepo.AssignToUserAsync(id, userId, currentUser.Id);

                if (!result)
                {
                    return Json(new { success = false, message = "خطا در تخصیص سرنخ" });
                }

                return Json(new { success = true, message = "سرنخ تخصیص داده شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== تعاملات (Interactions) ==========

        /// <summary>
        /// ثبت تعامل جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> AddInteraction(CrmLeadInteractionViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var interaction = new CrmLeadInteraction
                {
                    LeadId = model.LeadId,
                    InteractionType = model.InteractionType,
                    Direction = model.Direction,
                    Subject = model.Subject,
                    Description = model.Description ?? string.Empty,
                    InteractionDate = !string.IsNullOrEmpty(model.InteractionDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(model.InteractionDatePersian)
                        : DateTime.Now,
                    DurationMinutes = model.DurationMinutes,
                    Result = model.Result,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = currentUser.Id
                };

                await _interactionRepo.CreateAsync(interaction);

                // بروزرسانی تاریخ آخرین تماس در سرنخ
                await _leadRepo.UpdateLastContactDateAsync(model.LeadId, interaction.InteractionDate);

                // ایجاد پیگیری اگر نیاز باشد
                if (model.NeedsFollowUp && !string.IsNullOrEmpty(model.FollowUpDatePersian))
                {
                    var followUp = new CrmFollowUp
                    {
                        LeadId = model.LeadId,
                        Title = $"پیگیری: {model.Subject}",
                        Description = model.FollowUpNote,
                        DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.FollowUpDatePersian),
                        FollowUpType = model.InteractionType, // همان نوع تعامل
                        Priority = 1, // متوسط
                        Status = 0, // در انتظار
                        AssignedUserId = currentUser.Id,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatorUserId = currentUser.Id
                    };

                    await _followUpRepo.CreateAsync(followUp);
                }

                return Json(new { success = true, message = "تعامل با موفقیت ثبت شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// حذف تعامل
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> DeleteInteraction(int id)
        {
            try
            {
                var result = await _interactionRepo.DeleteAsync(id);
                if (!result)
                    return Json(new { success = false, message = "تعامل یافت نشد" });

                return Json(new { success = true, message = "تعامل حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== پیگیری‌ها (Follow-Ups) ==========

        /// <summary>
        /// ایجاد پیگیری جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> AddFollowUp(CrmFollowUpViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var followUp = new CrmFollowUp
                {
                    LeadId = model.LeadId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = !string.IsNullOrEmpty(model.DueDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(model.DueDatePersian)
                        : DateTime.Now.AddDays(1),
                    FollowUpType = model.FollowUpType,
                    Priority = model.Priority,
                    Status = 0, // در انتظار
                    AssignedUserId = currentUser.Id,
                    ReminderMinutesBefore = model.ReminderMinutesBefore,
                    SendEmailReminder = model.SendEmailReminder,
                    SendSmsReminder = model.SendSmsReminder,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = currentUser.Id
                };

                await _followUpRepo.CreateAsync(followUp);

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                await _leadRepo.UpdateNextFollowUpDateAsync(model.LeadId);

                return Json(new { success = true, message = "پیگیری با موفقیت ایجاد شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// بروزرسانی وضعیت پیگیری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> UpdateFollowUpStatus(int id, byte status)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = await _followUpRepo.UpdateStatusAsync(id, status, currentUser.Id);

                if (!result)
                    return Json(new { success = false, message = "پیگیری یافت نشد" });

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                var followUp = await _followUpRepo.GetByIdAsync(id);
                if (followUp != null)
                {
                    await _leadRepo.UpdateNextFollowUpDateAsync(followUp.LeadId);
                }

                return Json(new { success = true, message = "وضعیت پیگیری بروزرسانی شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        /// <summary>
        /// به تعویق انداختن پیگیری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.EDIT")]
        public async Task<IActionResult> PostponeFollowUp(int id, string newDate)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var newDueDate = ConvertDateTime.ConvertShamsiToMiladi(newDate);
                
                var result = await _followUpRepo.PostponeAsync(id, newDueDate, currentUser.Id);

                if (!result)
                    return Json(new { success = false, message = "پیگیری یافت نشد" });

                // بروزرسانی تاریخ پیگیری بعدی در سرنخ
                var followUp = await _followUpRepo.GetByIdAsync(id);
                if (followUp != null)
                {
                    await _leadRepo.UpdateNextFollowUpDateAsync(followUp.LeadId);
                }

                return Json(new { success = true, message = "پیگیری به تعویق افتاد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا: {ex.Message}" });
            }
        }

        // ========== جستجو ==========

        /// <summary>
        /// جستجوی سرنخ‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string search, int? branchId = null)
        {
            var leads = await _leadRepo.SearchAsync(search, branchId);
            return Json(leads.Select(l => new
            {
                id = l.Id,
                text = l.DisplayName,
                type = l.LeadType,
                status = l.Status?.Title,
                statusColor = l.Status?.ColorCode,
                phone = l.PrimaryPhone
            }));
        }

        /// <summary>
        /// جستجوی افراد برای Select2
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchContacts(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var contacts = await _contactRepo.SearchContactsAsync(term, 20);

            return Json(contacts.Select(c => new
            {
                id = c.Id,
                text = c.FullName,
                phone = c.Phones?.FirstOrDefault()?.PhoneNumber,
                email = c.PrimaryEmail
            }));
        }

        /// <summary>
        /// جستجوی سازمان‌ها برای Select2
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchOrganizations(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var organizations = await _organizationRepo.SearchOrganizationsAsync(term, 20);

            return Json(organizations.Select(o => new
            {
                id = o.Id,
                text = o.DisplayName,
                phone = o.Phones?.FirstOrDefault()?.PhoneNumber
            }));
        }

        // ========== Helper Methods ==========

        private CrmLeadViewModel MapToViewModel(CrmLead lead)
        {
            return new CrmLeadViewModel
            {
                Id = lead.Id,
                ContactId = lead.ContactId,
                ContactName = lead.Contact?.FullName,
                OrganizationId = lead.OrganizationId,
                OrganizationName = lead.Organization?.DisplayName,
                BranchId = lead.BranchId,
                BranchName = lead.Branch?.Name,
                AssignedUserId = lead.AssignedUserId,
                AssignedUserName = lead.AssignedUser != null 
                    ? $"{lead.AssignedUser.FirstName} {lead.AssignedUser.LastName}" 
                    : null,
                StatusId = lead.StatusId,
                StatusTitle = lead.Status?.Title,
                StatusColor = lead.Status?.ColorCode,
                StatusIcon = lead.Status?.Icon,
                Source = lead.Source,
                Score = lead.Score,
                Notes = lead.Notes,
                Tags = lead.Tags,
                TagsList = lead.TagsList,
                LastContactDate = lead.LastContactDate,
                LastContactDatePersian = lead.LastContactDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(lead.LastContactDate.Value, "yyyy/MM/dd")
                    : null,
                NextFollowUpDate = lead.NextFollowUpDate,
                NextFollowUpDatePersian = lead.NextFollowUpDate.HasValue
                    ? ConvertDateTime.ConvertMiladiToShamsi(lead.NextFollowUpDate.Value, "yyyy/MM/dd")
                    : null,
                EstimatedValue = lead.EstimatedValue,
                EstimatedValueFormatted = lead.EstimatedValue?.ToString("N0"),
                IsActive = lead.IsActive,
                LeadType = lead.LeadType,
                IsContact = lead.IsContact,
                IsOrganization = lead.IsOrganization,
                DisplayName = lead.DisplayName,
                PrimaryPhone = lead.PrimaryPhone,
                PrimaryEmail = lead.PrimaryEmail,
                InteractionsCount = lead.InteractionsCount,
                PendingFollowUpsCount = lead.PendingFollowUpsCount,
                NeedsFollowUp = lead.NeedsFollowUp,
                DaysSinceLastContact = lead.DaysSinceLastContact,
                CreatedDate = lead.CreatedDate,
                CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(lead.CreatedDate, "yyyy/MM/dd"),
                CreatorUserId = lead.CreatorUserId,
                CreatorName = lead.Creator != null 
                    ? $"{lead.Creator.FirstName} {lead.Creator.LastName}" 
                    : null
            };
        }

        private async Task PrepareDropdownsAsync()
        {
            var statuses = await _statusRepo.GetAllAsync();
            ViewBag.Statuses = new SelectList(statuses, "Id", "Title");

            // منابع پیشنهادی
            ViewBag.SuggestedSources = DataModelLayer.StaticClasses.StaticCrmLeadStatusSeedData.SuggestedSources;
        }

        // ========== ⭐⭐⭐ Quick Entry (ثبت سریع سرنخ) ==========

        /// <summary>
        /// صفحه ثبت سریع سرنخ
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.CREATE")]
        public async Task<IActionResult> QuickEntry(string? returnUrl = null, string? sourcePage = null)
        {
            var model = await PrepareQuickEntryModelAsync();
            model.ReturnUrl = returnUrl;
            model.SourcePage = sourcePage;

            return View(model);
        }

        /// <summary>
        /// مودال ثبت سریع سرنخ
        /// </summary>
        [HttpGet]
        [PermissionRequired("CRM.LEAD.CREATE")]
        public async Task<IActionResult> QuickEntryModal(string? returnUrl = null, string? sourcePage = null)
        {
            var model = await PrepareQuickEntryModelAsync();
            model.ReturnUrl = returnUrl;
            model.SourcePage = sourcePage;

            return PartialView("_QuickEntryModal", model);
        }

        /// <summary>
        /// ثبت سریع سرنخ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CRM.LEAD.CREATE")]
        public async Task<IActionResult> QuickEntry(QuickLeadEntryViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var result = new QuickLeadEntryResult();

                // ⭐ اعتبارسنجی
                var validationErrors = ValidateQuickEntry(model);
                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = false,
                            message = string.Join("، ", validationErrors),
                            errors = validationErrors
                        });
                    }

                    model = await PrepareQuickEntryModelAsync(model);
                    return View(model);
                }

                // ⭐ شروع تراکنش
                await _uow.BeginTransactionAsync();

                try
                {
                    int? contactId = null;
                    int? organizationId = null;

                    // 1️⃣ ایجاد Contact/Organization جدید (اگر نیاز باشد)
                    if (model.IsNewEntity)
                    {
                        if (model.LeadType == "Contact")
                        {
                            // ایجاد Contact از طریق Lead Repository
                            contactId = await _leadRepo.CreateQuickContactAndGetIdAsync(
                                model.FirstName,
                                model.LastName,
                                model.MobileNumber,
                                model.Email,
                                currentUser.Id);
                            result.ContactId = contactId;
                        }
                        else
                        {
                            // ایجاد Organization از طریق Lead Repository
                            organizationId = await _leadRepo.CreateQuickOrganizationAndGetIdAsync(
                                model.OrganizationName,
                                model.OrganizationPhone,
                                currentUser.Id);
                            result.OrganizationId = organizationId;
                        }
                    }
                    else
                    {
                        contactId = model.ContactId;
                        organizationId = model.OrganizationId;
                    }

                    // 2️⃣ ایجاد Lead
                    CrmLead lead;
                    if (contactId.HasValue)
                    {
                        lead = await _leadRepo.CreateFromContactAsync(
                            contactId.Value,
                            model.BranchId,
                            currentUser.Id, // مسئول = کاربر جاری
                            currentUser.Id);
                    }
                    else
                    {
                        lead = await _leadRepo.CreateFromOrganizationAsync(
                            organizationId.Value,
                            model.BranchId,
                            currentUser.Id,
                            currentUser.Id);
                    }

                    // 3️⃣ بروزرسانی اطلاعات تکمیلی Lead
                    lead.Source = model.Source;
                    lead.Notes = model.Notes;
                    if (model.StatusId.HasValue)
                        lead.StatusId = model.StatusId.Value;

                    // ⭐⭐⭐ تنظیم NextAction (اقدام بعدی)
                    lead.NextActionType = model.NextActionType;
                    lead.NextActionDate = ParseNextActionDateTime(model.NextActionDatePersian, model.NextActionTime);
                    lead.NextActionNote = model.NextActionNote;

                    await _leadRepo.UpdateAsync(lead);
                    result.LeadId = lead.Id;

                    // 4️⃣ ⭐⭐⭐ ایجاد تسک برای اقدام بعدی (اگر فعال باشد)
                    if (model.CreateTaskForNextAction)
                    {
                        var taskResult = await _coreIntegrationService.CreateTaskFromCrmLeadAsync(
                            new CrmLeadTaskRequest
                            {
                                LeadId = lead.Id,
                                ActionType = model.NextActionType,
                                DueDate = lead.NextActionDate ?? DateTime.Now.AddDays(1),
                                Description = model.NextActionNote,
                                Priority = model.TaskPriority,
                                CreatorUserId = currentUser.Id,
                                AssignedUserId = currentUser.Id,
                                CreateFollowUpRecord = true
                            });

                        if (taskResult.Success)
                        {
                            // ذخیره شناسه تسک در Lead
                            lead.NextActionTaskId = taskResult.TaskId;
                            await _leadRepo.UpdateAsync(lead);

                            result.TaskId = taskResult.TaskId;
                            result.TaskCode = taskResult.TaskCode;
                        }
                    }

                    await _uow.CommitTransactionAsync();

                    // ⭐ لاگ فعالیت
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "CrmLead",
                        "QuickEntry",
                        $"سرنخ جدید ایجاد شد: {lead.DisplayName}",
                        recordId: lead.Id.ToString(),
                        entityType: "CrmLead"
                    );

                    result.Success = true;
                    result.Message = "سرنخ با موفقیت ثبت شد";

                    // ⭐ پاسخ AJAX یا Redirect
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = result.Message,
                            leadId = result.LeadId,
                            taskId = result.TaskId,
                            taskCode = result.TaskCode,
                            redirectUrl = Url.Action("Details", new { id = result.LeadId })
                        });
                    }

                    TempData["SuccessMessage"] = result.Message;
                    
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);
                    
                    return RedirectToAction("Details", new { id = result.LeadId });
                }
                catch
                {
                    await _uow.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CrmLead", "QuickEntry", $"خطا در ثبت سریع سرنخ: {ex.Message}", ex);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = $"خطا: {ex.Message}"
                    });
                }

                ModelState.AddModelError("", $"خطا: {ex.Message}");
                model = await PrepareQuickEntryModelAsync(model);
                return View(model);
            }
        }

        /// <summary>
        /// آماده‌سازی مدل Quick Entry
        /// </summary>
        private async Task<QuickLeadEntryViewModel> PrepareQuickEntryModelAsync(QuickLeadEntryViewModel? existing = null)
        {
            var model = existing ?? new QuickLeadEntryViewModel();

            // لیست شعبه‌ها (از UnitOfWork)
            var userBranches = _uow.BranchUW.Get(
                b => b.IsActive,
                null,
                "").ToList();
            
            model.BranchesInitial = userBranches.Select(b => new CrmSelectListItem(
                b.Id.ToString(), b.Name, b.Id == model.BranchId
            )).ToList();

            // اگر یک شعبه داریم، انتخاب کن
            if (!model.BranchesInitial.Any(b => b.Selected) && model.BranchesInitial.Count == 1)
            {
                model.BranchesInitial[0].Selected = true;
                model.BranchId = int.Parse(model.BranchesInitial[0].Value);
            }

            // لیست منابع سرنخ
            model.SourcesInitial = DataModelLayer.StaticClasses.StaticCrmLeadStatusSeedData.SuggestedSources
                .Select(s => new CrmSelectListItem(s, s, s == model.Source))
                .ToList();

            // لیست وضعیت‌ها
            var statuses = await _statusRepo.GetAllAsync();
            model.StatusesInitial = statuses.Select(s => new CrmSelectListItem(
                s.Id.ToString(), s.Title, s.Id == model.StatusId
            )).ToList();

            // لیست انواع اقدام بعدی
            model.NextActionTypesInitial = Enum.GetValues<CrmNextActionType>()
                .Select(t => new CrmSelectListItem(
                    ((int)t).ToString(),
                    GetNextActionTypeText(t),
                    t == model.NextActionType
                )).ToList();

            // لیست اولویت‌های تسک
            model.TaskPrioritiesInitial = Enum.GetValues<CrmTaskPriority>()
                .Select(p => new CrmSelectListItem(
                    ((int)p).ToString(),
                    GetTaskPriorityText(p),
                    p == model.TaskPriority
                )).ToList();

            // تاریخ پیش‌فرض
            if (string.IsNullOrEmpty(model.NextActionDatePersian))
            {
                model.NextActionDatePersian = ConvertDateTime.ConvertMiladiToShamsi(
                    DateTime.Now.AddDays(1), "yyyy/MM/dd");
            }

            return model;
        }

        /// <summary>
        /// اعتبارسنجی Quick Entry
        /// </summary>
        private List<string> ValidateQuickEntry(QuickLeadEntryViewModel model)
        {
            var errors = new List<string>();

            // اعتبارسنجی نوع سرنخ
            if (model.IsNewEntity)
            {
                if (model.LeadType == "Contact")
                {
                    if (string.IsNullOrWhiteSpace(model.FirstName))
                        errors.Add("نام الزامی است");
                    if (string.IsNullOrWhiteSpace(model.LastName))
                        errors.Add("نام خانوادگی الزامی است");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(model.OrganizationName))
                        errors.Add("نام سازمان الزامی است");
                }
            }
            else
            {
                if (model.LeadType == "Contact" && !model.ContactId.HasValue)
                    errors.Add("لطفاً یک فرد انتخاب کنید");
                if (model.LeadType == "Organization" && !model.OrganizationId.HasValue)
                    errors.Add("لطفاً یک سازمان انتخاب کنید");
            }

            // اعتبارسنجی شعبه
            if (model.BranchId <= 0)
                errors.Add("شعبه الزامی است");

            // ⭐⭐⭐ اعتبارسنجی NextAction (اجباری!)
            if (string.IsNullOrWhiteSpace(model.NextActionDatePersian))
                errors.Add("تاریخ اقدام بعدی الزامی است");

            return errors;
        }

        /// <summary>
        /// تبدیل تاریخ و ساعت اقدام بعدی
        /// </summary>
        private DateTime ParseNextActionDateTime(string datePersian, string? time)
        {
            var date = ConvertDateTime.ConvertShamsiToMiladi(datePersian);

            if (!string.IsNullOrEmpty(time) && TimeSpan.TryParse(time, out var timeSpan))
            {
                date = date.Date.Add(timeSpan);
            }
            else
            {
                date = date.Date.AddHours(9); // 9 صبح پیش‌فرض
            }

            return date;
        }

        /// <summary>
        /// متن نوع اقدام بعدی
        /// </summary>
        private string GetNextActionTypeText(CrmNextActionType type) => type switch
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
            _ => type.ToString()
        };

        /// <summary>
        /// متن اولویت تسک
        /// </summary>
        private string GetTaskPriorityText(CrmTaskPriority priority) => priority switch
        {
            CrmTaskPriority.Low => "پایین",
            CrmTaskPriority.Normal => "عادی",
            CrmTaskPriority.High => "مهم",
            CrmTaskPriority.Urgent => "فوری",
            _ => priority.ToString()
        };

        // ========== END Quick Entry ==========
    }
}
