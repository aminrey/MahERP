using MahERP.DataModelLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MahERP.DataModelLayer.Services.CoreServices
{
    /// <summary>
    /// ⭐⭐⭐ درخواست عمومی ایجاد تسک از ماژول‌های مختلف
    /// </summary>
    public class CoreTaskRequest
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public byte Priority { get; set; } = 0;

        public bool IsImportant { get; set; }

        [Required]
        public string CreatorUserId { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }

        public int? BranchId { get; set; }

        public int? TaskCategoryId { get; set; }

        public byte DisplayLevel { get; set; } = 0;

        public decimal? EstimatedHours { get; set; }

        public bool AddToMyDay { get; set; } = true;

        public bool NotifyAssignee { get; set; } = true;

        // CRM Integration
        public ModuleSourceType SourceModule { get; set; } = ModuleSourceType.Tasking;
        public CrmTaskSourceType? CrmSourceType { get; set; }
        public int? CrmLeadId { get; set; }
        public int? CrmOpportunityId { get; set; }
        public int? CrmFollowUpId { get; set; }
        public int? CrmTicketId { get; set; }
        public int? CrmContractId { get; set; }
        public int? CrmCustomerId { get; set; }
        
        // Stakeholder
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ درخواست پایه برای ایجاد تسک
    /// </summary>
    public abstract class CoreTaskRequestBase
    {
        [MaxLength(500)]
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public CrmTaskPriority Priority { get; set; } = CrmTaskPriority.Normal;
        [Required]
        public string CreatorUserId { get; set; } = string.Empty;
        public string? AssignedUserId { get; set; }
        public int? BranchId { get; set; }
        public bool AddToMyDay { get; set; } = true;
        public bool CreateFollowUpRecord { get; set; } = false;
    }

    /// <summary>
    /// ⭐⭐⭐ درخواست ایجاد تسک از سرنخ CRM
    /// </summary>
    public class CrmLeadTaskRequest : CoreTaskRequestBase
    {
        [Required]
        public int LeadId { get; set; }
        [Required]
        public CrmNextActionType ActionType { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ درخواست ایجاد تسک از پیگیری CRM
    /// </summary>
    public class CrmFollowUpTaskRequest : CoreTaskRequestBase
    {
        [Required]
        public int FollowUpId { get; set; }
        public bool UseFollowUpData { get; set; } = true;
        public string? CustomTitle { get; set; }
        public string? CustomDescription { get; set; }
        public DateTime? CustomDueDate { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ درخواست ایجاد تسک از فرصت فروش
    /// </summary>
    public class CrmOpportunityTaskRequest : CoreTaskRequestBase
    {
        [Required]
        public int OpportunityId { get; set; }
        [Required]
        public CrmNextActionType ActionType { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ نتیجه ایجاد تسک
    /// </summary>
    public class CoreTaskResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? TaskId { get; set; }
        public string? TaskCode { get; set; }
        public int? ActivityId { get; set; }
        public int? FollowUpId { get; set; }
        public List<string> Errors { get; set; } = new();

        public static CoreTaskResult Succeeded(int taskId, string taskCode, int? activityId = null, string message = "تسک با موفقیت ایجاد شد")
        {
            return new CoreTaskResult { Success = true, TaskId = taskId, TaskCode = taskCode, ActivityId = activityId, Message = message };
        }

        public static CoreTaskResult Failed(string message, params string[] errors)
        {
            return new CoreTaskResult { Success = false, Message = message, Errors = errors?.ToList() ?? new List<string>() };
        }
    }
}
