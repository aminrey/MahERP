using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.StaticClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.CommonLayer.PublicClasses;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class StakeholderController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public StakeholderController(
            IUnitOfWork uow,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _uow = uow;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست طرف حساب‌ها
        public async Task<IActionResult> Index(int? type = null, bool includeDeleted = false)
        {
            try
            {
                var stakeholders = _stakeholderRepository.GetStakeholders(includeDeleted, type);

                // اضافه کردن اطلاعات به ViewBag برای نمایش در View
                ViewBag.IncludeDeleted = includeDeleted;
                ViewBag.CurrentType = type;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "Index",
                    $"مشاهده لیست طرف حساب‌ها - نوع: {type?.ToString() ?? "همه"}, شامل حذف شده: {includeDeleted}"
                );

                return View(stakeholders);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "Index",
                    "خطا در دریافت لیست طرف حساب‌ها",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // جزئیات طرف حساب
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var stakeholder = _stakeholderRepository.GetStakeholderById(id, true, true, true, true);
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

                var stakeholderCRM = _stakeholderRepository.GetStakeholderCRMById(id);

                var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);
                if (stakeholderCRM != null)
                {
                    viewModel.CRMInfo = _mapper.Map<StakeholderCRMViewModel>(stakeholderCRM);
                }

                // اضافه کردن اطلاعات کانتکت‌ها به ViewBag - همه کانتکت‌ها (فعال و غیرفعال)
                ViewBag.Contacts = _stakeholderRepository.GetStakeholderContacts(id, true); // true برای شامل شدن غیرفعال‌ها

                // اضافه کردن سایر اطلاعات مرتبط
                ViewBag.Contracts = stakeholder.Contracts?.ToList() ?? new List<Contract>();
                ViewBag.Tasks = stakeholder.TaskList?.ToList() ?? new List<Tasks>();
                ViewBag.CRMInteractions = new List<CRMInteraction>(); // نیاز به پیاده‌سازی در repository

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "Details",
                    $"مشاهده جزئیات طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "Details",
                    "خطا در دریافت جزئیات طرف حساب",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن طرف حساب جدید - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> AddStakeholder()
        {
            try
            {
                ViewBag.SalesReps = new SelectList(_userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                    "Id", "FullName");

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "AddStakeholder",
                    "مشاهده فرم افزودن طرف حساب جدید"
                );

                return View(new StakeholderViewModel { IsActive = true });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "AddStakeholder",
                    "خطا در نمایش فرم افزودن طرف حساب",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // افزودن طرف حساب جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStakeholder(StakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی یکتا بودن کد ملی و ایمیل
                    if (!string.IsNullOrEmpty(model.NationalCode) && !_stakeholderRepository.IsNationalCodeUnique(model.NationalCode))
                    {
                        ModelState.AddModelError("NationalCode", "کد ملی وارد شده قبلاً ثبت شده است");
                        ViewBag.SalesReps = new SelectList(_userManager.Users
                            .Where(u => u.IsActive && !u.IsRemoveUser)
                            .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                            "Id", "FullName");
                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(model.Email) && !_stakeholderRepository.IsEmailUnique(model.Email))
                    {
                        ModelState.AddModelError("Email", "ایمیل وارد شده قبلاً ثبت شده است");
                        ViewBag.SalesReps = new SelectList(_userManager.Users
                            .Where(u => u.IsActive && !u.IsRemoveUser)
                            .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                            "Id", "FullName");
                        return View(model);
                    }

                    // ایجاد طرف حساب جدید
                    var stakeholder = _mapper.Map<Stakeholder>(model);
                    stakeholder.CreateDate = DateTime.Now;
                    stakeholder.CreatorUserId = _userManager.GetUserId(User);
                    stakeholder.IsActive = true;
                    stakeholder.IsDeleted = false;

                    // ذخیره در دیتابیس
                    _uow.StakeholderUW.Create(stakeholder);
                    _uow.Save();

                    // اگر اطلاعات CRM وجود داشته باشد، آنها را هم ذخیره می‌کنیم
                    if (model.CRMInfo != null)
                    {
                        var stakeholderCRM = _mapper.Map<StakeholderCRM>(model.CRMInfo);
                        stakeholderCRM.StakeholderId = stakeholder.Id;
                        stakeholderCRM.CreateDate = DateTime.Now;

                        _uow.StakeholderCRMUW.Create(stakeholderCRM);
                        _uow.Save();
                    }

                    // ثبت لاگ موفقیت
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Stakeholders",
                        "AddStakeholder",
                        $"ایجاد طرف حساب جدید: {stakeholder.FirstName} {stakeholder.LastName}",
                        recordId: stakeholder.Id.ToString(),
                        entityType: "Stakeholder",
                        recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Stakeholders",
                        "AddStakeholder",
                        "خطا در ایجاد طرف حساب جدید",
                        ex
                    );
                    
                    ModelState.AddModelError("", "خطایی در ثبت طرف حساب رخ داد: " + ex.Message);
                }
            }
            
            ViewBag.SalesReps = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
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

                // دریافت اطلاعات CRM اگر وجود داشته باشد
                var stakeholderCRM = _stakeholderRepository.GetStakeholderCRMById(id);
                if (stakeholderCRM != null)
                {
                    viewModel.CRMInfo = _mapper.Map<StakeholderCRMViewModel>(stakeholderCRM);
                }

                ViewBag.SalesReps = new SelectList(_userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                    "Id", "FullName");

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "EditStakeholder",
                    $"مشاهده فرم ویرایش طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "EditStakeholder",
                    "خطا در نمایش فرم ویرایش طرف حساب",
                    ex,
                    recordId: id.ToString()
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ویرایش طرف حساب - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStakeholder(StakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن کد ملی و ایمیل
                if (!string.IsNullOrEmpty(model.NationalCode) &&
                    !_stakeholderRepository.IsNationalCodeUnique(model.NationalCode, model.Id))
                {
                    ModelState.AddModelError("NationalCode", "کد ملی وارد شده قبلاً ثبت شده است");
                    ViewBag.SalesReps = new SelectList(_userManager.Users
                        .Where(u => u.IsActive && !u.IsRemoveUser)
                        .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                        "Id", "FullName");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.Email) &&
                    !_stakeholderRepository.IsEmailUnique(model.Email, model.Id))
                {
                    ModelState.AddModelError("Email", "ایمیل وارد شده قبلاً ثبت شده است");
                    ViewBag.SalesReps = new SelectList(_userManager.Users
                        .Where(u => u.IsActive && !u.IsRemoveUser)
                        .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                        "Id", "FullName");
                    return View(model);
                }

                try
                {
                    // دریافت طرف حساب از دیتابیس
                    var stakeholder = _uow.StakeholderUW.GetById(model.Id);
                    if (stakeholder == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "Stakeholders",
                            "EditStakeholder",
                            "تلاش برای ویرایش طرف حساب غیرموجود",
                            recordId: model.Id.ToString()
                        );
                        return RedirectToAction("ErrorView", "Home");
                    }

                    // ذخیره مقادیر قبلی برای لاگ
                    var oldValues = new
                    {
                        stakeholder.FirstName,
                        stakeholder.LastName,
                        stakeholder.CompanyName,
                        stakeholder.Phone,
                        stakeholder.Mobile,
                        stakeholder.Email,
                        stakeholder.NationalCode,
                        stakeholder.IsActive
                    };

                    // حفظ مقادیر اصلی که نباید تغییر کنند
                    var originalCreateDate = stakeholder.CreateDate;
                    var originalCreatorUserId = stakeholder.CreatorUserId;

                    // به‌روزرسانی اطلاعات
                    _mapper.Map(model, stakeholder);

                    // بازگردانی مقادیر اصلی
                    stakeholder.CreateDate = originalCreateDate;
                    stakeholder.CreatorUserId = originalCreatorUserId;

                    _uow.StakeholderUW.Update(stakeholder);
                    _uow.Save();

                    // به‌روزرسانی اطلاعات CRM
                    if (model.CRMInfo != null)
                    {
                        var stakeholderCRM = _uow.StakeholderCRMUW.Get(c => c.StakeholderId == model.Id).FirstOrDefault();

                        if (stakeholderCRM == null)
                        {
                            // ایجاد رکورد جدید اگر وجود نداشته باشد
                            stakeholderCRM = new StakeholderCRM
                            {
                                StakeholderId = model.Id,
                                CreateDate = DateTime.Now
                            };
                            _mapper.Map(model.CRMInfo, stakeholderCRM);
                            _uow.StakeholderCRMUW.Create(stakeholderCRM);
                        }
                        else
                        {
                            // حفظ مقادیر اصلی CRM
                            var originalCRMCreateDate = stakeholderCRM.CreateDate;
                            var originalStakeholderId = stakeholderCRM.StakeholderId;

                            // بروزرسانی رکورد موجود
                            _mapper.Map(model.CRMInfo, stakeholderCRM);

                            // بازگردانی مقادیر اصلی
                            stakeholderCRM.CreateDate = originalCRMCreateDate;
                            stakeholderCRM.StakeholderId = originalStakeholderId;
                            stakeholderCRM.LastUpdateDate = DateTime.Now;

                            _uow.StakeholderCRMUW.Update(stakeholderCRM);
                        }

                        _uow.Save();
                    }

                    // مقادیر جدید برای لاگ
                    var newValues = new
                    {
                        stakeholder.FirstName,
                        stakeholder.LastName,
                        stakeholder.CompanyName,
                        stakeholder.Phone,
                        stakeholder.Mobile,
                        stakeholder.Email,
                        stakeholder.NationalCode,
                        stakeholder.IsActive
                    };

                    // ثبت لاگ تغییرات
                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Stakeholders",
                        "EditStakeholder",
                        $"ویرایش طرف حساب: {stakeholder.FirstName} {stakeholder.LastName}",
                        oldValues,
                        newValues,
                        recordId: stakeholder.Id.ToString(),
                        entityType: "Stakeholder",
                        recordTitle: $"{stakeholder.FirstName} {stakeholder.LastName}"
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Stakeholders",
                        "EditStakeholder",
                        "خطا در ویرایش طرف حساب",
                        ex,
                        recordId: model.Id.ToString()
                    );
                    
                    // لاگ کردن خطای دقیق‌تر
                    ModelState.AddModelError("", $"خطا در ذخیره اطلاعات: {ex.Message}");

                    // اگر خطای Inner Exception وجود دارد
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"جزئیات خطا: {ex.InnerException.Message}");
                    }
                }
            }

            // در صورت وجود خطا، ViewBag را دوباره تنظیم کنید
            ViewBag.SalesReps = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
            return View(model);
        }

        // ویرایش اطلاعات CRM طرف حساب - نمایش فرم
        [HttpGet]
        public IActionResult EditCRM(int id)
        {
            var stakeholder = _stakeholderRepository.GetStakeholderById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);

            // دریافت اطلاعات CRM اگر وجود داشته باشد
            var stakeholderCRM = _stakeholderRepository.GetStakeholderCRMById(id);
            if (stakeholderCRM != null)
            {
                viewModel.CRMInfo = _mapper.Map<StakeholderCRMViewModel>(stakeholderCRM);
            }
            else
            {
                // اگر اطلاعات CRM وجود نداشته باشد، یک نمونه جدید ایجاد می‌کنیم
                viewModel.CRMInfo = new StakeholderCRMViewModel();
            }

            ViewBag.SalesReps = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName", viewModel.CRMInfo.SalesRepUserId);

            return View(viewModel);
        }

        // ویرایش اطلاعات CRM طرف حساب - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCRM(StakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // دریافت طرف حساب از دیتابیس
                    var stakeholder = _uow.StakeholderUW.GetById(model.Id);
                    if (stakeholder == null)
                        return RedirectToAction("ErrorView", "Home");

                    // به‌روزرسانی اطلاعات CRM
                    if (model.CRMInfo != null)
                    {
                        var stakeholderCRM = _uow.StakeholderCRMUW.Get(c => c.StakeholderId == model.Id).FirstOrDefault();

                        if (stakeholderCRM == null)
                        {
                            // ایجاد رکورد جدید اگر وجود نداشته باشد
                            stakeholderCRM = new StakeholderCRM
                            {
                                StakeholderId = model.Id,
                                CreateDate = DateTime.Now
                            };
                            _mapper.Map(model.CRMInfo, stakeholderCRM);
                            _uow.StakeholderCRMUW.Create(stakeholderCRM);
                        }
                        else
                        {
                            // حفظ مقادیر اصلی CRM
                            var originalCRMCreateDate = stakeholderCRM.CreateDate;
                            var originalStakeholderId = stakeholderCRM.StakeholderId;

                            // بروزرسانی رکورد موجود
                            _mapper.Map(model.CRMInfo, stakeholderCRM);

                            // بازگردانی مقادیر اصلی
                            stakeholderCRM.CreateDate = originalCRMCreateDate;
                            stakeholderCRM.StakeholderId = originalStakeholderId;
                            stakeholderCRM.LastUpdateDate = DateTime.Now;

                            _uow.StakeholderCRMUW.Update(stakeholderCRM);
                        }

                        _uow.Save();
                    }

                    return RedirectToAction("Details", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    // لاگ کردن خطای دقیق‌تر
                    ModelState.AddModelError("", $"خطا در ذخیره اطلاعات CRM: {ex.Message}");

                    // اگر خطای Inner Exception وجود دارد
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"جزئیات خطا: {ex.InnerException.Message}");
                    }
                }
            }

            // در صورت وجود خطا، ViewBag را دوباره تنظیم کنید
            ViewBag.SalesReps = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName", model.CRMInfo?.SalesRepUserId);
            
            return View(model);
        }

        // فعال/غیرفعال کردن طرف حساب - نمایش مودال تأیید
        [HttpGet]
        public async Task<IActionResult> ActiveOrDeactiveStakeholder(int id)
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

                // ثبت لاگ
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

        // فعال/غیرفعال کردن طرف حساب - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActiveOrDeactiveStakeholderPost(int Id, bool IsActive)
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(Id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                var oldStatus = stakeholder.IsActive;
                
                if (IsActive == true)
                {
                    stakeholder.IsActive = false;
                }
                else
                {
                    stakeholder.IsActive = true;
                }

                _uow.StakeholderUW.Update(stakeholder);
                _uow.Save();

                // ثبت لاگ
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
                
                return RedirectToAction("Index");
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

        // حذف طرف حساب - نمایش مودال تأیید
        [HttpGet]
        public async Task<IActionResult> DeleteStakeholder(int id)
        {
            try
            {
                var stakeholder = _uow.StakeholderUW.GetById(id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ViewTitle = "حذف طرف حساب";

                // ثبت لاگ
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

        // حذف طرف حساب - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStakeholderPost(int id)
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

                // ثبت لاگ حذف
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Stakeholders",
                    "DeleteStakeholder",
                    $"حذف طرف حساب: {stakeholderName}",
                    recordId: id.ToString(),
                    entityType: "Stakeholder",
                    recordTitle: stakeholderName
                );

                // بررسی اینکه درخواست AJAX است یا نه
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("Index")
                    });
                }

                return RedirectToAction(nameof(Index));
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

        // خروجی اکسل از طرف حساب‌ها
        public async Task<IActionResult> ExportToExcel(StakeholderSearchViewModel model)
        {
            try
            {
                // اجرای دوباره جستجو برای بدست آوردن نتایج
                var query = _uow.StakeholderUW.Get().AsQueryable();

                // فیلتر وضعیت حذف شده
                if (!model.IncludeDeleted)
                {
                    query = query.Where(s => !s.IsDeleted);
                }

                // فیلتر وضعیت فعال
                if (model.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == model.IsActive.Value);
                }

                // سایر فیلترها مشابه متد Search
                // ...

                var stakeholders = query.OrderByDescending(s => s.CreateDate).ToList();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Download,
                    "Stakeholders",
                    "ExportToExcel",
                    $"دانلود فایل اکسل طرف حساب‌ها - تعداد رکورد: {stakeholders.Count}"
                );

                // ایجاد فایل اکسل
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("طرف حساب‌ها");

                    // سرستون‌ها
                    worksheet.Cell(1, 1).Value = "ردیف";
                    worksheet.Cell(1, 2).Value = "نام";
                    worksheet.Cell(1, 3).Value = "نام خانوادگی";
                    worksheet.Cell(1, 4).Value = "نام شرکت";
                    worksheet.Cell(1, 5).Value = "تلفن ثابت";
                    worksheet.Cell(1, 6).Value = "تلفن همراه";
                    worksheet.Cell(1, 7).Value = "ایمیل";
                    worksheet.Cell(1, 8).Value = "کد ملی";
                    worksheet.Cell(1, 9).Value = "نوع طرف حساب";
                    worksheet.Cell(1, 10).Value = "وضعیت";
                    worksheet.Cell(1, 11).Value = "آدرس";
                    worksheet.Cell(1, 12).Value = "تاریخ ثبت";

                    // استایل هدر
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // ترتیب ردیف
                    int row = 2;
                    foreach (var item in stakeholders)
                    {
                        worksheet.Cell(row, 1).Value = (row - 1);
                        worksheet.Cell(row, 2).Value = item.FirstName;
                        worksheet.Cell(row, 3).Value = item.LastName;
                        worksheet.Cell(row, 4).Value = item.CompanyName;
                        worksheet.Cell(row, 5).Value = item.Phone;
                        worksheet.Cell(row, 6).Value = item.Mobile;
                        worksheet.Cell(row, 7).Value = item.Email;
                        worksheet.Cell(row, 8).Value = item.NationalCode;

                        // نوع طرف حساب
                        string stakeholderType = item.StakeholderType switch
                        {
                            0 => "مشتری",
                            1 => "تامین کننده",
                            2 => "همکار",
                            3 => "سایر",
                            _ => "نامشخص"
                        };
                        worksheet.Cell(row, 9).Value = stakeholderType;

                        // وضعیت
                        string status = item.IsDeleted ? "حذف شده" : (item.IsActive ? "فعال" : "غیرفعال");
                        worksheet.Cell(row, 10).Value = status;

                        worksheet.Cell(row, 11).Value = item.Address;
                        worksheet.Cell(row, 12).Value = ConvertDateTime.ConvertMiladiToShamsi(item.CreateDate,"yyyy/MM/dd");

                        row++;
                    }

                    // تنظیم عرض ستون‌ها
                    worksheet.Columns().AdjustToContents();

                    // ذخیره به حافظه
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Flush();

                        return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            FileDownloadName = $"Stakeholders_{DateTime.Now:yyyy_MM_dd}.xlsx"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "ExportToExcel",
                    "خطا در دانلود فایل اکسل",
                    ex
                );
                
                return RedirectToAction("Index");
            }
        }

        // جستجوی پیشرفته - نمایش فرم
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch()
        {
            try
            {
                // دریافت لیست کارشناسان فروش برای dropdown
                ViewBag.SalesReps = _userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToList();

                ViewBag.Users = _userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToList();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Stakeholders",
                    "AdvancedSearch",
                    "مشاهده فرم جستجوی پیشرفته طرف حساب‌ها"
                );

                return PartialView("_AdvancedSearch", new StakeholderSearchViewModel());
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "AdvancedSearch",
                    "خطا در نمایش فرم جستجوی پیشرفته",
                    ex
                );
                
                return PartialView("_AdvancedSearch", new StakeholderSearchViewModel());
            }
        }

        // جستجوی پیشرفته - پردازش جستجو
        [HttpPost]
        public async Task<IActionResult> Search(StakeholderSearchViewModel model)
        {
            try
            {
                var query = _uow.StakeholderUW.Get().AsQueryable();

                // فیلتر وضعیت حذف شده
                if (!model.IncludeDeleted)
                {
                    query = query.Where(s => !s.IsDeleted);
                }

                // فیلتر وضعیت فعال
                if (model.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == model.IsActive.Value);
                }

                // فیلتر نوع طرف حساب
                if (model.StakeholderType.HasValue)
                {
                    query = query.Where(s => s.StakeholderType == model.StakeholderType.Value);
                }

                // جستجو در نام و نام خانوادگی
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    query = query.Where(s => s.FirstName.Contains(model.Name) || s.LastName.Contains(model.Name));
                }

                // جستجو در نام شرکت
                if (!string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    query = query.Where(s => s.CompanyName.Contains(model.CompanyName));
                }

                // جستجو در تلفن
                if (!string.IsNullOrWhiteSpace(model.Phone))
                {
                    query = query.Where(s => s.Phone.Contains(model.Phone) || s.Mobile.Contains(model.Phone));
                }

                // جستجو در ایمیل
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    query = query.Where(s => s.Email.Contains(model.Email));
                }

                // جستجو در کد ملی
                if (!string.IsNullOrWhiteSpace(model.NationalCode))
                {
                    query = query.Where(s => s.NationalCode.Contains(model.NationalCode));
                }

                // فیلتر تاریخ ایجاد
                if (!string.IsNullOrWhiteSpace(model.FromDate))
                {
                    DateTime fromDate = ConvertDateTime.ConvertShamsiToMiladi(model.FromDate);
                    query = query.Where(s => s.CreateDate >= fromDate);
                }

                if (!string.IsNullOrWhiteSpace(model.ToDate))
                {
                    DateTime toDate = ConvertDateTime.ConvertShamsiToMiladi(model.ToDate).AddDays(1);
                    query = query.Where(s => s.CreateDate <= toDate);
                }

                // مرتب‌سازی بر اساس تاریخ ایجاد (نزولی)
                var stakeholders = query.OrderByDescending(s => s.CreateDate).ToList();

                // ثبت لاگ جستجو
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Search,
                    "Stakeholders",
                    "Search",
                    $"جستجوی پیشرفته طرف حساب‌ها - کلمه کلیدی: {model.Name ?? "خالی"}, تعداد نتایج: {stakeholders.Count}"
                );

                // بررسی اینکه درخواست AJAX است یا نه
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("SearchResults", "Stakeholder", model)
                    });
                }

                ViewBag.SearchModel = model;
                return View("SearchResults", stakeholders);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Stakeholders",
                    "Search",
                    "خطا در جستجوی پیشرفته طرف حساب‌ها",
                    ex
                );
                
                return View("SearchResults", new List<Stakeholder>());
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
    }
}
