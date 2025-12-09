using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// CRM Controller - Ajax Methods
    /// </summary>
    public partial class CRMController
    {
        #region Branch Selection

        /// <summary>
        /// بارگذاری داده‌های شعبه (Contacts و Organizations)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BranchTriggerSelect(int branchId)
        {
            try
            {
                if (branchId <= 0)
                {
                    return Json(new { status = "error", message = "شعبه نامعتبر است" });
                }

                // ⭐ استفاده از Repository برای دریافت داده‌های شعبه
                var (contacts, organizations) = await _crmRepository.GetBranchDataAsync(branchId);

                // رندر کردن Partial View ها
                var contactsHtml = await this.RenderViewToStringAsync("_ContactsDropdown", contacts);
                var organizationsHtml = await this.RenderViewToStringAsync("_OrganizationsDropdown", organizations);

                var viewList = new List<object>
                {
                    new {
                        elementId = "ContactSelectionDiv",
                        view = new { result = contactsHtml }
                    },
                    new {
                        elementId = "OrganizationSelectionDiv",
                        view = new { result = organizationsHtml }
                    }
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CRM",
                    "BranchTriggerSelect",
                    $"بارگذاری داده‌های شعبه {branchId} - Contacts: {contacts.Count}, Organizations: {organizations.Count}"
                );

                return Json(new { status = "update-view", viewList = viewList });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CRM",
                    "BranchTriggerSelect",
                    "خطا در بارگذاری داده‌های شعبه",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new { status = "error", message = "خطا در بارگذاری داده‌ها" });
            }
        }

        #endregion

        #region Contact Selection

        /// <summary>
        /// دریافت سازمان‌های مرتبط با Contact
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ContactTriggerSelect(int contactId)
        {
            try
            {
                if (contactId <= 0)
                {
                    return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
                }

                // ⭐ استفاده از Repository برای دریافت سازمان‌های Contact
                var organizations = await _crmRepository.GetContactOrganizationsAsync(contactId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CRM",
                    "ContactTriggerSelect",
                    $"دریافت سازمان‌های Contact {contactId} - تعداد: {organizations.Count}",
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", organizations);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CRM",
                    "ContactTriggerSelect",
                    "خطا در دریافت سازمان‌های Contact",
                    ex,
                    recordId: contactId.ToString()
                );

                return PartialView("_ContactOrganizationsSelection", new List<OrganizationViewModel>());
            }
        }

        #endregion

        #region Organization Selection

        /// <summary>
        /// بارگذاری افراد مرتبط با Organization
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OrganizationTriggerSelect(int organizationId)
        {
            try
            {
                // ⭐ استفاده از Repository برای دریافت اعضای Organization
                var contacts = await _crmRepository.GetOrganizationContactsAsync(organizationId);

                var model = new
                {
                    Contacts = contacts,
                    OrganizationId = organizationId
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CRM",
                    "OrganizationTriggerSelect",
                    $"دریافت اعضای Organization {organizationId} - تعداد: {contacts.Count}",
                    recordId: organizationId.ToString()
                );

                return PartialView("_OrganizationContactsPartial", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CRM",
                    "OrganizationTriggerSelect",
                    "خطا در بارگذاری افراد سازمان",
                    ex,
                    recordId: organizationId.ToString()
                );

                return PartialView("_OrganizationContactsPartial", new
                {
                    Contacts = new List<ContactViewModel>(),
                    OrganizationId = organizationId
                });
            }
        }

        #endregion
    }
}
