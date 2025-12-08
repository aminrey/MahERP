using MahERP.DataModelLayer.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.OrganizationRepository
{
    /// <summary>
    /// Repository برای مدیریت سمت‌های استاندارد سازمانی
    /// </summary>
    public class PositionRepository : IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========== CRUD Operations ==========

        public List<OrganizationPosition> GetAllPositions(bool includeInactive = false)
        {
            var query = _context.OrganizationPosition_Tbl
                .Include(p => p.Creator)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayOrder)
                .ThenBy(p => p.Title)
                .ToList();
        }

        public List<OrganizationPosition> GetCommonPositions()
        {
            return _context.OrganizationPosition_Tbl
                .Where(p => p.IsCommon && p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayOrder)
                .ToList();
        }

        public List<OrganizationPosition> GetPositionsByCategory(string category, bool includeInactive = false)
        {
            var query = _context.OrganizationPosition_Tbl
                .Where(p => p.Category == category);

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Title)
                .ToList();
        }

        public List<OrganizationPosition> GetPositionsByLevel(int level, bool includeInactive = false)
        {
            var query = _context.OrganizationPosition_Tbl
                .Where(p => p.Level == level);

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayOrder)
                .ToList();
        }

        public OrganizationPosition? GetPositionById(int id)
        {
            return _context.OrganizationPosition_Tbl
                .Include(p => p.Creator)
                .Include(p => p.LastUpdater)
                .FirstOrDefault(p => p.Id == id);
        }

        public List<OrganizationPosition> SearchPositions(string searchTerm, string? category = null, int? level = null)
        {
            var query = _context.OrganizationPosition_Tbl
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    (p.TitleEnglish != null && p.TitleEnglish.Contains(searchTerm)) ||
                    (p.Description != null && p.Description.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (level.HasValue)
                query = query.Where(p => p.Level == level.Value);

            return query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayOrder)
                .ToList();
        }

        public async Task<int> CreatePositionAsync(OrganizationPosition position)
        {
            _context.OrganizationPosition_Tbl.Add(position);
            await _context.SaveChangesAsync();
            return position.Id;
        }

        public async Task<bool> UpdatePositionAsync(OrganizationPosition position)
        {
            try
            {
                var existing = await _context.OrganizationPosition_Tbl.FindAsync(position.Id);
                if (existing == null)
                    return false;

                // Update properties
                existing.Title = position.Title;
                existing.TitleEnglish = position.TitleEnglish;
                existing.Category = position.Category;
                existing.Description = position.Description;
                existing.Level = position.Level;
                existing.DefaultPowerLevel = position.DefaultPowerLevel;
                existing.IsCommon = position.IsCommon;
                existing.RequiresDegree = position.RequiresDegree;
                existing.MinimumDegree = position.MinimumDegree;
                existing.MinimumExperienceYears = position.MinimumExperienceYears;
                existing.SuggestedMinSalary = position.SuggestedMinSalary;
                existing.SuggestedMaxSalary = position.SuggestedMaxSalary;
                existing.CanHireSubordinates = position.CanHireSubordinates;
                existing.DisplayOrder = position.DisplayOrder;
                existing.IsActive = position.IsActive;
                existing.LastUpdateDate = position.LastUpdateDate;
                existing.LastUpdaterUserId = position.LastUpdaterUserId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePositionAsync(int id)
        {
            try
            {
                var position = await _context.OrganizationPosition_Tbl.FindAsync(id);
                if (position == null)
                    return false;

                // Soft delete
                position.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsPositionInUse(int positionId)
        {
            return _context.DepartmentPosition_Tbl
                .Any(dp => dp.BasePositionId == positionId && dp.IsActive);
        }

        public int GetUsageCount(int positionId)
        {
            return _context.DepartmentPosition_Tbl
                .Count(dp => dp.BasePositionId == positionId && dp.IsActive);
        }

        // ========== دسته‌بندی‌ها ==========

        public List<string> GetAllCategories()
        {
            return _context.OrganizationPosition_Tbl
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public Dictionary<string, int> GetCategoryStatistics()
        {
            return _context.OrganizationPosition_Tbl
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }

        public Dictionary<int, int> GetLevelStatistics()
        {
            return _context.OrganizationPosition_Tbl
                .Where(p => p.IsActive)
                .GroupBy(p => p.Level)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }
    }
}
