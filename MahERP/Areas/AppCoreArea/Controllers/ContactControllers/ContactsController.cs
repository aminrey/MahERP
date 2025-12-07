using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.Helpers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactGroupRepository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.ContactControllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("CONTACT.VIEW")]
    public class ContactsController : BaseController
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactGroupRepository _groupRepository; // ⭐ NEW
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public ContactsController(
            IContactRepository contactRepository,
            IContactGroupRepository groupRepository, // ⭐ NEW
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, IModuleTrackingService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _contactRepository = contactRepository;
            _groupRepository = groupRepository; // ⭐ NEW
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX ====================

        /// <summary>
        /// لیست افراد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = null, byte? gender = null, int? groupId = null, bool includeInactive = false)
        {
            try
            {
                List<Contact> contacts;

                // اگر گروه انتخاب شده
                if (groupId.HasValue)
                {
                    contacts = _groupRepository.GetGroupContacts(groupId.Value, includeInactive);
                    ViewBag.SelectedGroupId = groupId.Value;
                }
                // جستجو
                else if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    contacts = _contactRepository.SearchContacts(searchTerm, gender, includeInactive);
                }
                // همه افراد
                else
                {
                    contacts = _contactRepository.GetAllContacts(includeInactive);
                    
                    if (gender.HasValue)
                        contacts = contacts.Where(c => c.Gender == gender.Value).ToList();
                }

                // ⭐⭐⭐ مرتب‌سازی بر اساس نام خانوادگی و نام
                contacts = contacts
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                var viewModels = _mapper.Map<List<ContactViewModel>>(contacts);

                // ⭐⭐⭐ گروه‌بندی بر اساس حرف اول نام خانوادگی
                var groupedContacts = viewModels
                    .GroupBy(c => 
                    {
                        if (string.IsNullOrEmpty(c.LastName)) return "#";
                        var firstChar = c.LastName[0];
                        if (char.IsLetter(firstChar))
                            return char.ToUpper(firstChar).ToString();
                        return "#";
                    })
                    .OrderBy(g => g.Key == "#" ? "ي" : g.Key) // "#" در انتها
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.GroupedContacts = groupedContacts;
                ViewBag.AlphabeticalIndex = groupedContacts.Keys.ToList();

                // ⭐ دریافت لیست گروه‌ها برای فیلتر
                var allGroups = _groupRepository.GetAllGroups(includeInactive: false);
                ViewBag.ContactGroups = allGroups;

                // ⭐ دریافت گروه‌های هر فرد (برای نمایش در جدول)
                var contactIds = contacts.Select(c => c.Id).ToList();
                var contactGroupsDict = new Dictionary<int, List<ContactGroup>>();
                foreach (var contactId in contactIds)
                {
                    contactGroupsDict[contactId] = _groupRepository.GetContactGroups(contactId);
                }
                ViewBag.ContactGroupsDict = contactGroupsDict;

                ViewBag.SearchTerm = searchTerm;
                ViewBag.Gender = gender;
                ViewBag.IncludeInactive = includeInactive;

                // آمار
                var stats = await _contactRepository.GetContactStatisticsAsync();
                ViewBag.Statistics = stats;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Contacts",
                    "Index",
                    "مشاهده لیست افراد"
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// جزئیات فرد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var contact = await _contactRepository.GetContactByIdAsync(id, 
                    includePhones: true, 
                    includeDepartments: true, 
                    includeOrganizations: true);

                if (contact == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Contacts",
                        "Details",
                        "تلاش برای مشاهده فرد غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<ContactViewModel>(contact);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Contacts",
                    "Details",
                    $"مشاهده جزئیات فرد: {contact.FullName}",
                    recordId: id.ToString(),
                    entityType: "Contact",
                    recordTitle: contact.FullName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Details", "خطا در دریافت جزئیات", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// نمایش فرم افزودن فرد جدید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Contacts",
                    "Create",
                    "مشاهده فرم افزودن فرد جدید"
                );

                return View(new ContactViewModel { IsActive = true });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Create", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره فرد جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactViewModel model, List<ContactPhoneInputViewModel> Phones)
        {
            // ⭐ اصلاح شده: فقط نام خانوادگی الزامی است
            if (string.IsNullOrEmpty(model.LastName))
            {
                ModelState.AddModelError("LastName", "نام خانوادگی الزامی است");
            }

            // بررسی یکتا بودن کد ملی
            if (!string.IsNullOrEmpty(model.NationalCode))
            {
                if (!_contactRepository.IsNationalCodeUnique(model.NationalCode))
                {
                    ModelState.AddModelError("NationalCode", "این کد ملی قبلاً ثبت شده است");
                }
            }

            // بررسی یکتا بودن ایمیل
            if (!string.IsNullOrEmpty(model.PrimaryEmail))
            {
                if (!_contactRepository.IsPrimaryEmailUnique(model.PrimaryEmail))
                {
                    ModelState.AddModelError("PrimaryEmail", "این ایمیل قبلاً ثبت شده است");
                }
            }

            // ⭐⭐⭐ اعتبارسنجی شماره‌های تماس (اگر وارد شده باشند)
            if (Phones != null && Phones.Any())
            {
                for (int i = 0; i < Phones.Count; i++)
                {
                    var phone = Phones[i];
                    if (!string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    {
                        if (!PhoneNumberHelper.ValidateIranianPhoneNumber(phone.PhoneNumber, out string phoneError))
                        {
                            ModelState.AddModelError($"Phones[{i}].PhoneNumber", phoneError);
                        }
                        else
                        {
                            // نرمال‌سازی شماره
                            phone.PhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(phone.PhoneNumber);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var contact = _mapper.Map<Contact>(model);
                    contact.CreatedDate = DateTime.Now;
                    contact.CreatorUserId = GetUserId();

                    _uow.ContactUW.Create(contact);
                    _uow.Save();

                    // ⭐⭐⭐ افزودن شماره‌های تماس (اگر وارد شده باشند)
                    if (Phones != null && Phones.Any())
                    {
                        var validPhones = Phones.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();
                        
                        if (validPhones.Any())
                        {
                            // اگر هیچ شماره‌ای IsDefault نشده، اولین شماره را پیش‌فرض کن
                            if (!validPhones.Any(p => p.IsDefault))
                            {
                                validPhones[0].IsDefault = true;
                            }

                            // اگر هیچ شماره‌ای IsSmsDefault نشده، اولین شماره را پیش‌فرض کن
                            if (!validPhones.Any(p => p.IsSmsDefault))
                            {
                                validPhones[0].IsSmsDefault = true;
                            }

                            int displayOrder = 1;
                            foreach (var phoneInput in validPhones)
                            {
                                var phone = new ContactPhone
                                {
                                    ContactId = contact.Id,
                                    PhoneNumber = phoneInput.PhoneNumber, // شماره نرمال شده
                                    PhoneType = phoneInput.PhoneType,
                                    IsDefault = phoneInput.IsDefault,
                                    IsSmsDefault = phoneInput.IsSmsDefault,
                                    IsActive = true,
                                    DisplayOrder = displayOrder++,
                                    CreatedDate = DateTime.Now,
                                    CreatorUserId = GetUserId()
                                };

                                _uow.ContactPhoneUW.Create(phone);
                            }

                            _uow.Save();
                        }
                    }

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Contacts",
                        "Create",
                        $"ایجاد فرد جدید: {contact.FullName}" + (Phones?.Any(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)) == true ? $" با {Phones.Count(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))} شماره تماس" : " بدون شماره تماس"),
                        recordId: contact.Id.ToString(),
                        entityType: "Contact",
                        recordTitle: contact.FullName
                    );

                    TempData["SuccessMessage"] = "فرد با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = contact.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Contacts", "Create", "خطا در ایجاد", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== EDIT ====================

        /// <summary>
        /// نمایش فرم ویرایش فرد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // ⭐⭐⭐ اصلاح شده: دریافت با شماره‌ها
                var contact = await _contactRepository.GetContactByIdAsync(id, 
                    includePhones: true, 
                    includeDepartments: false, 
                    includeOrganizations: false);

                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");

                var viewModel = _mapper.Map<ContactViewModel>(contact);

                // ⭐⭐⭐ Map کردن شماره‌ها
                if (contact.Phones != null && contact.Phones.Any())
                {
                    viewModel.Phones = _mapper.Map<List<ContactPhoneViewModel>>(
                        contact.Phones.Where(p => p.IsActive).OrderByDescending(p => p.IsDefault).ThenBy(p => p.DisplayOrder)
                    );
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Contacts",
                    "Edit",
                    $"مشاهده فرم ویرایش فرد: {contact.FullName}",
                    recordId: id.ToString(),
                    entityType: "Contact",
                    recordTitle: contact.FullName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Edit", "خطا در نمایش فرم", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// ذخیره ویرایش فرد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactViewModel model, List<ContactPhoneInputViewModel> Phones)
        {
            // ⭐ اصلاح شده: بررسی نام خانوادگی
            if (string.IsNullOrEmpty(model.LastName))
            {
                ModelState.AddModelError("LastName", "نام خانوادگی الزامی است");
            }

            // بررسی یکتا بودن کد ملی
            if (!string.IsNullOrEmpty(model.NationalCode))
            {
                if (!_contactRepository.IsNationalCodeUnique(model.NationalCode, model.Id))
                {
                    ModelState.AddModelError("NationalCode", "این کد ملی قبلاً ثبت شده است");
                }
            }

            // بررسی یکتا بودن ایمیل
            if (!string.IsNullOrEmpty(model.PrimaryEmail))
            {
                if (!_contactRepository.IsPrimaryEmailUnique(model.PrimaryEmail, model.Id))
                {
                    ModelState.AddModelError("PrimaryEmail", "این ایمیل قبلاً ثبت شده است");
                }
            }

            // ⭐⭐⭐ اعتبارسنجی شماره‌های تماس
            if (Phones != null && Phones.Any())
            {
                for (int i = 0; i < Phones.Count; i++)
                {
                    var phone = Phones[i];
                    if (!string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    {
                        if (!PhoneNumberHelper.ValidateIranianPhoneNumber(phone.PhoneNumber, out string phoneError))
                        {
                            ModelState.AddModelError($"Phones[{i}].PhoneNumber", phoneError);
                        }
                        else
                        {
                            // نرمال‌سازی شماره
                            phone.PhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(phone.PhoneNumber);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var contact = _uow.ContactUW.GetById(model.Id);
                    if (contact == null)
                        return RedirectToAction("ErrorView", "Home");

                    var oldValues = new
                    {
                        contact.FirstName,
                        contact.LastName,
                        contact.NationalCode,
                        contact.PrimaryEmail,
                        contact.IsActive
                    };

                    var originalCreated = contact.CreatedDate;
                    var originalCreatorId = contact.CreatorUserId;

                    _mapper.Map(model, contact);

                    contact.CreatedDate = originalCreated;
                    contact.CreatorUserId = originalCreatorId;
                    contact.LastUpdateDate = DateTime.Now;
                    contact.LastUpdaterUserId = GetUserId();

                    _uow.ContactUW.Update(contact);
                    _uow.Save();

                    // ⭐⭐⭐ مدیریت شماره‌های تماس
                    if (Phones != null && Phones.Any())
                    {
                        var validPhones = Phones.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();
                        
                        // حذف شماره‌های قدیمی
                        var existingPhones = _contactRepository.GetContactPhones(model.Id);
                        foreach (var oldPhone in existingPhones)
                        {
                            _uow.ContactPhoneUW.Delete(oldPhone);
                        }
                        _uow.Save();

                        if (validPhones.Any())
                        {
                            // اگر هیچ شماره‌ای IsDefault نشده، اولین شماره را پیش‌فرض کن
                            if (!validPhones.Any(p => p.IsDefault))
                            {
                                validPhones[0].IsDefault = true;
                            }

                            // اگر هیچ شماره‌ای IsSmsDefault نشده، اولین شماره را پیش‌فرض کن
                            if (!validPhones.Any(p => p.IsSmsDefault))
                            {
                                validPhones[0].IsSmsDefault = true;
                            }

                            int displayOrder = 1;
                            foreach (var phoneInput in validPhones)
                            {
                                var phone = new ContactPhone
                                {
                                    ContactId = contact.Id,
                                    PhoneNumber = phoneInput.PhoneNumber, // شماره نرمال شده
                                    PhoneType = phoneInput.PhoneType,
                                    IsDefault = phoneInput.IsDefault,
                                    IsSmsDefault = phoneInput.IsSmsDefault,
                                    IsActive = true,
                                    DisplayOrder = displayOrder++,
                                    CreatedDate = DateTime.Now,
                                    CreatorUserId = GetUserId()
                                };

                                _uow.ContactPhoneUW.Create(phone);
                            }

                            _uow.Save();
                        }
                    }

                    var newValues = new
                    {
                        contact.FirstName,
                        contact.LastName,
                        contact.NationalCode,
                        contact.PrimaryEmail,
                        contact.IsActive
                    };

                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Contacts",
                        "Edit",
                        $"ویرایش فرد: {contact.FullName}",
                        oldValues,
                        newValues,
                        recordId: contact.Id.ToString(),
                        entityType: "Contact",
                        recordTitle: contact.FullName
                    );

                    TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Details), new { id = contact.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Contacts", "Edit", "خطا در ویرایش", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // اگر validation ناموفق بود، شماره‌ها رو دوباره بارگذاری کن
            if (Phones != null)
            {
                model.Phones = Phones.Select(p => new ContactPhoneViewModel
                {
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    IsDefault = p.IsDefault,
                    IsSmsDefault = p.IsSmsDefault
                }).ToList();
            }

            return View(model);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// نمایش مودال تایید حذف
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contact = _uow.ContactUW.GetById(id);
                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.themeclass = "bg-danger";
                ViewBag.ViewTitle = "حذف فرد";

                return PartialView("_DeleteContact", contact);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Delete", "خطا در نمایش فرم حذف", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// حذف فرد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contact = _uow.ContactUW.GetById(id);
                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");

                var contactName = contact.FullName;

                // حذف نرم - فقط IsActive = false
                contact.IsActive = false;
                contact.LastUpdateDate = DateTime.Now;
                contact.LastUpdaterUserId = GetUserId();

                _uow.ContactUW.Update(contact);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Contacts",
                    "Delete",
                    $"حذف فرد: {contactName}",
                    recordId: id.ToString(),
                    entityType: "Contact",
                    recordTitle: contactName
                );

                TempData["SuccessMessage"] = "فرد با موفقیت حذف شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Delete", "خطا در حذف", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در حذف فرد";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== MANAGE PHONES ====================

        /// <summary>
        /// مدیریت شماره‌های تماس
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManagePhones(int contactId)
        {
            try
            {
                var contact = await _contactRepository.GetContactByIdAsync(contactId, includePhones: true);
                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");

                var phones = _contactRepository.GetContactPhones(contactId);
                var viewModels = _mapper.Map<List<ContactPhoneViewModel>>(phones);

                ViewBag.ContactId = contactId;
                ViewBag.ContactName = contact.FullName;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "ManagePhones", "خطا در نمایش شماره‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// افزودن شماره جدید - Modal
        /// </summary>
        [HttpGet]
        public IActionResult AddPhoneModal(int contactId)
        {
            var model = new ContactPhoneViewModel
            {
                ContactId = contactId,
                IsActive = true,
                DisplayOrder = 1
            };

            return PartialView("_AddPhoneModal", model);
        }

        /// <summary>
        /// ویرایش شماره - Modal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditPhoneModal(int id)
        {
            try
            {
                var phone = _uow.ContactPhoneUW.GetById(id);
                if (phone == null)
                    return NotFound();

                var viewModel = _mapper.Map<ContactPhoneViewModel>(phone);
                return PartialView("_EditPhoneModal", viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "EditPhoneModal", "خطا در نمایش فرم ویرایش", ex);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// ذخیره شماره جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhone(ContactPhoneViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی تکراری نبودن شماره
                    if (_contactRepository.IsPhoneNumberExists(model.PhoneNumber))
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "warning", text = "این شماره قبلاً ثبت شده است" } }
                        });
                    }

                    var phone = _mapper.Map<ContactPhone>(model);
                    phone.CreatorUserId = GetUserId();
                    
                    _uow.ContactPhoneUW.Create(phone);
                    _uow.Save();

                    // ⭐ اگر IsSmsDefault انتخاب شده، باید سایر شماره‌ها غیرفعال شوند
                    if (model.IsSmsDefault)
                    {
                        await _contactRepository.SetSmsDefaultPhoneAsync(phone.Id, model.ContactId);
                    }

                    // ⭐ اگر IsDefault انتخاب شده
                    if (model.IsDefault)
                    {
                        await _contactRepository.SetDefaultPhoneAsync(phone.Id, model.ContactId);
                    }

                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action(nameof(ManagePhones), new { contactId = model.ContactId })
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Contacts", "AddPhone", "خطا در افزودن شماره", ex);
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

        /// <summary>
        /// ذخیره ویرایش شماره
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhone(ContactPhoneViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var phone = _uow.ContactPhoneUW.GetById(model.Id);
                    if (phone == null)
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "error", text = "شماره یافت نشد" } }
                        });
                    }

                    // بررسی تکراری نبودن شماره (به جز خودش)
                    if (_contactRepository.IsPhoneNumberExists(model.PhoneNumber, model.Id))
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "warning", text = "این شماره قبلاً ثبت شده است" } }
                        });
                    }

                    // ذخیره مقادیر قدیمی
                    var originalCreated = phone.CreatedDate;
                    var originalCreatorId = phone.CreatorUserId;

                    // Map کردن تغییرات
                    _mapper.Map(model, phone);

                    // بازگرداندن مقادیر سیستمی
                    phone.CreatedDate = originalCreated;
                    phone.CreatorUserId = originalCreatorId;

                    _uow.ContactPhoneUW.Update(phone);
                    _uow.Save();

                    // ⭐ اگر IsSmsDefault انتخاب شده، باید سایر شماره‌ها غیرفعال شوند
                    if (model.IsSmsDefault)
                    {
                        await _contactRepository.SetSmsDefaultPhoneAsync(phone.Id, model.ContactId);
                    }

                    // ⭐ اگر IsDefault انتخاب شده
                    if (model.IsDefault)
                    {
                        await _contactRepository.SetDefaultPhoneAsync(phone.Id, model.ContactId);
                    }

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Contacts",
                        "EditPhone",
                        $"ویرایش شماره تماس: {phone.FormattedNumber}",
                        recordId: phone.Id.ToString(),
                        entityType: "ContactPhone"
                    );

                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action(nameof(ManagePhones), new { contactId = model.ContactId })
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Contacts", "EditPhone", "خطا در ویرایش شماره", ex);
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

        /// <summary>
        /// تنظیم شماره به عنوان پیش‌فرض
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultPhone(int phoneId, int contactId)
        {
            try
            {
                var result = await _contactRepository.SetDefaultPhoneAsync(phoneId, contactId);
                
                if (result)
                {
                    return Json(new { success = true, message = "شماره پیش‌فرض تغییر کرد" });
                }

                return Json(new { success = false, message = "خطا در تغییر شماره پیش‌فرض" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "SetDefaultPhone", "خطا", ex);
                return Json(new { success = false, message = "خطا در عملیات" });
            }
        }

        /// <summary>
        /// تنظیم شماره به عنوان پیش‌فرض پیامک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSmsDefaultPhone(int phoneId, int contactId)
        {
            try
            {
                var result = await _contactRepository.SetSmsDefaultPhoneAsync(phoneId, contactId);
                
                if (result)
                {
                    return Json(new { success = true, message = "شماره پیش‌فرض پیامک تغییر کرد" });
                }

                return Json(new { success = false, message = "خطا در تغییر شماره پیش‌فرض پیامک" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "SetSmsDefaultPhone", "خطا", ex);
                return Json(new { success = false, message = "خطا در عملیات" });
            }
        }

        /// <summary>
        /// حذف شماره
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhone(int id)
        {
            try
            {
                var result = await _contactRepository.DeletePhoneAsync(id);
                
                if (result)
                {
                    return Json(new { success = true, message = "شماره حذف شد" });
                }

                return Json(new { success = false, message = "خطا در حذف شماره" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "DeletePhone", "خطا", ex);
                return Json(new { success = false, message = "خطا در عملیات" });
            }
        }

        // ==================== STATISTICS ====================

        /// <summary>
        /// داشبورد آماری افراد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var stats = await _contactRepository.GetContactStatisticsAsync();
                var upcomingBirthdays = _contactRepository.GetUpcomingBirthdays(30);

                ViewBag.UpcomingBirthdays = upcomingBirthdays;

                return View(stats);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Contacts", "Statistics", "خطا در دریافت آمار", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }
    }
}