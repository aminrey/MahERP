using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// مدیریت اعضای بخش
    /// </summary>
    public partial class OrganizationsController
    {
        /// <summary>
        /// افزودن عضو به بخش - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddMember(int departmentId)
        {
            try
            {
                var department = _organizationRepository.GetDepartmentById(departmentId);
                if (department == null)
                    return RedirectToAction("ErrorView", "Home");

                var contacts = _contactRepository.GetAllContacts(includeInactive: false);
                var positions = _organizationRepository.GetDepartmentPositions(departmentId, includeInactive: false);

                ViewBag.DepartmentId = departmentId;
                ViewBag.DepartmentTitle = department.Title;
                ViewBag.OrganizationId = department.OrganizationId; // ⭐ اضافه شده - برای دکمه‌های بازگشت و انصراف
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                ViewBag.Positions = new SelectList(positions, "Id", "Title");

                var model = new DepartmentMemberViewModel
                {
                    DepartmentId = departmentId,
                    IsActive = true,
                    JoinDatePersian = ConvertDateTime.ConvertMiladiToShamsi(DateTime.Now, "yyyy/MM/dd")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddMember", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره عضو جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddMember(DepartmentMemberViewModel model)
        {
            if (model.PositionId == 0)
            {
                ModelState.Remove(nameof(model.PositionId));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_organizationRepository.IsContactMemberOfDepartment(model.ContactId, model.DepartmentId))
                    {
                        ModelState.AddModelError("", "این شخص قبلاً به این بخش اضافه شده است");

                        var department = _organizationRepository.GetDepartmentById(model.DepartmentId, includePositions: true);
                        var contacts = _contactRepository.GetAllContacts(includeInactive: false);
                        var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);

                        ViewBag.DepartmentId = model.DepartmentId;
                        ViewBag.DepartmentTitle = department?.Title;
                        ViewBag.OrganizationId = department?.OrganizationId; // ⭐ اضافه شده
                        ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                        ViewBag.Positions = new SelectList(positions, "Id", "Title");

                        return View(model);
                    }

                    var member = _mapper.Map<DepartmentMember>(model);
                    member.CreatorUserId = GetUserId();

                    if (member.PositionId == 0)
                    {
                        member.PositionId = null;
                    }

                    var memberId = await _organizationRepository.AddMemberToDepartmentAsync(member);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddMember",
                        $"افزودن عضو به بخش",
                        recordId: memberId.ToString()
                    );

                    TempData["SuccessMessage"] = "عضو با موفقیت اضافه شد";

                    var dept = _organizationRepository.GetDepartmentById(model.DepartmentId);
                    return RedirectToAction(nameof(OrganizationChart), new { organizationId = dept.OrganizationId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddMember", "خطا در افزودن عضو", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            {
                var department = _organizationRepository.GetDepartmentById(model.DepartmentId, includePositions: true);
                var contacts = _contactRepository.GetAllContacts(includeInactive: false);
                var positions = _organizationRepository.GetDepartmentPositions(model.DepartmentId, includeInactive: false);

                ViewBag.DepartmentId = model.DepartmentId;
                ViewBag.DepartmentTitle = department?.Title;
                ViewBag.OrganizationId = department?.OrganizationId; // ⭐ اضافه شده
                ViewBag.AvailableContacts = new SelectList(contacts, "Id", "FullName");
                ViewBag.Positions = new SelectList(positions, "Id", "Title");
            }

            return View(model);
        }
    }
}
