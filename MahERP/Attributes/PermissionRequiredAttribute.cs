using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using System;
using System.Threading.Tasks;

namespace MahERP.Attributes
{
    /// <summary>
    /// Attribute برای بررسی دسترسی کاربر به Action ها
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermissionRequiredAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string Controller { get; set; }
        public string Action { get; set; } = "General";
        public byte RequiredActionType { get; set; } = 0; // 0=Read, 1=Create, 2=Edit, 3=Delete, 4=Approve

        public PermissionRequiredAttribute(string controller, byte actionType = 0)
        {
            Controller = controller;
            RequiredActionType = actionType;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // بررسی اینکه کاربر لاگین است
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // دریافت سرویس‌های مورد نیاز
            var roleRepository = context.HttpContext.RequestServices.GetService(typeof(IRoleRepository)) as IRoleRepository;
            var userManager = context.HttpContext.RequestServices.GetService(typeof(UserManager<AppUsers>)) as UserManager<AppUsers>;

            if (roleRepository == null || userManager == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var userId = userManager.GetUserId(context.HttpContext.User);
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // بررسی دسترسی
            var hasPermission = await roleRepository.CanAccessAsync(userId, Controller, Action) ||
                               roleRepository.HasPermission(userId, Controller, Action, RequiredActionType);

            if (!hasPermission)
            {
                // لاگ عدم دسترسی
                var log = new PermissionLog
                {
                    UserId = userId,
                    Controller = Controller,
                    Action = Action,
                    ActionType = RequiredActionType,
                    AccessGranted = false,
                    DenialReason = "کاربر دسترسی لازم را ندارد",
                    UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                roleRepository.LogPermissionAccess(log);

                // برگرداندن خطای عدم دسترسی
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // درخواست AJAX
                    context.Result = new JsonResult(new { 
                        success = false, 
                        message = "شما دسترسی لازم برای انجام این عملیات را ندارید" 
                    }) { StatusCode = 403 };
                }
                else
                {
                    // درخواست عادی
                    context.Result = new ViewResult
                    {
                        ViewName = "AccessDenied",
                        ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                            new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                            new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                        {
                            ["Controller"] = Controller,
                            ["Action"] = Action,
                            ["Message"] = "شما دسترسی لازم برای مشاهده این صفحه را ندارید"
                        }
                    };
                }
                return;
            }

            // لاگ دسترسی موفق
            var successLog = new PermissionLog
            {
                UserId = userId,
                Controller = Controller,
                Action = Action,
                ActionType = RequiredActionType,
                AccessGranted = true,
                UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            roleRepository.LogPermissionAccess(successLog);
        }
    }
}