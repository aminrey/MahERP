using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Controllers
{

    public class AccountController : Controller
    {

        private readonly SignInManager<AppUsers> _signInManager;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IUnitOfWork _Context;



        public AccountController(UserManager<AppUsers> usermanager, SignInManager<AppUsers> signInManager, IUnitOfWork Context)
        {
            _signInManager = signInManager;
            _UserManager = usermanager;
            _Context = Context;
        }


        [HttpGet]
        public async Task<IActionResult> Login(string? ReturnUrl)
        {

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
                var Roles = _Context.RoleUW.Get().ToList();

                foreach (var item in Roles)
                {
                    await _UserManager.AddToRoleAsync(User, item.Name);

                }

            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    var LoggedUser = _Context.UserManagerUW.GetById(_UserManager.GetUserId(HttpContext.User));
                    if (LoggedUser != null && LoggedUser.IsAdmin == true)
                        return Redirect("/AdminArea/Dashboard/Index");
                    else
                    {
                        await _signInManager.SignOutAsync();
                        return Redirect("/");
                    }

                }
            }

            ViewBag.Url = ReturnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var ThisUser = await _UserManager.FindByNameAsync(model.UserName);
                    if (ThisUser.IsAdmin == false)
                    {
                        await _signInManager.SignOutAsync();

                        ModelState.AddModelError("UserName", "اطلاعات ورود صحیح نیست");
                        return View(model);
                    }
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
                    ModelState.AddModelError("UserName", "اطلاعات ورود صحیح نیست");
                    return View(model);
                }
            }
            return View(model);
        }


        [Route("/Account/LogOut")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}