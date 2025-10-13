using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.Areas.AdminArea.Controllers.PermissionControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.USER.PERMISSION")]
    public class UserPermissionController : BaseController
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly IUserRoleRepository _roleService;
        private readonly IPermissionService _permissionService;

        public UserPermissionController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IUserPermissionService userPermissionService,
            IUserRoleRepository roleService,
            IPermissionService permissionService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _userPermissionService = userPermissionService;
            _roleService = roleService;
            _permissionService = permissionService;
        }

        // GET: UserPermission/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = _userManager.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToList();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserPermission",
                    "Index",
                    "مشاهده لیست کاربران برای مدیریت دسترسی"
                );

                return View(users);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "Index", "خطا در دریافت لیست کاربران", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // GET: UserPermission/ManageUserRoles/userId
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return RedirectToAction("ErrorView", "Home");

                var userRoles = await _userPermissionService.GetUserRolesAsync(userId);
                var allRoles = await _roleService.GetAllRolesAsync();

                ViewBag.UserId = userId;
                ViewBag.UserFullName = $"{user.FirstName} {user.LastName}";
                ViewBag.AvailableRoles = new SelectList(allRoles, "Id", "NameFa");

                return View(userRoles);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "ManageUserRoles", "خطا در نمایش نقش‌های کاربر", ex, recordId: userId);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // POST: UserPermission/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, int roleId, DateTime? startDate, DateTime? endDate, string notes)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var result = await _userPermissionService.AssignRoleToUserAsync(userId, roleId, currentUserId, startDate, endDate);

                if (result)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    var role = await _roleService.GetRoleByIdAsync(roleId);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "UserPermission",
                        "AssignRole",
                        $"تخصیص نقش '{role.NameFa}' به کاربر '{user.FirstName} {user.LastName}'",
                        recordId: userId,
                        entityType: "UserRole",
                        recordTitle: $"{user.FirstName} {user.LastName}"
                    );

                    return Json(new { success = true, message = "نقش با موفقیت تخصیص داده شد و دسترسی‌ها همگام‌سازی شدند" });
                }

                return Json(new { success = false, message = "خطا در تخصیص نقش یا این نقش قبلاً به کاربر داده شده است" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "AssignRole", "خطا در تخصیص نقش", ex, recordId: userId);
                return Json(new { success = false, message = "خطا در تخصیص نقش" });
            }
        }

        // POST: UserPermission/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, int roleId)
        {
            try
            {
                var result = await _userPermissionService.RemoveRoleFromUserAsync(userId, roleId);

                if (result)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    var role = await _roleService.GetRoleByIdAsync(roleId);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "UserPermission",
                        "RemoveRole",
                        $"حذف نقش '{role.NameFa}' از کاربر '{user.FirstName} {user.LastName}'",
                        recordId: userId,
                        entityType: "UserRole",
                        recordTitle: $"{user.FirstName} {user.LastName}"
                    );

                    return Json(new { success = true, message = "نقش با موفقیت حذف شد" });
                }

                return Json(new { success = false, message = "خطا در حذف نقش" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "RemoveRole", "خطا در حذف نقش", ex, recordId: userId);
                return Json(new { success = false, message = "خطا در حذف نقش" });
            }
        }

        // GET: UserPermission/ManageUserPermissions/userId
        [HttpGet]
        public async Task<IActionResult> ManageUserPermissions(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return RedirectToAction("ErrorView", "Home");

                var userRoles = await _userPermissionService.GetUserRolesAsync(userId);
                var userPermissions = await _userPermissionService.GetUserPermissionsAsync(userId);
                var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToList();
                var permissionTree = await _permissionService.BuildPermissionTreeAsync(userPermissionIds);

                var viewModel = new ManageUserPermissionsViewModel
                {
                    UserId = userId,
                    UserFullName = $"{user.FirstName} {user.LastName}",
                    UserName = user.UserName, // ✅ اضافه شد
                    UserRoles = userRoles.Select(ur => new UserRoleInfo
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.NameFa,
                        RoleColor = ur.Role.Color,
                        RoleIcon = ur.Role.Icon,
                        IsActive = ur.IsActive
                    }).ToList(),
                    PermissionTree = permissionTree,
                    UserPermissions = userPermissions.Select(up => new UserPermissionDetail
                    {
                        PermissionId = up.PermissionId,
                        PermissionName = up.Permission.NameFa,
                        PermissionCode = up.Permission.Code, // ✅ اضافه شد
                        SourceType = up.SourceType == 1 ? "Role" : (up.SourceType == 2 ? "Manual" : "Combined"),
                        SourceRoleId = up.SourceRoleId,
                        SourceRoleName = up.SourceRole?.NameFa,
                        IsManuallyModified = up.IsManuallyModified,
                        IsActive = up.IsActive
                    }).ToList(),
                    SelectedPermissionIds = userPermissionIds // ✅ اضافه شد
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "ManageUserPermissions", "خطا در نمایش دسترسی‌های کاربر", ex, recordId: userId);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // POST: UserPermission/ManageUserPermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserPermissions(ManageUserPermissionsViewModel model)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                
                // ✅ استفاده از متد جدید Repository
                var result = await _userPermissionService.ManageUserPermissionsAsync(
                    model.UserId, 
                    model.SelectedPermissionIds, 
                    currentUserId
                );

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "UserPermission",
                        "ManageUserPermissions",
                        $"به‌روزرسانی دسترسی‌های کاربر: {model.UserFullName}",
                        recordId: model.UserId,
                        entityType: "UserPermission",
                        recordTitle: model.UserFullName
                    );

                    TempData["SuccessMessage"] = "دسترسی‌های کاربر با موفقیت به‌روزرسانی شد";
                    return RedirectToAction(nameof(ManageUserPermissions), new { userId = model.UserId });
                }

                ModelState.AddModelError("", "خطا در به‌روزرسانی دسترسی‌ها");
                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "ManageUserPermissions", "خطا در به‌روزرسانی دسترسی‌های کاربر", ex, recordId: model.UserId);
                ModelState.AddModelError("", "خطا در به‌روزرسانی دسترسی‌ها");
                return View(model);
            }
        }
    }
}