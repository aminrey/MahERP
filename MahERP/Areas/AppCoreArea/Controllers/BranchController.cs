using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactGroupRepository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
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

namespace MahERP.Areas.AppCoreArea.Controllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("CORE.BRANCH.DEFINITIONS")]

    public partial class BranchController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;
        private readonly IContactGroupRepository _groupRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IBranchTaskVisibilitySettingsRepository _visibilitySettingsRepo;  // ⭐⭐⭐ اضافه شد


        public BranchController(
            IUnitOfWork uow,
            IBranchRepository branchRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger, 
            IContactGroupRepository groupRepository, 
            IBaseRepository BaseRepository,
            IUserManagerRepository userRepository, 
            IModuleTrackingService moduleTracking, 
            IModuleAccessService moduleAccessService,
            ITaskRepository taskRepository,
            IBranchTaskVisibilitySettingsRepository visibilitySettingsRepo)  // ⭐⭐⭐ اضافه شد
 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _uow = uow;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _taskRepository = taskRepository;
            _visibilitySettingsRepo = visibilitySettingsRepo;  // ⭐⭐⭐ اضافه شد
        }

        // لیست شعبه‌ها
        public IActionResult Index()
        {
            var UserLogin = _userManager.GetUserId(HttpContext.User);

            List<BranchViewModel> branches = _branchRepository.GetBrnachListByUserId(UserLogin);
            return View(branches);
        }
        /// <summary>
        /// جزئیات شعبه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐⭐⭐ استفاده از GetBranchDetailsById که همه چیز رو کامل می‌کنه
                var model = _branchRepository.GetBranchDetailsById(
                    branchId: id,
                    userId: currentUserId,
                    includeInactiveUsers: false,
                    includeInactiveStakeholders: false,
                    includeInactiveChildBranches: false
                );

                if (model == null)
                    return RedirectToAction("ErrorView", "Home");

                // ⭐ دریافت گروه‌های شعبه برای فیلتر
                var branchGroups = _groupRepository.GetBranchGroups(id, includeInactive: false);
                ViewBag.BranchGroups = branchGroups;

                // ⭐ دریافت گروه‌های هر BranchContact (برای نمایش در جدول)
                var branchContactIds = model.BranchContacts.Select(bc => bc.Id).ToList();
                var branchContactGroupsDict = new Dictionary<int, List<BranchContactGroup>>();

                foreach (var bcId in branchContactIds)
                {
                    branchContactGroupsDict[bcId] = _groupRepository.GetBranchContactGroups(bcId);
                }
                ViewBag.BranchContactGroupsDict = branchContactGroupsDict;

                // لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "Details",
                    $"مشاهده جزئیات شعبه: {model.Name}",
                    recordId: id.ToString()
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Branch", "Details", "خطا در دریافت جزئیات", ex);
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
        public async Task<IActionResult> ToggleActivationPost(int id)
        {
            try
            {
                var branch = _uow.BranchUW.GetById(id);
                if (branch == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "شعبه یافت نشد" } }
                    });
                }

                var previousStatus = branch.IsActive;
                branch.IsActive = !branch.IsActive;
                branch.LastUpdateDate = DateTime.Now;
                _uow.BranchUW.Update(branch);
                _uow.Save();

                // ⭐ لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Branch",
                    "ToggleActivation",
                    $"{(branch.IsActive ? "فعال" : "غیرفعال")} کردن شعبه: {branch.Name}",
                    recordId: id.ToString()
                );

                // ⭐⭐⭐ بازگشت JSON برای modal-ajax-save
                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "Branch"),
                    message = new[] 
                    { 
                        new 
                        { 
                            status = "success", 
                            text = $"شعبه با موفقیت {(branch.IsActive ? "فعال" : "غیرفعال")} شد" 
                        } 
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "ToggleActivation",
                    "خطا در تغییر وضعیت شعبه",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در انجام عملیات" } }
                });
            }
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
        #region BranchContact Management

        [HttpGet]
        public async Task<IActionResult> AddContactToBranch(int branchId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return NotFound();
                }

                // ⭐ استفاده از Repository
                var availableContacts = _branchRepository.GetAvailableContactsForBranch(branchId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FullName
                    })
                    .ToList();

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.Name;
                ViewBag.AvailableContacts = availableContacts;
                ViewBag.ModalTitle = "افزودن فرد به شعبه";
                ViewBag.ThemeClass = "bg-primary";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "AddContactToBranch",
                    $"مشاهده فرم افزودن فرد به شعبه: {branch.Name}",
                    recordId: branchId.ToString()
                );

                return PartialView("_AddContactToBranch");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddContactToBranch",
                    "خطا در نمایش فرم",
                    ex,
                    recordId: branchId.ToString()
                );
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContactToBranchPost(int branchId, int contactId, byte relationType, string notes)
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

                var contact = _uow.ContactUW.GetById(contactId);
                if (contact == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "فرد یافت نشد" } }
                    });
                }

                // ⭐ استفاده از Repository
                if (_branchRepository.IsContactAssignedToBranch(branchId, contactId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "warning", text = "این فرد قبلاً به این شعبه اختصاص یافته است" } }
                    });
                }

                var branchContact = new BranchContact
                {
                    BranchId = branchId,
                    ContactId = contactId,
                    RelationType = relationType,
                    IsActive = true,
                    AssignDate = DateTime.Now,
                    AssignedByUserId = GetUserId(),
                    Notes = notes
                };

                _uow.BranchContactUW.Create(branchContact);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Branch",
                    "AddContactToBranch",
                    $"افزودن فرد {contact.FullName} به شعبه {branch.Name}",
                    recordId: branchId.ToString()
                );

                // ⭐ استفاده از Repository
                var branchContacts = _branchRepository.GetBranchContacts(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchContactsTableRows", branchContacts);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "فرد با موفقیت به شعبه اضافه شد" } },
                    viewList = new[]
                    {
                new
                {
                    elementId = "branchContactsTableBody",
                    view = new { result = renderedView }
                }
            }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddContactToBranch",
                    "خطا در افزودن فرد",
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
        [HttpGet]
        public async Task<IActionResult> RemoveContactFromBranch(int id)
        {
            try
            {
                // ⭐ استفاده از Repository
                var branchContact = _branchRepository.GetBranchContactById(id);
                if (branchContact == null)
                {
                    return NotFound();
                }

                ViewBag.BranchContactId = id;
                ViewBag.BranchName = branchContact.Branch?.Name;
                ViewBag.ContactName = branchContact.Contact?.FullName;
                ViewBag.ModalTitle = "حذف فرد از شعبه";
                ViewBag.ThemeClass = "bg-danger";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "RemoveContactFromBranch",
                    $"مشاهده فرم حذف فرد {branchContact.Contact?.FullName} از شعبه {branchContact.Branch?.Name}",
                    recordId: id.ToString()
                );

                return PartialView("_RemoveContactFromBranch", branchContact);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveContactFromBranch",
                    "خطا در نمایش فرم حذف",
                    ex,
                    recordId: id.ToString()
                );
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveContactFromBranchPost(int id)
        {
            try
            {
                // ⭐ استفاده از Repository
                var branchContact = _branchRepository.GetBranchContactById(id);
                if (branchContact == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "رکورد یافت نشد" } }
                    });
                }

                var branchId = branchContact.BranchId;
                var contactName = branchContact.Contact?.FullName;
                var branchName = branchContact.Branch?.Name;

                branchContact.IsActive = false;
                _uow.BranchContactUW.Update(branchContact);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Branch",
                    "RemoveContactFromBranch",
                    $"حذف فرد {contactName} از شعبه {branchName}",
                    recordId: id.ToString()
                );

                // ⭐ استفاده از Repository
                var branchContacts = _branchRepository.GetBranchContacts(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchContactsTableRows", branchContacts);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "فرد با موفقیت از شعبه حذف شد" } },
                    viewList = new[]
                    {
                new
                {
                    elementId = "branchContactsTableBody",
                    view = new { result = renderedView }
                }
            }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveContactFromBranch",
                    "خطا در حذف فرد از شعبه",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف: " + ex.Message } }
                });
            }
        }

        #endregion

        #region BranchOrganization Management
        [HttpGet]
        public async Task<IActionResult> AddOrganizationToBranch(int branchId)
        {
            try
            {
                var branch = _branchRepository.GetBranchById(branchId);
                if (branch == null)
                {
                    return NotFound();
                }

                // ⭐ استفاده از Repository
                var availableOrganizations = _branchRepository.GetAvailableOrganizationsForBranch(branchId)
                    .Select(o => new SelectListItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.DisplayName
                    })
                    .ToList();

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.Name;
                ViewBag.AvailableOrganizations = availableOrganizations;
                ViewBag.ModalTitle = "افزودن سازمان به شعبه";
                ViewBag.ThemeClass = "bg-success";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "AddOrganizationToBranch",
                    $"مشاهده فرم افزودن سازمان به شعبه: {branch.Name}",
                    recordId: branchId.ToString()
                );

                return PartialView("_AddOrganizationToBranch");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddOrganizationToBranch",
                    "خطا در نمایش فرم",
                    ex,
                    recordId: branchId.ToString()
                );
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrganizationToBranchPost(int branchId, int organizationId, byte relationType, bool includeAllMembers, string notes)
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

                var organization = _uow.OrganizationUW.GetById(organizationId);
                if (organization == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "سازمان یافت نشد" } }
                    });
                }

                // ⭐ استفاده از Repository
                if (_branchRepository.IsOrganizationAssignedToBranch(branchId, organizationId))
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "warning", text = "این سازمان قبلاً به این شعبه اختصاص یافته است" } }
                    });
                }

                var branchOrganization = new BranchOrganization
                {
                    BranchId = branchId,
                    OrganizationId = organizationId,
                    RelationType = relationType,
                    IncludeAllMembers = includeAllMembers,
                    IsActive = true,
                    AssignDate = DateTime.Now,
                    AssignedByUserId = GetUserId(),
                    Notes = notes
                };

                _uow.BranchOrganizationUW.Create(branchOrganization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Branch",
                    "AddOrganizationToBranch",
                    $"افزودن سازمان {organization.DisplayName} به شعبه {branch.Name} (نمایش اعضا: {(includeAllMembers ? "بله" : "خیر")})",
                    recordId: branchId.ToString()
                );

                // ⭐ استفاده از Repository
                var branchOrganizations = _branchRepository.GetBranchOrganizations(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchOrganizationsTableRows", branchOrganizations);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "سازمان با موفقیت به شعبه اضافه شد" } },
                    viewList = new[]
                    {
                new
                {
                    elementId = "branchOrganizationsTableBody",
                    view = new { result = renderedView }
                }
            }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "AddOrganizationToBranch",
                    "خطا در افزودن سازمان",
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
        // GET
        [HttpGet]
        public async Task<IActionResult> RemoveOrganizationFromBranch(int id)
        {
            try
            {
                // ⭐ استفاده از Repository
                var branchOrganization = _branchRepository.GetBranchOrganizationById(id);
                if (branchOrganization == null)
                {
                    return NotFound();
                }

                ViewBag.BranchOrganizationId = id;
                ViewBag.BranchName = branchOrganization.Branch?.Name;
                ViewBag.OrganizationName = branchOrganization.Organization?.DisplayName;
                ViewBag.ModalTitle = "حذف سازمان از شعبه";
                ViewBag.ThemeClass = "bg-danger";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Branch",
                    "RemoveOrganizationFromBranch",
                    $"مشاهده فرم حذف سازمان {branchOrganization.Organization?.DisplayName} از شعبه {branchOrganization.Branch?.Name}",
                    recordId: id.ToString()
                );

                return PartialView("_RemoveOrganizationFromBranch", branchOrganization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveOrganizationFromBranch",
                    "خطا در نمایش فرم حذف",
                    ex,
                    recordId: id.ToString()
                );
                return StatusCode(500);
            }
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOrganizationFromBranchPost(int id)
        {
            try
            {
                // ⭐ استفاده از Repository
                var branchOrganization = _branchRepository.GetBranchOrganizationById(id);
                if (branchOrganization == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "رکورد یافت نشد" } }
                    });
                }

                var branchId = branchOrganization.BranchId;
                var organizationName = branchOrganization.Organization?.DisplayName;
                var branchName = branchOrganization.Branch?.Name;

                branchOrganization.IsActive = false;
                _uow.BranchOrganizationUW.Update(branchOrganization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Branch",
                    "RemoveOrganizationFromBranch",
                    $"حذف سازمان {organizationName} از شعبه {branchName}",
                    recordId: id.ToString()
                );

                // ⭐ استفاده از Repository
                var branchOrganizations = _branchRepository.GetBranchOrganizations(branchId);
                var renderedView = await this.RenderViewToStringAsync("_BranchOrganizationsTableRows", branchOrganizations);

                return Json(new
                {
                    status = "update-view",
                    message = new[] { new { status = "success", text = "سازمان با موفقیت از شعبه حذف شد" } },
                    viewList = new[]
                    {
                new
                {
                    elementId = "branchOrganizationsTableBody",
                    view = new { result = renderedView }
                }
            }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "RemoveOrganizationFromBranch",
                    "خطا در حذف سازمان از شعبه",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در حذف: " + ex.Message } }
                });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOrganizationIncludeMembers(int id)
        {
            try
            {
                // ⭐ استفاده از Repository
                var branchOrganization = _branchRepository.GetBranchOrganizationById(id);
                if (branchOrganization == null)
                {
                    return Json(new { success = false, message = "رکورد یافت نشد" });
                }

                branchOrganization.IncludeAllMembers = !branchOrganization.IncludeAllMembers;
                _uow.BranchOrganizationUW.Update(branchOrganization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Branch",
                    "ToggleOrganizationIncludeMembers",
                    $"تغییر وضعیت نمایش اعضا به {(branchOrganization.IncludeAllMembers ? "فعال" : "غیرفعال")} برای سازمان {branchOrganization.Organization?.DisplayName}",
                    recordId: id.ToString()
                );

                return Json(new
                {
                    success = true,
                    includeMembers = branchOrganization.IncludeAllMembers,
                    message = branchOrganization.IncludeAllMembers ? "اعضا نمایان شدند" : "اعضا مخفی شدند"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Branch",
                    "ToggleOrganizationIncludeMembers",
                    "خطا در تغییر وضعیت",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new { success = false, message = "خطا در عملیات" });
            }
        }
        #endregion
    }
}