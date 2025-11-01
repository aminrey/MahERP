using System;
using System.Linq;
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
    /// <summary>
    /// کنترلر مدیریت گروه‌های افراد در سطح شعبه
    /// </summary>
    [Area("AdminArea")]
    [Authorize]
    public class BranchContactGroupsController : BaseController
    {
        private readonly IContactGroupRepository _groupRepository;
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public BranchContactGroupsController(
            IContactGroupRepository groupRepository,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository , BaseRepository)
        {
            _groupRepository = groupRepository;
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX (لیست گروه‌های یک شعبه) ====================

        /// <summary>
        /// لیست گروه‌های افراد یک شعبه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
            try
            {
                var branch = _uow.BranchUW.GetById(branchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                var groups = _groupRepository.GetBranchGroups(branchId, includeInactive: false);
                var viewModels = _mapper.Map<List<BranchContactGroupViewModel>>(groups);

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.Name;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "BranchContactGroups",
                    "Index",
                    $"مشاهده گروه‌های افراد شعبه: {branch.Name}",
                    recordId: branchId.ToString()
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("BranchContactGroups", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// نمایش فرم افزودن گروه جدید برای شعبه
        /// </summary>
        [HttpGet]
        public IActionResult Create(int branchId)
        {
            var branch = _uow.BranchUW.GetById(branchId);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.BranchId = branchId;
            ViewBag.BranchName = branch.Name;

            return View(new BranchContactGroupViewModel 
            { 
                BranchId = branchId,
                IsActive = true, 
                DisplayOrder = 1 
            });
        }

        /// <summary>
        /// ذخیره گروه جدید برای شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchContactGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<BranchContactGroup>(model);
                    group.CreatedDate = DateTime.Now;
                    group.CreatorUserId = GetUserId();

                    await _groupRepository.CreateBranchGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "BranchContactGroups",
                        "Create",
                        $"ایجاد گروه جدید برای شعبه: {group.Title}",
                        recordId: group.Id.ToString()
                    );

                    TempData["SuccessMessage"] = "گروه با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index), new { branchId = model.BranchId });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Code", ex.Message);
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("BranchContactGroups", "Create", "خطا در ایجاد گروه", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            var branch = _uow.BranchUW.GetById(model.BranchId);
            ViewBag.BranchId = model.BranchId;
            ViewBag.BranchName = branch?.Name;

            return View(model);
        }

        // ==================== EDIT ====================

        /// <summary>
        /// نمایش فرم ویرایش گروه شعبه
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var group = _groupRepository.GetBranchGroupById(id);
            if (group == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchContactGroupViewModel>(group);
            
            ViewBag.BranchId = group.BranchId;
            ViewBag.BranchName = group.Branch?.Name;

            return View(viewModel);
        }

        /// <summary>
        /// ذخیره ویرایش گروه شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchContactGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<BranchContactGroup>(model);
                    group.LastUpdaterUserId = GetUserId();

                    await _groupRepository.UpdateBranchGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "BranchContactGroups",
                        "Edit",
                        $"ویرایش گروه شعبه: {group.Title}",
                        recordId: group.Id.ToString()
                    );

                    TempData["SuccessMessage"] = "گروه با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Index), new { branchId = model.BranchId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("BranchContactGroups", "Edit", "خطا در ویرایش", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            var branch = _uow.BranchUW.GetById(model.BranchId);
            ViewBag.BranchId = model.BranchId;
            ViewBag.BranchName = branch?.Name;

            return View(model);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// حذف گروه شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var group = _groupRepository.GetBranchGroupById(id);
                if (group == null)
                    return Json(new { success = false, message = "گروه یافت نشد" });

                var branchId = group.BranchId;

                await _groupRepository.DeleteBranchGroupAsync(id);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "BranchContactGroups",
                    "Delete",
                    $"حذف گروه شعبه: {group.Title}",
                    recordId: id.ToString()
                );

                return Json(new { success = true, message = "گروه با موفقیت حذف شد", branchId });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("BranchContactGroups", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف گروه" });
            }
        }

        // ==================== MANAGE MEMBERS ====================

        /// <summary>
        /// مدیریت اعضای گروه شعبه
        /// </summary>
        [HttpGet]
        public IActionResult ManageMembers(int groupId)
        {
            var group = _groupRepository.GetBranchGroupById(groupId, includeMembers: true);
            if (group == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;
            ViewBag.BranchId = group.BranchId;
            ViewBag.BranchName = group.Branch?.Name;

            return View(group.Members.Where(m => m.IsActive).ToList());
        }

        /// <summary>
        /// افزودن فرد به گروه شعبه - Modal
        /// </summary>
        [HttpGet]
        public IActionResult AddMemberModal(int groupId)
        {
            var group = _groupRepository.GetBranchGroupById(groupId);
            if (group == null)
                return NotFound();

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;
            ViewBag.BranchId = group.BranchId;


            // افراد قابل افزودن
            var availableBranchContacts = _groupRepository.GetAvailableBranchContactsForGroup(groupId);


            ViewBag.AvailableBranchContacts = availableBranchContacts;

            return PartialView("_AddMemberModal");
        }

        /// <summary>
        /// ذخیره افزودن فرد به گروه شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int branchContactId, string notes)
        {
            try
            {
                await _groupRepository.AddBranchContactToGroupAsync(
                    groupId, 
                    branchContactId, 
                    GetUserId(), 
                    notes);

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
        /// حذف فرد از گروه شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int branchContactId)
        {
            try
            {
                await _groupRepository.RemoveBranchContactFromGroupAsync(groupId, branchContactId);

                return Json(new { success = true, message = "فرد از گروه حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==================== API برای دریافت گروه‌ها (برای فیلتر) ====================

        /// <summary>
        /// دریافت گروه‌های یک شعبه به صورت JSON (برای استفاده در فیلتر)
        /// </summary>
        [HttpGet]
        public IActionResult GetBranchGroups(int branchId)
        {
            try
            {
                var groups = _groupRepository.GetBranchGroups(branchId, includeInactive: false);

                var result = groups.Select(g => new
                {
                    g.Id,
                    g.Code,
                    g.Title,
                    g.ColorHex,
                    g.IconClass,
                    MembersCount = g.ActiveMembersCount
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت افراد یک گروه شعبه به صورت JSON
        /// </summary>
        [HttpGet]
        public IActionResult GetGroupMembers(int groupId)
        {
            try
            {
                var contacts = _groupRepository.GetBranchGroupContacts(groupId, includeInactive: false);

                var result = contacts.Select(bc => new
                {
                    bc.Id,
                    bc.ContactId,
                    ContactName = bc.Contact?.FullName,
                    Phone = bc.Contact?.DefaultPhone?.FormattedNumber,
                    Email = bc.Contact?.PrimaryEmail
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}