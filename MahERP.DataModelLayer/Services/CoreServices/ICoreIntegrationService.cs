using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.CoreServices
{
    /// <summary>
    /// سرویس یکپارچگی مرکزی - واسط ارتباط بین ماژول‌های مختلف
    /// این سرویس مسئول هماهنگی بین CRM، Tasking و سایر ماژول‌هاست
    /// </summary>
    public interface ICoreIntegrationService
    {
        #region ========== Task Creation ==========

        /// <summary>
        /// ایجاد تسک از هر ماژول
        /// این متد اصلی‌ترین نقطه ورود برای ایجاد تسک از ماژول‌های دیگر است
        /// </summary>
        /// <param name="request">درخواست ایجاد تسک</param>
        /// <returns>نتیجه عملیات شامل TaskId</returns>
        Task<CoreTaskResult> CreateTaskFromModuleAsync(CoreTaskRequest request);

        /// <summary>
        /// ایجاد تسک از سرنخ CRM
        /// </summary>
        Task<CoreTaskResult> CreateTaskFromCrmLeadAsync(CrmLeadTaskRequest request);

        /// <summary>
        /// ایجاد تسک از فرصت فروش CRM
        /// </summary>
        Task<CoreTaskResult> CreateTaskFromCrmOpportunityAsync(CrmOpportunityTaskRequest request);

        /// <summary>
        /// ایجاد تسک از پیگیری CRM (تبدیل FollowUp به Task)
        /// </summary>
        Task<CoreTaskResult> CreateTaskFromCrmFollowUpAsync(CrmFollowUpTaskRequest request);

        /// <summary>
        /// ایجاد تسک از تیکت پشتیبانی (آینده)
        /// </summary>
        // Task<CoreTaskResult> CreateTaskFromCrmTicketAsync(int ticketId, string creatorUserId);

        /// <summary>
        /// ایجاد تسک تمدید قرارداد (آینده)
        /// </summary>
        // Task<CoreTaskResult> CreateTaskFromContractRenewalAsync(int contractId, string creatorUserId);

        #endregion

        #region ========== Task Events (Callbacks) ==========

        /// <summary>
        /// وقتی تسک تکمیل می‌شود - ماژول منبع را مطلع کن
        /// </summary>
        /// <param name="taskId">شناسه تسک</param>
        /// <param name="completedByUserId">کاربر تکمیل کننده</param>
        /// <param name="completionNote">یادداشت تکمیل</param>
        Task OnTaskCompletedAsync(int taskId, string completedByUserId, string? completionNote = null);

        /// <summary>
        /// وقتی وضعیت تسک تغییر می‌کند
        /// </summary>
        Task OnTaskStatusChangedAsync(int taskId, byte newStatus, string changedByUserId);

        /// <summary>
        /// وقتی تسک حذف می‌شود
        /// </summary>
        Task OnTaskDeletedAsync(int taskId, string deletedByUserId);

        #endregion

        #region ========== CRM Specific ==========

        /// <summary>
        /// اعمال قانون "اقدام بعدی اجباری" برای Lead
        /// اگر Lead بدون اقدام بعدی باشد، هشدار بده
        /// </summary>
        Task<bool> ValidateLeadHasNextActionAsync(int leadId);

        /// <summary>
        /// اعمال قانون "اقدام بعدی اجباری" برای Opportunity
        /// </summary>
        Task<bool> ValidateOpportunityHasNextActionAsync(int opportunityId);

        /// <summary>
        /// دریافت تسک‌های فعال یک Lead
        /// </summary>
        Task<List<Tasks>> GetActiveTasksForLeadAsync(int leadId);

        /// <summary>
        /// دریافت تسک‌های فعال یک Opportunity
        /// </summary>
        Task<List<Tasks>> GetActiveTasksForOpportunityAsync(int opportunityId);

        #endregion

        #region ========== Activity Logging ==========

        /// <summary>
        /// ثبت فعالیت در سیستم مرکزی
        /// </summary>
        Task<int> LogActivityAsync(
            string title,
            ModuleSourceType sourceModule,
            byte activityType,
            int branchId,
            string creatorUserId,
            string? description = null,
            int? relatedTaskId = null,
            int? relatedCrmId = null);

        #endregion

        #region ========== Utility ==========

        /// <summary>
        /// دریافت اطلاعات منبع تسک
        /// </summary>
        Task<CrmTaskSourceInfo?> GetTaskSourceInfoAsync(int taskId);

        /// <summary>
        /// بررسی اینکه آیا تسک از CRM آمده؟
        /// </summary>
        Task<bool> IsTaskFromCrmAsync(int taskId);

        #endregion
    }

    /// <summary>
    /// اطلاعات منبع تسک CRM
    /// </summary>
    public class CrmTaskSourceInfo
    {
        public CrmTaskSourceType SourceType { get; set; }
        public int? LeadId { get; set; }
        public int? OpportunityId { get; set; }
        public int? FollowUpId { get; set; }
        public int? TicketId { get; set; }
        public int? ContractId { get; set; }
        public int? CustomerId { get; set; }

        /// <summary>
        /// نام نمایشی منبع
        /// </summary>
        public string? SourceDisplayName { get; set; }

        /// <summary>
        /// لینک به منبع
        /// </summary>
        public string? SourceUrl { get; set; }
    }
}
