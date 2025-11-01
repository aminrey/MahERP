
using System;
using System.Threading.Tasks;
using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactGroupRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AppCoreArea.Controllers.ContactControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class ContactGroupsController : BaseController
    {
        private readonly IContactGroupRepository _groupRepository;
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public ContactGroupsController(
            IContactGroupRepository groupRepository,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository)
        {
            _groupRepository = groupRepository;
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX ====================

        /// <summary>
        /// لیست گروه‌های افراد
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var groups = _groupRepository.GetAllGroups(includeInactive: false);
                var viewModels = _mapper.Map<List<ContactGroupViewModel>>(groups);

                // آمار
                var stats = await _groupRepository.GetGroupStatisticsAsync();
                ViewBag.Statistics = stats;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ContactGroups",
                    "Index",
                    "مشاهده لیست گروه‌های افراد"
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ContactGroups", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// نمایش فرم افزودن گروه جدید
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ContactGroupViewModel { IsActive = true, DisplayOrder = 1 });
        }

        /// <summary>
        /// ذخیره گروه جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<ContactGroup>(model);
                    group.CreatedDate = DateTime.Now;
                    group.CreatorUserId = GetUserId();

                    await _groupRepository.CreateGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "ContactGroups",
                        "Create",
                        $"ایجاد گروه جدید: {group.Title}",
                        recordId: group.Id.ToString()
                    );

                    TempData["SuccessMessage"] = "گروه با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Code", ex.Message);
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("ContactGroups", "Create", "خطا در ایجاد گروه", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== EDIT ====================

        /// <summary>
        /// نمایش فرم ویرایش گروه
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var group = _groupRepository.GetGroupById(id);
            if (group == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<ContactGroupViewModel>(group);
            return View(viewModel);
        }

        /// <summary>
        /// ذخیره ویرایش گروه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<ContactGroup>(model);
                    group.LastUpdaterUserId = GetUserId();

                    await _groupRepository.UpdateGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "ContactGroups",
                        "Edit",
                        $"ویرایش گروه: {group.Title}",
                        recordId: group.Id.ToString()
                    );

                    TempData["SuccessMessage"] = "گروه با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("ContactGroups", "Edit", "خطا در ویرایش", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// حذف گروه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var group = _groupRepository.GetGroupById(id);
                if (group == null)
                    return Json(new { success = false, message = "گروه یافت نشد" });

                await _groupRepository.DeleteGroupAsync(id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "ContactGroups",
                    "Delete",
                    $"حذف گروه: {group.Title}",
                    recordId: id.ToString()
                );

                return Json(new { success = true, message = "گروه با موفقیت حذف شد" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("ContactGroups", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف گروه" });
            }
        }

        // ==================== MANAGE MEMBERS ====================

        /// <summary>
        /// مدیریت اعضای گروه
        /// </summary>
        [HttpGet]
        public IActionResult ManageMembers(int groupId)
        {
            var group = _groupRepository.GetGroupById(groupId, includeMembers: true);
            if (group == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;

            var members = _groupRepository.GetGroupMembers(groupId);
            return View(members);
        }

        /// <summary>
        /// افزودن فرد به گروه - Modal
        /// </summary>
        [HttpGet]
        public IActionResult AddMemberModal(int groupId)
        {
            var group = _groupRepository.GetGroupById(groupId);
            if (group == null)
                return NotFound();

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;

            // دریافت افراد موجود
            var allContacts = _uow.ContactUW.Get(c => c.IsActive).ToList();
            var existingContactIds = _groupRepository.GetGroupMembers(groupId)
                .Select(m => m.ContactId)
                .ToList();

            var availableContacts = allContacts
                .Where(c => !existingContactIds.Contains(c.Id))
                .ToList();

            ViewBag.AvailableContacts = availableContacts;

            return PartialView("_AddMemberModal");
        }

        /// <summary>
        /// ذخیره افزودن فرد به گروه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int contactId, string notes)
        {
            try
            {
                await _groupRepository.AddContactToGroupAsync(groupId, contactId, GetUserId(), notes);

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("ManageMembers", new { groupId }),
                    message = new[] { new { status = "success", text = "فرد با موفقیت به گروه اضافه شد" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = ex.Message } }
                });
            }
        }

        /// <summary>
        /// حذف فرد از گروه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int contactId)
        {
            try
            {
                await _groupRepository.RemoveContactFromGroupAsync(groupId, contactId);

                return Json(new { success = true, message = "فرد از گروه حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}