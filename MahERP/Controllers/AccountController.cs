using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.Helpers;
using MahERP.DataModelLayer.Enums; // ⭐ NEW
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUsers> _signInManager;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IUnitOfWork _Context;
        private readonly ActivityLoggerService _activityLogger;
        private readonly IModuleAccessService _moduleAccessService; // ⭐ NEW

        public AccountController(
            UserManager<AppUsers> usermanager, 
            SignInManager<AppUsers> signInManager, 
            IUnitOfWork Context,
            ActivityLoggerService activityLogger,
            IModuleAccessService moduleAccessService) // ⭐ NEW
        {
            _signInManager = signInManager;
            _UserManager = usermanager;
            _Context = Context;
            _activityLogger = activityLogger;
            _moduleAccessService = moduleAccessService; // ⭐ NEW
        }

        private void GetVersionInfo()
        {
            ViewBag.Version = VersionHelper.GetVersion();
            ViewBag.BuildDate = VersionHelper.GetBuildDate();
            ViewBag.FullVersionInfo = VersionHelper.GetFullVersionInfo();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? ReturnUrl)
        {
            try
            {
                GetVersionInfo();
                var Users = _Context.UserManagerUW.Get();
                if (Users.Count() == 0)
                {
                    // ⭐⭐⭐ ایجاد کاربر Admin
                    AppUsers User = new()
                    {
                        UserName = "Admin",
                        FirstName = "ادمین",
                        LastName = "نرم افزار",
                        IsAdmin = true,
                        IsActive = true,
                        RegisterDate = DateTime.Now,
                    };

                    IdentityResult result = await _UserManager.CreateAsync(User, "Admin1234@");

                    if (result.Succeeded)
                    {
                        // تخصیص همه نقش‌ها
                        var Roles = _Context.RoleUW.Get().ToList();
                        foreach (var item in Roles)
                        {
                            await _UserManager.AddToRoleAsync(User, item.Name);
                        }

                        // تخصیص الگوی نقش "مدیریت کامل" 
                        var adminRolePattern = new UserRolePattern
                        {
                            UserId = User.Id,
                            RolePatternId = 1,
                            AssignDate = DateTime.Now,
                            AssignedByUserId = User.Id,
                            IsActive = true,
                            StartDate = DateTime.Now,
                            Notes = "تخصیص خودکار هنگام ایجاد کاربر Admin"
                        };

                        _Context.UserRolePatternUW.Create(adminRolePattern);

                        // تخصیص کاربر به شعبه اصلی
                        var branchUser = new BranchUser
                        {
                            UserId = User.Id,
                            BranchId = 1,
                            AssignedByUserId = User.Id,
                            AssignDate = DateTime.Now,
                            IsActive = true
                        };

                        _Context.BranchUserUW.Create(branchUser);

                        // ⭐⭐⭐ NEW: اعطای دسترسی به تمام ماژول‌ها برای Admin
                        await _moduleAccessService.GrantModuleAccessToUserAsync(
                            User.Id, ModuleType.Core, User.Id, "دسترسی پیش‌فرض Admin");
                        await _moduleAccessService.GrantModuleAccessToUserAsync(
                            User.Id, ModuleType.Tasking, User.Id, "دسترسی پیش‌فرض Admin");
                        await _moduleAccessService.GrantModuleAccessToUserAsync(
                            User.Id, ModuleType.CRM, User.Id, "دسترسی پیش‌فرض Admin");

                        _Context.Save();

                        // ثبت لاگ ایجاد کاربر Admin اولیه
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Authentication",
                            "CreateInitialAdmin",
                            "ایجاد کاربر Admin اولیه سیستم",
                            recordId: User.Id,
                            entityType: "AppUsers",
                            recordTitle: "ادمین نرم افزار"
                        );
                    }
                }
                else
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var LoggedUser = _Context.UserManagerUW.GetById(_UserManager.GetUserId(HttpContext.User));
                        if (LoggedUser != null)
                        {
                            // ⭐⭐⭐ NEW: ریدایرکت به ماژول مناسب
                            return await RedirectToDefaultModuleAsync(LoggedUser.Id);
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            
                            await _activityLogger.LogActivityAsync(
                                ActivityTypeEnum.Logout,
                                "Authentication",
                                "ForceLogout",
                                "خروج اجباری کاربر غیرمجاز"
                            );
                            
                            return Redirect("/");
                        }
                    }
                }

                ViewBag.Url = ReturnUrl;
                return View();
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Authentication",
                    "Login",
                    "خطا در نمایش صفحه ورود",
                    ex
                );
                
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            GetVersionInfo();

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        var ThisUser = await _UserManager.FindByNameAsync(model.UserName);

                        // ثبت لاگ ورود موفق
                        await _activityLogger.LogLoginAsync(true, ThisUser.UserName);

                        // ⭐⭐⭐ NEW: بررسی دسترسی به ماژول‌ها و ریدایرکت
                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return await RedirectToDefaultModuleAsync(ThisUser.Id);
                        }
                    }
                    else
                    {
                        // ثبت لاگ تلاش ورود ناموفق
                        string failureReason = "نام کاربری یا رمز عبور اشتباه";
                        if (result.IsLockedOut)
                            failureReason = "حساب کاربری قفل شده";
                        else if (result.IsNotAllowed)
                            failureReason = "ورود مجاز نیست";
                        else if (result.RequiresTwoFactor)
                            failureReason = "نیاز به تایید دو مرحله‌ای";

                        await _activityLogger.LogLoginAsync(false, model.UserName, failureReason);

                        ModelState.AddModelError("UserName", "اطلاعات ورود صحیح نیست");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "Authentication",
                        "Login",
                        "خطا در فرآیند ورود به سیستم",
                        ex
                    );
                    
                    ModelState.AddModelError("", "خطایی در ورود به سیستم رخ داد");
                    return View(model);
                }
            }
            return View(model);
        }

        // ⭐⭐⭐ NEW: متد کمکی برای ریدایرکت به ماژول مناسب
        private async Task<IActionResult> RedirectToDefaultModuleAsync(string userId)
        {
            try
            {
                // 1. دریافت لیست ماژول‌های فعال
                var enabledModules = await _moduleAccessService.GetUserEnabledModulesAsync(userId);

                if (!enabledModules.Any())
                {
                    // ❌ هیچ ماژولی فعال نیست
                    return RedirectToAction("NoAccess", "Account");
                }

                if (enabledModules.Count == 1)
                {
                    // ✅ فقط یک ماژول فعال است
                    var singleModule = enabledModules.First();
                    await _moduleAccessService.SaveLastUsedModuleAsync(userId, singleModule);
                    return Redirect(singleModule.GetBaseUrl());
                }

                // ✅ چند ماژول فعال است
                // بررسی آخرین ماژول استفاده شده
                var defaultModule = await _moduleAccessService.GetDefaultModuleForLoginAsync(userId);

                if (defaultModule.HasValue)
                {
                    await _moduleAccessService.SaveLastUsedModuleAsync(userId, defaultModule.Value);
                    return Redirect(defaultModule.Value.GetBaseUrl());
                }

                // ⭐ نمایش صفحه انتخاب ماژول
                return RedirectToAction("SelectModule", "Account");
            }
            catch
            {
                // در صورت خطا، به Core می‌رویم
                return Redirect("/AppCoreArea/Dashboard/Index");
            }
        }

        // ⭐⭐⭐ NEW: صفحه عدم دسترسی
        [HttpGet]
        public IActionResult NoAccess()
        {
            ViewBag.Title = "عدم دسترسی";
            return View();
        }

        // ⭐⭐⭐ NEW: صفحه انتخاب ماژول
        [HttpGet]
        public async Task<IActionResult> SelectModule()
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var enabledModules = await _moduleAccessService.GetUserEnabledModulesAsync(userId);
                
                if (!enabledModules.Any())
                {
                    return RedirectToAction("NoAccess");
                }

                if (enabledModules.Count == 1)
                {
                    // اگر فقط یک ماژول بود، مستقیم redirect
                    return Redirect(enabledModules.First().GetBaseUrl());
                }

                ViewBag.EnabledModules = enabledModules;
                return View();
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        // ⭐⭐⭐ NEW: انتخاب ماژول توسط کاربر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectModule(byte moduleType, bool setAsDefault = false)
        {
            try
            {
                var userId = _UserManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var selectedModule = (ModuleType)moduleType;

                // ذخیره آخرین استفاده
                await _moduleAccessService.SaveLastUsedModuleAsync(userId, selectedModule);

                // اگر کاربر خواست پیش‌فرض شود
                if (setAsDefault)
                {
                    await _moduleAccessService.SetUserDefaultModuleAsync(userId, selectedModule);
                }

                return Redirect(selectedModule.GetBaseUrl());
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [Route("/Account/LogOut")]
        public async Task<IActionResult> LogOut()
        {
            try
            {
                // دریافت اطلاعات کاربر قبل از خروج
                string currentUserId = null;
                string currentUserName = null;
                
                if (User.Identity.IsAuthenticated)
                {
                    currentUserId = _UserManager.GetUserId(User);
                    var currentUser = await _UserManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";
                    }
                }

                await _signInManager.SignOutAsync();

                // ثبت لاگ خروج
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await _activityLogger.LogLogoutAsync();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Authentication",
                    "Logout",
                    "خطا در خروج از سیستم",
                    ex
                );
                
                return RedirectToAction("Login", "Account");
            }
        }
    }
}