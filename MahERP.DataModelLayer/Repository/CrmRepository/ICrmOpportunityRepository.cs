using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface مدیریت مراحل Pipeline
    /// </summary>
    public interface ICrmPipelineStageRepository
    {
        // ========== CRUD ==========
        Task<CrmPipelineStage?> GetByIdAsync(int id);
        Task<CrmPipelineStage> CreateAsync(CrmPipelineStage stage);
        Task<bool> UpdateAsync(CrmPipelineStage stage);
        Task<bool> DeleteAsync(int id);

        // ========== List ==========
        Task<List<CrmPipelineStage>> GetByBranchAsync(int branchId, bool includeInactive = false);
        Task<CrmPipelineStage?> GetDefaultStageAsync(int branchId);
        Task<CrmPipelineStage?> GetWonStageAsync(int branchId);
        Task<CrmPipelineStage?> GetLostStageAsync(int branchId);

        // ========== Operations ==========
        Task<bool> ReorderAsync(int branchId, List<int> stageIds);
        Task EnsureDefaultStagesAsync(int branchId, string creatorUserId);
    }

    /// <summary>
    /// Interface مدیریت فرصت‌های فروش
    /// </summary>
    public interface ICrmOpportunityRepository
    {
        // ========== CRUD ==========
        Task<CrmOpportunity?> GetByIdAsync(int id, bool includeDetails = false);
        Task<CrmOpportunity> CreateAsync(CrmOpportunity opportunity);
        Task<bool> UpdateAsync(CrmOpportunity opportunity);
        Task<bool> DeleteAsync(int id);

        // ========== List & Search ==========
        Task<(List<CrmOpportunity> Items, int TotalCount)> GetListAsync(
            CrmOpportunityFilterViewModel filter, 
            int pageNumber = 1, 
            int pageSize = 20);
        
        Task<List<CrmOpportunity>> GetByBranchAsync(int branchId, bool includeClosed = false);
        Task<List<CrmOpportunity>> GetByUserAsync(string userId, int? branchId = null);
        Task<List<CrmOpportunity>> GetByStageAsync(int stageId);
        Task<List<CrmOpportunity>> SearchAsync(string searchText, int? branchId = null, int maxResults = 20);

        // ========== Pipeline Board ==========
        Task<CrmPipelineBoardViewModel> GetPipelineBoardAsync(int branchId, CrmOpportunityFilterViewModel? filter = null);
        Task<CrmPipelineStatisticsViewModel> GetStatisticsAsync(int? branchId = null, string? userId = null);

        // ========== Stage Operations ==========
        Task<bool> MoveToStageAsync(int opportunityId, int newStageId, string userId, string? note = null);
        Task<bool> MarkAsWonAsync(int opportunityId, string userId, string? note = null);
        Task<bool> MarkAsLostAsync(int opportunityId, string userId, string? lostReason = null, string? competitor = null);
        Task<bool> ReopenAsync(int opportunityId, int stageId, string userId);

        // ========== Lead Conversion ==========
        Task<CrmOpportunity> CreateFromLeadAsync(int leadId, ConvertLeadToOpportunityViewModel model, string userId);
        Task<CrmOpportunity?> GetByLeadAsync(int leadId);

        // ========== Products ==========
        Task<List<CrmOpportunityProduct>> GetProductsAsync(int opportunityId);
        Task<CrmOpportunityProduct> AddProductAsync(CrmOpportunityProduct product);
        Task<bool> UpdateProductAsync(CrmOpportunityProduct product);
        Task<bool> RemoveProductAsync(int productId);
        Task RecalculateValueAsync(int opportunityId);

        // ========== Activities ==========
        Task<List<CrmOpportunityActivity>> GetActivitiesAsync(int opportunityId, int maxResults = 20);
        Task<CrmOpportunityActivity> LogActivityAsync(CrmOpportunityActivity activity);

        // ========== Utilities ==========
        Task UpdateProbabilityFromStageAsync(int opportunityId);
        Task<List<CrmOpportunity>> GetNeedingActionAsync(string? userId = null, int? branchId = null, int maxResults = 50);
        Task<List<CrmOpportunity>> GetOverdueAsync(string? userId = null, int? branchId = null);
    }
}
