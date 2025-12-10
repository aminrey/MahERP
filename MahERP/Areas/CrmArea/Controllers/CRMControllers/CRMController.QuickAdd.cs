using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// CRM Controller - Quick Add Methods (افزودن سریع فرد/سازمان)
    /// ⭐⭐⭐ نسخه جدید با پشتیبانی از چند شماره تماس
    /// </summary>
    public partial class CRMController
    {
        #region Quick Add Contact

        /// <summary>
        /// نمایش Partial View افزودن سریع فرد
        /// </summary>
        [HttpGet]
        public IActionResult QuickAddContactPartial()
        {
            var model = new QuickAddContactViewModel();
            return PartialView("Partials/_QuickAddContact", model);
        }

        /// <summary>
        /// ایجاد سریع فرد جدید با شماره‌های تماس و اتصال به شعبه/سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddContact(QuickAddContactViewModel model)
        {
            try
            {
                // ⭐ اعتبارسنجی اولیه
                if (string.IsNullOrWhiteSpace(model.LastName))
                {
                    return Json(new { success = false, message = "نام خانوادگی الزامی است" });
                }

                // ⭐ بررسی تکراری نبودن کد ملی
                if (!string.IsNullOrEmpty(model.NationalCode))
                {
                    var existingContact = await _crmRepository.GetContactByNationalCodeAsync(model.NationalCode);

                    if (existingContact != null)
                    {
                        return Json(new 
                        { 
                            success = false, 
                            message = "این کد ملی قبلاً ثبت شده است",
                            existingContact = new
                            {
                                id = existingContact.Id,
                                firstName = existingContact.FirstName,
                                lastName = existingContact.LastName
                            }
                        });
                    }
                }

                var currentUserId = _userManager.GetUserId(User);

                // ⭐⭐⭐ ایجاد Contact Entity
                var contact = new Contact
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NationalCode = model.NationalCode,
                    PrimaryEmail = model.PrimaryEmail,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = currentUserId
                };

                // ⭐ ایجاد Contact در دیتابیس
                _uow.ContactUW.Create(contact);
                _uow.Save();

                // ⭐⭐⭐ افزودن شماره‌های تماس (اگر وارد شده باشند)
                var validPhones = model.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<QuickAddPhoneViewModel>();
                
                if (validPhones.Any())
                {
                    // اگر هیچ شماره‌ای IsDefault نشده، اولین شماره را پیش‌فرض کن
                    if (!validPhones.Any(p => p.IsDefault))
                    {
                        validPhones[0].IsDefault = true;
                    }

                    // اگر هیچ شماره‌ای IsSmsDefault نشده، اولین شماره را پیش‌فرض کن
                    if (!validPhones.Any(p => p.IsSmsDefault))
                    {
                        validPhones[0].IsSmsDefault = true;
                    }

                    int displayOrder = 1;
                    foreach (var phoneInput in validPhones)
                    {
                        var phone = new ContactPhone
                        {
                            ContactId = contact.Id,
                            PhoneNumber = phoneInput.PhoneNumber,
                            PhoneType = phoneInput.PhoneType,
                            IsDefault = phoneInput.IsDefault,
                            IsSmsDefault = phoneInput.IsSmsDefault,
                            IsActive = true,
                            DisplayOrder = displayOrder++,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = currentUserId
                        };

                        _uow.ContactPhoneUW.Create(phone);
                    }

                    _uow.Save();
                }

                // ⭐ اتصال به شعبه
                if (model.BranchId > 0)
                {
                    await _crmRepository.AssignContactToBranchAsync(
                        contact.Id, 
                        model.BranchId, 
                        currentUserId
                    );
                }

                // ⭐⭐⭐ اتصال به سازمان (اگر انتخاب شده)
                if (model.OrganizationId.HasValue && model.OrganizationId.Value > 0)
                {
                    var organizationContact = new OrganizationContact
                    {
                        OrganizationId = model.OrganizationId.Value,
                        ContactId = contact.Id,
                        RelationType = model.OrganizationRelationType ?? 3, // پیش‌فرض: کارمند
                        IsPrimary = false,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatorUserId = currentUserId
                    };

                    _uow.OrganizationContactUW.Create(organizationContact);
                    _uow.Save();

                    // ⭐ افزودن به بخش "بدون بخش" (default department)
                    var defaultDepartment = await _crmRepository.GetOrCreateDefaultDepartmentAsync(
                        model.OrganizationId.Value,
                        currentUserId
                    );

                    if (defaultDepartment != null)
                    {
                        var member = new DepartmentMember
                        {
                            DepartmentId = defaultDepartment.Id,
                            ContactId = contact.Id,
                            PositionId = null,
                            IsActive = true,
                            JoinDate = DateTime.Now,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = currentUserId
                        };

                        _uow.DepartmentMemberUW.Create(member);
                        _uow.Save();
                    }
                }

                // ⭐ لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CRM",
                    "QuickAddContact",
                    $"افزودن سریع فرد: {contact.FullName}" +
                    (validPhones.Any() ? $" با {validPhones.Count} شماره تماس" : " بدون شماره تماس") +
                    (model.OrganizationId.HasValue ? " و اتصال به سازمان" : ""),
                    recordId: contact.Id.ToString(),
                    entityType: "Contact",
                    recordTitle: contact.FullName
                );

                // ⭐⭐⭐ برگرداندن اطلاعات کامل
                var primaryPhone = validPhones.FirstOrDefault(p => p.IsDefault) ?? validPhones.FirstOrDefault();

                return Json(new 
                { 
                    success = true, 
                    message = "فرد با موفقیت ایجاد شد",
                    contact = new
                    {
                        id = contact.Id,
                        firstName = contact.FirstName ?? "",
                        lastName = contact.LastName,
                        fullName = contact.FullName,
                        nationalCode = contact.NationalCode ?? "",
                        primaryPhone = primaryPhone?.PhoneNumber ?? "",
                        organizationId = model.OrganizationId
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "QuickAddContact", "خطا در ایجاد سریع فرد", ex);
                return Json(new { success = false, message = "خطا در ایجاد فرد: " + ex.Message });
            }
        }

        #endregion

        #region Quick Add Organization

        /// <summary>
        /// نمایش Partial View افزودن سریع سازمان
        /// </summary>
        [HttpGet]
        public IActionResult QuickAddOrganizationPartial()
        {
            var model = new QuickAddOrganizationViewModel();
            return PartialView("Partials/_QuickAddOrganization", model);
        }

        /// <summary>
        /// ایجاد سریع سازمان جدید با شماره‌های تماس و اتصال به شعبه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddOrganization(QuickAddOrganizationViewModel model)
        {
            try
            {
                // ⭐ اعتبارسنجی اولیه
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "نام سازمان الزامی است" });
                }

                // ⭐ بررسی تکراری نبودن شماره ثبت
                if (!string.IsNullOrEmpty(model.RegistrationNumber))
                {
                    var existingOrg = await _crmRepository.GetOrganizationByRegistrationNumberAsync(model.RegistrationNumber);

                    if (existingOrg != null)
                    {
                        return Json(new 
                        { 
                            success = false, 
                            message = "این شماره ثبت قبلاً ثبت شده است",
                            existingOrganization = new
                            {
                                id = existingOrg.Id,
                                name = existingOrg.Name
                            }
                        });
                    }
                }

                var currentUserId = _userManager.GetUserId(User);

                // ⭐⭐⭐ ایجاد Organization Entity
                var organization = new Organization
                {
                    Name = model.Name,
                    Brand = model.Brand,
                    RegistrationNumber = model.RegistrationNumber,
                    Email = model.Email,
                    OrganizationType = model.OrganizationType,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = currentUserId
                };

                // ⭐ ایجاد Organization در دیتابیس
                _uow.OrganizationUW.Create(organization);
                _uow.Save();

                // ⭐⭐⭐ افزودن شماره‌های تماس (اگر وارد شده باشند)
                var validPhones = model.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<QuickAddOrganizationPhoneViewModel>();
                
                if (validPhones.Any())
                {
                    // اگر هیچ شماره‌ای IsDefault نشده، اولین شماره را پیش‌فرض کن
                    if (!validPhones.Any(p => p.IsDefault))
                    {
                        validPhones[0].IsDefault = true;
                    }

                    int displayOrder = 1;
                    foreach (var phoneInput in validPhones)
                    {
                        var phone = new OrganizationPhone
                        {
                            OrganizationId = organization.Id,
                            PhoneNumber = phoneInput.PhoneNumber,
                            PhoneType = phoneInput.PhoneType,
                            Extension = phoneInput.Extension,
                            IsDefault = phoneInput.IsDefault,
                            IsActive = true,
                            DisplayOrder = displayOrder++,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = currentUserId
                        };

                        _uow.OrganizationPhoneUW.Create(phone);
                    }

                    _uow.Save();
                }

                // ⭐ اتصال به شعبه
                if (model.BranchId > 0)
                {
                    await _crmRepository.AssignOrganizationToBranchAsync(
                        organization.Id, 
                        model.BranchId, 
                        currentUserId
                    );
                }

                // ⭐⭐⭐ ایجاد بخش پیش‌فرض "بدون بخش"
                var defaultDepartment = new OrganizationDepartment
                {
                    OrganizationId = organization.Id,
                    Title = "بدون بخش",
                    Code = "DEFAULT",
                    Description = "بخش پیش‌فرض برای اعضای بدون بخش مشخص",
                    IsActive = true,
                    DisplayOrder = 0,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = currentUserId
                };

                _uow.OrganizationDepartmentUW.Create(defaultDepartment);
                _uow.Save();

                // ⭐ لاگ فعالیت
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "CRM",
                    "QuickAddOrganization",
                    $"افزودن سریع سازمان: {organization.Name}" +
                    (validPhones.Any() ? $" با {validPhones.Count} شماره تماس" : " بدون شماره تماس"),
                    recordId: organization.Id.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.Name
                );

                // ⭐⭐⭐ برگرداندن اطلاعات کامل
                var primaryPhone = validPhones.FirstOrDefault(p => p.IsDefault) ?? validPhones.FirstOrDefault();

                return Json(new 
                { 
                    success = true, 
                    message = "سازمان با موفقیت ایجاد شد",
                    organization = new
                    {
                        id = organization.Id,
                        name = organization.Name,
                        brand = organization.Brand ?? "",
                        registrationNumber = organization.RegistrationNumber ?? "",
                        primaryPhone = primaryPhone?.PhoneNumber ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "QuickAddOrganization", "خطا در ایجاد سریع سازمان", ex);
                return Json(new { success = false, message = "خطا در ایجاد سازمان: " + ex.Message });
            }
        }

        #endregion

        #region ⭐⭐⭐ Get Branch Organizations

        /// <summary>
        /// ⭐⭐⭐ دریافت لیست سازمان‌های شعبه برای Quick Add Modal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBranchOrganizations(int branchId)
        {
            try
            {
                if (branchId <= 0)
                {
                    return Json(new List<object>());
                }

                var organizations = await _crmRepository.GetBranchOrganizationsAsync(branchId);

                var result = organizations.Select(o => new
                {
                    id = o.Id,
                    name = o.Name,
                    brand = o.Brand
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "CRM",
                    "GetBranchOrganizations",
                    "خطا در دریافت سازمان‌های شعبه",
                    ex,
                    recordId: branchId.ToString()
                );

                return Json(new List<object>());
            }
        }

        #endregion
    }
}
