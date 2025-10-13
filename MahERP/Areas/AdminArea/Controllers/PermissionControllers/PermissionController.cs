using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;

namespace MahERP.Areas.AdminArea.Controllers.PermissionControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.PERMISSION")]
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IPermissionService permissionService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _permissionService = permissionService;
        }

        // GET: Permission
        public async Task<IActionResult> Index()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Permission",
                    "Index",
                    "مشاهده لیست دسترسی‌ها"
                );

                return View(permissions);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "Index", "خطا در دریافت لیست دسترسی‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // GET: Permission/Tree
        public async Task<IActionResult> Tree()
        {
            try
            {
                var tree = await _permissionService.GetPermissionTreeAsync();
                return View(tree);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "Tree", "خطا در دریافت درخت دسترسی‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // GET: Permission/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? parentId)
        {
            ViewBag.ParentId = parentId;

            if (parentId.HasValue)
            {
                var parent = await _permissionService.GetPermissionByIdAsync(parentId.Value);
                ViewBag.ParentName = parent?.NameFa;

                var hierarchy = new List<Permission>();
                var current = parent;

                while (current != null)
                {
                    hierarchy.Insert(0, current);
                    current = current.ParentId.HasValue
                        ? await _permissionService.GetPermissionByIdAsync(current.ParentId.Value)
                        : null;
                }

                ViewBag.ParentHierarchy = hierarchy;
            }

            return View();
        }
        // POST: Permission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Permission model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی تکراری نبودن نام انگلیسی
                    if (await _permissionService.PermissionExistsAsync(model.NameEn))
                    {
                        ModelState.AddModelError("NameEn", "این نام انگلیسی قبلاً استفاده شده است");
                        return View(model);
                    }

                    // ✅ تنظیم مقادیر پیش‌فرض
                    model.Icon = model.Icon ?? "fa fa-key";
                    model.Color = model.Color ?? "#007bff";
                    model.DisplayOrder = model.DisplayOrder == 0 ? 100 : model.DisplayOrder;
                    model.IsActive = true;
                    model.IsSystemPermission = false;

                    var currentUserId = _userManager.GetUserId(User);
                    var result = await _permissionService.CreatePermissionAsync(model, currentUserId);

                    if (result)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Permission",
                            "Create",
                            $"ایجاد دسترسی جدید: {model.NameFa}",
                            recordId: model.Id.ToString(),
                            entityType: "Permission",
                            recordTitle: model.NameFa
                        );

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "خطا در ایجاد دسترسی");
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Permission", "Create", "خطا در ایجاد دسترسی", ex);
                    ModelState.AddModelError("", "خطا در ایجاد دسترسی");
                }
            }

            return View(model);
        }

        // GET: Permission/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                
                if (permission == null || permission.IsSystemPermission)
                    return RedirectToAction("ErrorView", "Home");

                return View(permission);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "Edit", "خطا در نمایش فرم ویرایش", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // POST: Permission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Permission model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _permissionService.PermissionExistsAsync(model.NameEn, model.Id))
                    {
                        ModelState.AddModelError("NameEn", "این نام انگلیسی قبلاً استفاده شده است");
                        return View(model);
                    }

                    var currentUserId = _userManager.GetUserId(User);
                    var result = await _permissionService.UpdatePermissionAsync(model, currentUserId);

                    if (result)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "Permission",
                            "Edit",
                            $"ویرایش دسترسی: {model.NameFa}",
                            recordId: model.Id.ToString(),
                            entityType: "Permission",
                            recordTitle: model.NameFa
                        );

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "خطا در ویرایش دسترسی");
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Permission", "Edit", "خطا در ویرایش دسترسی", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", "خطا در ویرایش دسترسی");
                }
            }

            return View(model);
        }

        // GET: Permission/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                
                if (permission == null || permission.IsSystemPermission)
                    return RedirectToAction("ErrorView", "Home");

                return PartialView("_DeletePermission", permission);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "Delete", "خطا در نمایش مودال حذف", ex, recordId: id.ToString());
                return Json(new { status = "error", message = "خطا در بارگذاری مودال" });
            }
        }

        // POST: Permission/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                
                if (permission == null)
                    return Json(new { status = "error", message = "دسترسی یافت نشد" });

                if (permission.IsSystemPermission)
                    return Json(new { status = "error", message = "نمی‌توان دسترسی سیستمی را حذف کرد" });

                // چک کردن وجود زیرمجموعه
                var hasChildren = await _permissionService.HasChildrenAsync(id);
                if (hasChildren)
                    return Json(new { status = "error", message = "ابتدا زیرمجموعه‌های این دسترسی را حذف کنید" });

                var result = await _permissionService.DeletePermissionAsync(id);
                
                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Permission",
                        "DeleteConfirmed",
                        $"حذف دسترسی: {permission.NameFa}",
                        recordId: id.ToString(),
                        entityType: "Permission",
                        recordTitle: permission.NameFa
                    );

                    return Json(new { status = "success", message = "دسترسی با موفقیت حذف شد" });
                }

                return Json(new { status = "error", message = "خطا در حذف دسترسی" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "DeleteConfirmed", "خطا در حذف دسترسی", ex, recordId: id.ToString());
                return Json(new { status = "error", message = "خطا در حذف دسترسی" });
            }
        }
    }
}