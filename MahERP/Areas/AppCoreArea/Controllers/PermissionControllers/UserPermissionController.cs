using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.CommonLayer.ViewModels;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.PermissionControllers
{
    [Area("AppCoreArea")]
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
            IPermissionService permissionService, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
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

        /// <summary>
        /// ⭐⭐⭐ POST: UserPermission/AssignRole
        /// 
        /// تخصیص نقش به کاربر با استفاده از ResponseMessage برای پیام‌های استاندارد
        /// 
        /// 📖 توضیحات:
        /// - از ResponseMessage.CreateSuccessResponse برای پیام موفقیت
        /// - از ResponseMessage.CreateErrorResponse برای پیام خطا
        /// - پیام‌ها با SendResposeMessage در main.js نمایش داده می‌شوند
        /// 
        /// ⚙️ پارامترها:
        /// - userId: شناسه کاربر
        /// - roleId: شناسه نقش
        /// - startDate: تاریخ شروع (اختیاری)
        /// - endDate: تاریخ پایان (اختیاری)
        /// - notes: یادداشت (اختیاری)
        /// 
        /// 🔧 توسعه آینده:
        /// - می‌توان از ResponseMessage.CreateWarningResponse برای هشدارها استفاده کرد
        /// - می‌توان از ResponseMessage.CreateInfoResponse برای اطلاعات استفاده کرد
        /// 
        /// 📝 نکات:
        /// - بازگشت JSON با success و message
        /// - message از نوع List<WebResponseMessageViewModel> است
        /// </summary>
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

                    // ⭐ استفاده از ResponseMessage.CreateSuccessResponse
                    return Json(new
                    {
                        success = true,
                        message = ResponseMessage.CreateSuccessResponse(
                            $"نقش '{role.NameFa}' با موفقیت به کاربر '{user.FirstName} {user.LastName}' تخصیص داده شد",
                            "دسترسی‌ها به صورت خودکار همگام‌سازی شدند"
                        )
                    });
                }

                // ⭐ استفاده از ResponseMessage.CreateWarningResponse برای وضعیت تکراری
                return Json(new
                {
                    success = false,
                    message = ResponseMessage.CreateWarningResponse(
                        "این نقش قبلاً به کاربر تخصیص داده شده است"
                    )
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "AssignRole", "خطا در تخصیص نقش", ex, recordId: userId);

                // ⭐ استفاده از ResponseMessage.CreateErrorResponse
                return Json(new
                {
                    success = false,
                    message = ResponseMessage.CreateErrorResponse(
                        "خطا در تخصیص نقش",
                        ex.Message
                    )
                });
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

                    // ⭐ استفاده از ResponseMessage
                    return Json(new
                    {
                        success = true,
                        message = ResponseMessage.CreateSuccessResponse(
                            $"نقش '{role.NameFa}' با موفقیت از کاربر حذف شد"
                        )
                    });
                }

                return Json(new
                {
                    success = false,
                    message = ResponseMessage.CreateErrorResponse("خطا در حذف نقش")
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "RemoveRole", "خطا در حذف نقش", ex, recordId: userId);
                
                return Json(new
                {
                    success = false,
                    message = ResponseMessage.CreateErrorResponse("خطا در حذف نقش", ex.Message)
                });
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
                    UserName = user.UserName,
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
                        PermissionCode = up.Permission.Code,
                        SourceType = up.SourceType == 1 ? "Role" : (up.SourceType == 2 ? "Manual" : "Combined"),
                        SourceRoleId = up.SourceRoleId,
                        SourceRoleName = up.SourceRole?.NameFa,
                        IsManuallyModified = up.IsManuallyModified,
                        IsActive = up.IsActive
                    }).ToList(),
                    SelectedPermissionIds = userPermissionIds
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

                    // ⭐ استفاده از ResponseMessage
                    TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(
                        ResponseMessage.CreateSuccessResponse(
                            "دسترسی‌های کاربر با موفقیت به‌روزرسانی شد"
                        )
                    );

                    return RedirectToAction(nameof(ManageUserPermissions), new { userId = model.UserId });
                }

                ModelState.AddModelError("", "خطا در به‌روزرسانی دسترسی‌ها");
                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "ManageUserPermissions", "خطا در به‌روزرسانی دسترسی‌های کاربر", ex, recordId: model.UserId);

                // ⭐ استفاده از ResponseMessage
                TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(
                    ResponseMessage.CreateErrorResponse(
                        "خطا در به‌روزرسانی دسترسی‌ها",
                        ex.Message
                    )
                );

                return View(model);
            }
        }

        // GET: UserPermission/DeleteRoleConfirmation
        [HttpGet]
        public async Task<IActionResult> DeleteRoleConfirmation(string userId, int roleId)
        {
            try
            {
                var userRole = await _userPermissionService.GetUserRoleForDeleteAsync(userId, roleId);

                if (userRole == null)
                {
                    return Json(new { status = "error", message = "نقش یافت نشد" });
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserPermission",
                    "DeleteRoleConfirmation",
                    $"نمایش مودال حذف نقش '{userRole.Role.NameFa}' از کاربر '{userRole.User.FirstName} {userRole.User.LastName}'",
                    recordId: userId,
                    entityType: "UserRole"
                );

                return PartialView("_DeleteRoleConfirmation", userRole);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("UserPermission", "DeleteRoleConfirmation",
                    "خطا در نمایش مودال حذف نقش", ex, recordId: userId);
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }

        // POST: UserPermission/RemoveRoleConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRoleConfirmed(string userId, int roleId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _roleService.GetRoleByIdAsync(roleId);

                if (user == null || role == null)
                {
                    TempData["ErrorMessage"] = "کاربر یا نقش یافت نشد";
                    return RedirectToAction(nameof(ManageUserRoles), new { userId });
                }

                var result = await _userPermissionService.RemoveRoleFromUserAsync(userId, roleId);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "UserPermission",
                        "RemoveRoleConfirmed",
                        $"حذف نقش '{role.NameFa}' از کاربر '{user.FirstName} {user.LastName}' (از طریق Modal)",
                        recordId: userId,
                        entityType: "UserRole",
                        recordTitle: $"{user.FirstName} {user.LastName}"
                    );

                    TempData["SuccessMessage"] = $"نقش '{role.NameFa}' با موفقیت از کاربر حذف شد";
                    return RedirectToAction(nameof(ManageUserRoles), new { userId });
                }
                else
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Error,
                        "UserPermission",
                        "RemoveRoleConfirmed",
                        $"خطا در حذف نقش '{role.NameFa}' از کاربر '{user.FirstName} {user.LastName}'",
                        recordId: userId,
                        entityType: "UserRole"
                    );

                    TempData["ErrorMessage"] = "خطا در حذف نقش";
                    return RedirectToAction(nameof(ManageUserRoles), new { userId });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserPermission",
                    "RemoveRoleConfirmed",
                    "خطا در حذف نقش از کاربر",
                    ex,
                    recordId: userId
                );

                TempData["ErrorMessage"] = $"خطا در حذف نقش: {ex.Message}";
                return RedirectToAction(nameof(ManageUserRoles), new { userId });
            }
        }
    }
}