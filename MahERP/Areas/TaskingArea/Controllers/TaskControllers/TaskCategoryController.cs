using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("CORE.TASKCATEGORY")]
    public partial class TaskCategoryController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly ITaskRepository _taskRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        protected readonly IUserManagerRepository _userRepository;
        private readonly IBranchRepository _branchRepository;

        public TaskCategoryController(
            IUnitOfWork uow,
            ITaskRepository taskRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger, 
            IBaseRepository BaseRepository,
            IUserManagerRepository userRepository, 
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IBranchRepository branchRepository)
 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _uow = uow;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _branchRepository = branchRepository;
        }

        // لیست دسته‌بندی‌ها
        public IActionResult Index()
        {
            var categories = _taskRepository.GetAllCategories(activeOnly: false);
            var viewModels = _mapper.Map<List<TaskCategoryViewModel>>(categories);
            return View(viewModels);
        }

        // افزودن دسته‌بندی جدید - نمایش فرم
        [HttpGet]
        public IActionResult Create()
        {
            PopulateParentCategoriesDropdown();
            return View(new TaskCategoryViewModel { IsActive = true });
        }

        // افزودن دسته‌بندی جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaskCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<TaskCategory>(model);
                
                _uow.TaskCategoryUW.Create(category);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }
            
            PopulateParentCategoriesDropdown();
            return View(model);
        }

        // ویرایش دسته‌بندی - نمایش فرم
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _taskRepository.GetCategoryById(id);
            if (category == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<TaskCategoryViewModel>(category);
            
            PopulateParentCategoriesDropdown(id);
            return View(viewModel);
        }

        // ویرایش دسته‌بندی - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = _uow.TaskCategoryUW.GetById(model.Id);
                if (category == null)
                    return RedirectToAction("ErrorView", "Home");

                // بررسی حلقه در ساختار درختی
                if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value == model.Id)
                {
                    ModelState.AddModelError("ParentCategoryId", "دسته‌بندی نمی‌تواند والد خودش باشد");
                    PopulateParentCategoriesDropdown(model.Id);
                    return View(model);
                }

                _mapper.Map(model, category);
                _uow.TaskCategoryUW.Update(category);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }
            
            PopulateParentCategoriesDropdown(model.Id);
            return View(model);
        }

        // فعال/غیرفعال کردن دسته‌بندی - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleActiveStatus(int id)
        {
            var category = _uow.TaskCategoryUW.GetById(id);
            if (category == null)
                return RedirectToAction("ErrorView", "Home");

            if (category.IsActive)
            {
                // غیرفعال کردن
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن دسته‌بندی";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                // فعال کردن
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن دسته‌بندی";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActiveStatus", _mapper.Map<TaskCategoryViewModel>(category));
        }

        // فعال/غیرفعال کردن دسته‌بندی - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActiveStatusPost(int id)
        {
            var category = _uow.TaskCategoryUW.GetById(id);
            if (category == null)
                return RedirectToAction("ErrorView", "Home");

            category.IsActive = !category.IsActive;
            _uow.TaskCategoryUW.Update(category);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // پر کردن دراپ‌داون دسته‌بندی‌های والد
        private void PopulateParentCategoriesDropdown(int? excludeId = null)
        {
            var query = _taskRepository.GetAllCategories();
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value).ToList();
            }
            
            ViewBag.ParentCategories = new SelectList(query, "Id", "Title");
            
            // رنگ‌های رایج برای دراپ‌داون رنگ
            ViewBag.ColorClasses = new List<SelectListItem>
            {
                new SelectListItem { Text = "آبی", Value = "text-primary" },
                new SelectListItem { Text = "سبز", Value = "text-success" },
                new SelectListItem { Text = "قرمز", Value = "text-danger" },
                new SelectListItem { Text = "زرد", Value = "text-warning" },
                new SelectListItem { Text = "آبی روشن", Value = "text-info" },
                new SelectListItem { Text = "خاکستری", Value = "text-muted" },
                new SelectListItem { Text = "مشکی", Value = "text-dark" }
            };
            
            // آیکون‌های رایج برای دراپ‌داون آیکون
            ViewBag.IconClasses = new List<SelectListItem>
            {
                new SelectListItem { Text = "وظیفه", Value = "fa fa-tasks" },
                new SelectListItem { Text = "پروژه", Value = "fa fa-project-diagram" },
                new SelectListItem { Text = "فایل", Value = "fa fa-file" },
                new SelectListItem { Text = "کار", Value = "fa fa-briefcase" },
                new SelectListItem { Text = "تقویم", Value = "fa fa-calendar" },
                new SelectListItem { Text = "نمودار", Value = "fa fa-chart-line" },
                new SelectListItem { Text = "چک لیست", Value = "fa fa-clipboard-list" },
                new SelectListItem { Text = "یادداشت", Value = "fa fa-sticky-note" },
                new SelectListItem { Text = "گروه", Value = "fa fa-users" },
                new SelectListItem { Text = "پیام", Value = "fa fa-comment" }
            };
        }
    }
}