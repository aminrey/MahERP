using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// مدیریت بخش‌های سازمان
    /// </summary>
    public partial class OrganizationsController
    {
        /// <summary>
        /// افزودن بخش جدید - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddDepartment(int organizationId, int? parentDepartmentId = null)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.ParentDepartmentId = parentDepartmentId;

                var model = new OrganizationDepartmentViewModel
                {
                    OrganizationId = organizationId,
                    ParentDepartmentId = parentDepartmentId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddDepartment", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ⭐ دریافت لیست اعضای سازمان به صورت JSON (برای Select2)
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public JsonResult GetOrganizationMembersJson(int organizationId)
        {
            try
            {
                // ⭐ دریافت اعضای سازمان از جدول OrganizationContact
                var organizationContacts = _organizationRepository.GetOrganizationContacts(organizationId, false);

                var members = organizationContacts.Select(oc => new
                {
                    contactId = oc.ContactId,
                    firstName = oc.Contact?.FirstName ?? "",
                    lastName = oc.Contact?.LastName ?? "",
                    nationalCode = oc.Contact?.NationalCode ?? "",
                    primaryPhone = oc.Contact?.Phones?.FirstOrDefault(p => p.IsDefault)?.FormattedNumber ?? 
                                  oc.Contact?.Phones?.FirstOrDefault()?.FormattedNumber ?? "",
                    relationTypeText = oc.RelationType switch
                    {
                        0 => "مدیرعامل",
                        1 => "مدیر",
                        2 => "حسابدار",
                        3 => "کارمند",
                        _ => "سایر"
                    }
                }).ToList();

                return Json(members);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ⭐ ذخیره بخش جدید با مدیر و اعضا (JSON API)
        /// </summary>
        [HttpPost]
        [PermissionRequired("ORG.EDIT")]
        public async Task<JsonResult> AddDepartment([FromBody] AddDepartmentRequest request)
        {
            try
            {
                // اعتبارسنجی
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return Json(new { success = false, message = "عنوان بخش الزامی است" });
                }

                var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
                if (organization == null)
                {
                    return Json(new { success = false, message = "سازمان یافت نشد" });
                }

                // ⭐ ایجاد بخش
                var department = new OrganizationDepartment
                {
                    OrganizationId = request.OrganizationId,
                    ParentDepartmentId = request.ParentDepartmentId,
                    Title = request.Title,
                    Description = request.Description,
                    ManagerContactId = request.ManagerContactId,
                    Code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatorUserId = GetUserId(),
                    CreatedDate = DateTime.Now
                };

                var createdDepartment = await _organizationRepository.CreateDepartmentAsync(department);

                // ⭐ افزودن اعضا (اگر انتخاب شده باشند)
                if (request.MemberContactIds != null && request.MemberContactIds.Any())
                {
                    foreach (var contactId in request.MemberContactIds)
                    {
                        if (!_organizationRepository.IsMemberAlreadyInDepartment(createdDepartment.Id, contactId))
                        {
                            var member = new DepartmentMember
                            {
                                DepartmentId = createdDepartment.Id,
                                ContactId = contactId,
                                PositionId = 0,
                                IsActive = true,
                                CreatorUserId = GetUserId(),
                                CreatedDate = DateTime.Now
                            };

                            await _organizationRepository.AddMemberToDepartmentAsync(member);
                        }
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Organizations",
                    "AddDepartment",
                    $"ایجاد بخش جدید: {department.Title}" +
                    (request.ManagerContactId.HasValue ? " با مدیر" : "") +
                    (request.MemberContactIds?.Any() == true ? $" و {request.MemberContactIds.Count} عضو" : ""),
                    recordId: createdDepartment.Id.ToString(),
                    entityType: "OrganizationDepartment",
                    recordTitle: department.Title
                );

                return Json(new 
                { 
                    success = true, 
                    status = "redirect",
                    message = "بخش با موفقیت ایجاد شد",
                    departmentId = createdDepartment.Id
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddDepartment", "خطا در ایجاد بخش", ex);
                return Json(new { success = false, message = "خطا در ذخیره: " + ex.Message });
            }
        }

        /// <summary>
        /// ⭐ افزودن چند عضو به بخش به صورت همزمان (JSON API)
        /// </summary>
        [HttpPost]
        [PermissionRequired("ORG.EDIT")]
        public async Task<JsonResult> AddMultipleMembersToDepartment([FromBody] AddMultipleMembersRequest request)
        {
            try
            {
                if (request.MembersList == null || !request.MembersList.Any())
                {
                    return Json(new { success = false, message = "هیچ عضوی انتخاب نشده است" });
                }

                var department = _organizationRepository.GetDepartmentById(request.DepartmentId);
                if (department == null)
                {
                    return Json(new { success = false, message = "بخش یافت نشد" });
                }

                int addedCount = 0;
                foreach (var memberData in request.MembersList)
                {
                    // بررسی تکراری نبودن
                    if (_organizationRepository.IsMemberAlreadyInDepartment(request.DepartmentId, memberData.ContactId))
                    {
                        continue;
                    }

                    var member = new DepartmentMember
                    {
                        DepartmentId = request.DepartmentId,
                        ContactId = memberData.ContactId,
                        PositionId = memberData.PositionId > 0 ? memberData.PositionId : 0,
                        Notes = memberData.Notes,
                        IsActive = true,
                        CreatorUserId = GetUserId(),
                        CreatedDate = DateTime.Now
                    };

                    await _organizationRepository.AddMemberToDepartmentAsync(member);
                    addedCount++;
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Organizations",
                    "AddMultipleMembersToDepartment",
                    $"افزودن {addedCount} عضو به بخش: {department.Title}",
                    recordId: department.Id.ToString(),
                    entityType: "DepartmentMember"
                );

                return Json(new
                {
                    success = true,
                    status = "redirect",
                    message = $"{addedCount} عضو با موفقیت اضافه شد",
                    addedCount = addedCount
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddMultipleMembersToDepartment", "خطا در افزودن اعضا", ex);
                return Json(new { success = false, message = "خطا در ذخیره: " + ex.Message });
            }
        }
    }

    /// <summary>
    /// ⭐ DTO برای دریافت اطلاعات بخش جدید
    /// </summary>
    public class AddDepartmentRequest
    {
        public int OrganizationId { get; set; }
        public int? ParentDepartmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ManagerContactId { get; set; }
        public List<int> MemberContactIds { get; set; }
    }

    /// <summary>
    /// ⭐ DTO برای افزودن چند عضو
    /// </summary>
    public class AddMultipleMembersRequest
    {
        public int DepartmentId { get; set; }
        public List<MemberDataItem> MembersList { get; set; }
    }

    public class MemberDataItem
    {
        public int ContactId { get; set; }
        public int PositionId { get; set; }
        public string Notes { get; set; }
    }
}
