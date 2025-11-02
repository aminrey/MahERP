using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.ViewModels;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.PermissionControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.PERMISSION")]
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PermissionController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IPermissionService permissionService,
            IWebHostEnvironment webHostEnvironment, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking)
        {
            _permissionService = permissionService;
            _webHostEnvironment = webHostEnvironment;
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
                {
                    return Json(new
                    {
                        status = "error",
                        message = "دسترسی یافت نشد"
                    });
                }

                if (permission.IsSystemPermission)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "نمی‌توان دسترسی سیستمی را حذف کرد"
                    });
                }

                // چک کردن وجود زیرمجموعه
                var hasChildren = await _permissionService.HasChildrenAsync(id);
                if (hasChildren)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "ابتدا زیرمجموعه‌های این دسترسی را حذف کنید"
                    });
                }

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

                    // ✅ بازگشت با status = "update-view" برای حذف ردیف از جدول
                    return Json(new
                    {
                        status = "update-view",
                        message = new[]
                        {
            new { status = "success", text = "دسترسی با موفقیت حذف شد" }
        },
                        viewList = new[]
                        {
            new
            {
                elementId = "permissionRow_" + id,
                view = new { result = "" }, // خالی = حذف عنصر
                appendMode = false
            }
        }
                    });
                }
                return Json(new
                {
                    status = "error",
                    message = "خطا در حذف دسترسی"
                });

            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Permission", "DeleteConfirmed", "خطا در حذف دسترسی", ex, recordId: id.ToString());

                // ✅ بازگشت JSON با پیام خطا
                return Json(new
                {
                    status = "error",
                    message = $"خطا در حذف دسترسی: {ex.Message}"
                });
            }

        }

        /// <summary>
        /// ⭐⭐⭐ بارگذاری دسترسی‌ها از فایل JSON
        /// 
        /// 📖 توضیحات عملکرد:
        /// این متد دسترسی‌های استاندارد سیستم را از فایل Permissions.json در مسیر /Data/SeedData/ می‌خواند
        /// و آن‌ها را در دیتابیس ذخیره می‌کند.
        /// 
        /// ⚙️ نحوه کار:
        /// 1. فایل JSON را از مسیر مشخص شده می‌خواند
        /// 2. محتوای JSON را Parse می‌کند و به لیست DTO تبدیل می‌کند
        /// 3. در دو مرحله پردازش می‌کند:
        ///    - مرحله اول: دسترسی‌های بدون والد (Root) را ایجاد/بروزرسانی می‌کند
        ///    - مرحله دوم: دسترسی‌های دارای والد را به صورت بازگشتی پردازش می‌کند
        /// 4. برای هر دسترسی:
        ///    - اگر Code تکراری باشد → به‌روزرسانی می‌شود
        ///    - اگر جدید باشد → ایجاد می‌شود
        /// 5. ساختار درختی (Parent-Child) را حفظ می‌کند
        /// 6. آمار کامل پردازش را برمی‌گرداند
        /// 
        /// 🔧 توسعه آینده:
        /// - می‌توان گزینه "حذف دسترسی‌های اضافی" را اضافه کرد
        /// - می‌توان Validation بیشتری روی داده‌های JSON اضافه کرد
        /// - می‌توان قابلیت Import از چند فایل مختلف را اضافه کرد
        /// - می‌توان گزارش دقیق‌تر از تغییرات (Diff) ارائه داد
        /// - می‌توان قابلیت Rollback در صورت خطا را اضافه کرد
        /// 
        /// 📝 نکات مهم:
        /// - دسترسی‌های موجود در دیتابیس که در JSON نیستند، حذف نمی‌شوند
        /// - Code به عنوان شناسه یکتا استفاده می‌شود
        /// - حداکثر 10 سطح عمق برای جلوگیری از حلقه بی‌نهایت
        /// - تمام عملیات از طریق Repository Pattern انجام می‌شود
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportPermissionsFromJson()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ مرحله 1: خواندن فایل JSON
                var jsonPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Data", "SeedData", "Permissions.json");

                if (!System.IO.File.Exists(jsonPath))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"فایل Permissions.json در مسیر {jsonPath} یافت نشد"
                    });
                }

                var jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);

                // ⭐ مرحله 2: Parse JSON
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var permissionsArray = jsonDocument.RootElement.GetProperty("Permissions");

                var permissionsToImport = new List<PermissionImportDto>();

                foreach (var item in permissionsArray.EnumerateArray())
                {
                    permissionsToImport.Add(new PermissionImportDto
                    {
                        Code = item.GetProperty("Code").GetString(),
                        NameEn = item.GetProperty("NameEn").GetString(),
                        NameFa = item.GetProperty("NameFa").GetString(),
                        Description = item.TryGetProperty("Description", out var desc) ? desc.GetString() : null,
                        Icon = item.TryGetProperty("Icon", out var icon) ? icon.GetString() : "fa fa-key",
                        Color = item.TryGetProperty("Color", out var color) ? color.GetString() : "#007bff",
                        ParentCode = item.TryGetProperty("ParentCode", out var parentCode) && parentCode.ValueKind != JsonValueKind.Null
                            ? parentCode.GetString()
                            : null,
                        DisplayOrder = item.GetProperty("DisplayOrder").GetInt32(),
                        IsSystemPermission = item.GetProperty("IsSystemPermission").GetBoolean()
                    });
                }

                // ⭐⭐⭐ مرحله 3: پردازش و ذخیره در دیتابیس (با استفاده از Repository)
                int newCount = 0;
                int updatedCount = 0;
                var processedCodes = new Dictionary<string, int>(); // Code -> Id

                // ⭐ گام 1: ایجاد/بروزرسانی دسترسی‌های سطح اول (بدون والد)
                foreach (var dto in permissionsToImport.Where(p => string.IsNullOrEmpty(p.ParentCode)))
                {
                    var result = await _permissionService.UpsertPermissionAsync(
                        code: dto.Code,
                        nameEn: dto.NameEn,
                        nameFa: dto.NameFa,
                        description: dto.Description,
                        icon: dto.Icon,
                        color: dto.Color,
                        parentId: null,
                        displayOrder: dto.DisplayOrder,
                        isSystemPermission: dto.IsSystemPermission,
                        currentUserId: currentUserId
                    );

                    if (result.success)
                    {
                        processedCodes[dto.Code] = result.permissionId;

                        if (result.isNew)
                            newCount++;
                        else
                            updatedCount++;
                    }
                }

                // ⭐ گام 2: ایجاد/بروزرسانی دسترسی‌های دارای والد (بازگشتی)
                var remainingPermissions = permissionsToImport.Where(p => !string.IsNullOrEmpty(p.ParentCode)).ToList();
                int maxIterations = 10; // جلوگیری از حلقه بی‌نهایت
                int iteration = 0;

                while (remainingPermissions.Any() && iteration < maxIterations)
                {
                    iteration++;
                    var processedInThisRound = new List<PermissionImportDto>();

                    foreach (var dto in remainingPermissions)
                    {
                        // اگر والد پردازش شده باشد
                        if (processedCodes.ContainsKey(dto.ParentCode))
                        {
                            var result = await _permissionService.UpsertPermissionAsync(
                                code: dto.Code,
                                nameEn: dto.NameEn,
                                nameFa: dto.NameFa,
                                description: dto.Description,
                                icon: dto.Icon,
                                color: dto.Color,
                                parentId: processedCodes[dto.ParentCode],
                                displayOrder: dto.DisplayOrder,
                                isSystemPermission: dto.IsSystemPermission,
                                currentUserId: currentUserId
                            );

                            if (result.success)
                            {
                                processedCodes[dto.Code] = result.permissionId;

                                if (result.isNew)
                                    newCount++;
                                else
                                    updatedCount++;

                                processedInThisRound.Add(dto);
                            }
                        }
                    }

                    // حذف پردازش‌شده‌ها از لیست
                    remainingPermissions = remainingPermissions.Except(processedInThisRound).ToList();

                    if (!processedInThisRound.Any())
                        break; // اگر هیچ‌کدام پردازش نشد، خروج
                }

                stopwatch.Stop();

                // ⭐ ثبت در Activity Log
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Permission",
                    "ImportPermissionsFromJson",
                    $"بارگذاری دسترسی‌ها از JSON: {newCount} جدید، {updatedCount} به‌روزرسانی",
                    entityType: "Permission"
                );

                return Json(new
                {
                    success = true,
                    totalProcessed = newCount + updatedCount,
                    newPermissions = newCount,
                    updatedPermissions = updatedCount,
                    processingTime = stopwatch.Elapsed.TotalSeconds.ToString("F2") + " ثانیه"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _activityLogger.LogErrorAsync("Permission", "ImportPermissionsFromJson",
                    "خطا در بارگذاری دسترسی‌ها از JSON", ex);

                return Json(new
                {
                    success = false,
                    message = $"خطا در پردازش: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// ⭐ DTO برای Import دسترسی‌ها از JSON
    /// 
    /// 📖 توضیحات:
    /// این کلاس برای انتقال داده از فایل JSON به سیستم استفاده می‌شود.
    /// 
    /// 🔍 تفاوت با Permission Entity:
    /// - از ParentCode به جای ParentId استفاده می‌کند (برای پردازش راحت‌تر)
    /// - فیلدهای مدیریتی (CreateDate, CreatorUserId و ...) ندارد
    /// - Navigation Properties ندارد
    /// 
    /// 🔧 توسعه آینده:
    /// - می‌توان فیلدهای بیشتری مثل Priority اضافه کرد
    /// - می‌توان Validation Attributes اضافه کرد
    /// - می‌توان متد Validate() برای بررسی اعتبار داده اضافه کرد
    /// </summary>
    internal class PermissionImportDto
    {
        /// <summary>کد یکتا دسترسی (مثل: TASK.CREATE)</summary>
        public string Code { get; set; }

        /// <summary>نام انگلیسی</summary>
        public string NameEn { get; set; }

        /// <summary>نام فارسی</summary>
        public string NameFa { get; set; }

        /// <summary>توضیحات</summary>
        public string Description { get; set; }

        /// <summary>آیکون FontAwesome</summary>
        public string Icon { get; set; }

        /// <summary>رنگ (Hex Color)</summary>
        public string Color { get; set; }

        /// <summary>کد دسترسی والد (برای ساختار درختی)</summary>
        public string ParentCode { get; set; }

        /// <summary>ترتیب نمایش</summary>
        public int DisplayOrder { get; set; }

        /// <summary>آیا دسترسی سیستمی است؟</summary>
        public bool IsSystemPermission { get; set; }
    }
}