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
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using System;
using System.Linq;

namespace MahERP.Areas.AdminArea.Controllers.TaskControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class BranchTaskCategoryController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IBranchTaskCategoryRepository _branchTaskCategoryRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ITaskRepository _taskRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public BranchTaskCategoryController(
            IUnitOfWork uow,
            IBranchTaskCategoryRepository branchTaskCategoryRepository,
            IBranchRepository branchRepository,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _uow = uow;
            _branchTaskCategoryRepository = branchTaskCategoryRepository;
            _branchRepository = branchRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// نمایش لیست دسته‌بندی‌های تسک متصل به شعبه مشخص
        /// </summary>
        public IActionResult Index(int branchId)
        {
            var currentUserId = _userManager.GetUserId(HttpContext.User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            
            // بررسی دسترسی کاربر به این شعبه
            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            var branchTaskCategories = _branchTaskCategoryRepository.GetTaskCategoriesByBranchId(branchId, activeOnly: false);
            
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = userBranches.FirstOrDefault(b => b.Id == branchId)?.Name;
            
            return View(branchTaskCategories);
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
            var viewModel = _branchTaskCategoryRepository.GetAddTaskCategoryToBranchViewModel(branchId);
            
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
        public IActionResult AddTaskCategoryToBranch(int branchId)
        {
            // دریافت اطلاعات کامل از repository شامل:
            // - اطلاعات شعبه
            // - لیست دسته‌بندی‌های قابل انتساب
            var viewModel = _branchTaskCategoryRepository.GetAddTaskCategoryToBranchViewModel(branchId);
            
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
        public IActionResult AddTaskCategoryToBranch(BranchTaskCategoryViewModel model)
        {
            // حذف فیلدهای اضافی از ModelState چون اعتبارسنجی آن را دستی انجام می‌دهیم
            ModelState.Remove("TaskCategoriesSelected");
            ModelState.Remove("TaskCategoryTitle");
            ModelState.Remove("AssignedByUserId");
            ModelState.Remove("AssignedByUserName");
            ModelState.Remove("BranchName");
            ModelState.Remove("TaskCategoryId");

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
                    var viewModelForError = _branchTaskCategoryRepository.GetAddTaskCategoryToBranchViewModel(model.BranchId);
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

                // بررسی اینکه آیا دسته‌بندی‌ها قبلاً به شعبه اضافه شده‌اند
                var existingCategories = _uow.BranchTaskCategoryUW.Get(btc => btc.BranchId == model.BranchId && model.TaskCategoriesSelected.Contains(btc.TaskCategoryId))
                    .Select(btc => btc.TaskCategoryId)
                    .ToList();

                if (existingCategories.Any())
                {
                    // دریافت نام دسته‌بندی‌های تکراری
                    var duplicateCategoryNames = _taskRepository.GetAllCategories()
                        .Where(tc => existingCategories.Contains(tc.Id))
                        .Select(tc => tc.Title)
                        .ToList();

                    ModelState.AddModelError("TaskCategoriesSelected", $"دسته‌بندی‌های زیر قبلاً به شعبه اضافه شده‌اند: {string.Join(", ", duplicateCategoryNames)}");
                    
                    // دریافت اطلاعات کامل فرم از repository
                    var viewModelForError = _branchTaskCategoryRepository.GetAddTaskCategoryToBranchViewModel(model.BranchId);
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
                    var branchTaskCategory = new BranchTaskCategory
                    {
                        BranchId = model.BranchId,
                        TaskCategoryId = categoryId,
                        IsActive = model.IsActive,
                        AssignDate = assignDate,
                        AssignedByUserId = currentUserId
                    };

                    _uow.BranchTaskCategoryUW.Create(branchTaskCategory);
                }

                // ذخیره تمام تغییرات
                _uow.Save();

                return RedirectToAction("Index", new { branchId = model.BranchId });
            }

            // در صورت عدم اعتبار ModelState، دریافت اطلاعات کامل فرم از repository
            var viewModelForValidation = _branchTaskCategoryRepository.GetAddTaskCategoryToBranchViewModel(model.BranchId);
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
        /// ویرایش انتصاب دسته‌بندی تسک به شعبه - نمایش فرم
        /// </summary>
        [HttpGet]
        public IActionResult EditBranchTaskCategory(int id)
        {
            var branchTaskCategory = _branchTaskCategoryRepository.GetBranchTaskCategoryById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchTaskCategoryViewModel>(branchTaskCategory);
            viewModel.BranchName = branchTaskCategory.Branch.Name;
            viewModel.TaskCategoryTitle = branchTaskCategory.TaskCategory.Title;

            return View(viewModel);
        }

        /// <summary>
        /// ویرایش انتصاب دسته‌بندی تسک به شعبه - پردازش فرم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBranchTaskCategory(BranchTaskCategoryViewModel model)
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

            var branchTaskCategoryForError = _branchTaskCategoryRepository.GetBranchTaskCategoryById(model.Id);
            model.BranchName = branchTaskCategoryForError.Branch.Name;
            model.TaskCategoryTitle = branchTaskCategoryForError.TaskCategory.Title;
            
            return View(model);
        }

        /// <summary>
        /// حذف دسته‌بندی تسک از شعبه - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public IActionResult RemoveTaskCategory(int id)
        {
            var branchTaskCategory = _branchTaskCategoryRepository.GetBranchTaskCategoryById(id);
            if (branchTaskCategory == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<BranchTaskCategoryViewModel>(branchTaskCategory);
            viewModel.BranchName = branchTaskCategory.Branch.Name;
            viewModel.TaskCategoryTitle = branchTaskCategory.TaskCategory.Title;

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

            return PartialView("_ToggleActiveStatus", _mapper.Map<BranchTaskCategoryViewModel>(branchTaskCategory));
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