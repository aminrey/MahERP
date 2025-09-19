using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class UserManagerController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IMapper _Mapper;

        public UserManagerController(
            IUnitOfWork context, 
            UserManager<AppUsers> userManager, 
            IMapper Mapper, 
            PersianDateHelper persianDateHelper, 
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger)
            : base(context, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _Context = context;
            _UserManager = userManager;
            _Mapper = Mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = _Context.UserManagerUW.Get().Where(c => c.IsAdmin).ToList();

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "Index",
                    "مشاهده لیست کاربران"
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
                    if (await _UserManager.FindByNameAsync(model.UserName) != null)
                    {
                        ModelState.AddModelError("UserName", "نام کاربری تکراری می باشد.");
                        return View(model);
                    }
                    
                    var userMapped = _Mapper.Map<AppUsers>(model);
                    userMapped.IsAdmin = true;
                    userMapped.IsActive = true;
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

                        // ثبت لاگ تغییرات
                        await _activityLogger.LogChangeAsync(
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

        [HttpGet]
        public async Task<IActionResult> RemoveUser(string UserId)
        {
            try
            {
                var model = _Context.UserManagerUW.GetById(UserId);
                if (model == null)
                    return RedirectToAction("ErrorView", "Home");

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "RemoveUser",
                    $"مشاهده فرم حذف کاربر: {model.FirstName} {model.LastName}",
                    recordId: UserId,
                    entityType: "AppUsers",
                    recordTitle: $"{model.FirstName} {model.LastName}"
                );

                return PartialView("_RemoveUser", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "RemoveUser",
                    "خطا در نمایش فرم حذف کاربر",
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
                    
                    _Context.UserManagerUW.DeleteById(Id);
                    _Context.Save();

                    // ثبت لاگ حذف
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "UserManager",
                        "RemoveUser",
                        $"حذف کاربر: {userName}",
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
                        "RemoveUser",
                        "خطا در حذف کاربر",
                        ex,
                        recordId: Id
                    );
                    
                    return Json(new { status = "error", message = "خطا در حذف کاربر" });
                }
            }
            return BadRequest(ModelState);
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

                // ثبت لاگ تغییر وضعیت
                await _activityLogger.LogChangeAsync(
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
    }
}
