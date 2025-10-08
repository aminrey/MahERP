using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("Branch")]

    public class BranchController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;

        public BranchController(
            IUnitOfWork uow,
            IBranchRepository branchRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _uow = uow;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // لیست شعبه‌ها
        public IActionResult Index()
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            List<BranchViewModel> branches = _branchRepository.GetBrnachListByUserId(UserLogin);
            return View(branches);
        }

        // جزئیات شعبه
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var branchDetails = _branchRepository.GetBranchDetailsById(id, currentUserId,includeInactiveStakeholders:true);

                if (branchDetails == null)
                {
                    return RedirectToAction("ErrorView", "Home");
                }

                // ✅ انتقال BranchStakeholders به ViewBag
                ViewBag.BranchStakeholders = branchDetails.BranchStakeholders;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "Details",
                    $"مشاهده جزئیات شعبه: {branchDetails.Name}",
                    recordId: id.ToString(),
                    entityType: "Branch",
                    recordTitle: branchDetails.Name
                );

                return View(branchDetails);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Branch", "Details", "خطا در دریافت جزئیات", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
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
        public IActionResult AddUserToBranch(int branchId)
        {
            // دریافت اطلاعات کامل از repository شامل:
            // - اطلاعات شعبه
            // - لیست کاربران قابل انتساب
            var viewModel = _branchRepository.GetAddUserToBranchViewModel(branchId);
            
            // بررسی وجود شعبه
            if (viewModel == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی کاربر لاگین شده به این شعبه
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            // ViewModel حاوی تمام اطلاعات لازم است
            // نیاز به ViewBag نداریم چون همه چیز در ViewModel موجود است
            return View(viewModel);
        }

        // افزودن کاربر به شعبه - پردازش فرم
        /// <summary>
        /// افزودن کاربران به شعبه - پردازش فرم
        /// در صورت خطا، از repository برای بازیابی اطلاعات فرم استفاده می‌کند
        /// </summary>
        /// <param name="model">مدل اطلاعات کاربران شعبه</param>
        /// <returns>نتیجه عملیات افزودن کاربران</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUserTobranch(BranchUserViewModel model)
        {
            // حذف فیلد UsersSelected از ModelState چون اعتبارسنجی آن را دستی انجام می‌دهیم
            ModelState.Remove("UsersSelected");
            ModelState.Remove("UserFullName");
            ModelState.Remove("AssignedByUserId");
            ModelState.Remove("BranchName");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var branch = _uow.BranchUW.GetById(model.BranchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی اینکه آیا لیست کاربران انتخابی خالی نیست
                if (model.UsersSelected == null || !model.UsersSelected.Any())
                {
                    ModelState.AddModelError("UsersSelected", "حداقل یک کاربر باید انتخاب شود");
                    
                    // دریافت اطلاعات کامل فرم از repository
                    var viewModelForError = _branchRepository.GetAddUserToBranchViewModel(model.BranchId);
                    if (viewModelForError != null)
                    {
                        // انتقال اطلاعات فرم از model اصلی
                        viewModelForError.UsersSelected = model.UsersSelected;
                        viewModelForError.Role = model.Role;
                        viewModelForError.IsActive = model.IsActive;
                        viewModelForError.AssignDate = model.AssignDate;
                        return View("AddUserToBranch", viewModelForError);
                    }
                    
                    return RedirectToAction("ErrorView", "Home");
                }

                // بررسی اینکه آیا کاربران قبلاً به شعبه اضافه شده‌اند
                var existingUsers = _uow.BranchUserUW.Get(bu => bu.BranchId == model.BranchId && model.UsersSelected.Contains(bu.UserId))
                    .Select(bu => bu.UserId)
                    .ToList();

                if (existingUsers.Any())
                {
                    // دریافت نام کاربران تکراری
                    var duplicateUserNames = _userManager.Users
                        .Where(u => existingUsers.Contains(u.Id))
                        .Select(u => u.FirstName + " " + u.LastName)
                        .ToList();

                    ModelState.AddModelError("UsersSelected", $"کاربران زیر قبلاً به شعبه اضافه شده‌اند: {string.Join(", ", duplicateUserNames)}");
                    
                    // دریافت اطلاعات کامل فرم از repository
                    var viewModelForError = _branchRepository.GetAddUserToBranchViewModel(model.BranchId);
                    if (viewModelForError != null)
                    {
                        // انتقال اطلاعات فرم از model اصلی
                        viewModelForError.UsersSelected = model.UsersSelected;
                        viewModelForError.Role = model.Role;
                        viewModelForError.IsActive = model.IsActive;
                        viewModelForError.AssignDate = model.AssignDate;
                        return View("AddUserToBranch", viewModelForError);
                    }
                    
                    return RedirectToAction("ErrorView", "Home");
                }

                // ایجاد ارتباط جدید برای هر کاربر انتخابی
                var currentUserId = _userManager.GetUserId(User);
                var assignDate = DateTime.Now;

                foreach (var userId in model.UsersSelected)
                {
                    var branchUser = new BranchUser
                    {
                        BranchId = model.BranchId,
                        UserId = userId,
                        Role = model.Role,
                        IsActive = model.IsActive,
                        AssignDate = assignDate,
                        AssignedByUserId = currentUserId
                    };

                    _uow.BranchUserUW.Create(branchUser);
                }

                // ذخیره تمام تغییرات
                _uow.Save();

                return RedirectToAction("Details", new { id = model.BranchId });
            }

            // در صورت عدم اعتبار ModelState، دریافت اطلاعات کامل فرم از repository
            var viewModelForValidation = _branchRepository.GetAddUserToBranchViewModel(model.BranchId);
            if (viewModelForValidation != null)
            {
                // حفظ اطلاعات وارد شده توسط کاربر
                viewModelForValidation.UsersSelected = model.UsersSelected;
                viewModelForValidation.Role = model.Role;
                viewModelForValidation.IsActive = model.IsActive;
                viewModelForValidation.AssignDate = model.AssignDate;
                return View("AddUserToBranch", viewModelForValidation);
            }

            // در صورت بروز خطا در repository، بازگشت به صفحه خطا
            return RedirectToAction("ErrorView", "Home");
        }

        // ویرایش کاربر شعبه - نمایش فرم
        [HttpGet]
        public IActionResult EditBranchUser(int id)
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
        public IActionResult EditBranchUser(BranchUserViewModel model)
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
        
        /// <summary>
        /// افزودن طرف حساب به شعبه - نمایش مودال
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddStakeholderToBranch(int branchId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return NotFound();
                }

                // دریافت طرف حساب‌های فعال که قبلاً به این شعبه اختصاص نیافته‌اند
                var assignedStakeholderIds = _uow.StakeholderBranchUW
                    .Get(sb => sb.BranchId == branchId && sb.IsActive)
                    .Select(sb => sb.StakeholderId)
                    .ToList();

                var availableStakeholders = _uow.StakeholderUW
                    .Get(s => s.IsActive && !s.IsDeleted && !assignedStakeholderIds.Contains(s.Id))
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.DisplayName
                    })
                    .ToList();

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.Name;
                ViewBag.AvailableStakeholders = availableStakeholders;
                ViewBag.ModalTitle = "افزودن طرف حساب به شعبه";
                ViewBag.ThemeClass = "bg-primary";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "AddStakeholderToBranch",
                    $"مشاهده فرم افزودن طرف حساب به شعبه: {branch.Name}",
                    recordId: branchId.ToString()
                );

                return PartialView("_AddStakeholderToBranch");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddStakeholderToBranch",
                    "خطا در نمایش فرم",
                    ex,
                    recordId: branchId.ToString()
                );
                return StatusCode(500);
            }
        }

        /// <summary>
        /// افزودن طرف حساب به شعبه - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStakeholderToBranchPost(int branchId, int stakeholderId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شعبه یافت نشد" } }
                    });
                }

                var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
                if (stakeholder == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "طرف حساب یافت نشد" } }
                    });
                }

                // بررسی عدم تکراری بودن
                var exists = _uow.StakeholderBranchUW
                    .Get(sb => sb.BranchId == branchId && sb.StakeholderId == stakeholderId)
                    .FirstOrDefault();

                if (exists != null)
                {
                    if (exists.IsActive)
                    {
                        return Json(new
                        {
                            status = "error",
                            message = new[] { new { status = "warning", text = "این طرف حساب قبلاً به این شعبه اختصاص یافته است" } }
                        });
                    }
                    else
                    {
                        // فعال‌سازی مجدد
                        exists.IsActive = true;
                        exists.AssignDate = DateTime.Now;
                        exists.AssignedByUserId = GetUserId();
                        _uow.StakeholderBranchUW.Update(exists);
                    }
                }
                else
                {
                    // ایجاد رکورد جدید
                    var stakeholderBranch = new StakeholderBranch
                    {
                        BranchId = branchId,
                        StakeholderId = stakeholderId,
                        IsActive = true,
                        AssignDate = DateTime.Now,
                        AssignedByUserId = GetUserId(),
                        CreatorUserId = GetUserId(),
                        CreateDate = DateTime.Now
                    };

                    _uow.StakeholderBranchUW.Create(stakeholderBranch);
                }

                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Branch",
                    "AddStakeholderToBranch",
                    $"افزودن طرف حساب {stakeholder.DisplayName} به شعبه {branch.Name}",
                    recordId: branchId.ToString()
                );

                // رندر کردن لیست به‌روزرسانی شده طرف حساب‌های شعبه
                var branchStakeholders = _branchRepository.GetBranchStakeholders(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchStakeholdersTableRows", branchStakeholders);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "طرف حساب با موفقیت به شعبه اضافه شد" } },
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "branchStakeholdersTableBody",
                            view = new { result = renderedView }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddStakeholderToBranch",
                    "خطا در افزودن طرف حساب",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در ثبت: " + ex.Message } }
                });
            }
        }
        /// <summary>
        /// حذف طرف حساب از شعبه - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RemoveStakeholderFromBranch(int branchId, int stakeholderId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return NotFound();
                }

                var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
                if (stakeholder == null)
                {
                    return NotFound();
                }

                var stakeholderBranch = _uow.StakeholderBranchUW
                    .Get(sb => sb.BranchId == branchId && sb.StakeholderId == stakeholderId && sb.IsActive)
                    .FirstOrDefault();

                if (stakeholderBranch == null)
                {
                    return NotFound();
                }

                ViewBag.BranchId = branchId;
                ViewBag.StakeholderId = stakeholderId;
                ViewBag.BranchName = branch.Name;
                ViewBag.StakeholderName = stakeholder.DisplayName;
                ViewBag.ModalTitle = "حذف طرف حساب از شعبه";
                ViewBag.ThemeClass = "bg-danger";
                ViewBag.ButtonClass = "btn rounded-0 btn-hero btn-danger";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "RemoveStakeholderFromBranch",
                    $"مشاهده فرم حذف طرف حساب {stakeholder.DisplayName} از شعبه {branch.Name}",
                    recordId: branchId.ToString()
                );

                return PartialView("_RemoveStakeholderFromBranch", stakeholderBranch);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveStakeholderFromBranch",
                    "خطا در نمایش فرم حذف",
                    ex,
                    recordId: branchId.ToString()
                );
                return StatusCode(500);
            }
        }

        /// <summary>
        /// حذف طرف حساب از شعبه - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStakeholderFromBranchPost(int branchId, int stakeholderId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شعبه یافت نشد" } }
                    });
                }

                var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
                if (stakeholder == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "طرف حساب یافت نشد" } }
                    });
                }

                var stakeholderBranch = _uow.StakeholderBranchUW
                    .Get(sb => sb.BranchId == branchId && sb.StakeholderId == stakeholderId)
                    .FirstOrDefault();

                if (stakeholderBranch == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "ارتباط بین طرف حساب و شعبه یافت نشد" } }
                    });
                }

                // غیرفعال کردن ارتباط (soft delete)
                stakeholderBranch.IsActive = false;
             

                _uow.StakeholderBranchUW.Update(stakeholderBranch);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Branch",
                    "RemoveStakeholderFromBranch",
                    $"حذف طرف حساب {stakeholder.DisplayName} از شعبه {branch.Name}",
                    recordId: branchId.ToString()
                );

                // رندر کردن لیست به‌روزرسانی شده
                var branchStakeholders = _branchRepository.GetBranchStakeholders(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchStakeholdersTableRows", branchStakeholders);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "طرف حساب با موفقیت از شعبه حذف شد" } },
                    viewList = new[]
                    {
                new
                {
                    elementId = "branchStakeholdersTableBody",
                    view = new { result = renderedView }
                }
            }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveStakeholderFromBranch",
                    "خطا در حذف طرف حساب از شعبه",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف: " + ex.Message } }
                });
            }
        }
    }
}