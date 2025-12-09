using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// CRM Controller - CRUD Operations
    /// </summary>
    public partial class CRMController
    {
        #region Index & List

        /// <summary>
        /// لیست تعاملات CRM
        /// </summary>
        [Permission("CRM", "Index", 0)] // Read
        public IActionResult Index()
        {
            var interactions = _crmRepository.GetCRMInteractions();
            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);

            ViewBag.TotalCount = _crmRepository.GetTotalCRMInteractionsCount();
            ViewBag.TodayCount = _crmRepository.GetTodayCRMInteractionsCount();
            ViewBag.PendingFollowUps = _crmRepository.GetPendingFollowUpsCount();

            return View(viewModels);
        }

        /// <summary>
        /// نمایش تعاملات من
        /// </summary>
        public IActionResult MyInteractions()
        {
            var userId = _userManager.GetUserId(User);
            var interactions = _crmRepository.GetCRMInteractionsByUser(userId);
            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);

            ViewBag.Title = "تعاملات من";
            ViewBag.IsMyInteractions = true;

            return View("Index", viewModels);
        }

        #endregion

        #region Details

        /// <summary>
        /// جزئیات تعامل CRM
        /// </summary>
        public IActionResult Details(int id)
        {
            var interaction = _crmRepository.GetCRMInteractionById(id, includeAttachments: true, includeComments: true, includeParticipants: true);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<CRMInteractionViewModel>(interaction);

            return View(viewModel);
        }

        #endregion

        #region Create

        /// <summary>
        /// افزودن تعامل CRM جدید - نمایش فرم
        /// </summary>
        [HttpGet]
        [Permission("CRM", "Create", 1)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ استفاده از Repository برای آماده‌سازی مدل
                var model = await _crmRepository.PrepareCreateInteractionModelAsync(currentUserId);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CRM",
                    "Create",
                    "مشاهده فرم ایجاد تعامل جدید"
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "Create", "خطا در نمایش فرم ایجاد تعامل", ex);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// افزودن تعامل CRM جدید - پردازش فرم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Create", 1)]
        public async Task<IActionResult> Create(CRMInteractionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = _userManager.GetUserId(User);

                    // ⭐ استفاده از Repository برای ایجاد تعامل
                    var interactionId = await _crmRepository.CreateInteractionAsync(model, currentUserId, _webHostEnvironment.WebRootPath);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "CRM",
                        "Create",
                        $"ایجاد تعامل جدید: {model.Title}",
                        recordId: interactionId.ToString(),
                        entityType: "CRMInteraction",
                        recordTitle: model.Title
                    );

                    TempData["SuccessMessage"] = "تعامل با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("CRM", "Create", "خطا در ایجاد تعامل", ex);
                    ModelState.AddModelError("", "خطا در ثبت تعامل. لطفاً مجدداً تلاش کنید.");
                }
            }

            // بازیابی لیست‌ها در صورت خطا
            model = await _crmRepository.RepopulateCreateModelAsync(model, _userManager.GetUserId(User));
            return View(model);
        }

        #endregion

        #region Edit

        /// <summary>
        /// ویرایش تعامل CRM - نمایش فرم
        /// </summary>
        [HttpGet]
        [Permission("CRM", "Edit", 2)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ استفاده از Repository برای آماده‌سازی مدل ویرایش
                var model = await _crmRepository.PrepareEditInteractionModelAsync(id, currentUserId);

                if (model == null)
                {
                    TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "CRM",
                    "Edit",
                    $"مشاهده فرم ویرایش تعامل: {model.Title}",
                    recordId: id.ToString()
                );

                return View(model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "Edit", "خطا در نمایش فرم ویرایش", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم ویرایش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ویرایش تعامل CRM - پردازش فرم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Edit", 2)]
        public async Task<IActionResult> Edit(CRMInteractionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = _userManager.GetUserId(User);

                    // ⭐ استفاده از Repository برای به‌روزرسانی
                    var success = await _crmRepository.UpdateInteractionAsync(model, currentUserId, _webHostEnvironment.WebRootPath);

                    if (success)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "CRM",
                            "Edit",
                            $"ویرایش تعامل: {model.Title}",
                            recordId: model.Id.ToString(),
                            entityType: "CRMInteraction",
                            recordTitle: model.Title
                        );

                        TempData["SuccessMessage"] = "تعامل با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Details), new { id = model.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "تعامل مورد نظر یافت نشد");
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("CRM", "Edit", "خطا در ویرایش تعامل", ex, recordId: model.Id.ToString());
                    ModelState.AddModelError("", "خطا در ثبت تغییرات. لطفاً مجدداً تلاش کنید.");
                }
            }

            // بازیابی لیست‌ها در صورت خطا
            model = await _crmRepository.RepopulateCreateModelAsync(model, _userManager.GetUserId(User));
            return View(model);
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف تعامل - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        [Permission("CRM", "Delete", 3)]
        public IActionResult Delete(int id)
        {
            var interaction = _crmRepository.GetCRMInteractionById(id);
            if (interaction == null)
            {
                TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف تعامل CRM";

            return PartialView("_DeleteCRMInteraction", _mapper.Map<CRMInteractionViewModel>(interaction));
        }

        /// <summary>
        /// حذف تعامل - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Delete", 3)]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ استفاده از Repository برای حذف
                var (success, interactionTitle) = await _crmRepository.SoftDeleteInteractionAsync(id, currentUserId);

                if (success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "CRM",
                        "Delete",
                        $"حذف تعامل: {interactionTitle}",
                        recordId: id.ToString(),
                        entityType: "CRMInteraction",
                        recordTitle: interactionTitle
                    );

                    TempData["SuccessMessage"] = "تعامل با موفقیت حذف شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "Delete", "خطا در حذف تعامل", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در حذف تعامل";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Toggle Active Status

        /// <summary>
        /// فعال/غیرفعال کردن تعامل - نمایش مودال تأیید
        /// </summary>
        [HttpGet]
        public IActionResult ToggleActiveStatus(int id)
        {
            var interaction = _crmRepository.GetCRMInteractionById(id);
            if (interaction == null)
            {
                TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            if (interaction.IsActive)
            {
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن تعامل";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن تعامل";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActiveStatus", _mapper.Map<CRMInteractionViewModel>(interaction));
        }

        /// <summary>
        /// فعال/غیرفعال کردن تعامل - پردازش درخواست
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatusPost(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // ⭐ استفاده از Repository برای تغییر وضعیت
                var (success, isActive, title) = await _crmRepository.ToggleInteractionActiveStatusAsync(id, currentUserId);

                if (success)
                {
                    var status = isActive ? "فعال" : "غیرفعال";
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "CRM",
                        "ToggleActiveStatus",
                        $"تغییر وضعیت تعامل به {status}: {title}",
                        recordId: id.ToString()
                    );

                    TempData["SuccessMessage"] = $"تعامل {status} شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("CRM", "ToggleActiveStatus", "خطا در تغییر وضعیت تعامل", ex, recordId: id.ToString());
                TempData["ErrorMessage"] = "خطا در تغییر وضعیت";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}
