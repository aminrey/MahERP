using MahERP.CommonLayer.Helpers;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// Partial Controller: CRUD Actions برای QuickAdd
    /// </summary>
    public partial class QuickAddController
    {
        // ==================== CREATE CONTACT ====================

        /// <summary>
        /// ذخیره Contact جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContact(QuickAddContactViewModel model)
        {
            // ⭐⭐⭐ Debug logging
            Console.WriteLine($"📝 CreateContact called:");
            Console.WriteLine($"  - BranchId: {model.BranchId}");
            Console.WriteLine($"  - LastName: {model.LastName}");
            Console.WriteLine($"  - OrganizationId: {model.OrganizationId}");
            Console.WriteLine($"  - ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                Console.WriteLine($"❌ Validation errors: {errors}");

                return Json(new QuickAddResponseViewModel
                {
                    Status = "error",
                    Message = errors
                });
            }

            try
            {
                // ⭐⭐⭐ بررسی BranchId
                if (model.BranchId == 0)
                {
                    Console.WriteLine("❌ BranchId is 0!");
                    return Json(new QuickAddResponseViewModel
                    {
                        Status = "error",
                        Message = "شناسه شعبه نامعتبر است. لطفاً دوباره تلاش کنید."
                    });
                }

                // ایجاد Contact
                var contact = new Contact
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PrimaryEmail = model.Email,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = _userManager.GetUserId(User)
                };

                _uow.ContactUW.Create(contact);
                _uow.Save();

                // اضافه کردن شماره تماس (اگر وارد شده)
                if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    // اعتبارسنجی و نرمال‌سازی شماره
                    if (PhoneNumberHelper.ValidateIranianPhoneNumber(model.PhoneNumber, out string phoneError))
                    {
                        var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(model.PhoneNumber);

                        var phone = new ContactPhone
                        {
                            ContactId = contact.Id,
                            PhoneNumber = normalizedPhone,
                            PhoneType = 0, // موبایل
                            IsDefault = true,
                            IsSmsDefault = true,
                            IsActive = true,
                            DisplayOrder = 1,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = _userManager.GetUserId(User)
                        };

                        _uow.ContactPhoneUW.Create(phone);
                        _uow.Save();
                    }
                }

                // اتصال به شعبه
                var branchContact = new BranchContact
                {
                    BranchId = model.BranchId,
                    ContactId = contact.Id,
                    RelationType = 0, // مشتری
                    IsActive = true,
                    AssignDate = DateTime.Now,
                    AssignedByUserId = _userManager.GetUserId(User)!
                };

                _uow.BranchContactUW.Create(branchContact);
                _uow.Save();

                // اگر سازمان انتخاب شده، لینک کن
                if (model.OrganizationId.HasValue)
                {
                    var contactOrg = new OrganizationContact
                    {
                        ContactId = contact.Id,
                        OrganizationId = model.OrganizationId.Value,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatorUserId = _userManager.GetUserId(User)
                    };

                    _uow.OrganizationContactUW.Create(contactOrg);
                    _uow.Save();
                }

                // Log
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "QuickAdd",
                    "CreateContact",
                    $"افزودن سریع فرد: {contact.FullName}",
                    recordId: contact.Id.ToString()
                );

                return Json(new QuickAddResponseViewModel
                {
                    Status = "success",
                    Message = "فرد با موفقیت ایجاد شد",
                    ContactId = contact.Id,
                    ContactName = contact.FullName,
                    SelectValue = contact.Id.ToString(),
                    SelectText = contact.FullName
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("QuickAdd", "CreateContact", "خطا در ایجاد", ex);
                
                return Json(new QuickAddResponseViewModel
                {
                    Status = "error",
                    Message = $"خطا در ایجاد فرد: {ex.Message}"
                });
            }
        }

        // ==================== CREATE ORGANIZATION ====================

        /// <summary>
        /// ذخیره Organization جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganization(QuickAddOrganizationViewModel model)
        {
            // ⭐⭐⭐ Debug logging
            Console.WriteLine($"📝 CreateOrganization called:");
            Console.WriteLine($"  - BranchId: {model.BranchId}");
            Console.WriteLine($"  - Name: {model.Name}");
            Console.WriteLine($"  - ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                Console.WriteLine($"❌ Validation errors: {errors}");

                return Json(new QuickAddResponseViewModel
                {
                    Status = "error",
                    Message = errors
                });
            }

            try
            {
                // ⭐⭐⭐ بررسی BranchId
                if (model.BranchId == 0)
                {
                    Console.WriteLine("❌ BranchId is 0!");
                    return Json(new QuickAddResponseViewModel
                    {
                        Status = "error",
                        Message = "شناسه شعبه نامعتبر است. لطفاً دوباره تلاش کنید."
                    });
                }

                // ایجاد Organization
                var organization = new Organization
                {
                    Name = model.Name,
                    Brand = model.Brand,
                    Email = model.Email,
                    OrganizationType = model.OrganizationType,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = _userManager.GetUserId(User)
                };

                _uow.OrganizationUW.Create(organization);
                _uow.Save();

                // اضافه کردن شماره تماس (اگر وارد شده)
                if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    if (PhoneNumberHelper.ValidateIranianPhoneNumber(model.PhoneNumber, out string phoneError))
                    {
                        var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(model.PhoneNumber);

                        var phone = new OrganizationPhone
                        {
                            OrganizationId = organization.Id,
                            PhoneNumber = normalizedPhone,
                            PhoneType = 1, // ثابت
                            IsDefault = true,
                            IsActive = true,
                            DisplayOrder = 1,
                            CreatedDate = DateTime.Now,
                            CreatorUserId = _userManager.GetUserId(User)
                        };

                        _uow.OrganizationPhoneUW.Create(phone);
                        _uow.Save();
                    }
                }

                // اتصال به شعبه
                var branchOrg = new BranchOrganization
                {
                    BranchId = model.BranchId,
                    OrganizationId = organization.Id,
                    RelationType = 0, // مشتری
                    IncludeAllMembers = true,
                    IsActive = true,
                    AssignDate = DateTime.Now,
                    AssignedByUserId = _userManager.GetUserId(User)!
                };

                _uow.BranchOrganizationUW.Create(branchOrg);
                _uow.Save();

                // Log
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "QuickAdd",
                    "CreateOrganization",
                    $"افزودن سریع سازمان: {organization.DisplayName}",
                    recordId: organization.Id.ToString()
                );

                return Json(new QuickAddResponseViewModel
                {
                    Status = "success",
                    Message = "سازمان با موفقیت ایجاد شد",
                    OrganizationId = organization.Id,
                    OrganizationName = organization.DisplayName,
                    SelectValue = organization.Id.ToString(),
                    SelectText = organization.DisplayName
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("QuickAdd", "CreateOrganization", "خطا در ایجاد", ex);
                
                return Json(new QuickAddResponseViewModel
                {
                    Status = "error",
                    Message = $"خطا در ایجاد سازمان: {ex.Message}"
                });
            }
        }
    }
}
