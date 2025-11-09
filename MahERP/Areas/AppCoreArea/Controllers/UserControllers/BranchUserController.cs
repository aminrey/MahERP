using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace MahERP.Areas.AppCoreArea.Controllers.UserControllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("CORE.BRANCH.DEFINITIONS.USER")]

    public class BranchUserController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchRepository _branchRepository;
        private readonly IUserManagerRepository _userManagerRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;

        public BranchUserController(
            IUnitOfWork uow,
            IBranchRepository branchRepository,
            IUserManagerRepository userManagerRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger, IBaseRepository BaseRepository,
            IUserManagerRepository userRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _uow = uow;
            _branchRepository = branchRepository;
            _userManagerRepository = userManagerRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // لیست کاربران شعبه
        public IActionResult Index(int? branchId)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!branchId.HasValue && userBranches.Count() == 1)
            {
                branchId = userBranches.FirstOrDefault().Id;
            }
            // اگر شعبه مشخص شده باشد
            if (branchId.HasValue)
            {
                // بررسی دسترسی کاربر به شعبه
                if (!userBranches.Any(b => b.Id == branchId.Value))
                    return RedirectToAction("ErrorView", "Home");

                var branchUsers = _branchRepository.GetBranchUsers(branchId.Value, true);
                ViewBag.BranchId = branchId.Value;
                ViewBag.BranchName = userBranches.FirstOrDefault(b => b.Id == branchId.Value)?.Name;
                return View(branchUsers);
            }

            // نمایش لیست شعبه‌ها برای انتخاب
            ViewBag.UserBranches = userBranches;
            return View("SelectBranch");
        }

        // جزئیات کاربر شعبه
        public IActionResult Details(int id)
        {
            var branchUser = _branchRepository.GetBranchUserById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchUserViewModel>(branchUser);
            viewModel.BranchName = branchUser.Branch.Name;
            viewModel.UserFullName = $"{branchUser.User.FirstName} {branchUser.User.LastName}";

            return View(viewModel);
        }

        // افزودن کاربر به شعبه - نمایش فرم
        [HttpGet]
        public IActionResult Create(int? branchId)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            if (branchId.HasValue)
            {
                // بررسی دسترسی به شعبه
                if (!userBranches.Any(b => b.Id == branchId.Value))
                    return RedirectToAction("ErrorView", "Home");

                var branch = _uow.BranchUW.GetById(branchId.Value);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // دریافت لیست کاربران قابل اضافه کردن
                var availableUsers = _userManagerRepository.GetUserListBybranchId(branchId.Value);

                ViewBag.Users = new SelectList(availableUsers, "Id", "FullNamesString");
                ViewBag.BranchId = branchId.Value;
                ViewBag.BranchName = branch.Name;

                return View(new BranchUserViewModel
                {
                    BranchId = branchId.Value,
                    IsActive = true,
                    Role = 0, // پیش‌فرض: مشاهده‌گر
                    AssignDate = DateTime.Now
                });
            }

            // انتخاب شعبه
            ViewBag.Branches = new SelectList(userBranches, "Id", "Name");
            return View("SelectBranchForCreate");
        }

        // افزودن کاربر به شعبه - ارسال فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchUserViewModel model)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                // بررسی دسترسی به شعبه
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == model.BranchId))
                    return RedirectToAction("ErrorView", "Home");

                var branch = _uow.BranchUW.GetById(model.BranchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی اینکه کاربر قبلاً به شعبه اضافه نشده باشد
                var existingUser = _uow.BranchUserUW.Get(bu => bu.BranchId == model.BranchId && bu.UserId == model.UserId).FirstOrDefault();
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserId", "این کاربر قبلاً به شعبه اضافه شده است");

                    var availableUsers = _userManagerRepository.GetUserListBybranchId(model.BranchId);
                    ViewBag.Users = new SelectList(availableUsers, "Id", "FullNamesString");
                    ViewBag.BranchId = model.BranchId;
                    ViewBag.BranchName = branch.Name;

                    return View(model);
                }

                // ایجاد رابطه‌ی جدید بین کاربر و شعبه
                var branchUser = _mapper.Map<BranchUser>(model);
                branchUser.AssignDate = DateTime.Now;
                branchUser.AssignedByUserId = currentUserId;

                _uow.BranchUserUW.Create(branchUser);
                _uow.Save();

                return RedirectToAction("Index", new { branchId = model.BranchId });
            }

            // بازگشت فرم با خطا
            var branch2 = _uow.BranchUW.GetById(model.BranchId);
            var availableUsers2 = _userManagerRepository.GetUserListBybranchId(model.BranchId);

            ViewBag.Users = new SelectList(availableUsers2, "Id", "FullNamesString");
            ViewBag.BranchId = model.BranchId;
            ViewBag.BranchName = branch2?.Name;

            return View(model);
        }

        // ویرایش کاربر شعبه - نمایش فرم
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var branchUser = _branchRepository.GetBranchUserById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchUserViewModel>(branchUser);
            viewModel.BranchName = branchUser.Branch.Name;
            viewModel.UserFullName = $"{branchUser.User.FirstName} {branchUser.User.LastName}";

            return View(viewModel);
        }

        // ویرایش کاربر شعبه - ارسال فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BranchUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var branchUser = _uow.BranchUserUW.GetById(model.Id);
                if (branchUser == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی دسترسی
                var currentUserId = _userManager.GetUserId(HttpContext.User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                    return RedirectToAction("ErrorView", "Home");

                // بروزرسانی اطلاعات
                branchUser.Role = model.Role;
                branchUser.IsActive = model.IsActive;

                _uow.BranchUserUW.Update(branchUser);
                _uow.Save();

                return RedirectToAction("Index", new { branchId = branchUser.BranchId });
            }

            var branchUserForError = _branchRepository.GetBranchUserById(model.Id);
            model.BranchName = branchUserForError.Branch.Name;
            model.UserFullName = $"{branchUserForError.User.FirstName} {branchUserForError.User.LastName}";

            return View(model);
        }

        // حذف کاربر از شعبه - نمایش صفحه تایید
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var branchUser = _branchRepository.GetBranchUserById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchUserViewModel>(branchUser);
            viewModel.BranchName = branchUser.Branch.Name;
            viewModel.UserFullName = $"{branchUser.User.FirstName} {branchUser.User.LastName}";

            return View(viewModel);
        }

        // حذف کاربر از شعبه - تایید عملیات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var branchUser = _uow.BranchUserUW.GetById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                return RedirectToAction("ErrorView", "Home");

            int branchId = branchUser.BranchId;

            _uow.BranchUserUW.Delete(branchUser);
            _uow.Save();

            return RedirectToAction("Index", new { branchId = branchId });
        }

        // فعال/غیرفعال کردن کاربر شعبه
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var branchUser = _uow.BranchUserUW.GetById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == branchUser.BranchId))
                return RedirectToAction("ErrorView", "Home");

            branchUser.IsActive = !branchUser.IsActive;
            _uow.BranchUserUW.Update(branchUser);
            _uow.Save();

            return RedirectToAction("Index", new { branchId = branchUser.BranchId });
        }

        // جستجوی کاربران شعبه
        [HttpGet]
        public IActionResult Search(int branchId, string searchTerm, byte? role, bool? isActive)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            // بررسی دسترسی به شعبه
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            var branchUsers = _branchRepository.GetBranchUsers(branchId, true);

            // اعمال فیلترها
            if (!string.IsNullOrEmpty(searchTerm))
            {
                branchUsers = branchUsers.Where(bu =>
                    bu.User.FirstName.Contains(searchTerm) ||
                    bu.User.LastName.Contains(searchTerm) ||
                    bu.User.Email.Contains(searchTerm) ||
                    bu.User.UserName.Contains(searchTerm)).ToList();
            }

            if (role.HasValue)
            {
                branchUsers = branchUsers.Where(bu => bu.Role == role.Value).ToList();
            }

            if (isActive.HasValue)
            {
                branchUsers = branchUsers.Where(bu => bu.IsActive == isActive.Value).ToList();
            }

            ViewBag.BranchId = branchId;
            ViewBag.BranchName = userBranches.FirstOrDefault(b => b.Id == branchId)?.Name;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedRole = role;
            ViewBag.SelectedStatus = isActive;

            return View("Index", branchUsers);
        }
    }
}