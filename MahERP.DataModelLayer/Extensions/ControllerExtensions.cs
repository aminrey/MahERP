using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace MahERP.DataModelLayer.Extensions
{
    public static class ControllerExtensions
    {
        public static bool HasPermission(this Controller controller, string controllerName, string actionName, byte actionType = 0)
        {
            var roleRepository = controller.HttpContext.RequestServices.GetService<IRoleRepository>();
            var userManager = controller.HttpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (roleRepository == null || userManager == null)
                return false;

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return false;

            return roleRepository.HasPermission(userId, controllerName, actionName, actionType);
        }

        public static byte GetUserDataAccessLevel(this Controller controller, string controllerName, string actionName)
        {
            var roleRepository = controller.HttpContext.RequestServices.GetService<IRoleRepository>();
            var userManager = controller.HttpContext.RequestServices.GetService<UserManager<AppUsers>>();

            if (roleRepository == null || userManager == null)
                return 0; // Personal level only

            var userId = userManager.GetUserId(controller.User);
            if (string.IsNullOrEmpty(userId))
                return 0;

            var userRolePatterns = roleRepository.GetUserRolePatterns(userId);
            
            byte maxAccessLevel = 0;
            foreach (var userRolePattern in userRolePatterns)
            {
                var details = roleRepository.GetRolePatternDetails(userRolePattern.RolePatternId)
                    .Where(rpd => rpd.ControllerName == controllerName && rpd.ActionName == actionName)
                    .ToList();

                foreach (var detail in details)
                {
                    if (detail.DataAccessLevel > maxAccessLevel)
                        maxAccessLevel = detail.DataAccessLevel;
                }
            }

            return maxAccessLevel;
        }
    }
}