using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.Extentions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.OrganizationControllers
{
    /// <summary>
    /// مدیریت شماره‌های تماس سازمان
    /// </summary>
    public partial class OrganizationsController
    {
        /// <summary>
        /// افزودن Phone Row جدید با AJAX (Partial View)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddPhoneRowPartial(int index)
        {
            try
            {
                var phoneViewModel = new OrganizationPhoneViewModel
                {
                    PhoneType = 1, // ثابت
                    IsDefault = false,
                    IsActive = true
                };

                ViewBag.Index = index;
                ViewBag.IsNew = true;

                var partialViewResult = await this.RenderViewToStringAsync("_PhoneRowPartial", phoneViewModel);

                return Json(new
                {
                    status = "update-view",
                    viewList = new[]
                    {
                        new
                        {
                            elementId = "phonesContainer",
                            view = new { result = partialViewResult },
                            appendMode = true
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Organizations", "AddPhoneRowPartial", "خطا در افزودن شماره", ex);
                return Json(new
                {
                    status = "error",
                    message = new[] { new { status = "error", text = "خطا در افزودن شماره" } }
                });
            }
        }
    }
}
