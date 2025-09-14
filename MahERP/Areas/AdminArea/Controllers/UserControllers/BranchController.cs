using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using System;
using System.Linq;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class BranchController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public BranchController(
            IUnitOfWork uow,
            IBranchRepository branchRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache) : base(uow, userManager, persianDateHelper, memoryCache)
        {
            _uow = uow;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست شعبه‌ها
        public IActionResult Index()
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            List<BranchViewModel> branches = _branchRepository.GetBrnachListByUserId(UserLogin);
            return View(branches);
        }

        // جزئیات شعبه
        public IActionResult Details(int id)
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            // دریافت جزئیات کامل شعبه
            var branchDetails = _branchRepository.GetBranchDetailsById(id, UserLogin);
            if (branchDetails == null)
                return RedirectToAction("ErrorView", "Home");

            return View(branchDetails);
        }

        // افزودن شعبه جدید - نمایش فرم
        [HttpGet]
        public IActionResult AddBranch()
        {
            // دریافت لیست شعبه‌های اصلی برای انتخاب شعبه مادر
            ViewBag.ParentBranches = new SelectList(
                _branchRepository.GetBrnachListByUserId("0").Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name");

            return View(new BranchViewModel { IsActive = true });
        }

        // افزودن شعبه جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBranch(BranchViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن نام شعبه
                if (!_branchRepository.IsBranchNameUnique(model.Name))
                {
                    ModelState.AddModelError("Name", "نام شعبه تکراری است");
                    ViewBag.ParentBranches = new SelectList(
                        _branchRepository.GetBrnachListByUserId("0").Select(b => new { Id = b.Id, Name = b.Name }),
                        "Id", "Name");
                    return View(model);
                }

                // ایجاد شعبه جدید
                var branch = _mapper.Map<Branch>(model);
                branch.CreateDate = DateTime.Now;

                // ذخیره در دیتابیس
                _uow.BranchUW.Create(branch);
                _uow.Save();

                // افزودن کاربر ایجاد کننده به جدول BranchUser_Tbl
                var currentUserId = _userManager.GetUserId(User);
                var branchUser = new BranchUser
                {
                    BranchId = branch.Id,
                    UserId = currentUserId,
                    AssignedByUserId = currentUserId,
                    Role = 1, // مدیر شعبه
                    IsActive = true,
                    AssignDate = DateTime.Now
                };

                _uow.BranchUserUW.Create(branchUser);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentBranches = new SelectList(
                _branchRepository.GetBrnachListByUserId("0").Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name");
            return View(model);
        }

        // ویرایش شعبه - نمایش فرم
        [HttpGet]
        public IActionResult EditBranch(int id)
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            // دریافت شعبه مشخص بر اساس id
            var branch = _uow.BranchUW.GetById(id);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی کاربر به این شعبه
            var userBranches = _branchRepository.GetBrnachListByUserId(UserLogin);
            if (!userBranches.Any(b => b.Id == id))
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchViewModel>(branch);

            // دریافت لیست شعبه‌های اصلی برای انتخاب شعبه مادر
            ViewBag.ParentBranches = new SelectList(
                userBranches.Where(b => b.Id != id).Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name", viewModel.ParentId);

            return View(viewModel);
        }

        // ویرایش شعبه - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBranch(BranchViewModel model)
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن نام شعبه
                if (!_branchRepository.IsBranchNameUnique(model.Name, model.Id))
                {
                    ModelState.AddModelError("Name", "نام شعبه تکراری است");
                    
                    var userBranches = _branchRepository.GetBrnachListByUserId(UserLogin);
                    ViewBag.ParentBranches = new SelectList(
                        userBranches.Where(b => b.Id != model.Id).Select(b => new { Id = b.Id, Name = b.Name }),
                        "Id", "Name", model.ParentId);
                    return View(model);
                }

                // دریافت شعبه از دیتابیس
                var branch = _uow.BranchUW.GetById(model.Id);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی دسترسی کاربر به این شعبه
                var userBranches2 = _branchRepository.GetBrnachListByUserId(UserLogin);
                if (!userBranches2.Any(b => b.Id == model.Id))
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, branch);
                branch.LastUpdateDate = DateTime.Now;

                _uow.BranchUW.Update(branch);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            var userBranchesForError = _branchRepository.GetBrnachListByUserId(UserLogin);
            ViewBag.ParentBranches = new SelectList(
                userBranchesForError.Where(b => b.Id != model.Id).Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name", model.ParentId);
            return View(model);
        }

        // فعال/غیرفعال کردن شعبه - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleActivation(int id)
        {
            var branch = _uow.BranchUW.GetById(id);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            if (branch.IsActive)
            {
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن شعبه";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن شعبه";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActivation", branch);
        }

        // فعال/غیرفعال کردن شعبه - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActivationPost(int id)
        {
            var branch = _uow.BranchUW.GetById(id);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            branch.IsActive = !branch.IsActive;
            branch.LastUpdateDate = DateTime.Now;
            _uow.BranchUW.Update(branch);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // افزودن کاربر به شعبه - نمایش فرم
        [HttpGet]
        public IActionResult AddUser(int branchId)
        {
            var branch = _uow.BranchUW.GetById(branchId);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            // دریافت لیست کاربران فعال که در این شعبه نیستند
            var existingUserIds = _branchRepository.GetBranchUsers(branchId, true)
                .Select(bu => bu.UserId)
                .ToList();

            var availableUsers = _userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser && !existingUserIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();

            ViewBag.Users = new SelectList(availableUsers, "Id", "FullName");
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = branch.Name;

            return View(new BranchUserViewModel
            {
                BranchId = branchId,
                IsActive = true,
                Role = 0, // کارشناس به‌عنوان پیش‌فرض
                AssignDate = DateTime.Now
            });
        }

        // افزودن کاربر به شعبه - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(BranchUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var branch = _uow.BranchUW.GetById(model.BranchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی اینکه کاربر قبلاً به شعبه اضافه نشده باشد
                var existingUser = _uow.BranchUserUW.Get(bu => bu.BranchId == model.BranchId && bu.UserId == model.UserId).FirstOrDefault();
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserId", "این کاربر قبلاً به شعبه اضافه شده است");
                    
                    var existingUserIds = _branchRepository.GetBranchUsers(model.BranchId, true)
                        .Select(bu => bu.UserId)
                        .ToList();

                    var availableUsers = _userManager.Users
                        .Where(u => u.IsActive && !u.IsRemoveUser && !existingUserIds.Contains(u.Id))
                        .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                        .ToList();

                    ViewBag.Users = new SelectList(availableUsers, "Id", "FullName");
                    ViewBag.BranchId = model.BranchId;
                    ViewBag.BranchName = branch.Name;
                    
                    return View(model);
                }

                // ایجاد ارتباط جدید بین کاربر و شعبه
                var branchUser = _mapper.Map<BranchUser>(model);
                branchUser.AssignDate = DateTime.Now;
                branchUser.AssignedByUserId = _userManager.GetUserId(User);

                _uow.BranchUserUW.Create(branchUser);
                _uow.Save();

                return RedirectToAction("Details", new { id = model.BranchId });
            }

            var branch2 = _uow.BranchUW.GetById(model.BranchId);
            var existingUserIds2 = _branchRepository.GetBranchUsers(model.BranchId, true)
                .Select(bu => bu.UserId)
                .ToList();

            var availableUsers2 = _userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser && !existingUserIds2.Contains(u.Id))
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();

            ViewBag.Users = new SelectList(availableUsers2, "Id", "FullName");
            ViewBag.BranchId = model.BranchId;
            ViewBag.BranchName = branch2.Name;
            
            return View(model);
        }

        // ویرایش کاربر شعبه - نمایش فرم
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var branchUser = _branchRepository.GetBranchUserById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchUserViewModel>(branchUser);
            viewModel.BranchName = branchUser.Branch.Name;
            viewModel.UserFullName = $"{branchUser.User.FirstName} {branchUser.User.LastName}";

            return View(viewModel);
        }

        // ویرایش کاربر شعبه - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(BranchUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var branchUser = _uow.BranchUserUW.GetById(model.Id);
                if (branchUser == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                branchUser.Role = model.Role;
                branchUser.IsActive = model.IsActive;

                _uow.BranchUserUW.Update(branchUser);
                _uow.Save();

                return RedirectToAction("Details", new { id = branchUser.BranchId });
            }

            var branchUserForError = _branchRepository.GetBranchUserById(model.Id);
            model.BranchName = branchUserForError.Branch.Name;
            model.UserFullName = $"{branchUserForError.User.FirstName} {branchUserForError.User.LastName}";
            
            return View(model);
        }

        // حذف کاربر از شعبه - نمایش مودال تأیید
        [HttpGet]
        public IActionResult RemoveUser(int id)
        {
            var branchUser = _branchRepository.GetBranchUserById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchUserViewModel>(branchUser);
            viewModel.BranchName = branchUser.Branch.Name;
            viewModel.UserFullName = $"{branchUser.User.FirstName} {branchUser.User.LastName}";

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف کاربر از شعبه";

            return PartialView("_RemoveUser", viewModel);
        }

        // حذف کاربر از شعبه - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserPost(int id)
        {
            var branchUser = _uow.BranchUserUW.GetById(id);
            if (branchUser == null)
                return RedirectToAction("ErrorView", "Home");

            int branchId = branchUser.BranchId;

            _uow.BranchUserUW.Delete(branchUser);
            _uow.Save();

            return RedirectToAction("Details", new { id = branchId });
        }

        // افزودن طرف حساب به شعبه
        [HttpGet]
        public IActionResult AddStakeholder(int branchId)
        {
            var branch = _uow.BranchUW.GetById(branchId);
            if (branch == null)
                return RedirectToAction("ErrorView", "Home");

            // دریافت لیست طرف حساب‌های فعال که در این شعبه نیستند
            var existingStakeholderIds = _uow.StakeholderBranchUW.Get(sb => sb.BranchId == branchId)
                .Select(sb => sb.StakeholderId)
                .ToList();

            var availableStakeholders = _uow.StakeholderUW.Get(s => s.IsActive && !s.IsDeleted && !existingStakeholderIds.Contains(s.Id))
                .Select(s => new 
                { 
                    Id = s.Id, 
                    FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})" 
                })
                .ToList();

            ViewBag.Stakeholders = new SelectList(availableStakeholders, "Id", "FullName");
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = branch.Name;

            return View();
        }

        // افزودن طرف حساب به شعبه - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStakeholder(int branchId, int stakeholderId)
        {
            if (branchId <= 0 || stakeholderId <= 0)
            {
                return BadRequest("اطلاعات نامعتبر");
            }

            var branch = _uow.BranchUW.GetById(branchId);
            var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
            
            if (branch == null || stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی اینکه طرف حساب قبلاً به شعبه اضافه نشده باشد
            var existingRelation = _uow.StakeholderBranchUW.Get(sb => sb.BranchId == branchId && sb.StakeholderId == stakeholderId).FirstOrDefault();
            if (existingRelation != null)
            {
                // اگر قبلاً حذف شده بود، فعال کنیم
                if (!existingRelation.IsActive)
                {
                    existingRelation.IsActive = true;
                    _uow.StakeholderBranchUW.Update(existingRelation);
                    _uow.Save();
                }
                return RedirectToAction("Details", new { id = branchId });
            }

            // ایجاد ارتباط جدید بین طرف حساب و شعبه
            var stakeholderBranch = new StakeholderBranch
            {
                StakeholderId = stakeholderId,
                BranchId = branchId,
                IsActive = true,
                CreatorUserId = _userManager.GetUserId(User),
                CreateDate = DateTime.Now
            };

            _uow.StakeholderBranchUW.Create(stakeholderBranch);
            _uow.Save();

            return RedirectToAction("Details", new { id = branchId });
        }

        // حذف طرف حساب از شعبه
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveStakeholder(int branchId, int stakeholderId)
        {
            if (branchId <= 0 || stakeholderId <= 0)
            {
                return BadRequest("اطلاعات نامعتبر");
            }

            var stakeholderBranch = _uow.StakeholderBranchUW.Get(sb => sb.BranchId == branchId && sb.StakeholderId == stakeholderId).FirstOrDefault();
            if (stakeholderBranch == null)
                return RedirectToAction("ErrorView", "Home");

            stakeholderBranch.IsActive = false;
            _uow.StakeholderBranchUW.Update(stakeholderBranch);
            _uow.Save();

            return RedirectToAction("Details", new { id = branchId });
        }

        // جستجوی شعبه
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            var branches = _branchRepository.GetBrnachListByUserId("0");
            ViewBag.SearchTerm = searchTerm;
            return View("Index", branches);
        }
    }
}