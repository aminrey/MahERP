using System;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MahERP.DataModelLayer.Attributes
{
    /// <summary>
    /// Attribute برای بررسی دسترسی کاربر به یک Permission خاص
    /// مثال: [Permission("TASK.CREATE")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode; // ✅ تصحیح شده
        private readonly string _controller;
        private readonly string _action;
        private readonly byte _actionType;

        /// <summary>
        /// سازنده با کد دسترسی
        /// </summary>
        /// <param name="permissionCode">کد دسترسی (مثل: TASK.CREATE)</param>
        public PermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        /// <summary>
        /// سازنده با کنترلر و اکشن (برای سازگاری با کد قدیمی)
        /// </summary>
        public PermissionAttribute(string controller, string action, byte actionType = 0)
        {
            _controller = controller;
            _action = action;
            _actionType = actionType;
            _permissionCode = $"{controller}.{action}"; // ساخت کد از controller و action
        }

        /// <summary>
        /// ✅ پیاده‌سازی صحیح IAsyncAuthorizationFilter
        /// </summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 1️⃣ بررسی احراز هویت
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                // 2️⃣ دریافت سرویس‌ها
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<AppUsers>>();
                var roleRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IUserRoleRepository>();

                // 3️⃣ دریافت کاربر جاری
                var user = await userManager.GetUserAsync(context.HttpContext.User);
                if (user == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // 4️⃣ اگر Admin است، دسترسی کامل
                if (user.IsAdmin)
                {
                    await LogAccessAsync(
                        roleRepository, 
                        user.Id, 
                        _permissionCode, 
                        true, 
                        "Admin bypass", 
                        context
                    );
                    return;
                }

                // 5️⃣ بررسی دسترسی
                var hasPermission = await roleRepository.HasPermission(user.Id, _permissionCode);

                // 6️⃣ ثبت لاگ
                await LogAccessAsync(
                    roleRepository,
                    user.Id,
                    _permissionCode,
                    hasPermission,
                    hasPermission ? "Access granted" : "Permission denied",
                    context
                );

                // 7️⃣ اگر دسترسی نداشت، Forbid
                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، دسترسی رد می‌شود
                context.Result = new StatusCodeResult(500);
                
                // می‌توانید اینجا لاگ خطا ثبت کنید
                System.Diagnostics.Debug.WriteLine($"PermissionAttribute Error: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ متد کمکی برای ثبت لاگ دسترسی
        /// </summary>
        private async Task LogAccessAsync(
            IUserRoleRepository roleRepository,
            string userId,
            string permissionCode,
            bool granted,
            string notes,
            AuthorizationFilterContext context)
        {
            try
            {
                var actionName = context.ActionDescriptor?.DisplayName 
                    ?? $"{_controller}.{_action}";

                await roleRepository.LogPermissionAccess(
                    userId,
                    permissionCode,
                    actionName,
                    granted,
                    context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    context.HttpContext.Request.Headers["User-Agent"].ToString()
                );
            }
            catch
            {
                // Silent fail - لاگ نباید باعث خرابی سیستم شود
            }
        }
    }
}