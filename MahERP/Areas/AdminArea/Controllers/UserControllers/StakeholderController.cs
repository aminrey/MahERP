using AutoMapper;
using ClosedXML.Excel;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.StaticClasses;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.TaskManagement;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("Stakeholder")]
    public class StakeholderController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;

        public StakeholderController(
            IUnitOfWork uow,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _uow = uow;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // ==================== STAKEHOLDER CRUD ====================

        // لیست طرف حساب‌ها
        public async Task<IActionResult> Index(byte? personType = null, int? type = null, bool includeDeleted = false)
        {
            try
            {
                var stakeholders = _stakeholderRepository.GetStakeholders(includeDeleted, type, personType);

                ViewBag.IncludeDeleted = includeDeleted;
                ViewBag.CurrentType = type;
                ViewBag.PersonType = personType;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "Index",
                    $"مشاهده لیست طرف حساب‌ها - نوع شخص: {personType?.ToString() ?? "همه"}, نوع: {type?.ToString() ?? "همه"}"
                );

                return View(stakeholders);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Stakeholders", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // جزئیات طرف حساب
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var stakeholder = _stakeholderRepository.GetStakeholderById(id, true, true, true, true, true);
                if (stakeholder == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Stakeholders",
                        "Details",
                        "تلاش برای مشاهده طرف حساب غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);

                ViewBag.Contacts = _stakeholderRepository.GetStakeholderContacts(id, true);
                ViewBag.Contracts = stakeholder.Contracts?.ToList() ?? new List<Contract>();
                ViewBag.Tasks = stakeholder.TaskList?.ToList() ?? new List<Tasks>();
                ViewBag.Organizations = _stakeholderRepository.GetStakeholderOrganizations(id, false);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "Details",
                    $"مشاهده جزئیات طرف حساب: {stakeholder.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: stakeholder.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Stakeholders", "Details", "خطا در دریافت جزئیات", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن طرف حساب - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> AddStakeholder()
        {
            try
            {
                ViewBag.SalesReps = new SelectList(_userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                    "Id", "FullName");

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "AddStakeholder",
                    "مشاهده فرم افزودن طرف حساب جدید"
                );

                return View(new StakeholderViewModel { IsActive = true, PersonType = 0 });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Stakeholders", "AddStakeholder", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن طرف حساب - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStakeholder(StakeholderViewModel model)
        {
            // اعتبارسنجی بر اساس نوع شخص
            if (model.PersonType == 0) // شخص حقیقی
            {
                if (string.IsNullOrEmpty(model.FirstName))
                    ModelState.AddModelError("FirstName", "نام الزامی است");
                if (string.IsNullOrEmpty(model.LastName))
                    ModelState.AddModelError("LastName", "نام خانوادگی الزامی است");
            }
            else if (model.PersonType == 1) // شخص حقوقی
            {
                if (string.IsNullOrEmpty(model.CompanyName))
                    ModelState.AddModelError("CompanyName", "نام شرکت الزامی است");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی یکتا بودن
                    if (model.PersonType == 0 && !string.IsNullOrEmpty(model.NationalCode))
                    {
                        if (!_stakeholderRepository.IsNationalCodeUnique(model.NationalCode))
                        {
                            ModelState.AddModelError("NationalCode", "کد ملی تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }
                    }
                    else if (model.PersonType == 1)
                    {
                        if (!string.IsNullOrEmpty(model.RegistrationNumber) &&
                            !_stakeholderRepository.IsRegistrationNumberUnique(model.RegistrationNumber))
                        {
                            ModelState.AddModelError("RegistrationNumber", "شماره ثبت تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }

                        if (!string.IsNullOrEmpty(model.EconomicCode) &&
                            !_stakeholderRepository.IsEconomicCodeUnique(model.EconomicCode))
                        {
                            ModelState.AddModelError("EconomicCode", "کد اقتصادی تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Email) && !_stakeholderRepository.IsEmailUnique(model.Email))
                    {
                        ModelState.AddModelError("Email", "ایمیل تکراری است");
                        PrepareViewBag();
                        return View(model);
                    }

                    var stakeholder = _mapper.Map<Stakeholder>(model);
                    stakeholder.CreateDate = DateTime.Now;
                    stakeholder.CreatorUserId = _userManager.GetUserId(User);
                    stakeholder.IsActive = true;
                    stakeholder.IsDeleted = false;

                    _uow.StakeholderUW.Create(stakeholder);
                    _uow.Save();

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Stakeholders",
                        "AddStakeholder",
                        $"ایجاد طرف حساب جدید: {stakeholder.DisplayName}",
                        recordId: stakeholder.Id.ToString(),
                        entityType: "Stakeholder",
                        recordTitle: stakeholder.DisplayName
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Stakeholders", "AddStakeholder", "خطا در ایجاد", ex);
                    ModelState.AddModelError("", "خطا در ثبت: " + ex.Message);
                }
            }

            PrepareViewBag();
            return View(model);
        }

        // ویرایش طرف حساب - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> EditStakeholder(int id)
        {
            try
            {
                var stakeholder = _stakeholderRepository.GetStakeholderById(id);
                if (stakeholder == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Stakeholders",
                        "EditStakeholder",
                        "تلاش برای ویرایش طرف حساب غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);
                PrepareViewBag();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "EditStakeholder",
                    $"مشاهده فرم ویرایش طرف حساب: {stakeholder.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: stakeholder.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Stakeholders", "EditStakeholder", "خطا در نمایش فرم", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ویرایش طرف حساب - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStakeholder(StakeholderViewModel model)
        {
            // اعتبارسنجی مشابه AddStakeholder
            if (model.PersonType == 0)
            {
                if (string.IsNullOrEmpty(model.FirstName))
                    ModelState.AddModelError("FirstName", "نام الزامی است");
                if (string.IsNullOrEmpty(model.LastName))
                    ModelState.AddModelError("LastName", "نام خانوادگی الزامی است");
            }
            else if (model.PersonType == 1)
            {
                if (string.IsNullOrEmpty(model.CompanyName))
                    ModelState.AddModelError("CompanyName", "نام شرکت الزامی است");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی یکتا بودن با excludeId
                    if (model.PersonType == 0 && !string.IsNullOrEmpty(model.NationalCode))
                    {
                        if (!_stakeholderRepository.IsNationalCodeUnique(model.NationalCode, model.Id))
                        {
                            ModelState.AddModelError("NationalCode", "کد ملی تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }
                    }
                    else if (model.PersonType == 1)
                    {
                        if (!string.IsNullOrEmpty(model.RegistrationNumber) &&
                            !_stakeholderRepository.IsRegistrationNumberUnique(model.RegistrationNumber, model.Id))
                        {
                            ModelState.AddModelError("RegistrationNumber", "شماره ثبت تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }

                        if (!string.IsNullOrEmpty(model.EconomicCode) &&
                            !_stakeholderRepository.IsEconomicCodeUnique(model.EconomicCode, model.Id))
                        {
                            ModelState.AddModelError("EconomicCode", "کد اقتصادی تکراری است");
                            PrepareViewBag();
                            return View(model);
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Email) && !_stakeholderRepository.IsEmailUnique(model.Email, model.Id))
                    {
                        ModelState.AddModelError("Email", "ایمیل تکراری است");
                        PrepareViewBag();
                        return View(model);
                    }

                    var stakeholder = _uow.StakeholderUW.GetById(model.Id);
                    if (stakeholder == null)
                        return RedirectToAction("ErrorView", "Home");

                    var oldValues = new
                    {
                        stakeholder.PersonType,
                        stakeholder.FirstName,
                        stakeholder.LastName,
                        stakeholder.CompanyName,
                        stakeholder.IsActive
                    };

                    var originalCreateDate = stakeholder.CreateDate;
                    var originalCreatorUserId = stakeholder.CreatorUserId;

                    _mapper.Map(model, stakeholder);

                    stakeholder.CreateDate = originalCreateDate;
                    stakeholder.CreatorUserId = originalCreatorUserId;
                    stakeholder.LastUpdateDate = DateTime.Now;
                    stakeholder.LastUpdaterUserId = _userManager.GetUserId(User);

                    _uow.StakeholderUW.Update(stakeholder);
                    _uow.Save();

                    var newValues = new
                    {
                        stakeholder.PersonType,
                        stakeholder.FirstName,
                        stakeholder.LastName,
                        stakeholder.CompanyName,
                        stakeholder.IsActive
                    };

                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Stakeholders",
                        "EditStakeholder",
                        $"ویرایش طرف حساب: {stakeholder.DisplayName}",
                        oldValues,
                        newValues,
                        recordId: stakeholder.Id.ToString(),
                        entityType: "Stakeholder",
                        recordTitle: stakeholder.DisplayName
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Stakeholders", "EditStakeholder", "خطا در ویرایش", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", $"خطا در ذخیره: {ex.Message}");
                }
            }

            PrepareViewBag();
            return View(model);
        }

        // متد کمکی برای تنظیم ViewBag
        private void PrepareViewBag()
        {
            ViewBag.SalesReps = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
        }

       

        /// <summary>
        /// فعال/غیرفعال کردن طرف حساب - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ActiveOrDeactiveStakeholder(int id, string returnUrl = "Index")
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                if (stakeholder.IsActive == true)
                {
                    ViewBag.themeclass = "bg-danger";
                    ViewBag.ModalTitle = "غیرفعال کردن طرف حساب";
                }
                else
                {
                    ViewBag.themeclass = "bg-success";
                    ViewBag.ModalTitle = "فعال کردن طرف حساب";
                }

                ViewBag.ReturnUrl = returnUrl; // ✅ ذخیره مقصد بازگشت

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "ActiveOrDeactiveStakeholder",
                    $"مشاهده فرم تغییر وضعیت طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                return PartialView("_ActiveOrDeactiveStakeholder", stakeholder);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "ActiveOrDeactiveStakeholder",
                    "خطا در نمایش فرم تغییر وضعیت",
                    ex,
                    recordId: id.ToString()
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن طرف حساب - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActiveOrDeactiveStakeholderPost(int Id, bool IsActive, string returnUrl = "Index")
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(Id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                var oldStatus = stakeholder.IsActive;

                stakeholder.IsActive = !IsActive; // Toggle status

                _uow.StakeholderUW.Update(stakeholder);
                _uow.Save();

                await _activityLogger.LogChangeAsync(
                    ActivityTypeEnum.Edit,
                    "Stakeholders",
                    "ActiveOrDeactiveStakeholder",
                    $"تغییر وضعیت طرف حساب: {stakeholder.FirstName} {stakeholder.LastName} از {(oldStatus ? "فعال" : "غیرفعال")} به {(stakeholder.IsActive ? "فعال" : "غیرفعال")}",
                    new { IsActive = oldStatus },
                    new { IsActive = stakeholder.IsActive },
                    recordId: Id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                // ✅ بازگشت بر اساس returnUrl
                if (returnUrl == "Details")
                {
                    return RedirectToAction("Details", new { id = Id });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "ActiveOrDeactiveStakeholder",
                    "خطا در تغییر وضعیت طرف حساب",
                    ex,
                    recordId: Id.ToString()
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// حذف طرف حساب - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteStakeholder(int id, string returnUrl = "Index")
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ViewTitle = "حذف طرف حساب";
                ViewBag.ReturnUrl = returnUrl; // ✅ ذخیره مقصد بازگشت

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "DeleteStakeholder",
                    $"مشاهده فرم حذف طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                return PartialView("_DeleteStakeholder", stakeholder);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "DeleteStakeholder",
                    "خطا در نمایش فرم حذف طرف حساب",
                    ex,
                    recordId: id.ToString()
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// حذف طرف حساب - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStakeholderPost(int id, string returnUrl = "Index")
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                var stakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
                stakeholder.IsDeleted = true;
                _uow.StakeholderUW.Update(stakeholder);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Stakeholders",
                    "DeleteStakeholder",
                    $"حذف طرف حساب: {stakeholderName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: stakeholderName
                );

                // ✅ بازگشت بر اساس returnUrl - حذف شده پس فقط Index
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("Index")
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "DeleteStakeholder",
                    "خطا در حذف طرف حساب",
                    ex,
                    recordId: id.ToString()
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }



        // افزودن تماس مرتبط - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> AddContact(int stakeholderId)
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.StakeholderId = stakeholderId;
                ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "AddContact",
                    $"مشاهده فرم افزودن تماس برای طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                    recordId: stakeholderId.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                return View(new StakeholderContactViewModel
                {
                    StakeholderId = stakeholderId,
                    IsActive = true
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "AddContact",
                    "خطا در نمایش فرم افزودن تماس",
                    ex,
                    recordId: stakeholderId.ToString()
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن تماس مرتبط - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContact(StakeholderContactViewModel model)
        {
            try
            {
                // بررسی ModelState
                if (!ModelState.IsValid)
                {
                    // لاگ کردن خطاهای ModelState
                    foreach (var error in ModelState)
                    {
                        foreach (var subError in error.Value.Errors)
                        {
                            // می‌توانید اینجا لاگ کنید
                            Console.WriteLine($"Field: {error.Key}, Error: {subError.ErrorMessage}");
                        }
                    }

                    // بازگشت اطلاعات برای نمایش خطاها
                    var stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
                    if (stakeholder != null)
                    {
                        ViewBag.StakeholderId = model.StakeholderId;
                        ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
                    }
                    return View(model);
                }

                // بررسی وجود طرف حساب
                var stakeholderEntity = _uow.StakeholderUW.GetById(model.StakeholderId);
                if (stakeholderEntity == null)
                {
                    ModelState.AddModelError("", "طرف حساب مورد نظر یافت نشد");
                    return View(model);
                }

                // ایجاد تماس جدید
                var contact = _mapper.Map<StakeholderContact>(model);
                contact.CreateDate = DateTime.Now;
                contact.CreatorUserId = _userManager.GetUserId(User);

                // اگر این تماس به عنوان اصلی انتخاب شده، سایر تماس‌ها را از حالت اصلی خارج کنیم
                if (model.IsPrimary)
                {
                    var primaryContacts = _uow.StakeholderContactUW.Get(c => c.StakeholderId == model.StakeholderId && c.IsPrimary);
                    foreach (var primaryContact in primaryContacts)
                    {
                        primaryContact.IsPrimary = false;
                        _uow.StakeholderContactUW.Update(primaryContact);
                    }
                }

                // ذخیره در دیتابیس
                _uow.StakeholderContactUW.Create(contact);
                _uow.Save();

                // ثبت لاگ موفقیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Stakeholders",
                    "AddContact",
                    $"افزودن تماس جدید برای طرف حساب: {stakeholderEntity.FirstName} {stakeholderEntity.LastName} - تماس: {contact.FirstName} {contact.LastName}",
                    recordId: contact.Id.ToString(),
                    entityType: "StakeholderContact",
                    recordTitle: $"{contact.FirstName} {contact.LastName}"
                );

                return RedirectToAction("Details", new { id = model.StakeholderId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "AddContact",
                    "خطا در افزودن تماس جدید",
                    ex,
                    recordId: model.StakeholderId.ToString()
                );

                // لاگ کردن خطای دقیق
                Console.WriteLine($"Error in AddContact: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"خطا در ذخیره اطلاعات: {ex.Message}");

                // بازگشت اطلاعات
                var stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
                if (stakeholder != null)
                {
                    ViewBag.StakeholderId = model.StakeholderId;
                    ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
                }

                return View(model);
            }
        }

        // ویرایش تماس مرتبط - نمایش فرم
        [HttpGet]
        public IActionResult EditContact(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");

            var stakeholder = _uow.StakeholderUW.GetById(contact.StakeholderId);
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";

            var viewModel = _mapper.Map<StakeholderContactViewModel>(contact);

            return View(viewModel);
        }

        // ویرایش تماس مرتبط - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditContact(StakeholderContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = _uow.StakeholderContactUW.GetById(model.Id);
                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, contact);

                // اگر این تماس به عنوان اصلی انتخاب شده، سایر تماس‌ها را از حالت اصلی خارج کنیم
                if (model.IsPrimary)
                {
                    var primaryContacts = _uow.StakeholderContactUW.Get(c => c.StakeholderId == model.StakeholderId && c.IsPrimary && c.Id != model.Id);
                    foreach (var primaryContact in primaryContacts)
                    {
                        primaryContact.IsPrimary = false;
                        _uow.StakeholderContactUW.Update(primaryContact);
                    }
                }

                _uow.StakeholderContactUW.Update(contact);
                _uow.Save();

                return RedirectToAction("Details", new { id = model.StakeholderId });
            }

            // در صورت وجود خطا، اطلاعات را دوباره به ویو برگردانیم
            var stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";

            return View(model);
        }

        // حذف تماس مرتبط - نمایش مودال تأیید
        [HttpGet]
        public IActionResult DeleteContact(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");

            return PartialView("_DeleteContact", contact);
        }

        // حذف تماس مرتبط - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteContactPost(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");

            int stakeholderId = contact.StakeholderId;

            _uow.StakeholderContactUW.Delete(contact);
            _uow.Save();

            return RedirectToAction("Details", new { id = stakeholderId });
        }

        // فعال/غیرفعال کردن تماس مرتبط - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleContactStatus(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ModalTitle = contact.IsActive ? "غیرفعال کردن فرد مرتبط" : "فعال کردن فرد مرتبط";
            ViewBag.ButtonClass = contact.IsActive ? "btn btn-danger" : "btn btn-success";
            ViewBag.ActionText = contact.IsActive ? "غیرفعال کردن" : "فعال کردن";

            return PartialView("_ToggleContactStatus", contact);
        }

        // فعال/غیرفعال کردن تماس مرتبط - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleContactStatusPost(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");

            contact.IsActive = !contact.IsActive;
            _uow.StakeholderContactUW.Update(contact);
            _uow.Save();

            return RedirectToAction("Details", new { id = contact.StakeholderId });
        }

        // ==================== STAKEHOLDER ORGANIZATION CHART ====================

        /// <summary>
        /// نمایش چارت سازمانی طرف حساب (فقط برای شخص حقوقی)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrganizationChart(int stakeholderId)
        {
            try
            {
                var stakeholder = _stakeholderRepository.GetStakeholderById(stakeholderId, includeOrganizations: true);

                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی اینکه طرف حساب باید شخص حقوقی باشد
                if (stakeholder.PersonType != 1)
                {
                    TempData["ErrorMessage"] = "چارت سازمانی فقط برای اشخاص حقوقی قابل نمایش است";
                    return RedirectToAction("Details", new { id = stakeholderId });
                }

                var organizations = _stakeholderRepository.GetStakeholderOrganizations(stakeholderId, false);
                var rootOrganizations = organizations.Where(o => o.ParentOrganizationId == null).ToList();

                var viewModel = new StakeholderOrganizationChartViewModel
                {
                    StakeholderId = stakeholderId,
                    StakeholderName = stakeholder.DisplayName,
                    RootOrganizations = _mapper.Map<List<StakeholderOrganizationViewModel>>(rootOrganizations)
                };

                // پر کردن اطلاعات تودرتو
                foreach (var org in viewModel.RootOrganizations)
                {
                    PopulateOrganizationChildren(org, organizations);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "StakeholderOrganization",
                    "OrganizationChart",
                    $"مشاهده چارت سازمانی: {stakeholder.DisplayName}",
                    recordId: stakeholderId.ToString()
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "OrganizationChart",
                    "خطا در نمایش چارت",
                    ex,
                    recordId: stakeholderId.ToString()
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// متد کمکی برای پر کردن زیرمجموعه‌های سازمانی
        /// </summary>
        private void PopulateOrganizationChildren(StakeholderOrganizationViewModel parent, List<StakeholderOrganization> allOrganizations)
        {
            var children = allOrganizations.Where(o => o.ParentOrganizationId == parent.Id).ToList();

            if (children.Any())
            {
                parent.ChildOrganizations = _mapper.Map<List<StakeholderOrganizationViewModel>>(children);

                foreach (var child in parent.ChildOrganizations)
                {
                    PopulateOrganizationChildren(child, allOrganizations);
                }
            }
        }

        /// <summary>
        /// افزودن واحد سازمانی جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddOrganization(int stakeholderId, int? parentOrganizationId = null)
        {
            try
            {
                var stakeholder = _stakeholderRepository.GetStakeholderById(stakeholderId);

                if (stakeholder == null || stakeholder.PersonType != 1)
                    return RedirectToAction("ErrorView", "Home");

                var contacts = _stakeholderRepository.GetStakeholderContacts(stakeholderId, false);

                ViewBag.StakeholderId = stakeholderId;
                ViewBag.StakeholderName = stakeholder.DisplayName;
                ViewBag.ParentOrganizationId = parentOrganizationId;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

                var model = new StakeholderOrganizationViewModel
                {
                    StakeholderId = stakeholderId,
                    ParentOrganizationId = parentOrganizationId,
                    IsActive = true,
                    DisplayOrder = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "AddOrganization",
                    "خطا در نمایش فرم",
                    ex
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره واحد سازمانی جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrganization(StakeholderOrganizationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ دریافت UserId از BaseController
                    var currentUserId = GetUserId();
                    
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        ModelState.AddModelError("", "خطا در شناسایی کاربر. لطفاً دوباره وارد شوید");
                        
                        var stakeholderError = _stakeholderRepository.GetStakeholderById(model.StakeholderId);
                        var contactsError = _stakeholderRepository.GetStakeholderContacts(model.StakeholderId, false);

                        ViewBag.StakeholderId = model.StakeholderId;
                        ViewBag.StakeholderName = stakeholderError?.DisplayName;
                        ViewBag.ParentOrganizationId = model.ParentOrganizationId;
                        ViewBag.AvailableContacts = new SelectList(contactsError, "Id", "FullName");

                        return View(model);
                    }

                    // ✅ بررسی StakeholderId قبل از ایجاد Entity
                    if (model.StakeholderId <= 0)
                    {
                        ModelState.AddModelError("", "شناسه طرف حساب نامعتبر است");
                        
                        var stakeholderError = _stakeholderRepository.GetStakeholderById(model.StakeholderId);
                        var contactsError = _stakeholderRepository.GetStakeholderContacts(model.StakeholderId, false);

                        ViewBag.StakeholderId = model.StakeholderId;
                        ViewBag.StakeholderName = stakeholderError?.DisplayName;
                        ViewBag.ParentOrganizationId = model.ParentOrganizationId;
                        ViewBag.AvailableContacts = new SelectList(contactsError, "Id", "FullName");

                        return View(model);
                    }

                    // ✅ ایجاد Entity از ViewModel
                    var organization = _mapper.Map<StakeholderOrganization>(model);
                    organization.CreatorUserId = currentUserId;

                    // ✅ Debug: چاپ اطلاعات قبل از ثبت
                    System.Diagnostics.Debug.WriteLine($"🔍 StakeholderId: {organization.StakeholderId}");
                    System.Diagnostics.Debug.WriteLine($"🔍 Title: {organization.Title}");
                    System.Diagnostics.Debug.WriteLine($"🔍 CreatorUserId: {organization.CreatorUserId}");

                    // ✅ ثبت از طریق Repository
                    var organizationId = _stakeholderRepository.CreateOrganization(organization);

                    // ✅ ثبت لاگ
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "StakeholderOrganization",
                        "AddOrganization",
                        $"ایجاد واحد سازمانی: {organization.Title}",
                        recordId: organizationId.ToString()
                    );

                    return RedirectToAction("OrganizationChart", new { stakeholderId = model.StakeholderId });
                }
                catch (ArgumentException ex)
                {
                    // ✅ خطاهای Validation
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "AddOrganization",
                        "خطای اعتبارسنجی",
                        ex
                    );
                    ModelState.AddModelError("", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    // ✅ خطاهای منطقی
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "AddOrganization",
                        "خطای منطقی",
                        ex
                    );
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    // ✅ خطاهای عمومی
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "AddOrganization",
                        "خطا در ایجاد واحد",
                        ex
                    );
                    ModelState.AddModelError("", "خطا در ثبت: " + ex.Message);
                }
            }

            // ✅ بازگشت به View در صورت خطا
            var stakeholder = _stakeholderRepository.GetStakeholderById(model.StakeholderId);
            var contacts = _stakeholderRepository.GetStakeholderContacts(model.StakeholderId, false);

            ViewBag.StakeholderId = model.StakeholderId;
            ViewBag.StakeholderName = stakeholder?.DisplayName;
            ViewBag.ParentOrganizationId = model.ParentOrganizationId;
            ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");

            return View(model);
        }

        /// <summary>
        /// افزودن عضو به واحد سازمانی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddOrganizationMember(int organizationId)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(organizationId);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var availableContacts = _stakeholderRepository.GetAvailableContactsForOrganization(
                    organization.StakeholderId,
                    organizationId
                );

                var positions = _stakeholderRepository.GetOrganizationPositions(organizationId, false);

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationTitle = organization.Title;
                ViewBag.AvailableContacts = new SelectList(availableContacts, "Id", "FullName");
                ViewBag.Positions = new SelectList(positions, "Id", "Title");

                var model = new StakeholderOrganizationMemberViewModel
                {
                    OrganizationId = organizationId,
                    IsActive = true,
                    JoinDate = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "AddOrganizationMember",
                    "خطا در نمایش فرم",
                    ex
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// افزودن عضو به واحد سازمانی - با استفاده از RenderViewToStringAsync
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrganizationMember(StakeholderOrganizationMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی تکراری نبودن
                    if (_stakeholderRepository.IsContactAlreadyMember(model.OrganizationId, model.ContactId))
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[]
                            {
                        new { status = "warning", text = "این شخص قبلاً به این واحد اضافه شده است" }
                    }
                        });
                    }

                    var member = _mapper.Map<StakeholderOrganizationMember>(model);
                    member.CreateDate = DateTime.Now;
                    member.CreatorUserId = _userManager.GetUserId(User);

                    _uow.StakeholderOrganizationMemberUW.Create(member);
                    _uow.Save();

                    var organization = _stakeholderRepository.GetStakeholderOrganizationById(model.OrganizationId);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "StakeholderOrganization",
                        "AddOrganizationMember",
                        $"افزودن عضو به واحد: {organization.Title}",
                        recordId: member.Id.ToString()
                    );

                    // رندر کردن چارت سازمانی به‌روزرسانی شده
                    var stakeholder = _stakeholderRepository.GetStakeholderById(organization.StakeholderId, includeOrganizations: true);
                    var organizations = _stakeholderRepository.GetStakeholderOrganizations(organization.StakeholderId, false);
                    var rootOrganizations = organizations.Where(o => o.ParentOrganizationId == null).ToList();

                    var chartViewModel = new StakeholderOrganizationChartViewModel
                    {
                        StakeholderId = organization.StakeholderId,
                        StakeholderName = stakeholder.DisplayName,
                        RootOrganizations = _mapper.Map<List<StakeholderOrganizationViewModel>>(rootOrganizations)
                    };

                    // پر کردن اطلاعات تودرتو
                    foreach (var org in chartViewModel.RootOrganizations)
                    {
                        PopulateOrganizationChildren(org, organizations);
                    }

                    // رندر کردن چارت
                    var renderedChart = await this.RenderViewToStringAsync("_OrganizationChartNodes", chartViewModel.RootOrganizations);

                    return Json(new
                    {
                        status = "update-view",
                        message = new[]
                        {
                    new { status = "success", text = "عضو با موفقیت اضافه شد" }
                },
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "org-chart-container",
                        view = new
                        {
                            result = renderedChart
                        }
                    }
                }
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "AddOrganizationMember",
                        "خطا در افزودن عضو",
                        ex
                    );

                    return Json(new
                    {
                        status = "error",
                        message = new[]
                        {
                    new { status = "error", text = "خطا در ذخیره: " + ex.Message }
                }
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
                message = errors.Any() ? errors : new[]
                {
            new { status = "error", text = "اطلاعات نامعتبر است" }
        }
            });
        }


        // ==================== EDIT ORGANIZATION ====================

        /// <summary>
        /// ویرایش واحد سازمانی - نمایش فرم
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditOrganization(int id)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(id);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var viewModel = _mapper.Map<StakeholderOrganizationViewModel>(organization);

                var contacts = _stakeholderRepository.GetStakeholderContacts(organization.StakeholderId, false);
                var stakeholder = _stakeholderRepository.GetStakeholderById(organization.StakeholderId);

                ViewBag.StakeholderId = organization.StakeholderId;
                ViewBag.StakeholderName = stakeholder?.DisplayName;
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName", organization.ManagerContactId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "StakeholderOrganization",
                    "EditOrganization",
                    $"مشاهده فرم ویرایش واحد: {organization.Title}",
                    recordId: id.ToString()
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "EditOrganization",
                    "خطا در نمایش فرم",
                    ex,
                    recordId: id.ToString()
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ویرایش واحد سازمانی - با RenderViewToStringAsync
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganization(StakeholderOrganizationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var organization = _stakeholderRepository.GetStakeholderOrganizationById(model.Id);

                    if (organization == null)
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[]
                            {
                        new { status = "error", text = "واحد سازمانی یافت نشد" }
                    }
                        });
                    }

                    var oldValues = new
                    {
                        organization.Title,
                        organization.ManagerContactId,
                        organization.IsActive
                    };

                    var originalCreateDate = organization.CreateDate;
                    var originalCreatorUserId = organization.CreatorUserId;
                    var originalLevel = organization.Level;

                    _mapper.Map(model, organization);

                    organization.CreateDate = originalCreateDate;
                    organization.CreatorUserId = originalCreatorUserId;
                    organization.Level = originalLevel;
                    organization.LastUpdateDate = DateTime.Now;
                    organization.LastUpdaterUserId = _userManager.GetUserId(User);

                    _uow.StakeholderOrganizationUW.Update(organization);
                    _uow.Save();

                    var newValues = new
                    {
                        organization.Title,
                        organization.ManagerContactId,
                        organization.IsActive
                    };

                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "StakeholderOrganization",
                        "EditOrganization",
                        $"ویرایش واحد: {organization.Title}",
                        oldValues,
                        newValues,
                        recordId: organization.Id.ToString()
                    );

                    // رندر کردن چارت به‌روزرسانی شده
                    var stakeholder = _stakeholderRepository.GetStakeholderById(organization.StakeholderId, includeOrganizations: true);
                    var organizations = _stakeholderRepository.GetStakeholderOrganizations(organization.StakeholderId, false);
                    var rootOrganizations = organizations.Where(o => o.ParentOrganizationId == null).ToList();

                    var chartViewModel = new StakeholderOrganizationChartViewModel
                    {
                        StakeholderId = organization.StakeholderId,
                        StakeholderName = stakeholder.DisplayName,
                        RootOrganizations = _mapper.Map<List<StakeholderOrganizationViewModel>>(rootOrganizations)
                    };

                    foreach (var org in chartViewModel.RootOrganizations)
                    {
                        PopulateOrganizationChildren(org, organizations);
                    }

                    var renderedChart = await this.RenderViewToStringAsync("_OrganizationChartNodes", chartViewModel.RootOrganizations);

                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("OrganizationChart", new { stakeholderId = organization.StakeholderId }),
                        message = new[]
                        {
                    new { status = "success", text = "واحد با موفقیت ویرایش شد" }
                }
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "EditOrganization",
                        "خطا در ویرایش",
                        ex,
                        recordId: model.Id.ToString()
                    );

                    return Json(new
                    {
                        status = "error",
                        message = new[]
                        {
                    new { status = "error", text = "خطا در ذخیره: " + ex.Message }
                }
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
                message = errors.Any() ? errors : new[]
                {
            new { status = "error", text = "اطلاعات نامعتبر است" }
        }
            });
        }

        // ==================== DELETE ORGANIZATION ====================

        /// <summary>
        /// حذف واحد سازمانی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(id, true, true);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.HasChildren = organization.ChildOrganizations?.Any() ?? false;
                ViewBag.HasMembers = organization.Members?.Any() ?? false;

                return PartialView("_DeleteOrganization", organization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "DeleteOrganization",
                    "خطا در نمایش فرم حذف",
                    ex,
                    recordId: id.ToString()
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrganizationPost(int id)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(id, true, true);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی وجود زیرمجموعه
                if (organization.ChildOrganizations?.Any() ?? false)
                {
                    TempData["ErrorMessage"] = "امکان حذف واحدی که دارای زیرمجموعه است وجود ندارد";
                    return RedirectToAction("OrganizationChart", new { stakeholderId = organization.StakeholderId });
                }

                var stakeholderId = organization.StakeholderId;
                var title = organization.Title;

                // حذف اعضا
                if (organization.Members?.Any() ?? false)
                {
                    foreach (var member in organization.Members.ToList())
                    {
                        _uow.StakeholderOrganizationMemberUW.Delete(member);
                    }
                }

                // حذف سمت‌ها
                if (organization.Positions?.Any() ?? false)
                {
                    foreach (var position in organization.Positions.ToList())
                    {
                        _uow.StakeholderOrganizationPositionUW.Delete(position);
                    }
                }

                _uow.StakeholderOrganizationUW.Delete(organization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "StakeholderOrganization",
                    "DeleteOrganization",
                    $"حذف واحد سازمانی: {title}",
                    recordId: id.ToString()
                );

                return RedirectToAction("OrganizationChart", new { stakeholderId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "DeleteOrganization",
                    "خطا در حذف",
                    ex,
                    recordId: id.ToString()
                );
                TempData["ErrorMessage"] = "خطا در حذف واحد";
                return RedirectToAction("OrganizationChart");
            }
        }

        // ==================== MANAGE POSITIONS ====================

        /// <summary>
        /// مدیریت سمت‌های واحد سازمانی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManagePositions(int organizationId)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(organizationId, true, false);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var positions = _stakeholderRepository.GetOrganizationPositions(organizationId, true);

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationTitle = organization.Title;
                ViewBag.StakeholderId = organization.StakeholderId;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "StakeholderOrganization",
                    "ManagePositions",
                    $"مشاهده سمت‌های واحد: {organization.Title}",
                    recordId: organizationId.ToString()
                );

                return View(positions);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "ManagePositions",
                    "خطا در نمایش سمت‌ها",
                    ex,
                    recordId: organizationId.ToString()
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }


        /// <summary>
        /// نمایش modal برای افزودن سمت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddPositionModal(int organizationId)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(organizationId);

                if (organization == null)
                    return NotFound();

                var model = new StakeholderOrganizationPositionViewModel
                {
                    OrganizationId = organizationId,
                    IsActive = true,
                    DisplayOrder = 1,
                    PowerLevel = 1
                };

                ViewBag.OrganizationTitle = organization.Title;

                return PartialView("_AddPositionModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "AddPositionModal",
                    "خطا در نمایش modal",
                    ex
                );
                return StatusCode(500);
            }
        }

        /// <summary>
        /// ذخیره سمت جدید - برای استفاده با modal-ajax-save
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPosition(StakeholderOrganizationPositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // اگر این سمت پیش‌فرض است، سایر سمت‌ها را غیرپیش‌فرض کنیم
                    if (model.IsDefault)
                    {
                        var defaultPositions = _uow.StakeholderOrganizationPositionUW
                            .Get(p => p.OrganizationId == model.OrganizationId && p.IsDefault);

                        foreach (var pos in defaultPositions)
                        {
                            pos.IsDefault = false;
                            _uow.StakeholderOrganizationPositionUW.Update(pos);
                        }
                    }

                    var position = _mapper.Map<StakeholderOrganizationPosition>(model);
                    position.CreateDate = DateTime.Now;
                    position.CreatorUserId = _userManager.GetUserId(User);

                    _uow.StakeholderOrganizationPositionUW.Create(position);
                    _uow.Save();

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "StakeholderOrganization",
                        "AddPosition",
                        $"ایجاد سمت: {position.Title}",
                        recordId: position.Id.ToString()
                    );

                    // دریافت لیست به‌روزرسانی شده سمت‌ها
                    var organization = _stakeholderRepository.GetStakeholderOrganizationById(model.OrganizationId);
                    var positions = _stakeholderRepository.GetOrganizationPositions(model.OrganizationId, true);

                    // رندر کردن partial view با استفاده از RenderViewToStringAsync
                    var renderedView = await this.RenderViewToStringAsync("_PositionsTableRows", positions);

                    // پاسخ برای modal-ajax-save
                    return Json(new
                    {
                        status = "update-view",
                        message = new[]
                        {
                    new { status = "success", text = "سمت با موفقیت اضافه شد" }
                },
                        viewList = new[]
                        {
                    new
                    {
                        elementId = "positionsTableBody",
                        view = new
                        {
                            result = renderedView
                        }
                    }
                }
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "StakeholderOrganization",
                        "AddPosition",
                        "خطا در افزودن سمت",
                        ex
                    );

                    return Json(new
                    {
                        status = "error",
                        message = new[]
                        {
                    new { status = "error", text = "خطا در ذخیره: " + ex.Message }
                }
                    });
                }
            }

            // خطاهای validation
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => new { status = "error", text = e.ErrorMessage })
                .ToArray();

            return Json(new
            {
                status = "validation-error",
                message = errors.Any() ? errors : new[]
                {
            new { status = "error", text = "اطلاعات نامعتبر است" }
        }
            });
        }

        // ==================== ORGANIZATION DETAILS ====================

        /// <summary>
        /// جزئیات واحد سازمانی
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrganizationDetails(int id)
        {
            try
            {
                var organization = _stakeholderRepository.GetStakeholderOrganizationById(id, true, true);

                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var viewModel = _mapper.Map<StakeholderOrganizationViewModel>(organization);

                ViewBag.Positions = organization.Positions;
                ViewBag.Members = organization.Members;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "StakeholderOrganization",
                    "OrganizationDetails",
                    $"مشاهده جزئیات واحد: {organization.Title}",
                    recordId: id.ToString()
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "StakeholderOrganization",
                    "OrganizationDetails",
                    "خطا در نمایش جزئیات",
                    ex,
                    recordId: id.ToString()
                );
                return RedirectToAction("ErrorView", "Home");
            }
        } 
    
    }
}
