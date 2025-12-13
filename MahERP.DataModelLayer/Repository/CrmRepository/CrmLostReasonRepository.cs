using MahERP.DataModelLayer.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای مدیریت دلایل از دست رفتن CRM
    /// </summary>
    public class CrmLostReasonRepository
    {
        private readonly AppDbContext _context;

        public CrmLostReasonRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Get Methods

        /// <summary>
        /// دریافت همه دلایل (فعال)
        /// </summary>
        /// <param name="appliesTo">فیلتر نوع: null=همه، 0=هر دو، 1=فقط Lead، 2=فقط Opportunity</param>
        public async Task<List<CrmLostReason>> GetAllAsync(byte? appliesTo = null, bool includeInactive = false)
        {
            var query = _context.CrmLostReason_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            // فیلتر appliesTo
            if (appliesTo.HasValue)
            {
                // 0 = هر دو - پس همه چیز را شامل می‌شود
                if (appliesTo.Value == 1)
                    query = query.Where(x => x.AppliesTo == 0 || x.AppliesTo == 1);
                else if (appliesTo.Value == 2)
                    query = query.Where(x => x.AppliesTo == 0 || x.AppliesTo == 2);
            }

            return await query
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت دلایل برای Lead
        /// </summary>
        public async Task<List<CrmLostReason>> GetForLeadAsync()
        {
            return await _context.CrmLostReason_Tbl
                .Where(x => x.IsActive && (x.AppliesTo == 0 || x.AppliesTo == 1))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت دلایل برای Opportunity
        /// </summary>
        public async Task<List<CrmLostReason>> GetForOpportunityAsync()
        {
            return await _context.CrmLostReason_Tbl
                .Where(x => x.IsActive && (x.AppliesTo == 0 || x.AppliesTo == 2))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت دلیل با شناسه
        /// </summary>
        public async Task<CrmLostReason?> GetByIdAsync(int id)
        {
            return await _context.CrmLostReason_Tbl
                .Include(x => x.Creator)
                .Include(x => x.LastUpdater)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// دریافت دلیل با کد
        /// </summary>
        public async Task<CrmLostReason?> GetByCodeAsync(string code)
        {
            return await _context.CrmLostReason_Tbl
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
        }

        /// <summary>
        /// دریافت دلایل بر اساس دسته‌بندی
        /// </summary>
        public async Task<List<CrmLostReason>> GetByCategoryAsync(byte category)
        {
            return await _context.CrmLostReason_Tbl
                .Where(x => x.Category == category && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToListAsync();
        }

        /// <summary>
        /// آیا عنوان تکراری است؟
        /// </summary>
        public async Task<bool> IsTitleDuplicateAsync(string title, int? excludeId = null)
        {
            var query = _context.CrmLostReason_Tbl
                .Where(x => x.Title == title);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// آیا کد تکراری است؟
        /// </summary>
        public async Task<bool> IsCodeDuplicateAsync(string code, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(code)) return false;

            var query = _context.CrmLostReason_Tbl
                .Where(x => x.Code == code);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Create/Update/Delete

        /// <summary>
        /// ایجاد دلیل جدید
        /// </summary>
        public async Task<CrmLostReason> CreateAsync(CrmLostReason entity)
        {
            // اگر DisplayOrder تعیین نشده، آخرین را بگیر
            if (entity.DisplayOrder <= 0)
            {
                entity.DisplayOrder = await GetNextDisplayOrderAsync();
            }

            entity.CreatedDate = DateTime.Now;

            _context.CrmLostReason_Tbl.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// بروزرسانی دلیل
        /// </summary>
        public async Task<CrmLostReason> UpdateAsync(CrmLostReason entity)
        {
            var existing = await _context.CrmLostReason_Tbl
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
                throw new Exception($"دلیل با شناسه {entity.Id} یافت نشد");

            // اگر سیستمی است، برخی تغییرات مجاز نیست
            if (existing.IsSystem)
            {
                entity.IsSystem = true;
            }

            existing.Title = entity.Title;
            existing.TitleEnglish = entity.TitleEnglish;
            existing.Code = entity.Code;
            existing.AppliesTo = entity.AppliesTo;
            existing.Category = entity.Category;
            existing.Description = entity.Description;
            existing.Icon = entity.Icon;
            existing.ColorCode = entity.ColorCode;
            existing.DisplayOrder = entity.DisplayOrder;
            existing.RequiresNote = entity.RequiresNote;
            existing.IsActive = entity.IsActive;
            existing.LastUpdateDate = DateTime.Now;
            existing.LastUpdaterUserId = entity.LastUpdaterUserId;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// حذف دلیل
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CrmLostReason_Tbl
                .Include(x => x.Leads)
                .Include(x => x.Opportunities)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            // سیستمی قابل حذف نیست
            if (entity.IsSystem)
                throw new InvalidOperationException("دلایل سیستمی قابل حذف نیستند");

            // اگر استفاده شده، نمی‌توان حذف کرد
            var usageCount = (entity.Leads?.Count ?? 0) + (entity.Opportunities?.Count ?? 0);
            if (usageCount > 0)
                throw new InvalidOperationException($"این دلیل {usageCount} بار استفاده شده و قابل حذف نیست");

            _context.CrmLostReason_Tbl.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال
        /// </summary>
        public async Task<bool> ToggleActiveAsync(int id, string userId)
        {
            var entity = await _context.CrmLostReason_Tbl
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.LastUpdateDate = DateTime.Now;
            entity.LastUpdaterUserId = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت ترتیب بعدی
        /// </summary>
        public async Task<int> GetNextDisplayOrderAsync()
        {
            var max = await _context.CrmLostReason_Tbl
                .MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
            return max + 1;
        }

        /// <summary>
        /// بروزرسانی ترتیب نمایش
        /// </summary>
        public async Task UpdateDisplayOrdersAsync(List<int> orderedIds)
        {
            var entities = await _context.CrmLostReason_Tbl
                .Where(x => orderedIds.Contains(x.Id))
                .ToListAsync();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                var entity = entities.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (entity != null)
                {
                    entity.DisplayOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت آمار استفاده از دلایل
        /// </summary>
        public async Task<List<(int ReasonId, string ReasonTitle, int LeadCount, int OpportunityCount)>> GetUsageStatisticsAsync()
        {
            var stats = await _context.CrmLostReason_Tbl
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    ReasonId = x.Id,
                    ReasonTitle = x.Title,
                    LeadCount = x.Leads.Count,
                    OpportunityCount = x.Opportunities.Count
                })
                .ToListAsync();

            return stats.Select(x => (x.ReasonId, x.ReasonTitle, x.LeadCount, x.OpportunityCount)).ToList();
        }

        /// <summary>
        /// دریافت آمار بر اساس دسته‌بندی
        /// </summary>
        public async Task<Dictionary<byte, int>> GetStatsByCategory()
        {
            return await _context.CrmLostReason_Tbl
                .Where(x => x.IsActive)
                .GroupBy(x => x.Category)
                .Select(g => new { Category = g.Key, Count = g.Sum(x => x.Leads.Count + x.Opportunities.Count) })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        #endregion
    }
}
