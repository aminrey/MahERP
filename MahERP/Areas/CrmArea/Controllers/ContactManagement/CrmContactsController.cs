using AutoMapper;
using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers.ContactManagement
{
    /// <summary>
    /// مدیریت افراد در CRM با محدودیت شعبه
    /// ⚠️ فقط کاربران CRM با دسترسی CONTACT
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CONTACT.VIEW")]
    public class CrmContactsController : BaseController
    {
        private readonly IContactRepository _contactRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly AppDbContext _context;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public CrmContactsController(
            IContactRepository contactRepository,
            IBranchRepository branchRepository,
            AppDbContext context,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IUnitOfWork uow)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _contactRepository = contactRepository;
            _branchRepository = branchRepository;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX (انتخاب شعبه + لیست) ====================

        /// <summary>
        /// صفحه اصلی - انتخاب شعبه و نمایش لیست افراد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? branchId, string searchTerm = null, byte? gender = null)
        {
            try
            {
                // بررسی دسترسی به ماژول CRM
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                var userBranches = _branchRepository.GetBrnachListByUserId(userId);

                ViewBag.UserBranches = userBranches;
                ViewBag.HasSingleBranch = userBranches.Count == 1;

                // اگر فقط یک شعبه داره، اتوماتیک انتخاب کن
                if (userBranches.Count == 1 && !branchId.HasValue)
                {
                    branchId = userBranches.First().Id;
                }

                ViewBag.SelectedBranchId = branchId;

                // اگر شعبه انتخاب نشده، فقط لیست شعبه‌ها رو نمایش بده
                if (!branchId.HasValue)
                {
                    ViewBag.ShowBranchSelection = true;
                    return View(new List<ContactViewModel>());
                }

                // بررسی دسترسی به شعبه انتخاب شده
                if (!userBranches.Any(b => b.Id == branchId.Value))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var selectedBranch = userBranches.First(b => b.Id == branchId.Value);
                ViewBag.SelectedBranchName = selectedBranch.Name;
                ViewBag.ShowBranchSelection = false;

                // دریافت افراد شعبه با Include صریح
                var branchContacts = _context.BranchContact_Tbl
                    .Include(bc => bc.Contact)
                        .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                    .Where(bc => bc.BranchId == branchId.Value && bc.IsActive && bc.Contact.IsActive)
                    .ToList();

                var contacts = branchContacts.Select(bc => bc.Contact).ToList();

                // فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    contacts = contacts.Where(c =>
                        (c.FirstName != null && c.FirstName.ToLower().Contains(searchTerm)) ||
                        c.LastName.ToLower().Contains(searchTerm) ||
                        (c.PrimaryEmail != null && c.PrimaryEmail.ToLower().Contains(searchTerm))
                    ).ToList();
                }

                // فیلتر جنسیت
                if (gender.HasValue)
                {
                    contacts = contacts.Where(c => c.Gender == gender.Value).ToList();
                }

                // مرتب‌سازی
                contacts = contacts
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                var viewModels = _mapper.Map<List<ContactViewModel>>(contacts);

                // گروه‌بندی بر اساس حرف اول
                var groupedContacts = viewModels
                    .GroupBy(c => string.IsNullOrEmpty(c.LastName) ? "#" : c.LastName.Substring(0, 1).ToUpper())
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.GroupedContacts = groupedContacts;
                ViewBag.TotalCount = contacts.Count;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.GenderFilter = gender;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CrmContacts",
                    "Index",
                    $"مشاهده لیست افراد شعبه {selectedBranch.Name}",
                    recordId: branchId.ToString()
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Index", "خطا در دریافت لیست افراد", ex);
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// ⭐ بارگذاری لیست افراد به صورت Partial View (برای AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadContactsList(int branchId, string searchTerm = null, byte? gender = null)
        {
            try
            {
                var userId = GetUserId();

                // بررسی دسترسی
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شما به این شعبه دسترسی ندارید" } }
                    });
                }

                var selectedBranch = userBranches.First(b => b.Id == branchId);

                // دریافت افراد
                var branchContacts = _context.BranchContact_Tbl
                    .Include(bc => bc.Contact)
                        .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                    .Where(bc => bc.BranchId == branchId && bc.IsActive && bc.Contact.IsActive)
                    .ToList();

                var contacts = branchContacts.Select(bc => bc.Contact).ToList();

                // فیلترها
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    contacts = contacts.Where(c =>
                        (c.FirstName != null && c.FirstName.ToLower().Contains(searchTerm)) ||
                        c.LastName.ToLower().Contains(searchTerm) ||
                        (c.PrimaryEmail != null && c.PrimaryEmail.ToLower().Contains(searchTerm))
                    ).ToList();
                }

                if (gender.HasValue)
                {
                    contacts = contacts.Where(c => c.Gender == gender.Value).ToList();
                }

                contacts = contacts
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                var viewModels = _mapper.Map<List<ContactViewModel>>(contacts);

                var groupedContacts = viewModels
                    .GroupBy(c => string.IsNullOrEmpty(c.LastName) ? "#" : c.LastName.Substring(0, 1).ToUpper())
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                ViewBag.SelectedBranchId = branchId;
                ViewBag.SelectedBranchName = selectedBranch.Name;
                ViewBag.GroupedContacts = groupedContacts;
                ViewBag.TotalCount = contacts.Count;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.GenderFilter = gender;

                // ⭐ Render partial view
                var html = await this.RenderViewToStringAsync("_ContactsListPartial", viewModels);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "contactsListContainer",
                            view = new { result = html }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "LoadContactsList", "خطا در بارگذاری لیست", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در بارگذاری اطلاعات" } }
                });
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// صفحه ایجاد فرد جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("CONTACT.CREATE")]
        public async Task<IActionResult> Create(int? branchId)
        {
            try
            {
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                var userBranches = _branchRepository.GetBrnachListByUserId(userId);

                ViewBag.UserBranches = userBranches;
                ViewBag.HasSingleBranch = userBranches.Count == 1;

                // اگر فقط یک شعبه داره
                if (userBranches.Count == 1)
                {
                    branchId = userBranches.First().Id;
                }

                ViewBag.SelectedBranchId = branchId;

                // اگر شعبه انتخاب نشده
                if (!branchId.HasValue)
                {
                    ViewBag.ShowBranchSelection = true;
                    return View(new ContactCreateViewModel());
                }

                // بررسی دسترسی به شعبه
                if (!userBranches.Any(b => b.Id == branchId.Value))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var selectedBranch = userBranches.First(b => b.Id == branchId.Value);
                ViewBag.SelectedBranchName = selectedBranch.Name;
                ViewBag.ShowBranchSelection = false;

                var model = new ContactCreateViewModel
                {
                    BranchId = branchId.Value
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Create", "خطا در بارگذاری فرم", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ثبت فرد جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CONTACT.CREATE")]
        public async Task<IActionResult> Create(ContactCreateViewModel model)
        {
            try
            {
                var userId = GetUserId();
                var moduleAccess = await _moduleAccessService.CheckUserModuleAccessAsync(userId, ModuleType.CRM);
                if (!moduleAccess.HasAccess)
                {
                    TempData["ErrorMessage"] = "شما به ماژول CRM دسترسی ندارید";
                    return RedirectToAction("AccessDenied", "Error", new { area = "" });
                }

                if (!ModelState.IsValid)
                {
                    var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                    ViewBag.UserBranches = userBranches;
                    ViewBag.SelectedBranchId = model.BranchId;
                    return View(model);
                }

                // بررسی دسترسی به شعبه
                var userBranches2 = _branchRepository.GetBrnachListByUserId(userId);
                if (!userBranches2.Any(b => b.Id == model.BranchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                // 1️⃣ ایجاد Contact در دیتابیس اصلی
                var contact = _mapper.Map<Contact>(model);
                contact.CreatorUserId = userId;
                contact.CreatedDate = DateTime.Now;
                contact.IsActive = true;

                _context.Contact_Tbl.Add(contact);
                await _context.SaveChangesAsync();

                // 2️⃣ اتصال به شعبه (BranchContact)
                var branchContact = new BranchContact
                {
                    BranchId = model.BranchId,
                    ContactId = contact.Id,
                    RelationType = model.BranchRelationType,
                    AssignedByUserId = userId,
                    AssignDate = DateTime.Now,
                    IsActive = true
                };

                _context.BranchContact_Tbl.Add(branchContact);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CrmContacts",
                    "Create",
                    $"ایجاد فرد جدید: {contact.FullName}",
                    recordId: contact.Id.ToString()
                );

                TempData["SuccessMessage"] = $"فرد '{contact.FullName}' با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Details), new { id = contact.Id, branchId = model.BranchId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Create", "خطا در ایجاد فرد", ex);
                ModelState.AddModelError("", "خطا در ذخیره اطلاعات: " + ex.Message);

                var userId = GetUserId();
                var userBranches = _branchRepository.GetBrnachListByUserId(userId);
                ViewBag.UserBranches = userBranches;
                ViewBag.SelectedBranchId = model.BranchId;
                return View(model);
            }
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// جزئیات فرد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id, int branchId)
        {
            try
            {
                // بررسی دسترسی به شعبه
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var contact = _contactRepository.GetContactById(id);
                if (contact == null)
                {
                    TempData["ErrorMessage"] = "فرد یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                // بررسی اینکه این Contact در این شعبه هست
                var branchContact = _context.BranchContact_Tbl
                    .FirstOrDefault(bc => bc.ContactId == id && bc.BranchId == branchId && bc.IsActive);

                if (branchContact == null)
                {
                    TempData["ErrorMessage"] = "این فرد در شعبه انتخاب شده وجود ندارد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var viewModel = _mapper.Map<ContactViewModel>(contact);
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = userBranches.First(b => b.Id == branchId).Name;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CrmContacts",
                    "Details",
                    $"مشاهده جزئیات فرد: {contact.FullName}",
                    recordId: id.ToString()
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Details", "خطا در نمایش جزئیات", ex);
                TempData["ErrorMessage"] = "خطا در نمایش اطلاعات";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== EDIT ====================

        /// <summary>
        /// ویرایش فرد
        /// </summary>
        [HttpGet]
        [PermissionRequired("CONTACT.EDIT")]
        public async Task<IActionResult> Edit(int id, int branchId)
        {
            try
            {
                // بررسی دسترسی به شعبه
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var contact = _contactRepository.GetContactById(id);
                if (contact == null)
                {
                    TempData["ErrorMessage"] = "فرد یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                // بررسی وجود در شعبه
                var branchContact = _context.BranchContact_Tbl
                    .FirstOrDefault(bc => bc.ContactId == id && bc.BranchId == branchId && bc.IsActive);

                if (branchContact == null)
                {
                    TempData["ErrorMessage"] = "این فرد در شعبه انتخاب شده وجود ندارد";
                    return RedirectToAction(nameof(Index), new { branchId });
                }

                var viewModel = _mapper.Map<ContactEditViewModel>(contact);
                viewModel.BranchId = branchId;

                ViewBag.BranchName = userBranches.First(b => b.Id == branchId).Name;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Edit", "خطا در بارگذاری فرم ویرایش", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ذخیره تغییرات فرد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CONTACT.EDIT")]
        public async Task<IActionResult> Edit(ContactEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                    ViewBag.BranchName = userBranches.First(b => b.Id == model.BranchId).Name;
                    return View(model);
                }

                // بررسی دسترسی
                var userBranches2 = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches2.Any(b => b.Id == model.BranchId))
                {
                    TempData["ErrorMessage"] = "شما به این شعبه دسترسی ندارید";
                    return RedirectToAction(nameof(Index));
                }

                var contact = _contactRepository.GetContactById(model.Id);
                if (contact == null)
                {
                    TempData["ErrorMessage"] = "فرد یافت نشد";
                    return RedirectToAction(nameof(Index), new { branchId = model.BranchId });
                }

                // بروزرسانی
                _mapper.Map(model, contact);
                contact.LastUpdaterUserId = GetUserId();
                contact.LastUpdateDate = DateTime.Now;

                _context.Contact_Tbl.Update(contact);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "CrmContacts",
                    "Edit",
                    $"ویرایش فرد: {contact.FullName}",
                    recordId: contact.Id.ToString()
                );

                TempData["SuccessMessage"] = "اطلاعات با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Details), new { id = contact.Id, branchId = model.BranchId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Edit", "خطا در ویرایش", ex);
                ModelState.AddModelError("", "خطا در ذخیره تغییرات");
                return View(model);
            }
        }

        // ==================== DELETE ====================

        /// <summary>
        /// حذف فرد از شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("CONTACT.DELETE")]
        public async Task<IActionResult> Delete(int id, int branchId)
        {
            try
            {
                // بررسی دسترسی
                var userBranches = _branchRepository.GetBrnachListByUserId(GetUserId());
                if (!userBranches.Any(b => b.Id == branchId))
                {
                    return Json(new { success = false, message = "شما به این شعبه دسترسی ندارید" });
                }

                var contact = _contactRepository.GetContactById(id);
                if (contact == null)
                {
                    return Json(new { success = false, message = "فرد یافت نشد" });
                }

                // حذف رابطه با شعبه (نه خود Contact)
                var branchContact = _context.BranchContact_Tbl
                    .FirstOrDefault(bc => bc.ContactId == id && bc.BranchId == branchId);

                if (branchContact != null)
                {
                    branchContact.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "CrmContacts",
                    "Delete",
                    $"حذف فرد از شعبه: {contact.FullName}",
                    recordId: id.ToString()
                );

                return Json(new { success = true, message = "فرد از شعبه حذف شد" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CrmContacts", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف فرد" });
            }
        }
    }
}
