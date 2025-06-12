using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseController;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace Accounting.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize]
    public class UserManagerController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IMapper _Mapper;

        public UserManagerController(IUnitOfWork context, UserManager<AppUsers> userManager, IMapper Mapper) : base(context, userManager)
        {
            _Context = context;
            _UserManager = userManager;
            _Mapper = Mapper;
        }


        public IActionResult Index()
        {

            var model = _Context.UserManagerUW.Get().Where(c => c.IsAdmin).ToList();
            return View(model);
        }


        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
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
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult EditUser(string UserId)
        {
            if (UserId == null)
            {
                return RedirectToAction("ErrorView", "Home");
            }
            var user = _Context.UserManagerUW.GetById(UserId);
            var mapUser = _Mapper.Map<EditUserViewModel>(user);
            return View(mapUser);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                //update
                var user = await _UserManager.FindByIdAsync(model.Id);
                IdentityResult result = await _UserManager.UpdateAsync(_Mapper.Map(model, user));
                if (result.Succeeded)
                {

                    return RedirectToAction("Index");

                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult RemoveUser(string UserId)
        {
            var model = _Context.UserManagerUW.GetById(UserId);
            return PartialView("_RemoveUser", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserPost(string Id)
        {

            if (ModelState.IsValid)
            {
                _Context.UserManagerUW.DeleteById(Id);
                _Context.Save();
                return Json(new { status = "redirect", redirectUrl = Url.Action(nameof(Index)) });
            }
            return BadRequest(ModelState);
        }


        [HttpGet]
        public IActionResult ChangePasswordByAdmin(string UserId)
        {
            if (UserId == null)
            {
                return RedirectToAction("ErrorView", "Home");
            }
            ViewBag.userId = UserId;
            var User = _Context.UserManagerUW.GetById(UserId);
            ViewBag.FullName = User.FirstName + " " + User.LastName;
            return PartialView("_ChangePasswordByAdmin");
        }


        [HttpPost]
        public IActionResult ChangePassByAdmin(ChangePasswordByAdminViewModel model)
        {
            try
            {
                var user = _Context.UserManagerUW.Get(u => u.Id == model.userId).FirstOrDefault();
                user.PasswordHash = _UserManager.PasswordHasher.HashPassword(user, model.NewPassword);
                _Context.Save();
                return Json(new { status = "ok" });
            }
            catch
            {
                return Json(new { status = "error" });
            }
        }


        [HttpGet]
        public IActionResult ActiveOrDeactiveUser(string UserId)
        {

            var User = _Context.UserManagerUW.GetById(UserId);


            if (User.IsActive == true)
            {
                ViewBag.theme = "bg-danger";
                ViewBag.ViewTitle = "غیرفعال کردن کاربر";
                return PartialView("_ActiveOrDeactiveUser", User);
            }
            else
            {
                ViewBag.theme = "bg-success";
                ViewBag.ViewTitle = "فعال کردن کاربر";
                return PartialView("_ActiveOrDeactiveUser", User);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActiveOrDeactiveUser(string Id, bool IsActive)
        {

            var User = _Context.UserManagerUW.GetById(Id);

            if (IsActive == true)
            {
                User.IsActive = false;
                _Context.UserManagerUW.Update(User);
                _Context.Save();

            }
            else
            {
                User.IsActive = true;
                _Context.UserManagerUW.Update(User);
                _Context.Save();
            }
            return RedirectToAction("Index");

        }

    }
}
