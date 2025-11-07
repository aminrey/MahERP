using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
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
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    [Area("AppCoreArea")]
    [Authorize]
    public class BranchOrganizationGroupsController : BaseController
    {
        private readonly IOrganizationGroupRepository _groupRepository;
        private readonly IUnitOfWork _uow;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public BranchOrganizationGroupsController(
            IOrganizationGroupRepository groupRepository,
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
        {
            _groupRepository = groupRepository;
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
            try
            {
                var branch = _uow.BranchUW.GetById(branchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                var groups = _groupRepository.GetBranchGroups(branchId, includeInactive: false);
                var viewModels = _mapper.Map<List<BranchOrganizationGroupViewModel>>(groups);

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.Name;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "BranchOrganizationGroups",
                    "Index",
                    $"مشاهده گروه‌های سازمان‌های شعبه: {branch.Name}",
                    recordId: branchId.ToString()
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("BranchOrganizationGroups", "Index", "خطا در دریافت لیست", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpGet]
        public IActionResult Create(int branchId)
        {
            var branch = _uow.BranchUW.GetById(branchId);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.BranchId = branchId;
            ViewBag.BranchName = branch.Name;

            return View(new BranchOrganizationGroupViewModel 
            { 
                BranchId = branchId,
                IsActive = true, 
                DisplayOrder = 1 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchOrganizationGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = _mapper.Map<BranchOrganizationGroup>(model);
                    group.CreatedDate = DateTime.Now;
                    group.CreatorUserId = GetUserId();

                    await _groupRepository.CreateBranchGroupAsync(group);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "BranchOrganizationGroups",
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
                    await _activityLogger.LogErrorAsync("BranchOrganizationGroups", "Create", "خطا در ایجاد گروه", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            var branch = _uow.BranchUW.GetById(model.BranchId);
            ViewBag.BranchId = model.BranchId;
            ViewBag.BranchName = branch?.Name;

            return View(model);
        }

        [HttpGet]
        public IActionResult AddMemberModal(int groupId)
        {
            var group = _groupRepository.GetBranchGroupById(groupId);
            if (group == null)
                return NotFound();

            ViewBag.GroupId = groupId;
            ViewBag.GroupTitle = group.Title;
            ViewBag.BranchId = group.BranchId;

            var availableBranchOrganizations = _groupRepository.GetAvailableBranchOrganizationsForGroup(groupId);
            ViewBag.AvailableBranchOrganizations = availableBranchOrganizations;

            return PartialView("_AddMemberModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int branchOrganizationId, string notes)
        {
            try
            {
                await _groupRepository.AddBranchOrganizationToGroupAsync(
                    groupId, 
                    branchOrganizationId, 
                    GetUserId(), 
                    notes);

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
    }
}