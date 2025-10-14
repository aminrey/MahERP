using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace MahERP.Areas.AdminArea.Controllers.TaskControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.BRANCH")]

    public class BranchTaskCategoryController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchRepository _branchRepository;
        private readonly ITaskRepository _taskRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;

        public BranchTaskCategoryController(
            IUnitOfWork uow,
            IBranchRepository branchRepository,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _uow = uow;
            _branchRepository = branchRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        /// <summary>
        /// نمایش لیست دسته‌بندی‌های تسک متصل به شعبه مشخص
        /// </summary>
        public IActionResult Index(int branchId, int? stakeholderId = null)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            
            // بررسی دسترسی کاربر به این شعبه
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            var branchTaskCategories = _branchRepository.GetTaskCategoriesByBranchAndStakeholder(branchId, activeOnly: false);
            
            // فیلتر بر اساس طرف حساب در صورت انتخاب
            if (stakeholderId.HasValue)
            {
                branchTaskCategories = branchTaskCategories.Where(btc => btc.StakeholderId == stakeholderId.Value).ToList();
            }
            
            // تبدیل Entity ها به ViewModel
            var viewModels = branchTaskCategories.Select(btc => _mapper.Map<BranchTaskCategoryStakeholderViewModel>(btc)).ToList();
            
            // تکمیل اطلاعات نمایشی
            foreach (var viewModel in viewModels)
            {
                var entity = branchTaskCategories.FirstOrDefault(e => e.Id == viewModel.Id);
                if (entity != null)
                {
                    viewModel.BranchName = entity.Branch?.Name;
                    viewModel.TaskCategoryTitle = entity.TaskCategory?.Title;
                    viewModel.StakeholderName = entity.Stakeholder != null 
                        ? $"{entity.Stakeholder.FirstName} {entity.Stakeholder.LastName}" 
                        : "";
                    viewModel.AssignedByUserName = entity.AssignedByUser != null 
                        ? $"{entity.AssignedByUser.FirstName} {entity.AssignedByUser.LastName}" 
                        : "";
                }
            }
            
            // دریافت طرف‌حساب‌های یکتا از دسته‌بندی‌های موجود برای فیلتر
            var allBranchTaskCategories = _branchRepository.GetTaskCategoriesByBranchAndStakeholder(branchId, activeOnly: false);
            var uniqueStakeholders = allBranchTaskCategories
                .Where(btc => btc.Stakeholder != null)
                .Select(btc => new { 
                    Id = btc.StakeholderId, 
                    DisplayName = $"{btc.Stakeholder.FirstName} {btc.Stakeholder.LastName}",
                    CompanyName = btc.Stakeholder.CompanyName
                })
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .OrderBy(s => s.DisplayName)
                .ToList();
            
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = userBranches.FirstOrDefault(b => b.Id == branchId)?.Name;
            ViewBag.SelectedStakeholderId = stakeholderId;
            ViewBag.UniqueStakeholders = uniqueStakeholders;
            
            return View(viewModels);
        }

        /// <summary>
        /// افزودن دسته‌بندی تسک به شعبه - نمایش مودال
        /// </summary>
        [HttpGet]
        public IActionResult AssignTaskCategoryToBranch(int branchId)
        {
            // دریافت اطلاعات کامل از repository شامل:
            // - اطلاعات شعبه
            // - لیست دسته‌بندی‌های قابل انتساب
            BranchTaskCategoryStakeholderViewModel? viewModel = _branchRepository.GetAddTaskCategoryToBranchStakeholderViewModel(branchId);
            
            // بررسی وجود شعبه
            if (viewModel == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی کاربر لاگین شده به این شعبه
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            return PartialView("_AssignTaskCategoryToBranch", viewModel);
        }

        /// <summary>
        /// افزودن دسته‌بندی تسک به شعبه - نمایش فرم
        /// </summary>
        [HttpGet]
        public IActionResult AddTaskCategoryToBranch(int branchId, int? stakeholderId = null)
        {
            // دریافت اطلاعات کامل از repository شامل:
            // - اطلاعات شعبه
            // - لیست دسته‌بندی‌های قابل انتساب
            var viewModel = _branchRepository.GetAddTaskCategoryToBranchStakeholderViewModel(branchId, stakeholderId);
            
            // بررسی وجود شعبه
            if (viewModel == null)
                return RedirectToAction("ErrorView", "Home");

            // بررسی دسترسی کاربر لاگین شده به این شعبه
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            return View(viewModel);
        }

        /// <summary>
        /// افزودن دسته‌بندی‌های تسک به شعبه - پردازش فرم
        /// در صورت خطا، از repository برای بازیابی اطلاعات فرم استفاده می‌کند
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTaskCategoryToBranch(BranchTaskCategoryStakeholderViewModel model)
        {
            // حذف فیلدهای اضافی از ModelState چون اعتبارسنجی آن را دستی انجام می‌دهیم
            ModelState.Remove("TaskCategoriesSelected");
            ModelState.Remove("TaskCategoryTitle");
            ModelState.Remove("AssignedByUserId");
            ModelState.Remove("AssignedByUserName");
            ModelState.Remove("BranchName");
            ModelState.Remove("TaskCategoryIdSelected");

            if (ModelState.IsValid)
            {
                var branch = _uow.BranchUW.GetById(model.BranchId);
                if (branch == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی اینکه آیا لیست دسته‌بندی‌های انتخابی خالی نیست
                if (model.TaskCategoriesSelected == null || !model.TaskCategoriesSelected.Any())
                {
                    ModelState.AddModelError("TaskCategoriesSelected", "حداقل یک دسته‌بندی باید انتخاب شود");
                    
                    // دریافت اطلاعات کامل فرم از repository
                    var viewModelForError = _branchRepository.GetAddTaskCategoryToBranchStakeholderViewModel(model.BranchId, model.StakeholderId);
                    if (viewModelForError != null)
                    {
                        // انتقال اطلاعات فرم از model اصلی
                        viewModelForError.TaskCategoriesSelected = model.TaskCategoriesSelected;
                        viewModelForError.IsActive = model.IsActive;
                        viewModelForError.AssignDate = model.AssignDate;
                        return View(viewModelForError);
                    }
                    
                    return RedirectToAction("ErrorView", "Home");
                }

                // بررسی اینکه آیا دسته‌بندی‌ها قبلاً به شعبه و طرف حساب اضافه شده‌اند
                var existingCategories = model.TaskCategoriesSelected
                    .Where(categoryId => _branchRepository.IsTaskCategoryAssignedToBranchStakeholder(model.BranchId, categoryId, model.StakeholderId))
                    .ToList();

                if (existingCategories.Any())
                {
                    // دریافت نام دسته‌بندی‌های تکراری
                    var duplicateCategoryNames = _taskRepository.GetAllCategories()
                        .Where(tc => existingCategories.Contains(tc.Id))
                        .Select(tc => tc.Title)
                        .ToList();

                    ModelState.AddModelError("TaskCategoriesSelected", $"دسته‌بندی‌های زیر قبلاً به شعبه و طرف حساب اضافه شده‌اند: {string.Join(", ", duplicateCategoryNames)}");
                    
                    // دریافت اطلاعات کامل فرم از repository
                    var viewModelForError = _branchRepository.GetAddTaskCategoryToBranchStakeholderViewModel(model.BranchId, model.StakeholderId);
                    if (viewModelForError != null)
                    {
                        // انتقال اطلاعات فرم از model اصلی
                        viewModelForError.TaskCategoriesSelected = model.TaskCategoriesSelected;
                        viewModelForError.IsActive = model.IsActive;
                        viewModelForError.AssignDate = model.AssignDate;
                        return View(viewModelForError);
                    }
                    
                    return RedirectToAction("ErrorView", "Home");
                }

                // ایجاد ارتباط جدید برای هر دسته‌بندی انتخابی
                var currentUserId = _userManager.GetUserId(User);
                var assignDate = DateTime.Now;

                foreach (var categoryId in model.TaskCategoriesSelected)
                {
                    var branchTaskCategoryStakeholder = new BranchTaskCategoryStakeholder
                    {
                        BranchId = model.BranchId,
                        TaskCategoryId = categoryId,
                        StakeholderId = model.StakeholderId,
                        IsActive = model.IsActive,
                        AssignDate = assignDate,
                        AssignedByUserId = currentUserId
                    };

                    _uow.BranchTaskCategoryUW.Create(branchTaskCategoryStakeholder);
                }

                // ذخیره تمام تغییرات
                _uow.Save();

                return RedirectToAction("Index", new { branchId = model.BranchId });
            }

            // در صورت عدم اعتبار ModelState، دریافت اطلاعات کامل فرم از repository
            var viewModelForValidation = _branchRepository.GetAddTaskCategoryToBranchStakeholderViewModel(model.BranchId, model.StakeholderId);
            if (viewModelForValidation != null)
            {
                // حفظ اطلاعات وارد شده توسط کاربر
                viewModelForValidation.TaskCategoriesSelected = model.TaskCategoriesSelected;
                viewModelForValidation.IsActive = model.IsActive;
                viewModelForValidation.AssignDate = model.AssignDate;
                return View(viewModelForValidation);
            }

            // در صورت بروز خطا در repository، بازگشت به صفحه خطا
            return RedirectToAction("ErrorView", "Home");
        }

        /// <summary>
        /// ویرایش انتساب دسته‌بندی تسک به شعبه - نمایش فرم
        /// </summary>
        [HttpGet]
        public IActionResult EditBranchTaskCategory(int id)
        {
            var branchTaskCategory = _branchRepository.GetBranchTaskCategoryStakeholderById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchTaskCategoryStakeholderViewModel>(branchTaskCategory);
            viewModel.BranchName = branchTaskCategory.Branch.Name;
            viewModel.TaskCategoryTitle = branchTaskCategory.TaskCategory.Title;
            viewModel.StakeholderName = $"{branchTaskCategory.Stakeholder.FirstName} {branchTaskCategory.Stakeholder.LastName}";

            return View(viewModel);
        }

        /// <summary>
        /// ویرایش انتساب دسته‌بندی تسک به شعبه - پردازش فرم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBranchTaskCategory(BranchTaskCategoryStakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var branchTaskCategory = _uow.BranchTaskCategoryUW.GetById(model.Id);
                if (branchTaskCategory == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                branchTaskCategory.IsActive = model.IsActive;

                _uow.BranchTaskCategoryUW.Update(branchTaskCategory);
                _uow.Save();

                return RedirectToAction("Index", new { branchId = branchTaskCategory.BranchId });
            }

            var branchTaskCategoryForError = _branchRepository.GetBranchTaskCategoryStakeholderById(model.Id);
            model.BranchName = branchTaskCategoryForError.Branch.Name;
            model.TaskCategoryTitle = branchTaskCategoryForError.TaskCategory.Title;
            model.StakeholderName = $"{branchTaskCategoryForError.Stakeholder.FirstName} {branchTaskCategoryForError.Stakeholder.LastName}";
            
            return View(model);
        }

        /// <summary>
        /// حذف دسته‌بندی تسک از شعبه - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public IActionResult RemoveTaskCategory(int id)
        {
            var branchTaskCategory = _branchRepository.GetBranchTaskCategoryStakeholderById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchTaskCategoryStakeholderViewModel>(branchTaskCategory);
            viewModel.BranchName = branchTaskCategory.Branch.Name;
            viewModel.TaskCategoryTitle = branchTaskCategory.TaskCategory.Title;
            viewModel.StakeholderName = $"{branchTaskCategory.Stakeholder.FirstName} {branchTaskCategory.Stakeholder.LastName}";

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف دسته‌بندی از شعبه";

            return PartialView("_RemoveTaskCategory", viewModel);
        }

        /// <summary>
        /// حذف دسته‌بندی تسک از شعبه - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveTaskCategoryPost(int id)
        {
            var branchTaskCategory = _uow.BranchTaskCategoryUW.GetById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            int branchId = branchTaskCategory.BranchId;

            _uow.BranchTaskCategoryUW.Delete(branchTaskCategory);
            _uow.Save();

            return RedirectToAction("Index", new { branchId = branchId });
        }

        /// <summary>
        /// فعال/غیرفعال کردن انتساب دسته‌بندی - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public IActionResult ToggleActiveStatus(int id)
        {
            var branchTaskCategory = _uow.BranchTaskCategoryUW.GetById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            if (branchTaskCategory.IsActive)
            {
                // غیرفعال کردن
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن انتساب دسته‌بندی";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                // فعال کردن
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن انتساب دسته‌بندی";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActiveStatus", _mapper.Map<BranchTaskCategoryStakeholderViewModel>(branchTaskCategory));
        }

        /// <summary>
        /// فعال/غیرفعال کردن انتساب دسته‌بندی - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActiveStatusPost(int id)
        {
            var branchTaskCategory = _uow.BranchTaskCategoryUW.GetById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            branchTaskCategory.IsActive = !branchTaskCategory.IsActive;
            _uow.BranchTaskCategoryUW.Update(branchTaskCategory);
            _uow.Save();

            return RedirectToAction("Index", new { branchId = branchTaskCategory.BranchId });
        }
    }
}