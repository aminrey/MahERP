using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin,Manager")]
    public class RolePatternController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IRoleRepository _roleRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public RolePatternController(
            IUnitOfWork uow,
            IRoleRepository roleRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _uow = uow;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست الگوهای نقش
        public IActionResult Index()
        {
            var rolePatterns = _roleRepository.GetAllRolePatterns();
            var viewModels = _mapper.Map<List<RolePatternViewModel>>(rolePatterns);

            // محاسبه آمار کاربران برای هر الگو
            foreach (var viewModel in viewModels)
            {
                var users = _roleRepository.GetUsersByRolePattern(viewModel.Id);
                viewModel.UsersCount = users.Count;
                viewModel.ActiveUsersCount = users.Count(u => u.IsActive && !u.IsRemoveUser);
            }

            return View(viewModels);
        }

        // جزئیات الگوی نقش
        public IActionResult Details(int id)
        {
            var rolePattern = _roleRepository.GetRolePatternById(id, includeDetails: true);
            if (rolePattern == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<RolePatternViewModel>(rolePattern);
            viewModel.Details = _mapper.Map<List<RolePatternDetailsViewModel>>(rolePattern.RolePatternDetails);

            // دریافت کاربران این الگو
            var users = _roleRepository.GetUsersByRolePattern(id);
            ViewBag.Users = users;

            return View(viewModel);
        }

        // ایجاد الگوی نقش جدید - نمایش فرم
        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new RolePatternViewModel 
            { 
                IsActive = true,
                AccessLevel = 5 // کاربر عادی به عنوان پیش‌فرض
            });
        }

        // ایجاد الگوی نقش جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RolePatternViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rolePattern = _mapper.Map<RolePattern>(model);
                rolePattern.CreatorUserId = _userManager.GetUserId(User);

                if (_roleRepository.CreateRolePattern(rolePattern))
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "خطا در ذخیره الگوی نقش");
            }

            PopulateDropdowns();
            return View(model);
        }

        // ویرایش الگوی نقش - نمایش فرم
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var rolePattern = _roleRepository.GetRolePatternById(id);
            if (rolePattern == null || rolePattern.IsSystemPattern)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<RolePatternViewModel>(rolePattern);
            PopulateDropdowns();

            return View(viewModel);
        }

        // ویرایش الگوی نقش - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RolePatternViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rolePattern = _roleRepository.GetRolePatternById(model.Id);
                if (rolePattern == null || rolePattern.IsSystemPattern)
                    return RedirectToAction("ErrorView", "Home");

                _mapper.Map(model, rolePattern);
                rolePattern.LastUpdaterUserId = _userManager.GetUserId(User);

                if (_roleRepository.UpdateRolePattern(rolePattern))
                {
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                ModelState.AddModelError("", "خطا در بروزرسانی الگوی نقش");
            }

            PopulateDropdowns();
            return View(model);
        }

        // حذف الگوی نقش - نمایش مودال تأیید
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var rolePattern = _roleRepository.GetRolePatternById(id);
            if (rolePattern == null || rolePattern.IsSystemPattern)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف الگوی نقش";

            return PartialView("_DeleteRolePattern", _mapper.Map<RolePatternViewModel>(rolePattern));
        }

        // حذف الگوی نقش - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            if (_roleRepository.DeleteRolePattern(id))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("ErrorView", "Home");
        }

        // مدیریت دسترسی‌ها
        [HttpGet]
        public IActionResult ManagePermissions(int id)
        {
            var rolePattern = _roleRepository.GetRolePatternById(id, includeDetails: true);
            if (rolePattern == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = new RolePatternPermissionViewModel
            {
                RolePatternId = id,
                PatternName = rolePattern.PatternName,
                Controllers = GetAvailableControllers(),
                CurrentPermissions = _mapper.Map<List<RolePatternDetailsViewModel>>(rolePattern.RolePatternDetails)
            };

            return View(viewModel);
        }

        // بروزرسانی دسترسی‌ها
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePermissions(RolePatternPermissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // حذف دسترسی‌های قبلی
                var existingPermissions = _roleRepository.GetRolePatternDetails(model.RolePatternId);
                foreach (var permission in existingPermissions)
                {
                    _uow.RolePatternDetailsUW.Delete(permission);
                }

                // اضافه کردن دسترسی‌های جدید
                foreach (var permission in model.Permissions)
                {
                    if (permission.CanRead || permission.CanCreate || permission.CanEdit || permission.CanDelete || permission.CanApprove)
                    {
                        var detail = _mapper.Map<RolePatternDetails>(permission);
                        detail.RolePatternId = model.RolePatternId;
                        _uow.RolePatternDetailsUW.Create(detail);
                    }
                }

                _uow.Save();
                return RedirectToAction(nameof(Details), new { id = model.RolePatternId });
            }

            model.Controllers = GetAvailableControllers();
            return View("ManagePermissions", model);
        }

        // توابع کمکی
        private void PopulateDropdowns()
        {
            ViewBag.AccessLevels = new SelectList(new[]
            {
                new { Value = 1, Text = "مدیر سیستم" },
                new { Value = 2, Text = "مدیر" },
                new { Value = 3, Text = "سرپرست" },
                new { Value = 4, Text = "کارشناس" },
                new { Value = 5, Text = "کاربر عادی" }
            }, "Value", "Text");
        }

        private List<ControllerInfo> GetAvailableControllers()
        {
            return new List<ControllerInfo>
            {
                new ControllerInfo { Name = "Task", DisplayName = "مدیریت تسک‌ها", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست تسک‌ها" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد تسک" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش تسک" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف تسک" },
                    new ActionInfo { Name = "Details", DisplayName = "جزئیات تسک" },
                    new ActionInfo { Name = "MyTasks", DisplayName = "تسک‌های من" }
                }},
                new ControllerInfo { Name = "CRM", DisplayName = "مدیریت ارتباط با مشتری", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست تعاملات" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد تعامل" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش تعامل" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف تعامل" }
                }},
                new ControllerInfo { Name = "Stakeholder", DisplayName = "مدیریت طرف‌های حساب", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست طرف‌های حساب" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد طرف حساب" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش طرف حساب" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف طرف حساب" }
                }},
                new ControllerInfo { Name = "Contract", DisplayName = "مدیریت قراردادها", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست قراردادها" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد قرارداد" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش قرارداد" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف قرارداد" }
                }},
                new ControllerInfo { Name = "User", DisplayName = "مدیریت کاربران", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست کاربران" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد کاربر" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش کاربر" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف کاربر" }
                }},
                new ControllerInfo { Name = "RolePattern", DisplayName = "مدیریت الگوهای نقش", Actions = new[]
                {
                    new ActionInfo { Name = "Index", DisplayName = "لیست الگوهای نقش" },
                    new ActionInfo { Name = "Create", DisplayName = "ایجاد الگوی نقش" },
                    new ActionInfo { Name = "Edit", DisplayName = "ویرایش الگوی نقش" },
                    new ActionInfo { Name = "Delete", DisplayName = "حذف الگوی نقش" }
                }}
            };
        }
    }

}