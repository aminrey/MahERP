using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Repository برای وضعیت لید (جدول استاتیک)
    /// </summary>
    public class LeadStageStatusRepository : ILeadStageStatusRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeadStageStatusRepository> _logger;

        public LeadStageStatusRepository(AppDbContext context, ILogger<LeadStageStatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LeadStageStatus>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.LeadStageStatus_Tbl.AsQueryable();

            if (activeOnly)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<LeadStageStatus?> GetByIdAsync(int id)
        {
            return await _context.LeadStageStatus_Tbl
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<LeadStageStatus?> GetByStageTypeAsync(LeadStageType stageType)
        {
            return await _context.LeadStageStatus_Tbl
                .FirstOrDefaultAsync(s => s.StageType == stageType && s.IsActive);
        }

        public async Task<LeadStageStatus?> GetPurchaseStageAsync()
        {
            return await GetByStageTypeAsync(LeadStageType.Purchase);
        }
    }

    /// <summary>
    /// Repository برای وضعیت بعد از خرید (جدول استاتیک)
    /// </summary>
    public class PostPurchaseStageRepository : IPostPurchaseStageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostPurchaseStageRepository> _logger;

        public PostPurchaseStageRepository(AppDbContext context, ILogger<PostPurchaseStageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PostPurchaseStage>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.PostPurchaseStage_Tbl.AsQueryable();

            if (activeOnly)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PostPurchaseStage?> GetByIdAsync(int id)
        {
            return await _context.PostPurchaseStage_Tbl
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<PostPurchaseStage?> GetByStageTypeAsync(PostPurchaseStageType stageType)
        {
            return await _context.PostPurchaseStage_Tbl
                .FirstOrDefaultAsync(s => s.StageType == stageType && s.IsActive);
        }

        public async Task<PostPurchaseStage?> GetReferralStageAsync()
        {
            return await GetByStageTypeAsync(PostPurchaseStageType.Referral);
        }
    }
}
