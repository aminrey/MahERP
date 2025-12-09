using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای مدیریت وضعیت‌های سرنخ CRM
    /// </summary>
    public class CrmLeadStatusRepository : ICrmLeadStatusRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CrmLeadStatusRepository> _logger;

        public CrmLeadStatusRepository(
            AppDbContext context,
            ILogger<CrmLeadStatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<List<CrmLeadStatus>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.CrmLeadStatus_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<CrmLeadStatus?> GetByIdAsync(int id)
        {
            return await _context.CrmLeadStatus_Tbl
                .Include(s => s.Creator)
                .Include(s => s.LastUpdater)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<CrmLeadStatus> CreateAsync(CrmLeadStatus status)
        {
            status.CreatedDate = DateTime.Now;

            // اگر وضعیت پیش‌فرض است، بقیه را غیرپیش‌فرض کن
            if (status.IsDefault)
            {
                await ClearDefaultStatusAsync();
            }

            _context.CrmLeadStatus_Tbl.Add(status);
            await _context.SaveChangesAsync();

            _logger.LogInformation("وضعیت سرنخ جدید ایجاد شد: {Title} (ID: {Id})", status.Title, status.Id);

            return status;
        }

        public async Task<bool> UpdateAsync(CrmLeadStatus status)
        {
            try
            {
                var existing = await _context.CrmLeadStatus_Tbl.FindAsync(status.Id);
                if (existing == null)
                    return false;

                // اگر وضعیت پیش‌فرض شد، بقیه را غیرپیش‌فرض کن
                if (status.IsDefault && !existing.IsDefault)
                {
                    await ClearDefaultStatusAsync();
                }

                existing.Title = status.Title;
                existing.TitleEnglish = status.TitleEnglish;
                existing.ColorCode = status.ColorCode;
                existing.Icon = status.Icon;
                existing.DisplayOrder = status.DisplayOrder;
                existing.IsDefault = status.IsDefault;
                existing.IsFinal = status.IsFinal;
                existing.IsPositive = status.IsPositive;
                existing.Description = status.Description;
                existing.IsActive = status.IsActive;
                existing.LastUpdateDate = DateTime.Now;
                existing.LastUpdaterUserId = status.LastUpdaterUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("وضعیت سرنخ بروزرسانی شد: {Title} (ID: {Id})", status.Title, status.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی وضعیت سرنخ: {Id}", status.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var status = await _context.CrmLeadStatus_Tbl.FindAsync(id);
                if (status == null)
                    return false;

                // بررسی استفاده
                if (await IsStatusInUseAsync(id))
                {
                    _logger.LogWarning("وضعیت در حال استفاده است و قابل حذف نیست: {Id}", id);
                    return false;
                }

                // Soft Delete
                status.IsActive = false;
                status.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("وضعیت سرنخ غیرفعال شد: {Title} (ID: {Id})", status.Title, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف وضعیت سرنخ: {Id}", id);
                return false;
            }
        }

        // ========== Queries ==========

        public async Task<CrmLeadStatus?> GetDefaultStatusAsync()
        {
            return await _context.CrmLeadStatus_Tbl
                .FirstOrDefaultAsync(s => s.IsDefault && s.IsActive);
        }

        public async Task<List<CrmLeadStatus>> GetFinalStatusesAsync()
        {
            return await _context.CrmLeadStatus_Tbl
                .Where(s => s.IsFinal && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> IsStatusInUseAsync(int statusId)
        {
            return await _context.CrmLead_Tbl
                .AnyAsync(l => l.StatusId == statusId);
        }

        public async Task<Dictionary<int, int>> GetLeadsCountByStatusAsync(int? branchId = null)
        {
            var query = _context.CrmLead_Tbl
                .Where(l => l.IsActive);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            return await query
                .GroupBy(l => l.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusId, x => x.Count);
        }

        // ========== Utilities ==========

        public async Task<bool> SetAsDefaultAsync(int statusId, string userId)
        {
            try
            {
                await ClearDefaultStatusAsync();

                var status = await _context.CrmLeadStatus_Tbl.FindAsync(statusId);
                if (status == null)
                    return false;

                status.IsDefault = true;
                status.LastUpdateDate = DateTime.Now;
                status.LastUpdaterUserId = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("وضعیت پیش‌فرض تنظیم شد: {Title} (ID: {Id})", status.Title, statusId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تنظیم وضعیت پیش‌فرض: {Id}", statusId);
                return false;
            }
        }

        public async Task<bool> IsTitleUniqueAsync(string title, int? excludeId = null)
        {
            var query = _context.CrmLeadStatus_Tbl
                .Where(s => s.Title == title && s.IsActive);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        // ========== Private Methods ==========

        private async Task ClearDefaultStatusAsync()
        {
            var currentDefault = await _context.CrmLeadStatus_Tbl
                .Where(s => s.IsDefault)
                .ToListAsync();

            foreach (var status in currentDefault)
            {
                status.IsDefault = false;
            }
        }
    }
}
