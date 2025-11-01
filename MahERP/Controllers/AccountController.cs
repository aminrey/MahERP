using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using MahERP.Helpers;
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

        public AccountController(
            UserManager<AppUsers> usermanager, 
            SignInManager<AppUsers> signInManager, 
            IUnitOfWork Context,
            ActivityLoggerService activityLogger)
        {
            _signInManager = signInManager;
            _UserManager = usermanager;
            _Context = Context;
            _activityLogger = activityLogger;
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
                            RolePatternId = 1, // الگوی مدیریت کامل
                            AssignDate = DateTime.Now,
                            AssignedByUserId = User.Id, // خود کاربر
                            IsActive = true,
                            StartDate = DateTime.Now,
                            Notes = "تخصیص خودکار هنگام ایجاد کاربر Admin"
                        };

                        _Context.UserRolePatternUW.Create(adminRolePattern);

                        // تخصیص کاربر به شعبه اصلی
                        var branchUser = new BranchUser
                        {
                            UserId = User.Id,
                            BranchId = 1, // شعبه اصلی
                            AssignedByUserId = User.Id,
                            AssignDate = DateTime.Now,
                            IsActive = true
                        };

                        _Context.BranchUserUW.Create(branchUser);
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
                        if (LoggedUser != null && LoggedUser.IsAdmin == true)
                        {
                            // ثبت لاگ ورود مجدد کاربر لاگین شده
                            await _activityLogger.LogActivityAsync(
                                ActivityTypeEnum.View,
                                "Authentication",
                                "AlreadyLoggedIn",
                                $"کاربر از قبل وارد سیستم شده: {LoggedUser.FirstName} {LoggedUser.LastName}",
                                recordId: LoggedUser.Id,
                                entityType: "AppUsers",
                                recordTitle: $"{LoggedUser.FirstName} {LoggedUser.LastName}"
                            );
                            
                            return Redirect("/AppCoreArea/Dashboard/Index");
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            
                            // ثبت لاگ خروج اجباری
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
                        //if (ThisUser.IsAdmin == false)
                        //{
                        //    await _signInManager.SignOutAsync();

                        //    // ثبت لاگ تلاش ورود کاربر غیرمجاز
                        //    await _activityLogger.LogLoginAsync(
                        //        false, 
                        //        model.UserName, 
                        //        "کاربر دسترسی مدیریت ندارد"
                        //    );

                        //    ModelState.AddModelError("UserName", "اطلاعات ورود صحیح نیست");
                        //    return View(model);
                        //}

                        // ثبت لاگ ورود موفق
                        await _activityLogger.LogLoginAsync(true, ThisUser.UserName);

                        if (ReturnUrl == null)
                        {
                            return Redirect("/AdminArea/Dashboard/Index");
                        }
                        else
                        {
                            return Redirect(ReturnUrl);
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