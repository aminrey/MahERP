using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]

    [PermissionRequired("RolePattern")]
    public class RolePatternController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IRoleRepository _roleRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;

        public RolePatternController(
            IUnitOfWork uow,
            IRoleRepository roleRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger , userRepository)
        {
            _uow = uow;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
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
        new ControllerInfo { Name = "TaskInitialSettings", DisplayName = "تعاریف اولیه تسک‌ینگ", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "شامل دسته‌بندی تسک و اتصال به شعب" }
        }},
        new ControllerInfo { Name = "Dashboard", DisplayName = "📊 داشبورد و گزارشات", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به داشبورد و گزارشات" }
        }},
        new ControllerInfo { Name = "Tasks", DisplayName = "📋 عملیات تسک‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به عملیات تسک‌ها" }
        }},
        new ControllerInfo { Name = "Branch", DisplayName = "🏢 مدیریت شعب", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به مدیریت شعب" }
        }},
        new ControllerInfo { Name = "BranchUser", DisplayName = "👥 کاربران شعب", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به کاربران شعب" }
        }},
        new ControllerInfo { Name = "Team", DisplayName = "👥 مدیریت تیم‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به مدیریت تیم‌ها" }
        }},
        new ControllerInfo { Name = "UserManager", DisplayName = "👤 مدیریت کاربران", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به مدیریت کاربران" }
        }},
        new ControllerInfo { Name = "RolePattern", DisplayName = "🔑 مدیریت نقش‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به مدیریت نقش‌ها" }
        }},
        new ControllerInfo { Name = "UserPermission", DisplayName = "🔐 دسترسی کاربران", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به تنظیم دسترسی کاربران" }
        }},
        new ControllerInfo { Name = "Stakeholder", DisplayName = "🤝 طرف حساب‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به طرف حساب‌ها" }
        }},
        new ControllerInfo { Name = "Contract", DisplayName = "📄 قراردادها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به قراردادها" }
        }},
        new ControllerInfo { Name = "CRM", DisplayName = "📊 مدیریت CRM", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به مدیریت CRM" }
        }},
        new ControllerInfo { Name = "UserActivityLog", DisplayName = "📊 لاگ فعالیت‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به لاگ فعالیت‌ها" }
        }},
        new ControllerInfo { Name = "Notification", DisplayName = "🔔 نوتیفیکیشن‌ها", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به نوتیفیکیشن‌ها" }
        }},
        new ControllerInfo { Name = "Settings", DisplayName = "⚙️ تنظیمات سیستم", Actions = new[]
        {
            new ActionInfo { Name = "General", DisplayName = "دسترسی به تنظیمات سیستم" }
        }}
    };
        }
    }

}