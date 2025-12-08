using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
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
            if (ModelState.IsValid)
            {
                try
                {
                    var position = _mapper.Map<DepartmentPosition>(model);
                    position.CreatorUserId = GetUserId();

                    var createdPosition = await _organizationRepository.CreatePositionAsync(position);

                    var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);
                    var viewModels = _mapper.Map<List<DepartmentPositionViewModel>>(positions);
                    var renderedView = await this.RenderViewToStringAsync("_PositionsTableRows", viewModels);

                    return Json(new
                    {
                        status = "update-view",
                        message = new[] { new { status = "success", text = "سمت با موفقیت اضافه شد" } },
                        viewList = new[]
                        {
                            new
                            {
                                elementId = "positionsTableBody",
                                view = new { result = renderedView }
                            }
                        }
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
    }
}
