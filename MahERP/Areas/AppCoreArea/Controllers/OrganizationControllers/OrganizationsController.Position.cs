using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// مدیریت سمت‌ها
    /// </summary>
    public partial class OrganizationsController
    {
        /// <summary>
        /// مدیریت سمت‌ها
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> ManagePositions(int departmentId)
        {
            try
            {
                var department = _organizationRepository.GetDepartmentById(departmentId, includePositions: true);
                if (department == null)
                    return RedirectToAction("ErrorView", "Home");

                var positions = _organizationRepository.GetDepartmentPositions(departmentId, includeInactive: false);
                var viewModels = _mapper.Map<List<DepartmentPositionViewModel>>(positions);

                ViewBag.DepartmentId = departmentId;
                ViewBag.DepartmentTitle = department.Title;
                ViewBag.OrganizationId = department.OrganizationId;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "ManagePositions", "خطا در نمایش سمت‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// افزودن سمت جدید - Modal
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public IActionResult AddPositionModal(int departmentId)
        {
            var department = _organizationRepository.GetDepartmentById(departmentId);

            ViewBag.DepartmentTitle = department?.Title;

            // ⭐ دریافت سمت‌های استاندارد از دیتابیس (seed شده توسط SystemSeedDataRepository)
            var standardPositions = _positionRepository.GetCommonPositions();

            // ⭐ اگر دیتابیس خالی بود، از StaticPositionSeedData استفاده کن (Fallback)
            if (standardPositions == null || !standardPositions.Any())
            {
                standardPositions = MahERP.DataModelLayer.StaticClasses.StaticPositionSeedData.CommonPositions
                    .Where(p => p.IsActive && p.IsCommon)
                    .ToList();
            }

            // ⭐ گروه‌بندی بر اساس دسته‌بندی
            ViewBag.StandardPositions = standardPositions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayOrder)
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new
                    {
                        Id = p.Id,
                        Title = p.Title,
                        TitleEnglish = p.TitleEnglish,
                        Category = p.Category,
                        DefaultPowerLevel = p.DefaultPowerLevel
                    }).ToList<dynamic>()
                );

            var model = new DepartmentPositionViewModel
            {
                DepartmentId = departmentId,
                IsActive = true,
                DisplayOrder = 1,
                PowerLevel = 50
            };

            return PartialView("_AddPositionModal", model);
        }

        /// <summary>
        /// ذخیره سمت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddPosition(DepartmentPositionViewModel model)
        {
            // ⭐ اگر از سمت استاندارد انتخاب شده، Title را از آن بگیر
            if (model.BasePositionId.HasValue && model.BasePositionId.Value > 0 && string.IsNullOrWhiteSpace(model.Title))
            {
                var standardPosition = _positionRepository.GetPositionById(model.BasePositionId.Value);
                if (standardPosition != null)
                {
                    model.Title = standardPosition.Title;
                    model.PowerLevel = standardPosition.DefaultPowerLevel;
                }
            }

            // ⭐ حذف validation error برای Title اگر از استاندارد انتخاب شده
            if (model.BasePositionId.HasValue && model.BasePositionId.Value > 0)
            {
                ModelState.Remove("Title");
            }

            if (ModelState.IsValid || (model.BasePositionId.HasValue && model.BasePositionId.Value > 0))
            {
                try
                {
                    var position = _mapper.Map<DepartmentPosition>(model);
                    position.CreatorUserId = GetUserId();
                    position.IsActive = true;

                    // ⭐ اگر Title خالی است و BasePositionId دارد
                    if (string.IsNullOrWhiteSpace(position.Title) && model.BasePositionId.HasValue && model.BasePositionId.Value > 0)
                    {
                        var standardPosition = _positionRepository.GetPositionById(model.BasePositionId.Value);
                        if (standardPosition != null)
                        {
                            position.Title = standardPosition.Title;
                        }
                    }

                    var createdPosition = await _organizationRepository.CreatePositionAsync(position);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddPosition",
                        $"افزودن سمت جدید: {position.Title}",
                        recordId: createdPosition.Id.ToString()
                    );

                    // ⭐ استفاده از redirect برای reload صفحه
                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("ManagePositions", "Organizations", new { departmentId = model.DepartmentId }),
                        message = new[] { new { status = "success", text = "سمت با موفقیت اضافه شد" } }
                    });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddPosition", "خطا در افزودن سمت", ex);
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "خطا در ذخیره: " + ex.Message } }
                    });
                }
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => new { status = "error", text = e.ErrorMessage })
                .ToArray();

            return Json(new
            {
                status = "validation-error",
                message = errors
            });
        }

        /// <summary>
        /// ⭐ حذف سمت (JSON API)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<JsonResult> DeletePosition(int id)
        {
            try
            {
                var position = _organizationRepository.GetPositionById(id);
                if (position == null)
                    return Json(new { success = false, message = "سمت یافت نشد" });

                // بررسی استفاده
                if (!_organizationRepository.CanDeletePosition(id))
                    return Json(new { success = false, message = "این سمت دارای عضو فعال است و قابل حذف نیست" });

                var deleted = await _organizationRepository.DeletePositionAsync(id);

                if (deleted)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Organizations",
                        "DeletePosition",
                        $"حذف سمت: {position.DisplayTitle}",
                        recordId: id.ToString()
                    );

                    return Json(new { success = true, message = "سمت با موفقیت حذف شد" });
                }

                return Json(new { success = false, message = "خطا در حذف سمت" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "DeletePosition", "خطا در حذف سمت", ex);
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        /// <summary>
        /// ⭐ دریافت لیست سمت‌های استاندارد به صورت JSON
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public JsonResult GetStandardPositionsJson()
        {
            try
            {
                var positions = _positionRepository.GetCommonPositions();
                var result = positions.Select(p => new
                {
                    id = p.Id,
                    title = p.FullTitle,
                    category = p.Category,
                    description = p.Description,
                    defaultPowerLevel = p.DefaultPowerLevel,
                    level = p.Level
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ⭐ دریافت جزئیات یک سمت استاندارد
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public JsonResult GetPositionDetailsJson(int id)
        {
            try
            {
                var position = _positionRepository.GetPositionById(id);
                if (position == null)
                    return Json(new { error = "سمت یافت نشد" });

                var result = new
                {
                    id = position.Id,
                    title = position.FullTitle,
                    category = position.Category,
                    description = position.Description,
                    defaultPowerLevel = position.DefaultPowerLevel,
                    level = position.Level
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
