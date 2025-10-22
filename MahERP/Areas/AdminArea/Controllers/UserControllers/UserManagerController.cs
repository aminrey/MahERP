using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    //[PermissionRequired("CORE.USER")]
    public class UserManagerController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IMapper _Mapper;
        private readonly IUserManagerRepository _UserRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserManagerController(
            IUnitOfWork context, 
            UserManager<AppUsers> userManager, 
            IMapper Mapper,
            IUserManagerRepository userrepository, // تصحیح نوع پارامتر
            PersianDateHelper persianDateHelper, 
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger, IWebHostEnvironment webHostEnvironment)
            : base(context, userManager, persianDateHelper, memoryCache, activityLogger , userrepository)
        {
            _Context = context;
            _UserManager = userManager;
            _Mapper = Mapper;
            _UserRepository = userrepository; // ✅ اصلاح شده
            _webHostEnvironment = webHostEnvironment;

        }

        /// <summary>
        /// صفحه اصلی لیست کاربران - نمایش کاربران فعال و امکان مشاهده کاربران بایگانی برای ادمین
        /// </summary>
        /// <param name="showArchived">نمایش کاربران بایگانی شده</param>
        /// <returns>لیست کاربران</returns>
        public async Task<IActionResult> Index(bool showArchived = false)
        {
            try
            {
                // بررسی اینکه کاربر جاری ادمین است یا نه
                var currentUser = await _UserManager.GetUserAsync(User);
                bool isCurrentUserAdmin = currentUser?.IsAdmin == true;

                List<AppUsers> model;

                if (showArchived && isCurrentUserAdmin)
                {
                    // نمایش کاربران بایگانی شده فقط برای ادمین
                    model = _Context.UserManagerUW.Get()
                        .Where(c => c.IsAdmin && c.IsRemoveUser && !c.IsCompletelyDeleted)
                        .ToList();
                }
                else
                {
                    // نمایش کاربران فعال
                    model = _Context.UserManagerUW.Get()
                        .Where(c => c.IsAdmin && c.IsActive && !c.IsRemoveUser && !c.IsCompletelyDeleted)
                        .ToList();
                }

                // ارسال اطلاعات برای نمایش در ویو
                ViewBag.IsCurrentUserAdmin = isCurrentUserAdmin;
                ViewBag.ShowArchived = showArchived;
                ViewBag.ArchivedUsersCount = isCurrentUserAdmin ? 
                    _Context.UserManagerUW.Get().Count(c => c.IsAdmin && c.IsRemoveUser && !c.IsCompletelyDeleted) : 0;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "Index",
                    showArchived ? "مشاهده لیست کاربران بایگانی شده" : "مشاهده لیست کاربران"
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "Index",
                    "خطا در دریافت لیست کاربران",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// صفحه لیست کاربران غیر ادمین - نمایش کاربران عادی سیستم
        /// </summary>
        /// <param name="showArchived">نمایش کاربران بایگانی شده</param>
        /// <returns>لیست کاربران غیر ادمین</returns>
        public async Task<IActionResult> Users(bool showArchived = false)
        {
            try
            {
                // بررسی اینکه کاربر جاری ادمین است یا نه
                var currentUser = await _UserManager.GetUserAsync(User);
                bool isCurrentUserAdmin = currentUser?.IsAdmin == true;

                List<AppUsers> model;

                if (showArchived && isCurrentUserAdmin)
                {
                    // نمایش کاربران غیر ادمین بایگانی شده فقط برای ادمین
                    model = _Context.UserManagerUW.Get()
                        .Where(c => !c.IsAdmin && c.IsRemoveUser && !c.IsCompletelyDeleted)
                        .ToList();
                }
                else
                {
                    // نمایش کاربران غیر ادمین فعال
                    model = _Context.UserManagerUW.Get()
                        .Where(c => !c.IsAdmin && c.IsActive && !c.IsRemoveUser && !c.IsCompletelyDeleted)
                        .ToList();
                }

                // ارسال اطلاعات برای نمایش در ویو
                ViewBag.IsCurrentUserAdmin = isCurrentUserAdmin;
                ViewBag.ShowArchived = showArchived;
                ViewBag.ArchivedUsersCount = isCurrentUserAdmin ?
                    _Context.UserManagerUW.Get().Count(c => !c.IsAdmin && c.IsRemoveUser && !c.IsCompletelyDeleted) : 0;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "Users",
                    showArchived ? "مشاهده لیست کاربران غیر ادمین بایگانی شده" : "مشاهده لیست کاربران غیر ادمین"
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "Users",
                    "خطا در دریافت لیست کاربران غیر ادمین",
                    ex
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }
        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            try
            {
                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "AddUser",
                    "مشاهده فرم افزودن کاربر جدید"
                );

                return View();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "AddUser",
                    "خطا در نمایش فرم افزودن کاربر",
                    ex
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی وجود یوزرنیم (حتی در کاربران حذف شده کامل)
                    bool usernameExists = await _UserRepository.IsUsernameExistsAsync(model.UserName);
                    if (usernameExists)
                    {
                        ModelState.AddModelError("UserName", "فردی با این یوزرنیم وجود دارد. برای استفاده از این یوزرنیم باید کاربر قبلی را بطور کامل حذف کنید.");
                        return View(model);
                    }
                    
                    var userMapped = _Mapper.Map<AppUsers>(model);
                    userMapped.IsAdmin = true;
                    userMapped.IsActive = true;
                    userMapped.IsRemoveUser = false;
                    userMapped.IsCompletelyDeleted = false;
                    userMapped.RegisterDate = DateTime.Now;
                    
                    IdentityResult result = await _UserManager.CreateAsync(userMapped, "Admin1234@");
                    if (result.Succeeded)
                    {
                        // ثبت لاگ موفقیت
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "UserManager",
                            "AddUser",
                            $"ایجاد کاربر جدید: {userMapped.FirstName} {userMapped.LastName} ({userMapped.UserName})",
                            recordId: userMapped.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{userMapped.FirstName} {userMapped.LastName}"
                        );

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // ثبت لاگ خطا
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "UserManager",
                            "AddUser",
                            $"شکست در ایجاد کاربر: {model.UserName} - خطاها: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                        );
                        
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "AddUser",
                        "خطا در ایجاد کاربر جدید",
                        ex
                    );
                    
                    ModelState.AddModelError("", "خطایی در ایجاد کاربر رخ داد: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string UserId)
        {
            try
            {
                if (UserId == null)
                {
                    return RedirectToAction("ErrorView", "Home");
                }
                
                var user = _Context.UserManagerUW.GetById(UserId);
                if (user == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "EditUser",
                        "تلاش برای ویرایش کاربر غیرموجود",
                        recordId: UserId
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var mapUser = _Mapper.Map<EditUserViewModel>(user);

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "EditUser",
                    $"مشاهده فرم ویرایش کاربر: {user.FirstName} {user.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{user.FirstName} {user.LastName}"
                );

                return View(mapUser);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "EditUser",
                    "خطا در نمایش فرم ویرایش کاربر",
                    ex,
                    recordId: UserId
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // دریافت کاربر قبل از تغییر برای لاگ
                    var originalUser = await _UserManager.FindByIdAsync(model.Id);
                    if (originalUser == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "EditUser",
                            "تلاش برای ویرایش کاربر غیرموجود",
                            recordId: model.Id
                        );
                        return RedirectToAction("ErrorView", "Home");
                    }

                    // ذخیره مقادیر قبلی برای لاگ
                    var oldValues = new
                    {
                        originalUser.FirstName,
                        originalUser.LastName,
                        originalUser.Email,
                        originalUser.PhoneNumber,
                        originalUser.CompanyName,
                        originalUser.PositionName
                    };

                    // به‌روزرسانی کاربر
                    var user = await _UserManager.FindByIdAsync(model.Id);
                    IdentityResult result = await _UserManager.UpdateAsync(_Mapper.Map(model, user));
                    
                    if (result.Succeeded)
                    {
                        // مقادیر جدید برای لاگ
                        var newValues = new
                        {
                            user.FirstName,
                            user.LastName,
                            user.Email,
                            user.PhoneNumber,
                            user.CompanyName,
                            user.PositionName
                        };

                        // ثبت لاگ تغییرات - تصحیح خطای type inference
                        await _activityLogger.LogChangeAsync<object>(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "EditUser",
                            $"ویرایش کاربر: {user.FirstName} {user.LastName}",
                            oldValues,
                            newValues,
                            recordId: model.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{user.FirstName} {user.LastName}"
                        );

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "EditUser",
                            $"شکست در ویرایش کاربر: {originalUser.UserName} - خطاها: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                            recordId: model.Id
                        );
                        
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "EditUser",
                        "خطا در ویرایش کاربر",
                        ex,
                        recordId: model.Id
                    );
                    
                    ModelState.AddModelError("", "خطایی در ویرایش کاربر رخ داد: " + ex.Message);
                }
            }
            return View(model);
        }

        /// <summary>
        /// نمایش مودال حذف کامل کاربر
        /// </summary>
        /// <param name="UserId">شناسه کاربر</param>
        /// <returns>مودال حذف کامل</returns>
        [HttpGet]
        public async Task<IActionResult> CompletelyDeleteUser(string UserId)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "CompletelyDeleteUser",
                        "تلاش برای نمایش مودال حذف کامل کاربر بدون شناسه"
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                // دریافت اطلاعات کاربر
                var user = _Context.UserManagerUW.GetById(UserId);
                if (user == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "CompletelyDeleteUser",
                        "تلاش برای نمایش مودال حذف کامل کاربر غیرموجود",
                        recordId: UserId
                    );
                    return NotFound();
                }

                // ایجاد ViewModel برای نمایش در مودال
                var userViewModel = new UserViewModelFull
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullNamesString = $"{user.FirstName} {user.LastName}",
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };

                // ثبت لاگ نمایش مودال حذف کامل
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "CompletelyDeleteUser",
                    $"نمایش مودال حذف کامل کاربر: {user.FirstName} {user.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{user.FirstName} {user.LastName}"
                );

                return PartialView("_CompletelyDeleteUser", userViewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "CompletelyDeleteUser",
                    "خطا در نمایش مودال حذف کامل کاربر",
                    ex,
                    recordId: UserId
                );
                
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }

        /// <summary>
        /// پردازش حذف کامل کاربر
        /// </summary>
        /// <param name="Id">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletelyDeleteUserPost(string Id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی وجود کاربر
                    var user = _Context.UserManagerUW.GetById(Id);
                    if (user == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Delete,
                            "UserManager",
                            "CompletelyDeleteUser",
                            "تلاش برای حذف کامل کاربر غیرموجود",
                            recordId: Id
                        );
                        return Json(new { status = "error", message = "کاربر یافت نشد" });
                    }

                    var userName = $"{user.FirstName} {user.LastName}";
                    var userInfo = new
                    {
                        user.Id,
                        user.UserName,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.IsActive,
                        user.RegisterDate
                    };
                    
                    // حذف کامل کاربر با استفاده از Repository
                    bool result = await _UserRepository.CompletelyDeleteUserAsync(Id);
                    
                    if (!result)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Delete,
                            "UserManager",
                            "CompletelyDeleteUser",
                            $"شکست در حذف کامل کاربر: {userName}",
                            recordId: Id,
                            entityType: "AppUsers",
                            recordTitle: userName
                        );
                        return Json(new { status = "error", message = "خطا در حذف کامل کاربر" });
                    }

                    // ثبت لاگ حذف کامل موفق
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "UserManager",
                        "CompletelyDeleteUser",
                        $"حذف کامل کاربر: {userName} - تمام اطلاعات کاربر از سیستم حذف شد",
                        recordId: Id,
                        entityType: "AppUsers",
                        recordTitle: userName
                    );

                    // ثبت لاگ اطلاعات حذف شده برای بررسی‌های آینده - تصحیح خطای type inference
                    await _activityLogger.LogChangeAsync<object>(
                        ActivityTypeEnum.Delete,
                        "UserManager",
                        "CompletelyDeleteUser",
                        $"جزئیات کاربر حذف شده: {userName}",
                        userInfo,
                        new { Status = "Completely Deleted", DeletedAt = DateTime.Now },
                        recordId: Id,
                        entityType: "AppUsers",
                        recordTitle: userName
                    );

                    return Json(new { status = "redirect", redirectUrl = Url.Action(nameof(Index)) });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "CompletelyDeleteUser",
                        "خطا در حذف کامل کاربر",
                        ex,
                        recordId: Id
                    );
                    
                    return Json(new { status = "error", message = "خطا در حذف کامل کاربر" });
                }
            }

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.Delete,
                "UserManager",
                "CompletelyDeleteUser",
                "تلاش برای حذف کامل کاربر با داده‌های نامعتبر",
                recordId: Id
            );

            return BadRequest(ModelState);
        }

        /// <summary>
        /// به‌روزرسانی RemoveUser برای نمایش مودال بایگانی با ثبت لاگ کامل
        /// </summary>
        /// <param name="UserId">شناسه کاربر</param>
        /// <returns>مودال بایگانی</returns>
        [HttpGet]
        public async Task<IActionResult> RemoveUser(string UserId)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "RemoveUser",
                        "تلاش برای نمایش مودال بایگانی کاربر بدون شناسه"
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var model = _Context.UserManagerUW.GetById(UserId);
                if (model == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "RemoveUser",
                        "تلاش برای نمایش مودال بایگانی کاربر غیرموجود",
                        recordId: UserId
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                // ایجاد ViewModel برای نمایش در مودال
                var userViewModel = new UserViewModelFull
                {
                    Id = model.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FullNamesString = $"{model.FirstName} {model.LastName}",
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                // ثبت لاگ نمایش مودال بایگانی
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "RemoveUser",
                    $"نمایش مودال بایگانی کاربر: {model.FirstName} {model.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{model.FirstName} {model.LastName}"
                );

                return PartialView("_RemoveUser", userViewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "RemoveUser",
                    "خطا در نمایش مودال بایگانی کاربر",
                    ex,
                    recordId: UserId
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserPost(string Id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _Context.UserManagerUW.GetById(Id);
                    if (user == null)
                        return Json(new { status = "error", message = "کاربر یافت نشد" });

                    var userName = $"{user.FirstName} {user.LastName}";
                    
                    // بایگانی کاربر به جای حذف
                    bool result = await _UserRepository.ArchiveUserAsync(Id);
                    
                    if (!result)
                        return Json(new { status = "error", message = "خطا در بایگانی کاربر" });

                    // ثبت لاگ بایگانی - استفاده از Edit به جای Update
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "UserManager",
                        "ArchiveUser",
                        $"بایگانی کاربر: {userName}",
                        recordId: Id,
                        entityType: "AppUsers",
                        recordTitle: userName
                    );

                    return Json(new { status = "redirect", redirectUrl = Url.Action(nameof(Index)) });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "ArchiveUser",
                        "خطا در بایگانی کاربر",
                        ex,
                        recordId: Id
                    );
                    
                    return Json(new { status = "error", message = "خطا در بایگانی کاربر" });
                }
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// بازیابی کاربر بایگانی شده
        /// </summary>
        /// <param name="UserId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(string UserId)
        {
            try
            {
                var user = _Context.UserManagerUW.GetById(UserId);
                if (user == null)
                    return Json(new { status = "error", message = "کاربر یافت نشد" });

                if (!user.IsRemoveUser)
                    return Json(new { status = "error", message = "کاربر بایگانی نشده است" });

                // بازیابی کاربر
                user.IsRemoveUser = false;
                user.IsActive = true;
                user.ArchivedDate = null;

                _Context.UserManagerUW.Update(user);
                _Context.Save();

                var userName = $"{user.FirstName} {user.LastName}";

                // ثبت لاگ بازیابی - استفاده از Edit به جای Update
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "UserManager",
                    "RestoreUser",
                    $"بازیابی کاربر بایگانی شده: {userName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: userName
                );

                return Json(new { status = "redirect", redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "RestoreUser",
                    "خطا در بازیابی کاربر",
                    ex,
                    recordId: UserId
                );
                
                return Json(new { status = "error", message = "خطا در بازیابی کاربر" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePasswordByAdmin(string UserId)
        {
            try
            {
                if (UserId == null)
                {
                    return RedirectToAction("ErrorView", "Home");
                }
                
                var User = _Context.UserManagerUW.GetById(UserId);
                if (User == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.userId = UserId;
                ViewBag.FullName = User.FirstName + " " + User.LastName;

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "ChangePasswordByAdmin",
                    $"مشاهده فرم تغییر رمز عبور کاربر: {User.FirstName} {User.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{User.FirstName} {User.LastName}"
                );

                return PartialView("_ChangePasswordByAdmin");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "ChangePasswordByAdmin",
                    "خطا در نمایش فرم تغییر رمز عبور",
                    ex,
                    recordId: UserId
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassByAdmin(ChangePasswordByAdminViewModel model)
        {
            try
            {
                var user = _Context.UserManagerUW.Get(u => u.Id == model.userId).FirstOrDefault();
                if (user == null)
                    return Json(new { status = "error", message = "کاربر یافت نشد" });

                user.PasswordHash = _UserManager.PasswordHasher.HashPassword(user, model.NewPassword);
                _Context.Save();

                // ثبت لاگ تغییر رمز عبور
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "UserManager",
                    "ChangePasswordByAdmin",
                    $"تغییر رمز عبور کاربر توسط مدیر: {user.FirstName} {user.LastName}",
                    recordId: model.userId,
                    entityType: "AppUsers",
                    recordTitle: $"{user.FirstName} {user.LastName}"
                );

                return Json(new { status = "ok" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "ChangePasswordByAdmin",
                    "خطا در تغییر رمز عبور کاربر",
                    ex,
                    recordId: model.userId
                );
                
                return Json(new { status = "error", message = "خطا در تغییر رمز عبور" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActiveOrDeactiveUser(string UserId)
        {
            try
            {
                var User = _Context.UserManagerUW.GetById(UserId);
                if (User == null)
                    return RedirectToAction("ErrorView", "Home");

                if (User.IsActive == true)
                {
                    ViewBag.theme = "bg-danger";
                    ViewBag.ViewTitle = "غیرفعال کردن کاربر";
                }
                else
                {
                    ViewBag.theme = "bg-success";
                    ViewBag.ViewTitle = "فعال کردن کاربر";
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "ActiveOrDeactiveUser",
                    $"مشاهده فرم تغییر وضعیت کاربر: {User.FirstName} {User.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{User.FirstName} {User.LastName}"
                );

                return PartialView("_ActiveOrDeactiveUser", User);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "ActiveOrDeactiveUser",
                    "خطا در نمایش فرم تغییر وضعیت کاربر",
                    ex,
                    recordId: UserId
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActiveOrDeactiveUser(string Id, bool IsActive)
        {
            try
            {
                var User = _Context.UserManagerUW.GetById(Id);
                if (User == null)
                    return RedirectToAction("ErrorView", "Home");

                var oldStatus = User.IsActive;
                
                if (IsActive == true)
                {
                    User.IsActive = false;
                }
                else
                {
                    User.IsActive = true;
                }
                
                _Context.UserManagerUW.Update(User);
                _Context.Save();

                // ثبت لاگ تغییر وضعیت - تصحیح خطای type inference
                await _activityLogger.LogChangeAsync<object>(
                    ActivityTypeEnum.Edit,
                    "UserManager",
                    "ActiveOrDeactiveUser",
                    $"تغییر وضعیت کاربر: {User.FirstName} {User.LastName} از {(oldStatus ? "فعال" : "غیرفعال")} به {(User.IsActive ? "فعال" : "غیرفعال")}",
                    new { IsActive = oldStatus },
                    new { IsActive = User.IsActive },
                    recordId: Id,
                    entityType: "AppUsers",
                    recordTitle: $"{User.FirstName} {User.LastName}"
                );

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "ActiveOrDeactiveUser",
                    "خطا در تغییر وضعیت کاربر",
                    ex,
                    recordId: Id
                );
                
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// بررسی دسترسی یوزرنیم جدید
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="userId">شناسه کاربر جهت استثنا (برای ویرایش)</param>
        /// <returns>نتیجه بررسی</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckUsernameAvailability(string username, string userId = null)
        {
            try
            {
                bool exists = await _UserRepository.IsUsernameExistsAsync(username, userId);
                
                if (exists)
                {
                    return Json(new { 
                        status = "exists", 
                        message = "فردی با این یوزرنیم وجود دارد. برای استفاده از این یوزرنیم باید کاربر قبلی را بطور کامل حذف کنید.",
                        showCompleteDeleteOption = true
                    });
                }
                
                return Json(new { status = "available", message = "یوزرنیم قابل استفاده است" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "CheckUsernameAvailability",
                    "خطا در بررسی یوزرنیم",
                    ex
                );
                
                return Json(new { status = "error", message = "خطا در بررسی یوزرنیم" });
            }
        }
        /// <summary>
        /// صفحه پروفایل کاربر جاری - مشاهده و ویرایش اطلاعات شخصی
        /// </summary>
        /// <returns>صفحه پروفایل کاربر</returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var currentUser = await _UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "Profile",
                        "تلاش برای مشاهده پروفایل کاربر غیرموجود"
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                // دریافت اطلاعات کامل کاربر
                var userProfile = _UserRepository.GetUserInfoData(currentUser.Id);
                if (userProfile == null)
                {
                    userProfile = new UserViewModelFull
                    {
                        Id = currentUser.Id,
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        Email = currentUser.Email,
                        PhoneNumber = currentUser.PhoneNumber,
                        UserName = currentUser.UserName,
                        CompanyName = currentUser.CompanyName,
                        PositionName = currentUser.PositionName,
                        City = currentUser.City,
                        Province = currentUser.Province,
                        Address = currentUser.Address,
                        MelliCode = currentUser.MelliCode,
                        Gender = currentUser.Gender,
                        IsActive = currentUser.IsActive,
                        ProfileImagePath = currentUser.ProfileImagePath,
                        RegisterDate = currentUser.RegisterDate
                    };
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "Profile",
                    $"مشاهده پروفایل شخصی: {currentUser.FirstName} {currentUser.LastName}",
                    recordId: currentUser.Id,
                    entityType: "AppUsers",
                    recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                );

                return View(userProfile);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "Profile",
                    "خطا در نمایش پروفایل کاربر",
                    ex
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// ویرایش اطلاعات پروفایل کاربر جاری
        /// </summary>
        /// <param name="model">مدل اطلاعات کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserViewModelFull model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _UserManager.GetUserAsync(User);
                    if (currentUser == null || currentUser.Id != model.Id)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            "تلاش برای ویرایش پروفایل کاربر غیرمجاز"
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse("دسترسی غیرمجاز");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }

                    // ذخیره مقادیر قبلی برای لاگ
                    var oldValues = new
                    {
                        currentUser.FirstName,
                        currentUser.LastName,
                        currentUser.Email,
                        currentUser.PhoneNumber,
                        currentUser.CompanyName,
                        currentUser.PositionName,
                        currentUser.City,
                        currentUser.Province,
                        currentUser.Address,
                        currentUser.MelliCode,
                        currentUser.Gender,
                        currentUser.ProfileImagePath
                    };

                    // به‌روزرسانی اطلاعات
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;
                    currentUser.Email = model.Email;
                    currentUser.PhoneNumber = model.PhoneNumber;
                    currentUser.CompanyName = model.CompanyName;
                    currentUser.PositionName = model.PositionName;
                    currentUser.City = model.City;
                    currentUser.Province = model.Province;
                    currentUser.Address = model.Address;
                    currentUser.MelliCode = model.MelliCode;
                    currentUser.ProfileImagePath = model.ProfileImagePath;
                    currentUser.Gender = model.Gender ?? 1;

                    var result = await _UserManager.UpdateAsync(currentUser);
                    if (result.Succeeded)
                    {
                        // مقادیر جدید برای لاگ
                        var newValues = new
                        {
                            currentUser.FirstName,
                            currentUser.LastName,
                            currentUser.Email,
                            currentUser.PhoneNumber,
                            currentUser.CompanyName,
                            currentUser.PositionName,
                            currentUser.City,
                            currentUser.Province,
                            currentUser.Address,
                            currentUser.MelliCode,
                            currentUser.Gender,
                            currentUser.ProfileImagePath
                        };

                        // ثبت لاگ تغییرات
                        await _activityLogger.LogChangeAsync<object>(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            $"به‌روزرسانی پروفایل شخصی: {currentUser.FirstName} {currentUser.LastName}",
                            oldValues,
                            newValues,
                            recordId: currentUser.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                        );

                        var successMessages = ResponseMessage.CreateSuccessResponse("اطلاعات با موفقیت به‌روزرسانی شد");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(successMessages);
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            $"شکست در ویرایش پروفایل: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                            recordId: currentUser.Id
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse(result.Errors.Select(e => e.Description).ToArray());
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return View("Profile", model);
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "UpdateProfile",
                        "خطا در به‌روزرسانی پروفایل",
                        ex,
                        recordId: model.Id
                    );

                    var errorMessages = ResponseMessage.CreateErrorResponse("خطا در به‌روزرسانی اطلاعات");
                    TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                    return View("Profile", model);
                }
            }

            // در صورت نامعتبر بودن ModelState، فقط از ResponseMessage استفاده کنید
            // ModelState.Clear() تا خطاهای مکرر نمایش داده نشوند
            var modelErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToArray();

            if (modelErrors.Any())
            {
                // پاک کردن ModelState تا خطاهای مکرر نمایش داده نشوند
                ModelState.Clear();

                var errorMessages = ResponseMessage.CreateErrorResponse(modelErrors);
                TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
            }

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.Edit,
                "UserManager",
                "UpdateProfile",
                $"تلاش ناموفق برای به‌روزرسانی پروفایل - خطاهای validation: {string.Join(", ", modelErrors)}",
                recordId: model.Id
            );

            return View("Profile", model);
        }
        /// <summary>
        /// تغییر رمز عبور کاربر جاری
        /// </summary>
        /// <param name="model">مدل تغییر رمز عبور</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _UserManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            "تلاش برای تغییر رمز عبور کاربر غیرموجود"
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse("کاربر یافت نشد");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }

                    var result = await _UserManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        // ثبت لاگ تغییر رمز عبور موفق
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            $"تغییر رمز عبور: {currentUser.FirstName} {currentUser.LastName}",
                            recordId: currentUser.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                        );

                        var successMessages = ResponseMessage.CreateSuccessResponse("رمز عبور با موفقیت تغییر یافت");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(successMessages);
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            $"شکست در تغییر رمز عبور: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                            recordId: currentUser.Id
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse(result.Errors.Select(e => e.Description).ToArray());
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "ChangePassword",
                        "خطا در تغییر رمز عبور",
                        ex
                    );

                    var errorMessages = ResponseMessage.CreateErrorResponse("خطا در تغییر رمز عبور");
                    TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                    return RedirectToAction("Profile");
                }
            }

            // در صورت نامعتبر بودن ModelState
            var modelErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToArray();

            if (modelErrors.Any())
            {
                // پاک کردن ModelState تا خطاهای مکرر نمایش داده نشوند
                ModelState.Clear();

                var errorMessages = ResponseMessage.CreateErrorResponse(modelErrors);
                TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
            }

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.Edit,
                "UserManager",
                "ChangePassword",
                $"تلاش ناموفق برای تغییر رمز عبور - خطاهای validation: {string.Join(", ", modelErrors)}"
            );

            return RedirectToAction("Profile");
        }

        /// <summary>
        /// آپلود تصویر پروفایل کاربر
        /// </summary>
        /// <param name="profileImage">فایل تصویر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                var currentUser = await _UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { status = "error", message = "کاربر یافت نشد" });
                }

                if (profileImage == null || profileImage.Length == 0)
                {
                    return Json(new { status = "error", message = "فایلی انتخاب نشده است" });
                }

                // بررسی نوع فایل
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { status = "error", message = "فرمت تصویر مجاز نیست" });
                }

                // بررسی حجم فایل (5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    return Json(new { status = "error", message = "حجم تصویر نباید بیشتر از 5 مگابایت باشد" });
                }

                // حذف تصویر قبلی در صورت وجود
                if (!string.IsNullOrEmpty(currentUser.ProfileImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, currentUser.ProfileImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // ایجاد مسیر ذخیره‌سازی
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // ایجاد نام فایل یکتا
                var uniqueFileName = $"{currentUser.Id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                // ذخیره فایل
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                // به‌روزرسانی مسیر در دیتابیس
                var relativePath = $"/uploads/profiles/{uniqueFileName}";
                currentUser.ProfileImagePath = relativePath;

                var result = await _UserManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "UserManager",
                        "UploadProfileImage",
                        $"آپلود تصویر پروفایل: {currentUser.FirstName} {currentUser.LastName}",
                        recordId: currentUser.Id
                    );

                    return Json(new { status = "success", message = "تصویر با موفقیت آپلود شد", imagePath = relativePath });
                }
                else
                {
                    // حذف فایل در صورت خطا
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    return Json(new { status = "error", message = "خطا در ذخیره تصویر در دیتابیس" });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "UploadProfileImage",
                    "خطا در آپلود تصویر پروفایل",
                    ex
                );

                return Json(new { status = "error", message = $"خطا در آپلود تصویر: {ex.Message}" });
            }
        }
    }
}
