using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.Attributes
{
    /// <summary>
    /// Attribute برای بررسی دسترسی کاربر به Action ها
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PermissionRequiredAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode;

        public PermissionRequiredAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // بررسی احراز هویت
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUsers>>();
            var userPermissionService = context.HttpContext.RequestServices.GetRequiredService<IUserPermissionService>();

            var user = await userManager.GetUserAsync(context.HttpContext.User);
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // اگر Admin است، همه دسترسی‌ها را دارد
            if (user.IsAdmin)
                return;

            // بررسی دسترسی
            var hasPermission = await userPermissionService.UserHasPermissionAsync(user.Id, _permissionCode);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}