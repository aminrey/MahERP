using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای ارجاع/توصیه (Referral)
    /// </summary>
    public class ReferralRepository : IReferralRepository
    {
        private readonly AppDbContext _context;
        private readonly IPostPurchaseStageRepository _postPurchaseStageRepo;
        private readonly ILogger<ReferralRepository> _logger;

        public ReferralRepository(
            AppDbContext context,
            IPostPurchaseStageRepository postPurchaseStageRepo,
            ILogger<ReferralRepository> logger)
        {
            _context = context;
            _postPurchaseStageRepo = postPurchaseStageRepo;
            _logger = logger;
        }

        public async Task<Referral?> GetByIdAsync(int id)
        {
            return await _context.Referral_Tbl
                .Include(r => r.ReferrerContact)
                .Include(r => r.ReferredContact)
                .Include(r => r.ReferrerInteraction)
                .Include(r => r.ReferredInteraction)
                .Include(r => r.Creator)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Referral>> GetByReferrerAsync(int referrerContactId)
        {
            return await _context.Referral_Tbl
                .Include(r => r.ReferredContact)
                .Where(r => r.ReferrerContactId == referrerContactId)
                .OrderByDescending(r => r.ReferralDate)
                .ToListAsync();
        }

        public async Task<List<Referral>> GetByReferredAsync(int referredContactId)
        {
            return await _context.Referral_Tbl
                .Include(r => r.ReferrerContact)
                .Where(r => r.ReferredContactId == referredContactId)
                .OrderByDescending(r => r.ReferralDate)
                .ToListAsync();
        }

        public async Task<Referral> CreateAsync(Referral referral)
        {
            try
            {
                // Validation: بررسی اینکه معرفی‌کننده مشتری باشد
                var referrerContact = await _context.Contact_Tbl.FindAsync(referral.ReferrerContactId);
                if (referrerContact == null)
                    throw new ArgumentException("توصیه‌کننده یافت نشد");

                if (referrerContact.ContactType != ContactType.Customer)
                    throw new InvalidOperationException("توصیه‌کننده باید مشتری باشد (حداقل یک بار خرید کرده باشد)");

                // بررسی تکراری نبودن
                if (await ExistsAsync(referral.ReferrerContactId, referral.ReferredContactId))
                    throw new InvalidOperationException("این ارجاع قبلاً ثبت شده است");

                referral.ReferralDate = DateTime.Now;
                referral.CreatedDate = DateTime.Now;
                referral.Status = ReferralStatus.Pending;
                referral.ReferralType = 0; // توصیه مشتری

                _context.Referral_Tbl.Add(referral);
                await _context.SaveChangesAsync();

                _logger.LogInformation("ارجاع جدید ثبت شد: معرفی‌کننده={ReferrerId}, معرفی‌شده={ReferredId}",
                    referral.ReferrerContactId, referral.ReferredContactId);

                return referral;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت ارجاع");
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int referralId, ReferralStatus status, string userId)
        {
            try
            {
                var referral = await _context.Referral_Tbl.FindAsync(referralId);
                if (referral == null)
                    return false;

                referral.Status = status;
                referral.StatusChangeDate = DateTime.Now;
                referral.LastUpdateDate = DateTime.Now;
                referral.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("وضعیت ارجاع تغییر کرد: ID={Id}, Status={Status}", referralId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت ارجاع: {Id}", referralId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var referral = await _context.Referral_Tbl.FindAsync(id);
                if (referral == null)
                    return false;

                _context.Referral_Tbl.Remove(referral);
                await _context.SaveChangesAsync();

                _logger.LogInformation("ارجاع حذف شد: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف ارجاع: {Id}", id);
                return false;
            }
        }

        public async Task<int> GetSuccessfulReferralCountAsync(int referrerContactId)
        {
            return await _context.Referral_Tbl
                .CountAsync(r => r.ReferrerContactId == referrerContactId && 
                                 r.Status == ReferralStatus.Successful);
        }

        public async Task<bool> ExistsAsync(int referrerContactId, int referredContactId)
        {
            return await _context.Referral_Tbl
                .AnyAsync(r => r.ReferrerContactId == referrerContactId && 
                               r.ReferredContactId == referredContactId);
        }
    }
}
