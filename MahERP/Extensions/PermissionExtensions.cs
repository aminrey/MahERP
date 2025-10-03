using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using System.Threading.Tasks;

namespace MahERP.Extensions
{
    public static class PermissionExtensions
    {
        /// <summary>
        /// بررسی دسترسی کاربر به کنترلر مشخص
        /// </summary>
        public static bool CanAccess(this IHtmlHelper htmlHelper, string controller, byte actionType = 0)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var roleRepository = httpContext.RequestServices.GetService<IRoleRepository>();
            var userManager = httpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (roleRepository == null || userManager == null) return false;

            var userId = userManager.GetUserId(httpContext.User);
            if (string.IsNullOrEmpty(userId)) return false;

            return roleRepository.HasPermission(userId, controller, "General", actionType);
        }

        /// <summary>
        /// بررسی اینکه کاربر Admin یا Manager است
        /// </summary>
        public static bool IsAdminOrManager(this IHtmlHelper htmlHelper)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var roleRepository = httpContext.RequestServices.GetService<IRoleRepository>();
            var userManager = httpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (roleRepository == null || userManager == null) return false;

            var userId = userManager.GetUserId(httpContext.User);
            if (string.IsNullOrEmpty(userId)) return false;

            // بررسی async به صورت sync (برای استفاده در View)
            return Task.Run(async () => await roleRepository.IsAdminOrManagerAsync(userId)).Result;
        }

        /// <summary>
        /// نمایش لینک یا آیتم منو فقط در صورت داشتن دسترسی
        /// </summary>
        public static IHtmlContent PermissionLink(this IHtmlHelper htmlHelper,
            string controller, byte actionType, string linkContent, string href,
            string cssClass = "", object htmlAttributes = null)
        {
            if (!htmlHelper.CanAccess(controller, actionType))
                return new HtmlString("");

            var attributes = htmlAttributes != null ?
                string.Join(" ", htmlAttributes.GetType().GetProperties()
                    .Select(p => $"{p.Name.Replace('_', '-')}=\"{p.GetValue(htmlAttributes)}\"")) : "";

            return new HtmlString($"<a href=\"{href}\" class=\"{cssClass}\" {attributes}>{linkContent}</a>");
        }

        /// <summary>
        /// نمایش آیتم لیست (li) فقط در صورت داشتن دسترسی
        /// </summary>
        public static IHtmlContent PermissionMenuItem(this IHtmlHelper htmlHelper,
            string controller, byte actionType, string menuContent)
        {
            if (!htmlHelper.CanAccess(controller, actionType))
                return new HtmlString("");

            return new HtmlString($"<li class=\"nav-main-item\">{menuContent}</li>");
        }

        /// <summary>
        /// بررسی نمایش کل بخش (اگر حداقل یک دسترسی داشته باشد)
        /// </summary>
        public static bool CanAccessAnyIn(this IHtmlHelper htmlHelper, params string[] controllers)
        {
            foreach (var controller in controllers)
            {
                if (htmlHelper.CanAccess(controller))
                    return true;
            }
            return false;
        }
    }
}