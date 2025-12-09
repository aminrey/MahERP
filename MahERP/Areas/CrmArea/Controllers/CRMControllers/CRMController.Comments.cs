using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// CRM Controller - Comments & Participants Management
    /// </summary>
    public partial class CRMController
    {
        #region Comments

        /// <summary>
        /// افزودن نظر - نمایش مودال
        /// </summary>
        [HttpGet]
        public IActionResult AddComment(int crmInteractionId)
        {
            var interaction = _crmRepository.GetCRMInteractionById(crmInteractionId);
            if (interaction == null)
            {
                TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CRMInteractionId = crmInteractionId;
            ViewBag.CRMTitle = interaction.Title;

            return PartialView("_AddComment", new CRMCommentViewModel
            {
                CRMInteractionId = crmInteractionId
            });
        }

        /// <summary>
        /// افزودن نظر - پردازش مودال
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CRMCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = _userManager.GetUserId(User);

                    // ⭐ استفاده از Repository برای ایجاد نظر
                    var commentId = await _crmRepository.CreateCommentAsync(model, currentUserId);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "CRM",
                        "AddComment",
                        $"افزودن نظر به تعامل {model.CRMInteractionId}",
                        recordId: commentId.ToString()
                    );

                    TempData["SuccessMessage"] = "نظر با موفقیت ثبت شد";
                    return RedirectToAction(nameof(Details), new { id = model.CRMInteractionId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("CRM", "AddComment", "خطا در ثبت نظر", ex);
                    ModelState.AddModelError("", "خطا در ثبت نظر");
                }
            }

            var interaction = _crmRepository.GetCRMInteractionById(model.CRMInteractionId);
            ViewBag.CRMInteractionId = model.CRMInteractionId;
            ViewBag.CRMTitle = interaction?.Title ?? "";

            return PartialView("_AddComment", model);
        }

        #endregion

        #region Participants

        /// <summary>
        /// افزودن شرکت کننده - نمایش مودال
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddParticipant(int crmInteractionId)
        {
            var interaction = _crmRepository.GetCRMInteractionById(crmInteractionId);
            if (interaction == null)
            {
                TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CRMInteractionId = crmInteractionId;
            ViewBag.CRMTitle = interaction.Title;

            // ⭐ استفاده از Repository برای دریافت لیست‌ها
            var (users, contacts) = await _crmRepository.GetParticipantDropdownsAsync(crmInteractionId);
            ViewBag.Users = users;
            ViewBag.StakeholderContacts = contacts;

            return PartialView("_AddParticipant", new CRMParticipantViewModel
            {
                CRMInteractionId = crmInteractionId
            });
        }

        /// <summary>
        /// افزودن شرکت کننده - پردازش مودال
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant(CRMParticipantViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ⭐ استفاده از Repository برای ایجاد شرکت کننده
                    var participantId = await _crmRepository.CreateParticipantAsync(model);

                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Create,
                        "CRM",
                        "AddParticipant",
                        $"افزودن شرکت کننده به تعامل {model.CRMInteractionId}",
                        recordId: participantId.ToString()
                    );

                    TempData["SuccessMessage"] = "شرکت کننده با موفقیت اضافه شد";
                    return RedirectToAction(nameof(Details), new { id = model.CRMInteractionId });
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("CRM", "AddParticipant", "خطا در اضافه کردن شرکت کننده", ex);
                    ModelState.AddModelError("", "خطا در اضافه کردن شرکت کننده");
                }
            }

            var interaction = _crmRepository.GetCRMInteractionById(model.CRMInteractionId);
            ViewBag.CRMInteractionId = model.CRMInteractionId;
            ViewBag.CRMTitle = interaction?.Title ?? "";

            // بازیابی لیست‌ها
            var (users, contacts) = await _crmRepository.GetParticipantDropdownsAsync(model.CRMInteractionId);
            ViewBag.Users = users;
            ViewBag.StakeholderContacts = contacts;

            return PartialView("_AddParticipant", model);
        }

        #endregion
    }
}
