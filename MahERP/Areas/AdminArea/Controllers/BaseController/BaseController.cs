using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.BaseController
{
    [Area("AdminArea")]
    public class BaseController : Controller
    {
        private readonly IUnitOfWork _Context;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IMemoryCache _memoryCache;
        public BaseController(IUnitOfWork Context, UserManager<AppUsers> UserManager)
        {
            _Context = Context;
            _UserManager = UserManager;
        }



        public AppUsers GetUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _UserManager.GetUserId(HttpContext.User);
                var User = _Context.UserManagerUW.Get().Where(u => u.Id == userId).FirstOrDefault();
                ViewBag.UserName = User.FirstName + " " + User.LastName;
                ViewBag.User = User;
                return User;
            }
            else
            {
                return null;
            }
        }



        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //base Codes Here
            if (User.Identity.IsAuthenticated)
            {
                var User = GetUser();

                if (User.IsAdmin == false)
                {
                    RedirectToAction("Error404").ExecuteResult(this.ControllerContext);
                }
            }

        }
    }
}
