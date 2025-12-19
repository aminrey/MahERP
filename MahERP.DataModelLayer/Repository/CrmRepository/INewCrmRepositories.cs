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
        Task<LeadStageStatus> CreateAsync(LeadStageStatus stage);
        Task<bool> UpdateAsync(LeadStageStatus stage);
        Task<bool> DeleteAsync(int id);
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
        Task<PostPurchaseStage> CreateAsync(PostPurchaseStage stage);
        Task<bool> UpdateAsync(PostPurchaseStage stage);
        Task<bool> DeleteAsync(int id);
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
        Task<(List<Goal> Goals, int TotalCount)> GetListAsync(
            GoalFilterViewModel filter, int pageNumber = 1, int pageSize = 20);
        Task<Goal> CreateAsync(Goal goal);
        Task<bool> UpdateAsync(Goal goal);
        Task<bool> DeleteAsync(int id);
        Task<bool> MarkAsConvertedAsync(int goalId, decimal? actualValue = null);
        Task<bool> UpdateCurrentStageAsync(int goalId, int leadStageStatusId);
        
        /// <summary>
        /// دریافت خلاصه اهداف یک Contact برای نمایش در Tab ها
        /// </summary>
        Task<List<GoalSummaryViewModel>> GetGoalSummariesForContactAsync(int contactId);
        
        /// <summary>
        /// دریافت خلاصه اهداف یک Organization برای نمایش در Tab ها
        /// </summary>
        Task<List<GoalSummaryViewModel>> GetGoalSummariesForOrganizationAsync(int organizationId);
        
        /// <summary>
        /// دریافت اهداف یک Contact با فیلتر شعبه
        /// </summary>
        Task<List<Goal>> GetByContactAndBranchAsync(int contactId, int? branchId = null, bool activeOnly = false);
        
        /// <summary>
        /// دریافت اهداف یک Organization با فیلتر شعبه
        /// </summary>
        Task<List<Goal>> GetByOrganizationAndBranchAsync(int organizationId, int? branchId = null, bool activeOnly = false);
        
        /// <summary>
        /// دریافت 10 هدف اخیری که تغییر کرده‌اند (ایجاد شده یا وضعیت تغییر کرده)
        /// </summary>
        Task<List<RecentGoalItemViewModel>> GetRecentlyChangedGoalsAsync(int count = 10);
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
        
        /// <summary>
        /// دریافت تعاملات یک هدف خاص با صفحه‌بندی
        /// </summary>
        Task<(List<Interaction> Interactions, int TotalCount)> GetByGoalAsync(
            int goalId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10);
        
        /// <summary>
        /// دریافت تعاملات یک Contact (بدون هدف خاص) با صفحه‌بندی
        /// </summary>
        Task<(List<Interaction> Interactions, int TotalCount)> GetByContactPagedAsync(
            int contactId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10);
        
        /// <summary>
        /// دریافت تعاملات یک Organization با صفحه‌بندی
        /// </summary>
        Task<(List<Interaction> Interactions, int TotalCount)> GetByOrganizationPagedAsync(
            int organizationId, InteractionFilterViewModel? filter = null, int pageNumber = 1, int pageSize = 10);
        
        /// <summary>
        /// دریافت 10 تعامل اخیر کل سیستم
        /// </summary>
        Task<List<RecentInteractionItemViewModel>> GetRecentInteractionsAsync(int count = 10);
    }

    /// <summary>
    /// Interface برای Repository ارجاع/توصیه
    /// </summary>
    public interface IReferralRepository
    {
        Task<List<Referral>> GetAllAsync();
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
