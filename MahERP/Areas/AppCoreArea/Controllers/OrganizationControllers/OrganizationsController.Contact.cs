using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// مدیریت افراد مرتبط با سازمان
    /// </summary>
    public partial class OrganizationsController
    {
        /// <summary>
        /// افزودن عضو/فرد به سازمان - نمایش فرم
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddOrganizationContact(int organizationId)
        {
            try
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                // ⭐⭐⭐ دریافت افراد با اطلاعات کامل و گروه‌بندی شده
                var contacts = _contactRepository.GetAllContacts(includeInactive: false)
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                // ⭐ ایجاد لیست با اطلاعات کامل (نام خانوادگی، نام، توضیحات تا 50 کاراکتر، کد ملی)
                var contactOptions = contacts.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.LastName} {c.FirstName ?? ""}" +
                           (!string.IsNullOrEmpty(c.Notes) ? $" - {(c.Notes.Length > 50 ? c.Notes.Substring(0, 50) + "..." : c.Notes)}" : "") +
                           (!string.IsNullOrEmpty(c.NationalCode) ? $" | کد ملی: {c.NationalCode}" : ""),
                    Group = new SelectListGroup { Name = GetFirstLetter(c.LastName) }
                }).ToList();

                ViewBag.OrganizationId = organizationId;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.AvailableContacts = contactOptions;

                var model = new OrganizationContactViewModel
                {
                    OrganizationId = organizationId,
                    IsActive = true,
                    IsPrimary = false,
                    ImportanceLevel = 50,
                    RelationType = 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddOrganizationContact", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره عضو/فرد جدید به سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> AddOrganizationContact(OrganizationContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingContact = _organizationRepository.GetOrganizationContacts(model.OrganizationId, false)
                        .FirstOrDefault(oc => oc.ContactId == model.ContactId && oc.IsActive);

                    if (existingContact != null)
                    {
                        ModelState.AddModelError("", "این شخص قبلاً به این سازمان اضافه شده است");

                        var organization = await _organizationRepository.GetOrganizationByIdAsync(model.OrganizationId);
                        var contacts = _contactRepository.GetAllContacts(includeInactive: false)
                            .OrderBy(c => c.LastName)
                            .ThenBy(c => c.FirstName)
                            .ToList();

                        var contactOptions = contacts.Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = $"{c.LastName} {c.FirstName ?? ""}" +
                                   (!string.IsNullOrEmpty(c.Notes) ? $" - {(c.Notes.Length > 50 ? c.Notes.Substring(0, 50) + "..." : c.Notes)}" : "") +
                                   (!string.IsNullOrEmpty(c.NationalCode) ? $" | کد ملی: {c.NationalCode}" : ""),
                            Group = new SelectListGroup { Name = GetFirstLetter(c.LastName) }
                        }).ToList();

                        ViewBag.OrganizationId = model.OrganizationId;
                        ViewBag.OrganizationName = organization?.DisplayName;
                        ViewBag.AvailableContacts = contactOptions;

                        return View(model);
                    }

                    var organizationContact = _mapper.Map<OrganizationContact>(model);
                    organizationContact.CreatorUserId = GetUserId();

                    var contactId = await _organizationRepository.AddContactToOrganizationAsync(organizationContact);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "AddOrganizationContact",
                        $"افزودن فرد به سازمان",
                        recordId: contactId.ToString()
                    );

                    TempData["SuccessMessage"] = "فرد با موفقیت به سازمان اضافه شد";
                    return RedirectToAction(nameof(Details), new { id = model.OrganizationId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "AddOrganizationContact", "خطا در افزودن فرد", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // بازگشت به فرم در صورت خطا
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(model.OrganizationId);
                var contacts = _contactRepository.GetAllContacts(includeInactive: false)
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                var contactOptions = contacts.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.LastName} {c.FirstName ?? ""}" +
                           (!string.IsNullOrEmpty(c.Notes) ? $" - {(c.Notes.Length > 50 ? c.Notes.Substring(0, 50) + "..." : c.Notes)}" : "") +
                           (!string.IsNullOrEmpty(c.NationalCode) ? $" | کد ملی: {c.NationalCode}" : ""),
                    Group = new SelectListGroup { Name = GetFirstLetter(c.LastName) }
                }).ToList();

                ViewBag.OrganizationId = model.OrganizationId;
                ViewBag.OrganizationName = organization?.DisplayName;
                ViewBag.AvailableContacts = contactOptions;
            }

            return View(model);
        }

        /// <summary>
        /// حذف عضو از سازمان
        /// </summary>
        [HttpPost]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> RemoveOrganizationContact(int id)
        {
            try
            {
                var organizationContact = _organizationRepository.GetOrganizationContactById(id);
                if (organizationContact == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "عضو یافت نشد"
                    });
                }

                var contactName = organizationContact.Contact.FullName;
                var organizationId = organizationContact.OrganizationId;

                var result = await _organizationRepository.RemoveContactFromOrganizationAsync(id);

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Organizations",
                        "RemoveOrganizationContact",
                        $"حذف {contactName} از سازمان",
                        recordId: id.ToString()
                    );

                    return Json(new
                    {
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = organizationId }),
                        message = new[] { new { status = "success", text = "عضو با موفقیت حذف شد" } }
                    });
                }

                return Json(new { status = "error", message = "خطا در حذف عضو" });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "RemoveOrganizationContact", "خطا در حذف عضو", ex);
                return Json(new { status = "error", message = "خطا: " + ex.Message });
            }
        }

        /// <summary>
        /// دریافت لیست افراد موجود به صورت JSON (برای Select2)
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public JsonResult GetAvailableContactsJson(int organizationId)
        {
            try
            {
                var contacts = _contactRepository.GetAllContacts(includeInactive: false)
                    .OrderBy(c => c.LastName ?? "")
                    .ThenBy(c => c.FirstName ?? "")
                    .ToList();

                // ⭐ حذف افرادی که قبلاً به سازمان اضافه شده‌اند
                var existingContactIds = _organizationRepository.GetOrganizationContacts(organizationId, false)
                    .Select(oc => oc.ContactId)
                    .ToList();

                var availableContacts = contacts
                    .Where(c => !existingContactIds.Contains(c.Id))
                    .Select(c => new
                    {
                        id = c.Id,
                        firstName = c.FirstName ?? "", // ⭐ اصلاح شده: null safety
                        lastName = c.LastName ?? "",    // ⭐ اصلاح شده: null safety
                        nationalCode = c.NationalCode ?? "",
                        primaryPhone = c.Phones?.FirstOrDefault(p => p.IsDefault)?.FormattedNumber ?? 
                                      c.Phones?.FirstOrDefault()?.FormattedNumber ?? "",
                        notes = !string.IsNullOrEmpty(c.Notes) && c.Notes.Length > 70 
                            ? c.Notes.Substring(0, 70) + "..." 
                            : c.Notes ?? ""
                    })
                    .ToList();

                return Json(availableContacts);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// افزودن چند عضو همزمان به سازمان
        /// </summary>
        [HttpPost]
        [PermissionRequired("ORG.EDIT")]
        public async Task<JsonResult> AddMultipleOrganizationContacts([FromBody] AddMultipleOrganizationContactsRequest request)
        {
            try
            {
                if (request.Members == null || !request.Members.Any())
                {
                    return Json(new { success = false, message = "هیچ عضوی انتخاب نشده است" });
                }

                var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
                if (organization == null)
                {
                    return Json(new { success = false, message = "سازمان یافت نشد" });
                }

                int addedCount = 0;
                var userId = GetUserId();

                foreach (var member in request.Members)
                {
                    var exists = _organizationRepository.GetOrganizationContacts(request.OrganizationId, false)
                        .Any(oc => oc.ContactId == member.Id && oc.IsActive);

                    if (!exists)
                    {
                        var organizationContact = new DataModelLayer.Entities.Contacts.OrganizationContact
                        {
                            OrganizationId = request.OrganizationId,
                            ContactId = member.Id,
                            RelationType = member.RelationType,
                            Notes = member.Notes,
                            IsActive = true,
                            IsPrimary = false,
                            ImportanceLevel = 50,
                            CreatorUserId = userId,
                            CreatedDate = DateTime.Now
                        };

                        await _organizationRepository.AddContactToOrganizationAsync(organizationContact);
                        addedCount++;
                    }
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Organizations",
                    "AddMultipleOrganizationContacts",
                    $"افزودن {addedCount} عضو به سازمان: {organization.DisplayName}",
                    recordId: request.OrganizationId.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.DisplayName
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
                await _activityLogger.LogErrorAsync("Organizations", "AddMultipleOrganizationContacts", "خطا در افزودن اعضا", ex);
                return Json(new { success = false, message = "خطا در ذخیره: " + ex.Message });
            }
        }

        /// <summary>
        /// DTO برای دریافت چند عضو (سازمان)
        /// </summary>
        public class AddMultipleOrganizationContactsRequest
        {
            public int OrganizationId { get; set; }
            public List<OrganizationContactData> Members { get; set; }
        }

        public class OrganizationContactData
        {
            public int Id { get; set; }
            public byte RelationType { get; set; }
            public string Notes { get; set; }
        }
    }
}
