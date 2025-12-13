using MahERP.DataModelLayer.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای مدیریت منابع سرنخ CRM
    /// </summary>
    public class CrmLeadSourceRepository
    {
        private readonly AppDbContext _context;

        public CrmLeadSourceRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Get Methods

        /// <summary>
        /// دریافت همه منابع سرنخ (فعال)
        /// </summary>
        public async Task<List<CrmLeadSource>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.CrmLeadSource_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت منبع سرنخ با شناسه
        /// </summary>
        public async Task<CrmLeadSource?> GetByIdAsync(int id)
        {
            return await _context.CrmLeadSource_Tbl
                .Include(x => x.Creator)
                .Include(x => x.LastUpdater)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// دریافت منبع پیش‌فرض
        /// </summary>
        public async Task<CrmLeadSource?> GetDefaultAsync()
        {
            return await _context.CrmLeadSource_Tbl
                .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive);
        }

        /// <summary>
        /// دریافت منابع سرنخ با کد
        /// </summary>
        public async Task<CrmLeadSource?> GetByCodeAsync(string code)
        {
            return await _context.CrmLeadSource_Tbl
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
        }

        /// <summary>
        /// آیا نام تکراری است؟
        /// </summary>
        public async Task<bool> IsNameDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _context.CrmLeadSource_Tbl
                .Where(x => x.Name == name);

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

            var query = _context.CrmLeadSource_Tbl
                .Where(x => x.Code == code);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Create/Update/Delete

        /// <summary>
        /// ایجاد منبع سرنخ جدید
        /// </summary>
        public async Task<CrmLeadSource> CreateAsync(CrmLeadSource entity)
        {
            // اگر پیش‌فرض است، سایر پیش‌فرض‌ها را غیرفعال کن
            if (entity.IsDefault)
            {
                await ClearDefaultAsync();
            }

            // اگر DisplayOrder تعیین نشده، آخرین را بگیر
            if (entity.DisplayOrder <= 0)
            {
                entity.DisplayOrder = await GetNextDisplayOrderAsync();
            }

            entity.CreatedDate = DateTime.Now;

            _context.CrmLeadSource_Tbl.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// بروزرسانی منبع سرنخ
        /// </summary>
        public async Task<CrmLeadSource> UpdateAsync(CrmLeadSource entity)
        {
            var existing = await _context.CrmLeadSource_Tbl
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
                throw new Exception($"منبع سرنخ با شناسه {entity.Id} یافت نشد");

            // اگر سیستمی است، برخی تغییرات مجاز نیست
            if (existing.IsSystem)
            {
                entity.IsSystem = true; // نمی‌توان IsSystem را تغییر داد
            }

            // اگر پیش‌فرض می‌شود، سایر پیش‌فرض‌ها را غیرفعال کن
            if (entity.IsDefault && !existing.IsDefault)
            {
                await ClearDefaultAsync();
            }

            existing.Name = entity.Name;
            existing.NameEnglish = entity.NameEnglish;
            existing.Code = entity.Code;
            existing.Icon = entity.Icon;
            existing.ColorCode = entity.ColorCode;
            existing.Description = entity.Description;
            existing.DisplayOrder = entity.DisplayOrder;
            existing.IsDefault = entity.IsDefault;
            existing.IsActive = entity.IsActive;
            existing.LastUpdateDate = DateTime.Now;
            existing.LastUpdaterUserId = entity.LastUpdaterUserId;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// حذف منبع سرنخ
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CrmLeadSource_Tbl
                .Include(x => x.Leads)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            // سیستمی قابل حذف نیست
            if (entity.IsSystem)
                throw new InvalidOperationException("منابع سیستمی قابل حذف نیستند");

            // اگر سرنخ دارد، نمی‌توان حذف کرد
            if (entity.Leads?.Count > 0)
                throw new InvalidOperationException($"این منبع دارای {entity.Leads.Count} سرنخ است و قابل حذف نیست");

            _context.CrmLeadSource_Tbl.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال
        /// </summary>
        public async Task<bool> ToggleActiveAsync(int id, string userId)
        {
            var entity = await _context.CrmLeadSource_Tbl
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
        /// غیرفعال کردن همه پیش‌فرض‌ها
        /// </summary>
        private async Task ClearDefaultAsync()
        {
            var defaults = await _context.CrmLeadSource_Tbl
                .Where(x => x.IsDefault)
                .ToListAsync();

            foreach (var item in defaults)
            {
                item.IsDefault = false;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// دریافت ترتیب بعدی
        /// </summary>
        public async Task<int> GetNextDisplayOrderAsync()
        {
            var max = await _context.CrmLeadSource_Tbl
                .MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
            return max + 1;
        }

        /// <summary>
        /// بروزرسانی ترتیب نمایش
        /// </summary>
        public async Task UpdateDisplayOrdersAsync(List<int> orderedIds)
        {
            var entities = await _context.CrmLeadSource_Tbl
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
        /// دریافت آمار استفاده از منابع
        /// </summary>
        public async Task<List<(int SourceId, string SourceName, int LeadCount)>> GetUsageStatisticsAsync()
        {
            var stats = await _context.CrmLeadSource_Tbl
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    SourceId = x.Id,
                    SourceName = x.Name,
                    LeadCount = x.Leads.Count
                })
                .ToListAsync();

            return stats.Select(x => (x.SourceId, x.SourceName, x.LeadCount)).ToList();
        }

        #endregion
    }
}
