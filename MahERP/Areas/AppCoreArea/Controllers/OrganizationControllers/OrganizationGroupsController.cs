using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.OrganizationGroupRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    [Area("AppCoreArea")]
    [Authorize]
    public class OrganizationGroupsController : BaseController
    {
        private readonly IOrganizationGroupRepository _groupRepository;
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public OrganizationGroupsController(
            IOrganizationGroupRepository groupRepository,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _groupRepository = groupRepository;
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==================== INDEX ====================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var groups = _groupRepository.GetAllGroups(includeInactive: false);
                var viewModels = _mapper.Map<List<OrganizationGroupViewModel>>(groups);

                var stats = await _groupRepository.GetGroupStatisticsAsync();
                ViewBag.Statistics = stats;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "OrganizationGroups",
                    "Index",
                    "مشاهده لیست گروه‌های سازمان‌ها"
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("OrganizationGroups", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new OrganizationGroupViewModel { IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<OrganizationGroup>(model);
                    group.CreatedDate = DateTime.Now;
                    group.CreatorUserId = GetUserId();

                    await _groupRepository.CreateGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "OrganizationGroups",
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
                    await _activityLogger.LogErrorAsync("OrganizationGroups", "Create", "خطا در ایجاد گروه", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== EDIT ====================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var group = _groupRepository.GetGroupById(id);
            if (group == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<OrganizationGroupViewModel>(group);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizationGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<OrganizationGroup>(model);
                    group.LastUpdaterUserId = GetUserId();

                    await _groupRepository.UpdateGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "OrganizationGroups",
                        "Edit",
                        $"ویرایش گروه: {group.Title}",
                        recordId: group.Id.ToString()
                    );

                    TempData["SuccessMessage"] = "گروه با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("OrganizationGroups", "Edit", "خطا در ویرایش", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== DELETE ====================

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
                    "OrganizationGroups",
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
                await _activityLogger.LogErrorAsync("OrganizationGroups", "Delete", "خطا در حذف", ex);
                return Json(new { success = false, message = "خطا در حذف گروه" });
            }
        }

        // ==================== MANAGE MEMBERS ====================

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

        [HttpGet]
        public IActionResult AddMemberModal(int groupId)
        {
            var group = _groupRepository.GetGroupById(groupId);
            if (group == null)
                return NotFound();

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;

            var allOrganizations = _uow.OrganizationUW.Get(o => o.IsActive).ToList();
            var existingOrganizationIds = _groupRepository.GetGroupMembers(groupId)
                .Select(m => m.OrganizationId)
                .ToList();

            var availableOrganizations = allOrganizations
                .Where(o => !existingOrganizationIds.Contains(o.Id))
                .ToList();

            ViewBag.AvailableOrganizations = availableOrganizations;

            return PartialView("_AddMemberModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int organizationId, string notes)
        {
            try
            {
                await _groupRepository.AddOrganizationToGroupAsync(groupId, organizationId, GetUserId(), notes);

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("ManageMembers", new { groupId }),
                    message = new[] { new { status = "success", text = "سازمان با موفقیت به گروه اضافه شد" } }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int organizationId)
        {
            try
            {
                await _groupRepository.RemoveOrganizationFromGroupAsync(groupId, organizationId);

                return Json(new { success = true, message = "سازمان از گروه حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}