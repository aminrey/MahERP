using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// CRM Controller - Search & Statistics
    /// </summary>
    public partial class CRMController
    {
        #region Advanced Search

        /// <summary>
        /// جستجوی پیشرفته - نمایش فرم
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch()
        {
            // ⭐ استفاده از Repository برای دریافت لیست‌ها
            var (stakeholders, branches) = await _crmRepository.GetSearchDropdownsAsync();
            ViewBag.Stakeholders = stakeholders;
            ViewBag.Branches = branches;

            return PartialView("_AdvancedSearch", new CRMSearchViewModel());
        }

        /// <summary>
        /// جستجوی پیشرفته - پردازش جستجو
        /// </summary>
        [HttpPost]
        public IActionResult Search(CRMSearchViewModel model)
        {
            var interactions = _crmRepository.SearchCRMInteractions(
                model.SearchTerm,
                model.CRMType,
                model.Direction,
                model.Result,
                model.StakeholderId);

            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);

            ViewBag.SearchModel = model;
            ViewBag.Title = "نتایج جستجو";

            return View("SearchResults", viewModels);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// گزارش آماری
        /// </summary>
        public IActionResult Statistics()
        {
            ViewBag.TotalCount = _crmRepository.GetTotalCRMInteractionsCount();
            ViewBag.TodayCount = _crmRepository.GetTodayCRMInteractionsCount();
            ViewBag.PendingFollowUps = _crmRepository.GetPendingFollowUpsCount();
            ViewBag.InteractionsByType = _crmRepository.GetCRMInteractionsByType();
            ViewBag.InteractionsByResult = _crmRepository.GetCRMInteractionsByResult();

            return View();
        }

        #endregion

        #region Download Attachment

        /// <summary>
        /// دانلود پیوست
        /// </summary>
        public IActionResult DownloadAttachment(int id)
        {
            var attachment = _crmRepository.GetCRMAttachmentById(id);
            if (attachment == null)
                return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", attachment.FileName);
        }

        #endregion
    }
}
