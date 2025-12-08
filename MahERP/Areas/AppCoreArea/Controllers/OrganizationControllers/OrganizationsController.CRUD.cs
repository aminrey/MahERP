using MahERP.Attributes;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// عملیات CRUD سازمان‌ها
    /// </summary>
    public partial class OrganizationsController
    {
        // ==================== DETAILS ====================

        /// <summary>
        /// جزئیات سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.VIEW")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // ⭐⭐⭐ اصلاح شده: بارگذاری شماره‌ها
                var organization = await _organizationRepository.GetOrganizationByIdAsync(id,
                    includeDepartments: true,
                    includeContacts: true,
                    includePhones: true); // ⭐ اضافه شده

                if (organization == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "Organizations",
                        "Details",
                        "تلاش برای مشاهده سازمان غیرموجود",
                        recordId: id.ToString()
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                var viewModel = _mapper.Map<OrganizationViewModel>(organization);

                // ⭐⭐⭐ Map کردن شماره‌ها
                if (organization.Phones != null && organization.Phones.Any())
                {
                    viewModel.Phones = _mapper.Map<System.Collections.Generic.List<OrganizationPhoneViewModel>>(
                        organization.Phones.Where(p => p.IsActive)
                            .OrderByDescending(p => p.IsDefault)
                            .ThenBy(p => p.DisplayOrder)
                    );
                }

                var stats = await _organizationRepository.GetOrganizationStatisticsAsync(id);
                ViewBag.Statistics = stats;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Details",
                    $"مشاهده جزئیات سازمان: {organization.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Details", "خطا در دریافت جزئیات", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // ==================== CREATE ====================

        /// <summary>
        /// نمایش فرم افزودن سازمان جدید
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create()
        {
            try
            {
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Create",
                    "مشاهده فرم افزودن سازمان جدید"
                );

                return View(new OrganizationViewModel { IsActive = true, OrganizationType = 0 });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Create", "خطا در نمایش فرم", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره سازمان جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.CREATE")]
        public async Task<IActionResult> Create(OrganizationViewModel model, System.Collections.Generic.List<OrganizationPhoneInputViewModel> Phones)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", "نام سازمان الزامی است");
            }

            if (!string.IsNullOrEmpty(model.RegistrationNumber))
            {
                if (!_organizationRepository.IsRegistrationNumberUnique(model.RegistrationNumber))
                {
                    ModelState.AddModelError("RegistrationNumber", "این شماره ثبت قبلاً استفاده شده است");
                }
            }

            if (!string.IsNullOrEmpty(model.EconomicCode))
            {
                if (!_organizationRepository.IsEconomicCodeUnique(model.EconomicCode))
                {
                    ModelState.AddModelError("EconomicCode", "این کد اقتصادی قبلاً استفاده شده است");
                }
            }

            // ⭐⭐⭐ اعتبارسنجی شماره‌های تماس (اگر وارد شده باشند)
            if (Phones != null && Phones.Any())
            {
                for (int i = 0; i < Phones.Count; i++)
                {
                    var phone = Phones[i];
                    if (!string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    {
                        // اینجا می‌توانید اعتبارسنجی شماره را اضافه کنید
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var organization = _mapper.Map<DataModelLayer.Entities.Contacts.Organization>(model);
                    organization.CreatedDate = DateTime.Now;
                    organization.CreatorUserId = GetUserId();
                    organization.IsActive = true;

                    _uow.OrganizationUW.Create(organization);
                    _uow.Save();

                    // ⭐⭐⭐ افزودن شماره‌های تماس (اگر وارد شده باشند)
                    if (Phones != null && Phones.Any())
                    {
                        var validPhones = Phones.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();

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
                                var phone = new DataModelLayer.Entities.Contacts.OrganizationPhone
                                {
                                    OrganizationId = organization.Id,
                                    PhoneNumber = phoneInput.PhoneNumber,
                                    PhoneType = phoneInput.PhoneType,
                                    Extension = phoneInput.Extension,
                                    IsDefault = phoneInput.IsDefault,
                                    IsActive = true,
                                    DisplayOrder = displayOrder++,
                                    CreatedDate = DateTime.Now,
                                    CreatorUserId = GetUserId()
                                };

                                _uow.OrganizationPhoneUW.Create(phone);
                            }

                            _uow.Save();
                        }
                    }

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "Organizations",
                        "Create",
                        $"ایجاد سازمان جدید: {organization.DisplayName}" +
                        (Phones?.Any(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)) == true
                            ? $" با {Phones.Count(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))} شماره تماس"
                            : " بدون شماره تماس"),
                        recordId: organization.Id.ToString(),
                        entityType: "Organization",
                        recordTitle: organization.DisplayName
                    );

                    TempData["SuccessMessage"] = "سازمان با موفقیت ایجاد شد. اکنون می‌توانید چارت سازمانی را تکمیل کنید 📊";
                    return RedirectToAction("OrganizationChart", new { organizationId = organization.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "Create", "خطا در ایجاد", ex);
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            return View(model);
        }

        // ==================== EDIT ====================

        /// <summary>
        /// نمایش فرم ویرایش سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // ⭐⭐⭐ اصلاح شده: دریافت با شماره‌ها
                var organization = await _organizationRepository.GetOrganizationByIdAsync(id);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var viewModel = _mapper.Map<OrganizationViewModel>(organization);

                // ⭐⭐⭐ Map کردن شماره‌ها
                if (organization.Phones != null && organization.Phones.Any())
                {
                    viewModel.Phones = _mapper.Map<System.Collections.Generic.List<OrganizationPhoneViewModel>>(
                        organization.Phones.Where(p => p.IsActive)
                            .OrderByDescending(p => p.IsDefault)
                            .ThenBy(p => p.DisplayOrder)
                    );
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "Edit",
                    $"مشاهده فرم ویرایش سازمان: {organization.DisplayName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organization.DisplayName
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Edit", "خطا در نمایش فرم", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// ذخیره ویرایش سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> Edit(OrganizationViewModel model, System.Collections.Generic.List<OrganizationPhoneInputViewModel> Phones)
        {
            if (!string.IsNullOrEmpty(model.RegistrationNumber))
            {
                if (!_organizationRepository.IsRegistrationNumberUnique(model.RegistrationNumber, model.Id))
                {
                    ModelState.AddModelError("RegistrationNumber", "این شماره ثبت قبلاً استفاده شده است");
                }
            }

            if (!string.IsNullOrEmpty(model.EconomicCode))
            {
                if (!_organizationRepository.IsEconomicCodeUnique(model.EconomicCode, model.Id))
                {
                    ModelState.AddModelError("EconomicCode", "این کد اقتصادی قبلاً استفاده شده است");
                }
            }

            // ⭐⭐⭐ اعتبارسنجی شماره‌های تماس (اگر وارد شده باشند)
            if (Phones != null && Phones.Any())
            {
                for (int i = 0; i < Phones.Count; i++)
                {
                    var phone = Phones[i];
                    if (!string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    {
                        // اینجا می‌توانید اعتبارسنجی شماره را اضافه کنید
                        // مثلاً بررسی فرمت شماره ثابت یا موبایل
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var organization = _uow.OrganizationUW.GetById(model.Id);
                    if (organization == null)
                        return RedirectToAction("ErrorView", "Home");

                    var oldValues = new
                    {
                        organization.Name,
                        organization.RegistrationNumber,
                        organization.EconomicCode,
                        organization.IsActive
                    };

                    var originalCreated = organization.CreatedDate;
                    var originalCreatorId = organization.CreatorUserId;

                    _mapper.Map(model, organization);

                    organization.CreatedDate = originalCreated;
                    organization.CreatorUserId = originalCreatorId;
                    organization.LastUpdateDate = DateTime.Now;
                    organization.LastUpdaterUserId = GetUserId();

                    _uow.OrganizationUW.Update(organization);
                    _uow.Save();

                    // ⭐⭐⭐ مدیریت شماره‌های تماس
                    if (Phones != null && Phones.Any())
                    {
                        var validPhones = Phones.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();

                        // حذف شماره‌های قدیمی
                        var existingPhones = _uow.OrganizationPhoneUW
                            .Get(p => p.OrganizationId == model.Id)
                            .ToList();

                        foreach (var oldPhone in existingPhones)
                        {
                            _uow.OrganizationPhoneUW.Delete(oldPhone);
                        }
                        _uow.Save();

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
                                var phone = new DataModelLayer.Entities.Contacts.OrganizationPhone
                                {
                                    OrganizationId = organization.Id,
                                    PhoneNumber = phoneInput.PhoneNumber,
                                    PhoneType = phoneInput.PhoneType,
                                    Extension = phoneInput.Extension,
                                    IsDefault = phoneInput.IsDefault,
                                    IsActive = true,
                                    DisplayOrder = displayOrder++,
                                    CreatedDate = DateTime.Now,
                                    CreatorUserId = GetUserId()
                                };

                                _uow.OrganizationPhoneUW.Create(phone);
                            }

                            _uow.Save();
                        }
                    }

                    var newValues = new
                    {
                        organization.Name,
                        organization.RegistrationNumber,
                        organization.EconomicCode,
                        organization.IsActive
                    };

                    await _activityLogger.LogChangeAsync(
                        ActivityTypeEnum.Edit,
                        "Organizations",
                        "Edit",
                        $"ویرایش سازمان: {organization.DisplayName}" +
                        (Phones?.Any(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)) == true
                            ? $" با {Phones.Count(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))} شماره تماس"
                            : ""),
                        oldValues,
                        newValues,
                        recordId: organization.Id.ToString(),
                        entityType: "Organization",
                        recordTitle: organization.DisplayName
                    );

                    TempData["SuccessMessage"] = "اطلاعات با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Details), new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Organizations", "Edit", "خطا در ویرایش", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", "خطا در ذخیره: " + ex.Message);
                }
            }

            // اگر validation ناموفق بود، شماره‌ها رو دوباره بارگذاری کن
            if (Phones != null)
            {
                model.Phones = Phones.Select(p => new OrganizationPhoneViewModel
                {
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension,
                    IsDefault = p.IsDefault
                }).ToList();
            }

            return View(model);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// نمایش مودال تایید حذف
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                ViewBag.themeclass = "bg-danger";
                ViewBag.ViewTitle = "حذف سازمان";

                return PartialView("_DeleteOrganization", organization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Delete", "خطا در نمایش فرم حذف", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        /// <summary>
        /// حذف سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.DELETE")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                    return RedirectToAction("ErrorView", "Home");

                var organizationName = organization.DisplayName;

                organization.IsActive = false;
                organization.LastUpdateDate = DateTime.Now;
                organization.LastUpdaterUserId = GetUserId();

                _uow.OrganizationUW.Update(organization);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Organizations",
                    "Delete",
                    $"حذف سازمان: {organizationName}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organizationName
                );

                TempData["SuccessMessage"] = "سازمان با موفقیت حذف شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "Delete", "خطا در حذف", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در حذف سازمان";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== TOGGLE ACTIVATION ====================

        /// <summary>
        /// نمایش مودال تایید فعال/غیرفعال کردن سازمان
        /// </summary>
        [HttpGet]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> ToggleActivation(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                    return NotFound();

                ViewBag.OrganizationId = id;
                ViewBag.OrganizationName = organization.DisplayName;
                ViewBag.IsActive = organization.IsActive;

                if (organization.IsActive)
                {
                    ViewBag.ModalTitle = "غیرفعال کردن سازمان";
                    ViewBag.ThemeClass = "bg-warning";
                    ViewBag.ButtonClass = "btn btn-warning";
                    ViewBag.ActionText = "غیرفعال";
                }
                else
                {
                    ViewBag.ModalTitle = "فعال کردن سازمان";
                    ViewBag.ThemeClass = "bg-success";
                    ViewBag.ButtonClass = "btn btn-success";
                    ViewBag.ActionText = "فعال";
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Organizations",
                    "ToggleActivation",
                    $"مشاهده فرم تغییر وضعیت سازمان: {organization.DisplayName}",
                    recordId: id.ToString()
                );

                return PartialView("_ToggleActivation", organization);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "ToggleActivation", "خطا در نمایش فرم", ex, recordId: id.ToString());
                return StatusCode(500);
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن سازمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionRequired("ORG.EDIT")]
        public async Task<IActionResult> ToggleActivationPost(int id)
        {
            try
            {
                var organization = _uow.OrganizationUW.GetById(id);
                if (organization == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = new[] { new { status = "error", text = "سازمان یافت نشد" } }
                    });
                }

                var organizationName = organization.DisplayName;
                organization.IsActive = !organization.IsActive;
                organization.LastUpdateDate = DateTime.Now;
                organization.LastUpdaterUserId = GetUserId();

                _uow.OrganizationUW.Update(organization);
                _uow.Save();

                var actionText = organization.IsActive ? "فعال" : "غیرفعال";

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Organizations",
                    "ToggleActivation",
                    $"تغییر وضعیت سازمان {organizationName} به {actionText}",
                    recordId: id.ToString(),
                    entityType: "Organization",
                    recordTitle: organizationName
                );

                return Json(new
                {
                    status = "redirect",
                    redirectUrl = Url.Action("Index", "Organizations", new { area = "AppCoreArea" }),
                    message = new[] { new { status = "success", text = $"سازمان با موفقیت {actionText} شد" } }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "ToggleActivation", "خطا در تغییر وضعیت", ex, recordId: id.ToString());
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در تغییر وضعیت: " + ex.Message } }
                });
            }
        }
    }
}
