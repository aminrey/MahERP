using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CrmViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.CrmRepository
{
    /// <summary>
    /// Interface برای Repository وضعیت لید
    /// </summary>
    public interface ILeadStageStatusRepository
    {
        Task<List<LeadStageStatus>> GetAllAsync(bool activeOnly = true);
        Task<LeadStageStatus?> GetByIdAsync(int id);
        Task<LeadStageStatus?> GetByStageTypeAsync(Enums.LeadStageType stageType);
        Task<LeadStageStatus?> GetPurchaseStageAsync();
    }

    /// <summary>
    /// Interface برای Repository وضعیت بعد از خرید
    /// </summary>
    public interface IPostPurchaseStageRepository
    {
        Task<List<PostPurchaseStage>> GetAllAsync(bool activeOnly = true);
        Task<PostPurchaseStage?> GetByIdAsync(int id);
        Task<PostPurchaseStage?> GetByStageTypeAsync(Enums.PostPurchaseStageType stageType);
        Task<PostPurchaseStage?> GetReferralStageAsync();
    }

    /// <summary>
    /// Interface برای Repository نوع تعامل
    /// </summary>
    public interface IInteractionTypeRepository
    {
        Task<List<InteractionType>> GetAllAsync(bool activeOnly = true);
        Task<InteractionType?> GetByIdAsync(int id);
        Task<InteractionType> CreateAsync(InteractionType interactionType);
        Task<bool> UpdateAsync(InteractionType interactionType);
        Task<bool> DeleteAsync(int id);
        Task<List<InteractionType>> GetByLeadStageStatusAsync(int leadStageStatusId);
    }

    /// <summary>
    /// Interface برای Repository هدف
    /// </summary>
    public interface IGoalRepository
    {
        Task<Goal?> GetByIdAsync(int id, bool includeInteractions = false);
        Task<List<Goal>> GetByContactAsync(int contactId, bool activeOnly = true);
        Task<List<Goal>> GetByOrganizationAsync(int organizationId, bool activeOnly = true);
        Task<Goal> CreateAsync(Goal goal);
        Task<bool> UpdateAsync(Goal goal);
        Task<bool> DeleteAsync(int id);
        Task<bool> MarkAsConvertedAsync(int goalId, decimal? actualValue = null);
        Task<bool> UpdateCurrentStageAsync(int goalId, int leadStageStatusId);
    }

    /// <summary>
    /// Interface برای Repository تعامل
    /// </summary>
    public interface IInteractionRepository
    {
        Task<Interaction?> GetByIdAsync(int id, bool includeGoals = false);
        Task<List<Interaction>> GetByContactAsync(int contactId, bool includeGoals = false);
        Task<(List<Interaction> Interactions, int TotalCount)> GetListAsync(
            InteractionFilterViewModel filter, int pageNumber = 1, int pageSize = 20);
        Task<Interaction> CreateAsync(Interaction interaction, List<int>? goalIds = null);
        Task<bool> UpdateAsync(Interaction interaction, List<int>? goalIds = null);
        Task<bool> DeleteAsync(int id);
        Task<Interaction?> GetLastInteractionForContactAsync(int contactId);
        Task<int> GetInteractionCountByContactAsync(int contactId);
    }

    /// <summary>
    /// Interface برای Repository ارجاع/توصیه
    /// </summary>
    public interface IReferralRepository
    {
        Task<Referral?> GetByIdAsync(int id);
        Task<List<Referral>> GetByReferrerAsync(int referrerContactId);
        Task<List<Referral>> GetByReferredAsync(int referredContactId);
        Task<Referral> CreateAsync(Referral referral);
        Task<bool> UpdateStatusAsync(int referralId, Enums.ReferralStatus status, string userId);
        Task<bool> DeleteAsync(int id);
        Task<int> GetSuccessfulReferralCountAsync(int referrerContactId);
        Task<bool> ExistsAsync(int referrerContactId, int referredContactId);
    }
}
