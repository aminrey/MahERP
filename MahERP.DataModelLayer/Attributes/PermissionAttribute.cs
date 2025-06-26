using MahERP.DataModelLayer.Services;
using Microsoft.AspNetCore.Identity;
using MahERP.DataModelLayer.Entities.AcControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;

namespace MahERP.DataModelLayer.Attributes
{
    public class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _controller;
        private readonly string _action;
        private readonly byte _actionType;

        public PermissionAttribute(string controller, string action, byte actionType = 0)
        {
            _controller = controller;
            _action = action;
            _actionType = actionType; // 0=Read, 1=Create, 2=Edit, 3=Delete, 4=Approve
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if user is not authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var serviceProvider = context.HttpContext.RequestServices;
            var roleRepository = serviceProvider.GetService<IRoleRepository>();
            var userManager = serviceProvider.GetService<UserManager<AppUsers>>();

            if (roleRepository == null || userManager == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var userId = userManager.GetUserId(context.HttpContext.User);

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Check if user has permission
            if (!roleRepository.HasPermission(userId, _controller, _action, _actionType))
            {
                // Log access attempt
                LogPermissionAccess(roleRepository, userId, _controller, _action, _actionType, false, "عدم دسترسی", context.HttpContext);

                context.Result = new ForbidResult();
                return;
            }

            // Log successful access
            LogPermissionAccess(roleRepository, userId, _controller, _action, _actionType, true, null, context.HttpContext);
        }

        private void LogPermissionAccess(IRoleRepository roleRepository, string userId, string controller, string action, byte actionType, bool granted, string denialReason, HttpContext httpContext)
        {
            try
            {
                var log = new PermissionLog
                {
                    UserId = userId,
                    Controller = controller,
                    Action = action,
                    ActionType = actionType,
                    AccessGranted = granted,
                    DenialReason = denialReason,
                    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext.Request.Headers["User-Agent"].ToString()
                };

                roleRepository.LogPermissionAccess(log);
            }
            catch
            {
                // Silent fail for logging
            }
        }
    }
}