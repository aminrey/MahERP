using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions; // اضافه شده
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.TaskControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly ITaskRepository _taskRepository;           
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRoleRepository _roleRepository;

        public TasksController(
            IUnitOfWork uow,
            ITaskRepository taskRepository,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            IWebHostEnvironment webHostEnvironment,
            IRoleRepository roleRepository) : base(uow, userManager, persianDateHelper, memoryCache)
        {
            _uow = uow;
            _taskRepository = taskRepository;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _roleRepository = roleRepository;
        }

        // لیست تسک‌ها - با کنترل سطح دسترسی داده
        [Permission("Tasks", "Index", 0)] // Read permission
        public IActionResult Index()
        {
            var dataAccessLevel = this.GetUserDataAccessLevel("Tasks", "Index");
            var userId = _userManager.GetUserId(User);
            TaskListForIndexViewModel Filters = new TaskListForIndexViewModel
            {
                UserLoginid = userId,

            };




            TaskListForIndexViewModel Model = new TaskListForIndexViewModel();
           
            
            switch (dataAccessLevel)
            {
                case 0: // Personal - فقط تسک‌های خود کاربر
                    //tasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
                    Model = _taskRepository.GetTaskForIndexByUser(Filters);
                    break;
                case 1: // Branch - تسک‌های شعبه
                    //tasks = _taskRepository.GetTasksByBranch(GetUserBranchId(userId));
                    break;
                case 2: // All - همه تسک‌ها
                    //tasks = _taskRepository.GetTasks();
                    break;
                default:
                    //tasks = new List<Tasks>();
                    break;
            }

            
          
            
            return View(Model);
        }

        // نمایش تسک‌های اختصاص داده شده به کاربر جاری
        [Permission("Tasks", "MyTasks", 0)] // Read permission
        public IActionResult MyTasks()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = _taskRepository.GetTasksByUser(userId, includeAssigned: true, includeCreated: false);
            var viewModels = _mapper.Map<List<TaskViewModel>>(tasks);
            
            // تکمیل اطلاعات اضافی
            foreach (var viewModel in viewModels)
            {
                var operations = _taskRepository.GetTaskOperations(viewModel.Id);
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);
            }
            
            ViewBag.Title = "تسک‌های من";
            ViewBag.IsMyTasks = true;
            
            return View("Index", viewModels);
        }

        // جزئیات تسک
        [Permission("Tasks", "Details", 0)] // Read permission
        public IActionResult Details(int id)
        {
            var task = _taskRepository.GetTaskById(id, includeOperations: true, includeAssignments: true, includeAttachments: true, includeComments: true);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<TaskViewModel>(task);
            
            // تکمیل اطلاعات عملیات‌ها
            viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(task.TaskOperations);
            
            // تکمیل اطلاعات اختصاص‌ها
            viewModel.AssignmentsTaskUser = _mapper.Map<List<TaskAssignmentViewModel>>(task.TaskAssignments);
            
            return View(viewModel);
        }

        // افزودن تسک جدید - نمایش فرم
        // در متد Create (GET)
        [HttpGet]
        [Permission("Tasks", "CreateNewTask", 1)] // Create permission
        /// <summary>
        /// AddressRoute=از مسیری که وارد این  url شدی 
        /// taskUserId چه کاربری این تسک را می سازد 
        public IActionResult CreateNewTask(string AddressRouteInComingUrl, int TaskTeamMember = 0)
        {
            if (AddressRouteInComingUrl == null)
                AddressRouteInComingUrl = "nolink";
            string LogingUser = _userManager.GetUserId(HttpContext.User);
           

            PopulateDropdowns();

            TaskViewModel Model  =  _taskRepository.CreateTaskAndCollectData(LogingUser);   
            //return View(new TaskViewModel
            //{ 
            //    IsActive = true,
            //    CreateDate = DateTime.Now
            //});
            
            return View(Model);

        }

        // در متد Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "CreateNewTask", 1)]
        public IActionResult CreateNewTask(TaskViewModel model)
        {

            List<string>      AssimentUserTask= model.AssignmentsSelectedTaskUserArraysString;
           if (model.TaskCode == null)
            {
                model.TaskCode = "0";
            }

            try
            {

                if (ModelState.IsValid)
                {
                    // ایجاد تسک جدید
                    var task = _mapper.Map<Tasks>(model);
                    task.CreateDate = DateTime.Now;
                    task.CreatorUserId = _userManager.GetUserId(User);
                    task.IsActive = model.IsActive;
                    task.IsDeleted = false;
                    task.TaskTypeInput = 1; // کاربر عادی نرم افزار ساخته
                    task.VisibilityLevel = 0; // محرمانه به طور پیش‌فرض
                    task.Priority = 0; // عادی به طور پیش‌فرض
                    task.Important = false;
                    task.Status = 0; // ایجاد شده
                    task.CreationMode = 0; // دستی
                    task.TaskCategoryId= model.TaskCategoryIdSelected;

                    // تبدیل تاریخ شمسی به میلادی
                    if (!string.IsNullOrEmpty(model.DueDatePersian))
                    {
                        task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.DueDatePersian);
                    }

                    // ذخیره در دیتابیس
                    _uow.TaskUW.Create(task);
                    _uow.Save();

                    // ذخیره فایل‌های پیوست
                    if (model.Attachments != null && model.Attachments.Count > 0)
                    {
                        SaveTaskAttachments(task.Id, model.Attachments);
                    }


                    // اختصاص به کاربر جاری (خود کاربر ایجاد کننده)
                    if (!AssimentUserTask.Contains(_userManager.GetUserId(User)))
                    {
                        // اگر کاربر جاری در لیست اختصاص‌ها نیست، آن را اضافه می‌کنیم
                        var Selfassignment = new TaskAssignment
                        {
                            TaskId = task.Id,
                            AssignedUserId = _userManager.GetUserId(User),
                            AssignerUserId = _userManager.GetUserId(User),
                            AssignmentType = 1,
                            AssignmentDate = DateTime.Now,
                            Description = "سازنده تسک" // اگر توضیحات رونوشت وجود دارد
                        };
                        _uow.TaskAssignmentUW.Create(Selfassignment);
                        _uow.Save();
                    }

                    foreach (var userId in AssimentUserTask)
                    {
                        // اختصاص تسک به کاربر
                        var assignment = new TaskAssignment
                        {
                            TaskId = task.Id,
                            AssignedUserId = userId,
                            AssignerUserId = _userManager.GetUserId(User),
                            AssignmentType = 0, // اصلی
                            AssignmentDate = DateTime.Now,
                            Description ="انتصاب تسک به کاربر", // اگر توضیحات رونوشت وجود دارد

                        };
                        _uow.TaskAssignmentUW.Create(assignment);
                        _uow.Save();    
                    }

                    TempData["SuccessMessage"] = "تسک با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                ModelState.AddModelError("", "خطایی در ثبت تسک رخ داد: " + ex.Message);
            }

            PopulateDropdowns();
            return View(model);
        }

        // ویرایش تسک - نمایش فرم
        [HttpGet]
        [Permission("Tasks", "Edit", 2)] // Edit permission
        public IActionResult Edit(int id)
        {
            var task = _taskRepository.GetTaskById(id, includeOperations: true);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<TaskViewModel>(task);
            viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(task.TaskOperations);
            
            // تبدیل تاریخ میلادی به شمسی
            if (task.DueDate.HasValue)
            {
                viewModel.DueDatePersian = ConvertDateTime.ConvertMiladiToShamsi(task.DueDate, "yyyy/MM/dd HH:mm");
            }
            
            PopulateDropdowns();
            
            return View(viewModel);
        }

        // ویرایش تسک - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "Edit", 2)] // Edit permission
        public IActionResult Edit(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {
               

                // دریافت تسک از دیتابیس
                var task = _uow.TaskUW.GetById(model.Id);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, task);
                task.LastUpdateDate = DateTime.Now;
                
                // تبدیل تاریخ شمسی به میلادی
                if (!string.IsNullOrEmpty(model.DueDatePersian))
                {
                    task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.DueDatePersian);
                }
                else
                {
                    task.DueDate = null;
                }
                
                _uow.TaskUW.Update(task);
                _uow.Save();
                
                // ذخیره فایل‌های پیوست
                if (model.Attachments != null && model.Attachments.Count > 0)
                {
                    SaveTaskAttachments(task.Id, model.Attachments);
                }

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            
            PopulateDropdowns();
            return View(model);
        }

        // فعال/غیرفعال کردن تسک - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleActiveStatus(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            if (task.IsActive)
            {
                // غیرفعال کردن
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن تسک";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                // فعال کردن
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن تسک";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActiveStatus", _mapper.Map<TaskViewModel>(task));
        }

        // فعال/غیرفعال کردن تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActiveStatusPost(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            task.IsActive = !task.IsActive;
            task.LastUpdateDate = DateTime.Now;
            
            _uow.TaskUW.Update(task);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // حذف تسک - نمایش مودال تأیید
        [HttpGet]
        [Permission("Tasks", "Delete", 3)] // Delete permission
        public IActionResult Delete(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف تسک";

            return PartialView("_DeleteTask", _mapper.Map<TaskViewModel>(task));
        }

        // حذف تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Tasks", "Delete", 3)] // Delete permission
        public IActionResult DeletePost(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            task.IsDeleted = true;
            task.LastUpdateDate = DateTime.Now;
            
            _uow.TaskUW.Update(task);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // افزودن عملیات به تسک - نمایش مودال
        [HttpGet]
        public IActionResult AddOperation(int taskId)
        {
            var task = _uow.TaskUW.GetById(taskId);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            // تعیین ترتیب پیش‌فرض برای عملیات جدید
            var operations = _taskRepository.GetTaskOperations(taskId);
            int nextOrder = operations.Count > 0 ? operations.Max(o => o.OperationOrder) + 1 : 1;

            ViewBag.TaskId = taskId;
            ViewBag.TaskTitle = task.Title;
            ViewBag.OperationOrder = nextOrder;

            return PartialView("_AddOperation", new TaskOperationViewModel
            {
                TaskId = taskId,
                OperationOrder = nextOrder,
                IsCompleted = false
            });
        }

        // افزودن عملیات به تسک - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOperation(TaskOperationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = _uow.TaskUW.GetById(model.TaskId);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");

                var operation = _mapper.Map<TaskOperation>(model);
                operation.CreateDate = DateTime.Now;
                operation.CreatorUserId = _userManager.GetUserId(User);

                _uow.TaskOperationUW.Create(operation);
                _uow.Save();

                return RedirectToAction(nameof(Details), new { id = model.TaskId });
            }

            var currentTask = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = currentTask.Title;
            ViewBag.OperationOrder = model.OperationOrder;

            return PartialView("_AddOperation", model);
        }

        // ویرایش عملیات تسک - نمایش مودال
        [HttpGet]
        public IActionResult EditOperation(int id)
        {
            var operation = _taskRepository.GetTaskOperationById(id);
            if (operation == null)
                return RedirectToAction("ErrorView", "Home");

            var task = _uow.TaskUW.GetById(operation.TaskId);
            ViewBag.TaskId = operation.TaskId;
            ViewBag.TaskTitle = task.Title;

            return PartialView("_EditOperation", _mapper.Map<TaskOperationViewModel>(operation));
        }

        // ویرایش عملیات تسک - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditOperation(TaskOperationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var operation = _uow.TaskOperationUW.GetById(model.Id);
                if (operation == null)
                    return RedirectToAction("ErrorView", "Home");

                // اگر وضعیت تکمیل تغییر کرده است
                bool completionChanged = operation.IsCompleted != model.IsCompleted;
                
                // به‌روزرسانی اطلاعات
                _mapper.Map(model, operation);
                
                // اگر عملیات به تازگی تکمیل شده است
                if (completionChanged && model.IsCompleted)
                {
                    operation.CompletionDate = DateTime.Now;
                    operation.CompletedByUserId = _userManager.GetUserId(User);
                }
                // اگر عملیات از حالت تکمیل خارج شده است
                else if (completionChanged && !model.IsCompleted)
                {
                    operation.CompletionDate = null;
                    operation.CompletedByUserId = null;
                }

                _uow.TaskOperationUW.Update(operation);
                _uow.Save();
                
                // بررسی اگر همه عملیات‌های تسک تکمیل شده‌اند، تسک را تکمیل کنیم
                UpdateTaskCompletionStatus(operation.TaskId);

                return RedirectToAction(nameof(Details), new { id = operation.TaskId });
            }

            var task = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = task.Title;

            return PartialView("_EditOperation", model);
        }

        // حذف عملیات تسک - نمایش مودال تأیید
        [HttpGet]
        public IActionResult DeleteOperation(int id)
        {
            var operation = _taskRepository.GetTaskOperationById(id);
            if (operation == null)
                return RedirectToAction("ErrorView", "Home");

            var task = _uow.TaskUW.GetById(operation.TaskId);
            ViewBag.TaskId = operation.TaskId;
            ViewBag.TaskTitle = task.Title;
            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";

            return PartialView("_DeleteOperation", _mapper.Map<TaskOperationViewModel>(operation));
        }

        // حذف عملیات تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOperationPost(int id)
        {
            var operation = _uow.TaskOperationUW.GetById(id);
            if (operation == null)
                return RedirectToAction("ErrorView", "Home");

            int taskId = operation.TaskId;

            _uow.TaskOperationUW.Delete(operation);
            _uow.Save();
            
            // بازتعیین ترتیب عملیات‌ها
            ReorderOperations(taskId);
            
            // بررسی وضعیت تکمیل تسک
            UpdateTaskCompletionStatus(taskId);

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // تغییر وضعیت تکمیل عملیات - اکشن AJAX
        [HttpPost]
        public IActionResult ToggleOperationCompletion(int id)
        {
            var operation = _uow.TaskOperationUW.GetById(id);
            if (operation == null)
                return Json(new { success = false, message = "عملیات یافت نشد" });

            operation.IsCompleted = !operation.IsCompleted;
            
            if (operation.IsCompleted)
            {
                operation.CompletionDate = DateTime.Now;
                operation.CompletedByUserId = _userManager.GetUserId(User);
            }
            else
            {
                operation.CompletionDate = null;
                operation.CompletedByUserId = null;
            }

            _uow.TaskOperationUW.Update(operation);
            _uow.Save();
            
            // بررسی وضعیت تکمیل تسک
            UpdateTaskCompletionStatus(operation.TaskId);

            return Json(new { 
                success = true, 
                isCompleted = operation.IsCompleted,
                completionDate = operation.CompletionDate != null ? ConvertDateTime.ConvertMiladiToShamsi(operation.CompletionDate, "yyyy/MM/dd") : null
            });
        }

        // اختصاص تسک به کاربر - نمایش مودال
        [HttpGet]
        public IActionResult AssignTask(int taskId)
        {
            var task = _uow.TaskUW.GetById(taskId);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.TaskId = taskId;
            ViewBag.TaskTitle = task.Title;
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            return PartialView("_AssignTask", new TaskAssignmentViewModel
            {
                TaskId = taskId,
                AssignDate = DateTime.Now
            });
        }

        // اختصاص تسک به کاربر - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignTask(TaskAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = _uow.TaskUW.GetById(model.TaskId);
                if (task == null)
                    return RedirectToAction("ErrorView", "Home");
                
                // بررسی آیا این کاربر قبلاً به این تسک اختصاص داده شده است
                var existingAssignment = _taskRepository.GetTaskAssignmentByUserAndTask(model.AssignedUserId, model.TaskId);
                if (existingAssignment != null)
                {
                    ModelState.AddModelError("AssignedUserId", "این کاربر قبلاً به این تسک اختصاص داده شده است");
                    ViewBag.TaskId = model.TaskId;
                    ViewBag.TaskTitle = task.Title;
                    ViewBag.Users = new SelectList(_userManager.Users
                        .Where(u => u.IsActive && !u.IsRemoveUser)
                        .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                        "Id", "FullName");
                    return PartialView("_AssignTask", model);
                }

                var assignment = _mapper.Map<TaskAssignment>(model);
                assignment.AssignmentDate = DateTime.Now;
                assignment.AssignerUserId = _userManager.GetUserId(User);

                _uow.TaskAssignmentUW.Create(assignment);
                _uow.Save();

                return RedirectToAction(nameof(Details), new { id = model.TaskId });
            }

            var currentTask = _uow.TaskUW.GetById(model.TaskId);
            ViewBag.TaskId = model.TaskId;
            ViewBag.TaskTitle = currentTask.Title;
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            return PartialView("_AssignTask", model);
        }

        // حذف اختصاص تسک - نمایش مودال تأیید
        [HttpGet]
        public IActionResult RemoveAssignment(int id)
        {
            var assignment = _taskRepository.GetTaskAssignmentById(id);
            if (assignment == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.TaskId = assignment.TaskId;
            ViewBag.TaskTitle = assignment.Task.Title;
            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";

            return PartialView("_RemoveAssignment", _mapper.Map<TaskAssignmentViewModel>(assignment));
        }

        // حذف اختصاص تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveAssignmentPost(int id)
        {
            var assignment = _uow.TaskAssignmentUW.GetById(id);
            if (assignment == null)
                return RedirectToAction("ErrorView", "Home");

            int taskId = assignment.TaskId;

            _uow.TaskAssignmentUW.Delete(assignment);
            _uow.Save();

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // تکمیل تسک - نمایش مودال
        [HttpGet]
        public IActionResult CompleteTask(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            if (task.CompletionDate.HasValue)
            {
                ViewBag.ModalTitle = "بازگشایی تسک";
                ViewBag.ButtonText = "بازگشایی تسک";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-warning";
                ViewBag.themeclass = "bg-gd-sun";
                ViewBag.IsReopening = true;
            }
            else
            {
                ViewBag.ModalTitle = "تکمیل تسک";
                ViewBag.ButtonText = "تکمیل تسک";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
                ViewBag.themeclass = "bg-gd-sea";
                ViewBag.IsReopening = false;
            }

            return PartialView("_CompleteTask", _mapper.Map<TaskViewModel>(task));
        }

        // تکمیل تسک - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteTaskPost(int id, bool isReopening)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return RedirectToAction("ErrorView", "Home");

            if (isReopening)
            {
                task.CompletionDate = null;
                task.SupervisorApprovedDate = null;
                task.ManagerApprovedDate = null;
            }
            else
            {
                task.CompletionDate = DateTime.Now;
                
                // اگر کاربر جاری سرپرست یا مدیر است، تایید متناظر را هم ثبت کنیم
                var currentUser = _userManager.GetUserAsync(User).Result;
                var isManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                var isSupervisor = User.IsInRole("Supervisor") || isManager;
                
                if (isSupervisor)
                {
                    task.SupervisorApprovedDate = DateTime.Now;
                }
                
                if (isManager)
                {
                    task.ManagerApprovedDate = DateTime.Now;
                }
                
                // همه عملیات‌ها را به حالت تکمیل شده تغییر دهیم
                var operations = _taskRepository.GetTaskOperations(id);
                foreach (var operation in operations)
                {
                    if (!operation.IsCompleted)
                    {
                        operation.IsCompleted = true;
                        operation.CompletionDate = DateTime.Now;
                        operation.CompletedByUserId = _userManager.GetUserId(User);
                        
                        _uow.TaskOperationUW.Update(operation);
                    }
                }
            }
            
            task.LastUpdateDate = DateTime.Now;
            _uow.TaskUW.Update(task);
            _uow.Save();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // تایید تسک توسط سرپرست - اکشن AJAX
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Supervisor")]
        [Permission("Tasks  ", "ApproveTaskBySupervisor", 4)] // Approve permission
        public IActionResult ApproveTaskBySupervisor(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return Json(new { success = false, message = "تسک یافت نشد" });
                
            if (!task.CompletionDate.HasValue)
                return Json(new { success = false, message = "این تسک هنوز تکمیل نشده است" });

            task.SupervisorApprovedDate = DateTime.Now;
            task.LastUpdateDate = DateTime.Now;
            
            _uow.TaskUW.Update(task);
            _uow.Save();

            return Json(new { 
                success = true, 
                approvalDate = ConvertDateTime.ConvertMiladiToShamsi(task.SupervisorApprovedDate,"yyyy/MM/dd")
            });
        }

        // تایید تسک توسط مدیر - اکشن AJAX
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [Permission("Tasks  ", "ApproveTaskByManager", 4)] // Approve permission
        public IActionResult ApproveTaskByManager(int id)
        {
            var task = _uow.TaskUW.GetById(id);
            if (task == null)
                return Json(new { success = false, message = "تسک یافت نشد" });
                
            if (!task.CompletionDate.HasValue)
                return Json(new { success = false, message = "این تسک هنوز تکمیل نشده است" });
                
            if (!task.SupervisorApprovedDate.HasValue)
                return Json(new { success = false, message = "این تسک هنوز توسط سرپرست تایید نشده است" });

            task.ManagerApprovedDate = DateTime.Now;
            task.LastUpdateDate = DateTime.Now;
            
            _uow.TaskUW.Update(task);
            _uow.Save();

            return Json(new { 
                success = true, 
                approvalDate = ConvertDateTime.ConvertMiladiToShamsi(task.ManagerApprovedDate, "yyyy/MM/dd")
            });
        }

        // جستجوی پیشرفته - نمایش فرم
        [HttpGet]
        public IActionResult AdvancedSearch()
        {
            ViewBag.Categories = new SelectList(_taskRepository.GetAllCategories(), "Id", "Title");
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
                
            ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                "Id", "FullName");

            return PartialView("_AdvancedSearch", new TaskSearchViewModel());
        }

        // جستجوی پیشرفته - پردازش جستجو
        [HttpPost]
        public IActionResult Search(TaskSearchViewModel model)
        {
            // انجام جستجو بر اساس پارامترهای دریافتی
            var tasks = _taskRepository.SearchTasks(
                model.SearchTerm, 
                model.CategoryId, 
                model.AssignedUserId,
                model.IsCompleted);
                
            var viewModels = _mapper.Map<List<TaskViewModel>>(tasks);
            
            // تکمیل اطلاعات اضافی
            foreach (var viewModel in viewModels)
            {
                var operations = _taskRepository.GetTaskOperations(viewModel.Id);
                viewModel.Operations = _mapper.Map<List<TaskOperationViewModel>>(operations);
            }

            // ذخیره پارامترهای جستجو در ViewBag برای استفاده در صفحه نتایج
            ViewBag.SearchModel = model;
            ViewBag.Title = "نتایج جستجو";

            return View("SearchResults", viewModels);
        }

        // توابع کمکی
        
        // پر کردن لیست‌های کشویی
        private void PopulateDropdowns()
        {
            ViewBag.Categories = new SelectList(_taskRepository.GetAllCategories(), "Id", "Title");
            ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                "Id", "FullName");
        }
        
        // ذخیره فایل‌های پیوست
        private void SaveTaskAttachments(int taskId, List<IFormFile> files)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks", taskId.ToString());
            
            // ایجاد پوشه اگر وجود ندارد
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // نام فایل یکتا
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    
                    // ذخیره اطلاعات فایل در دیتابیس
                    var attachment = new TaskAttachment
                    {
                        TaskId = taskId,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        FilePath = $"/uploads/tasks/{taskId}/{uniqueFileName}",
                        FileType = file.ContentType,
                        UploadDate = DateTime.Now,
                        UploaderUserId = _userManager.GetUserId(User)
                    };
                    
                    _uow.TaskAttachmentUW.Create(attachment);
                }
            }
            
            _uow.Save();
        }
        
        // بازتعیین ترتیب عملیات‌ها
        private void ReorderOperations(int taskId)
        {
            var operations = _taskRepository.GetTaskOperations(taskId)
                .OrderBy(o => o.OperationOrder)
                .ToList();
                
            for (int i = 0; i < operations.Count; i++)
            {
                operations[i].OperationOrder = i + 1;
                _uow.TaskOperationUW.Update(operations[i]);
            }
            
            _uow.Save();
        }
        
        // بررسی و به‌روزرسانی وضعیت تکمیل تسک
        private void UpdateTaskCompletionStatus(int taskId)
        {
            var task = _uow.TaskUW.GetById(taskId);
            var operations = _taskRepository.GetTaskOperations(taskId);
            
            // اگر عملیاتی وجود ندارد، کاری انجام نمی‌دهیم
            if (!operations.Any())
                return;
                
            // اگر همه عملیات‌ها تکمیل شده‌اند، تسک را تکمیل کنیم
            bool allCompleted = operations.All(o => o.IsCompleted);
            
            if (allCompleted && !task.CompletionDate.HasValue)
            {
                task.CompletionDate = DateTime.Now;
                task.LastUpdateDate = DateTime.Now;
                _uow.TaskUW.Update(task);
                _uow.Save();
            }
            else if (!allCompleted && task.CompletionDate.HasValue)
            {
                task.CompletionDate = null;
                task.SupervisorApprovedDate = null;
                task.ManagerApprovedDate = null;
                task.LastUpdateDate = DateTime.Now;
                _uow.TaskUW.Update(task);
                _uow.Save();
            }
        }

        // متد کمکی برای دریافت شعبه کاربر
        private int GetUserBranchId(string userId)
        {
            var branchUser = _uow.BranchUserUW.Get(bu => bu.UserId == userId && bu.IsActive).FirstOrDefault();
            return branchUser?.BranchId ?? 1; // پیش‌فرض شعبه اصلی
        }
    }
}