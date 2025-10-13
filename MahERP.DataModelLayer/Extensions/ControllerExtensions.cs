using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.DataModelLayer.Extensions
{
    /// <summary>
    /// Extension Methods برای کنترلرها جهت بررسی دسترسی
    /// </summary>
    public static class ControllerExtensions
    {
        #region ✅ NEW: Permission-Based Methods

        /// <summary>
        /// بررسی دسترسی کاربر به یک Permission خاص (Async)
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <param name="permissionCode">کد دسترسی (مثل: TASK.CREATE)</param>
        /// <returns>true اگر کاربر دسترسی داشته باشد</returns>
        public static async Task<bool> HasPermissionAsync(this Controller controller, string permissionCode)
        {
            var userPermissionService = controller.HttpContext.RequestServices
                .GetService<IUserPermissionService>();
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return false;

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            // بررسی Admin
            var user = await userManager.GetUserAsync(controller.User);
            if (user?.IsAdmin == true)
                return true;

            return await userPermissionService.UserHasPermissionAsync(userId, permissionCode);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک Permission خاص (Sync - استفاده نشود)
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <param name="permissionCode">کد دسترسی</param>
        /// <returns>true اگر کاربر دسترسی داشته باشد</returns>
        [Obsolete("Use HasPermissionAsync instead")]
        public static bool HasPermission(this Controller controller, string permissionCode)
        {
            return controller.HasPermissionAsync(permissionCode).GetAwaiter().GetResult();
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یکی از Permissions (OR logic)
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <param name="permissionCodes">لیست کدهای دسترسی</param>
        /// <returns>true اگر کاربر حداقل یکی از دسترسی‌ها را داشته باشد</returns>
        public static async Task<bool> HasAnyPermissionAsync(this Controller controller, params string[] permissionCodes)
        {
            if (permissionCodes == null || !permissionCodes.Any())
                return false;

            var userPermissionService = controller.HttpContext.RequestServices
                .GetService<IUserPermissionService>();
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return false;

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            // بررسی Admin
            var user = await userManager.GetUserAsync(controller.User);
            if (user?.IsAdmin == true)
                return true;

            return await userPermissionService.UserHasAnyPermissionAsync(userId, permissionCodes.ToList());
        }

        /// <summary>
        /// بررسی دسترسی کاربر به همه Permissions (AND logic)
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <param name="permissionCodes">لیست کدهای دسترسی</param>
        /// <returns>true اگر کاربر همه دسترسی‌ها را داشته باشد</returns>
        public static async Task<bool> HasAllPermissionsAsync(this Controller controller, params string[] permissionCodes)
        {
            if (permissionCodes == null || !permissionCodes.Any())
                return false;

            foreach (var code in permissionCodes)
            {
                if (!await controller.HasPermissionAsync(code))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// دریافت لیست تمام دسترسی‌های فعال کاربر
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <returns>لیست کدهای دسترسی</returns>
        public static async Task<List<string>> GetUserPermissionsAsync(this Controller controller)
        {
            var userPermissionService = controller.HttpContext.RequestServices
                .GetService<IUserPermissionService>();
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return new List<string>();

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            return await userPermissionService.GetUserPermissionCodesAsync(userId);
        }

        /// <summary>
        /// دریافت لیست نقش‌های فعال کاربر
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <returns>لیست نقش‌ها</returns>
        public static async Task<List<Role>> GetUserRolesAsync(this Controller controller)
        {
            var userPermissionService = controller.HttpContext.RequestServices
                .GetService<IUserPermissionService>();
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userPermissionService == null || userManager == null)
                return new List<Role>();

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return new List<Role>();

            var userRoles = await userPermissionService.GetUserRolesAsync(userId);
            return userRoles.Select(ur => ur.Role).ToList();
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر Admin است
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <returns>true اگر کاربر Admin باشد</returns>
        public static async Task<bool> IsAdminAsync(this Controller controller)
        {
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userManager == null)
                return false;

            var user = await userManager.GetUserAsync(controller.User);
            return user?.IsAdmin == true;
        }

        /// <summary>
        /// دریافت UserId کاربر جاری
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <returns>شناسه کاربر</returns>
        public static string GetCurrentUserId(this Controller controller)
        {
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            return userManager?.GetUserId(controller.User);
        }

        /// <summary>
        /// دریافت کاربر جاری
        /// </summary>
        /// <param name="controller">کنترلر جاری</param>
        /// <returns>کاربر جاری</returns>
        public static async Task<AppUsers> GetCurrentUserAsync(this Controller controller)
        {
            var userManager = controller.HttpContext.RequestServices
                .GetService<UserManager<AppUsers>>();

            if (userManager == null)
                return null;

            return await userManager.GetUserAsync(controller.User);
        }

        #endregion

        #region 🔴 DEPRECATED: Old RolePattern Methods (برای سازگاری با کد قدیمی)

        /// <summary>
        /// ⚠️ DEPRECATED: استفاده از HasPermissionAsync توصیه می‌شود
        /// </summary>
        [Obsolete("Use HasPermissionAsync with permission code instead")]
        public static bool HasPermission(this Controller controller, string controllerName, string actionName, byte actionType = 0)
        {
            // تبدیل به سیستم جدید
            string permissionCode = $"{controllerName}.{actionName}";
            return controller.HasPermissionAsync(permissionCode).GetAwaiter().GetResult();
        }

        /// <summary>
        /// ⚠️ DEPRECATED: این متد در سیستم جدید معنا ندارد
        /// </summary>
        [Obsolete("DataAccessLevel removed in new permission system")]
        public static byte GetUserDataAccessLevel(this Controller controller, string controllerName, string actionName)
        {
            // در سیستم جدید DataAccessLevel حذف شده
            // می‌توانید یک مقدار ثابت برگردانید یا بر اساس نقش کاربر تصمیم بگیرید
            var isAdmin = controller.IsAdminAsync().GetAwaiter().GetResult();
            return isAdmin ? (byte)2 : (byte)0; // 0=Personal, 1=Team, 2=Branch
        }

        #endregion
    }
}