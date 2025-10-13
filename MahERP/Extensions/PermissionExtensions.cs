using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.Extensions
{
    /// <summary>
    /// Extension Methods برای استفاده در View ها (Razor Pages)
    /// </summary>
    public static class PermissionExtensions
    {
        #region ✅ NEW: Permission-Based Methods

        /// <summary>
        /// بررسی دسترسی کاربر به یک Permission خاص
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCode">کد دسترسی (مثل: TASK.CREATE)</param>
        /// <returns>true اگر کاربر دسترسی داشته باشد</returns>
        public static bool CanAccess(this IHtmlHelper htmlHelper, string permissionCode)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var userPermissionService = httpContext.RequestServices.GetService<IUserPermissionService>();
            var userManager = httpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return false;

            var userId = userManager.GetUserId(httpContext.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            // بررسی Admin
            var user = userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.IsAdmin == true)
                return true;

            // بررسی دسترسی (تبدیل async به sync برای استفاده در View)
            return Task.Run(async () =>
                await userPermissionService.UserHasPermissionAsync(userId, permissionCode)
            ).GetAwaiter().GetResult();
        }

        /// <summary>
        /// بررسی دسترسی به یکی از Permissions (OR logic)
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCodes">کدهای دسترسی</param>
        /// <returns>true اگر کاربر حداقل یک دسترسی داشته باشد</returns>
        public static bool CanAccessAny(this IHtmlHelper htmlHelper, params string[] permissionCodes)
        {
            if (permissionCodes == null || !permissionCodes.Any())
                return false;

            foreach (var code in permissionCodes)
            {
                if (htmlHelper.CanAccess(code))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// بررسی دسترسی به همه Permissions (AND logic)
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCodes">کدهای دسترسی</param>
        /// <returns>true اگر کاربر همه دسترسی‌ها را داشته باشد</returns>
        public static bool CanAccessAll(this IHtmlHelper htmlHelper, params string[] permissionCodes)
        {
            if (permissionCodes == null || !permissionCodes.Any())
                return false;

            foreach (var code in permissionCodes)
            {
                if (!htmlHelper.CanAccess(code))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// بررسی اینکه کاربر Admin است
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <returns>true اگر کاربر Admin باشد</returns>
        public static bool IsAdmin(this IHtmlHelper htmlHelper)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var userManager = httpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (userManager == null)
                return false;

            var userId = userManager.GetUserId(httpContext.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            var user = userManager.Users.FirstOrDefault(u => u.Id == userId);
            return user?.IsAdmin == true;
        }

        /// <summary>
        /// بررسی اینکه کاربر Admin یا دارای نقش خاص است
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="roleNames">نام نقش‌ها (مثل: Manager, Supervisor)</param>
        /// <returns>true اگر کاربر Admin یا دارای یکی از نقش‌ها باشد</returns>
        public static bool IsAdminOrHasRole(this IHtmlHelper htmlHelper, params string[] roleNames)
        {
            if (htmlHelper.IsAdmin())
                return true;

            var httpContext = htmlHelper.ViewContext.HttpContext;
            var userPermissionService = httpContext.RequestServices.GetService<IUserPermissionService>();
            var userManager = httpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return false;

            var userId = userManager.GetUserId(httpContext.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            var userRoles = Task.Run(async () =>
                await userPermissionService.GetUserRolesAsync(userId)
            ).GetAwaiter().GetResult();

            return userRoles.Any(ur => roleNames.Contains(ur.Role.NameEn, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// نمایش لینک فقط در صورت داشتن دسترسی
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCode">کد دسترسی</param>
        /// <param name="linkContent">محتوای لینک (HTML)</param>
        /// <param name="href">آدرس URL</param>
        /// <param name="cssClass">کلاس CSS</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML اضافی</param>
        /// <returns>لینک HTML یا خالی</returns>
        public static IHtmlContent PermissionLink(
            this IHtmlHelper htmlHelper,
            string permissionCode,
            string linkContent,
            string href,
            string cssClass = "",
            object htmlAttributes = null)
        {
            if (!htmlHelper.CanAccess(permissionCode))
                return new HtmlString("");

            var attributes = htmlAttributes != null
                ? string.Join(" ", htmlAttributes.GetType().GetProperties()
                    .Select(p => $"{p.Name.Replace('_', '-')}=\"{p.GetValue(htmlAttributes)}\""))
                : "";

            return new HtmlString(
                $"<a href=\"{href}\" class=\"{cssClass}\" {attributes}>{linkContent}</a>"
            );
        }

        /// <summary>
        /// نمایش دکمه فقط در صورت داشتن دسترسی
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCode">کد دسترسی</param>
        /// <param name="buttonContent">محتوای دکمه</param>
        /// <param name="cssClass">کلاس CSS</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML اضافی</param>
        /// <returns>دکمه HTML یا خالی</returns>
        public static IHtmlContent PermissionButton(
            this IHtmlHelper htmlHelper,
            string permissionCode,
            string buttonContent,
            string cssClass = "btn btn-primary",
            object htmlAttributes = null)
        {
            if (!htmlHelper.CanAccess(permissionCode))
                return new HtmlString("");

            var attributes = htmlAttributes != null
                ? string.Join(" ", htmlAttributes.GetType().GetProperties()
                    .Select(p => $"{p.Name.Replace('_', '-')}=\"{p.GetValue(htmlAttributes)}\""))
                : "";

            return new HtmlString(
                $"<button class=\"{cssClass}\" {attributes}>{buttonContent}</button>"
            );
        }

        /// <summary>
        /// نمایش آیتم منو فقط در صورت داشتن دسترسی
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCode">کد دسترسی</param>
        /// <param name="menuContent">محتوای منو (HTML)</param>
        /// <returns>آیتم منو HTML یا خالی</returns>
        public static IHtmlContent PermissionMenuItem(
            this IHtmlHelper htmlHelper,
            string permissionCode,
            string menuContent)
        {
            if (!htmlHelper.CanAccess(permissionCode))
                return new HtmlString("");

            return new HtmlString($"<li class=\"nav-main-item\">{menuContent}</li>");
        }

        /// <summary>
        /// نمایش بخش کامل منو اگر حداقل یک دسترسی داشته باشد
        /// </summary>
        /// <param name="htmlHelper">IHtmlHelper</param>
        /// <param name="permissionCodes">کدهای دسترسی</param>
        /// <returns>true اگر حداقل یک دسترسی داشته باشد</returns>
        public static bool CanAccessAnyIn(
            this IHtmlHelper htmlHelper,
            params string[] permissionCodes)
        {
            return htmlHelper.CanAccessAny(permissionCodes);
        }

        #endregion

        #region 🔴 DEPRECATED: Old Methods (برای سازگاری با کد قدیمی)

        /// <summary>
        /// ⚠️ DEPRECATED: استفاده از CanAccess(permissionCode) توصیه می‌شود
        /// </summary>
        [Obsolete("Use CanAccess(permissionCode) instead")]
        public static bool CanAccess(
            this IHtmlHelper htmlHelper,
            string controller,
            byte actionType = 0)
        {
            // تبدیل به سیستم جدید
            string permissionCode = $"{controller}.General";
            return htmlHelper.CanAccess(permissionCode);
        }

        /// <summary>
        /// ⚠️ DEPRECATED: استفاده از IsAdminOrHasRole توصیه می‌شود
        /// </summary>
        [Obsolete("Use IsAdmin() or IsAdminOrHasRole() instead")]
        public static bool IsAdminOrManager(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.IsAdminOrHasRole("Manager");
        }

        #endregion
    }
}