using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// کنترلر مدیریت ارجاعات/توصیه‌ها (Referral)
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM")]
    public class ReferralController : BaseController
    {
        private readonly IReferralRepository _referralRepo;
        private readonly IContactRepository _contactRepo;

        public ReferralController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IReferralRepository referralRepo,
            IContactRepository contactRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _referralRepo = referralRepo;
            _contactRepo = contactRepo;
        }

        /// <summary>
        /// صفحه اصلی لیست همه ارجاعات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm = null, ReferralStatus? status = null, int page = 1, int pageSize = 20)
        {
            var referrals = await _referralRepo.GetAllAsync();

            // فیلتر بر اساس وضعیت
            if (status.HasValue)
            {
                referrals = referrals.Where(r => r.Status == status.Value).ToList();
            }

            // فیلتر بر اساس جستجو
            if (!string.IsNullOrEmpty(searchTerm))
            {
                referrals = referrals.Where(r => 
                    (r.ReferrerContact?.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.ReferredContact?.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Notes?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // آمار
            var allReferrals = await _referralRepo.GetAllAsync();
            ViewBag.TotalCount = allReferrals.Count;
            ViewBag.PendingCount = allReferrals.Count(r => r.Status == ReferralStatus.Pending);
            ViewBag.SuccessfulCount = allReferrals.Count(r => r.Status == ReferralStatus.Successful);
            ViewBag.FailedCount = allReferrals.Count(r => r.Status == ReferralStatus.Failed);

            // فیلترهای فعلی
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)referrals.Count / pageSize);

            // Pagination
            var pagedReferrals = referrals
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = pagedReferrals.Select(r => MapToViewModel(r)).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// لیست ارجاعات یک مشتری (چه کسانی را معرفی کرده)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByReferrer(int contactId)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "فرد یافت نشد";
                return RedirectToAction("Index", "Contact", new { area = "ContactArea" });
            }

            var referrals = await _referralRepo.GetByReferrerAsync(contactId);

            ViewBag.Contact = contact;
            ViewBag.ContactId = contactId;
            ViewBag.ContactName = contact.FullName;
            ViewBag.IsReferrer = true;

            var viewModel = referrals.Select(r => MapToViewModel(r)).ToList();

            return View("ByContact", viewModel);
        }

        /// <summary>
        /// لیست ارجاعات یک لید (چه کسی معرفی کرده)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByReferred(int contactId)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId);
            if (contact == null)
            {
                TempData["ErrorMessage"] = "فرد یافت نشد";
                return RedirectToAction("Index", "Contact", new { area = "ContactArea" });
            }

            var referrals = await _referralRepo.GetByReferredAsync(contactId);

            ViewBag.Contact = contact;
            ViewBag.ContactId = contactId;
            ViewBag.ContactName = contact.FullName;
            ViewBag.IsReferrer = false;

            var viewModel = referrals.Select(r => MapToViewModel(r)).ToList();

            return View("ByContact", viewModel);
        }

        /// <summary>
        /// جزئیات ارجاع
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var referral = await _referralRepo.GetByIdAsync(id);
            if (referral == null)
            {
                TempData["ErrorMessage"] = "ارجاع یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = MapToViewModel(referral);

            return View(viewModel);
        }

        /// <summary>
        /// تغییر وضعیت ارجاع به موفق
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSuccessful(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _referralRepo.UpdateStatusAsync(id, ReferralStatus.Successful, userId!);

            if (success)
                TempData["SuccessMessage"] = "وضعیت ارجاع به موفق تغییر کرد";
            else
                TempData["ErrorMessage"] = "خطا در تغییر وضعیت";

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// تغییر وضعیت ارجاع به ناموفق
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFailed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _referralRepo.UpdateStatusAsync(id, ReferralStatus.Failed, userId!);

            if (success)
                TempData["SuccessMessage"] = "وضعیت ارجاع به ناموفق تغییر کرد";
            else
                TempData["ErrorMessage"] = "خطا در تغییر وضعیت";

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// حذف ارجاع
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? referrerContactId = null)
        {
            var success = await _referralRepo.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = "ارجاع با موفقیت حذف شد";
            else
                TempData["ErrorMessage"] = "خطا در حذف ارجاع";

            if (referrerContactId.HasValue)
                return RedirectToAction(nameof(ByReferrer), new { contactId = referrerContactId });

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private ReferralViewModel MapToViewModel(Referral r)
        {
            return new ReferralViewModel
            {
                Id = r.Id,
                ReferrerContactId = r.ReferrerContactId,
                ReferrerContactName = r.ReferrerContact?.FullName,
                ReferredContactId = r.ReferredContactId,
                ReferredContactName = r.ReferredContact?.FullName,
                ReferredContactType = r.ReferredContact?.ContactType,
                ReferrerInteractionId = r.ReferrerInteractionId,
                ReferredInteractionId = r.ReferredInteractionId,
                ReferralDate = r.ReferralDate,
                ReferralDatePersian = ConvertDateTime.ConvertMiladiToShamsi(r.ReferralDate, "yyyy/MM/dd"),
                Notes = r.Notes,
                Status = r.Status,
                StatusName = GetStatusName(r.Status),
                StatusColor = GetStatusColor(r.Status),
                StatusChangeDate = r.StatusChangeDate,
                CreatedDate = r.CreatedDate,
                CreatorName = r.Creator?.UserName
            };
        }

        private string GetStatusName(ReferralStatus status)
        {
            return status switch
            {
                ReferralStatus.Pending => "در انتظار",
                ReferralStatus.Successful => "موفق",
                ReferralStatus.Failed => "ناموفق",
                _ => "نامشخص"
            };
        }

        private string GetStatusColor(ReferralStatus status)
        {
            return status switch
            {
                ReferralStatus.Pending => "#ffc107",
                ReferralStatus.Successful => "#28a745",
                ReferralStatus.Failed => "#dc3545",
                _ => "#6c757d"
            };
        }

        #endregion
    }
}
