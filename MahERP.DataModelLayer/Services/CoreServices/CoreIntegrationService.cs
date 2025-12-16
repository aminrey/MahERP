using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.CoreServices
{
    /// <summary>
    /// سرویس یکپارچگی مرکزی - مسئول هماهنگی بین ماژول‌های مختلف
    /// </summary>
    public class CoreIntegrationService : ICoreIntegrationService
    {
        private readonly AppDbContext _context;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        private readonly ILogger<CoreIntegrationService> _logger;

        public CoreIntegrationService(
            AppDbContext context,
            TaskCodeGenerator taskCodeGenerator,
            ILogger<CoreIntegrationService> logger)
        {
            _context = context;
            _taskCodeGenerator = taskCodeGenerator;
            _logger = logger;
        }

        #region ========== Task Creation ==========

        /// <summary>
        /// ایجاد تسک از هر ماژول - متد اصلی
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromModuleAsync(CoreTaskRequest request)
        {
            try
            {
                // 1. Validation
                if (string.IsNullOrEmpty(request.Title))
                    return CoreTaskResult.Failed("عنوان تسک الزامی است");

                if (string.IsNullOrEmpty(request.AssignedUserId))
                    return CoreTaskResult.Failed("کاربر انجام‌دهنده الزامی است");

                // 2. Generate Task Code
                string taskCode = _taskCodeGenerator.GenerateTaskCode();

                // 3. Create Task
                var task = new Tasks
                {
                    TaskCode = taskCode,
                    Title = request.Title,
                    Description = request.Description,
                    BranchId = request.BranchId,
                    CreatorUserId = request.CreatorUserId,
                    CreateDate = DateTime.Now,
                    DueDate = request.DueDate,
                    StartDate = request.StartDate,
                    Priority = request.Priority,
                    Important = request.IsImportant,
                    TaskCategoryId = request.TaskCategoryId,
                    DisplayLevel = request.DisplayLevel,
                    EstimatedHours = request.EstimatedHours,
                    Status = 0, // ایجاد شده
                    TaskType = 1, // تسک جهت اجرا
                    TaskTypeInput = 1, // کاربر ساخته
                    IsActive = true,
                    IsDeleted = false,

                    // Contact/Organization
                    ContactId = request.ContactId,
                    OrganizationId = request.OrganizationId
                };

                _context.Tasks_Tbl.Add(task);

                // 4. Create Task Assignment
                var assignment = new TaskAssignment
                {
                    Task = task,
                    AssignedUserId = request.AssignedUserId,
                    AssignerUserId = request.CreatorUserId,
                    AssignmentDate = DateTime.Now,
                    AssignmentType = 0, // اصلی (اجراکننده)
                    Status = 0, // تخصیص داده شده
                    IsFavorite = false,
                    IsMyDay = false,
                    IsRead = false
                };

                _context.TaskAssignment_Tbl.Add(assignment);

                await _context.SaveChangesAsync();

                // 5. Create ActivityBase + ActivityTask (برای ردیابی مرکزی)
                int? activityId = null;
                if (request.SourceModule != ModuleSourceType.Tasking)
                {
                    activityId = await CreateActivityForTaskAsync(task, request);
                }

                _logger.LogInformation(
                    "تسک از ماژول {Module} ایجاد شد: TaskId={TaskId}, Code={TaskCode}, CrmLeadId={LeadId}",
                    request.SourceModule, task.Id, taskCode, request.CrmLeadId);

                return CoreTaskResult.Succeeded(task.Id, taskCode, activityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد تسک از ماژول {Module}", request.SourceModule);
                return CoreTaskResult.Failed($"خطا در ایجاد تسک: {ex.Message}");
            }
        }

        /// <summary>
        /// ایجاد تسک از سرنخ CRM
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromCrmLeadAsync(CrmLeadTaskRequest request)
        {
            // TODO: نیاز به بازنویسی با سیستم جدید CRM
            // سیستم قبلی CrmLead حذف شده و با Interaction جایگزین شده است
            return CoreTaskResult.Failed("این قابلیت نیاز به بازنویسی دارد - سیستم CRM جدید");
        }

        /// <summary>
        /// ایجاد تسک از فرصت فروش CRM
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromCrmOpportunityAsync(CrmOpportunityTaskRequest request)
        {
            // TODO: نیاز به بازنویسی با سیستم جدید CRM
            return CoreTaskResult.Failed("این قابلیت نیاز به بازنویسی دارد - سیستم CRM جدید");
        }

        /// <summary>
        /// ایجاد تسک از پیگیری CRM (تبدیل FollowUp به Task)
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromCrmFollowUpAsync(CrmFollowUpTaskRequest request)
        {
            // TODO: نیاز به بازنویسی با سیستم جدید CRM
            // سیستم قبلی CrmFollowUp حذف شده و با Interaction جایگزین شده است
            return CoreTaskResult.Failed("این قابلیت نیاز به بازنویسی دارد - سیستم CRM جدید");
        }

        #endregion

        #region ========== Task Events (Callbacks) ==========

        /// <summary>
        /// وقتی تسک تکمیل می‌شود - ماژول منبع را مطلع کن
        /// </summary>
        public async Task OnTaskCompletedAsync(int taskId, string completedByUserId, string? completionNote = null)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null) return;

                // TODO: اگر تسک از CRM آمده - نیاز به بازنویسی با سیستم جدید
                // if (task.SourceModule == ModuleSourceType.CRM) { ... }

                // بروزرسانی ActivityBase
                await UpdateActivityStatusAsync(taskId, 2); // تکمیل شده
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش تکمیل تسک: TaskId={TaskId}", taskId);
            }
        }

        /// <summary>
        /// وقتی وضعیت تسک تغییر می‌کند
        /// </summary>
        public async Task OnTaskStatusChangedAsync(int taskId, byte newStatus, string changedByUserId)
        {
            try
            {
                // بروزرسانی ActivityBase
                await UpdateActivityStatusAsync(taskId, newStatus);

                _logger.LogInformation(
                    "وضعیت تسک تغییر کرد: TaskId={TaskId}, NewStatus={Status}",
                    taskId, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش تغییر وضعیت تسک: TaskId={TaskId}", taskId);
            }
        }

        /// <summary>
        /// وقتی تسک حذف می‌شود
        /// </summary>
        public async Task OnTaskDeletedAsync(int taskId, string deletedByUserId)
        {
            try
            {
                var task = await _context.Tasks_Tbl.FindAsync(taskId);
                if (task == null) return;

                // TODO: اگر تسک از CRM آمده - نیاز به بازنویسی با سیستم جدید
                
                // غیرفعال کردن ActivityTask
                var activityTask = await _context.ActivityTask_Tbl
                    .FirstOrDefaultAsync(at => at.TaskId == taskId && at.IsActive);
                
                if (activityTask != null)
                {
                    activityTask.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("تسک حذف شد: TaskId={TaskId}", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش حذف تسک: TaskId={TaskId}", taskId);
            }
        }

        #endregion

        #region ========== CRM Specific ==========

        /// <summary>
        /// اعمال قانون "اقدام بعدی اجباری" برای Lead
        /// </summary>
        public async Task<bool> ValidateLeadHasNextActionAsync(int leadId)
        {
            // TODO: نیاز به بازنویسی با سیستم جدید CRM
            // بررسی وجود تسک فعال
            var hasActiveTask = await _context.Tasks_Tbl
                .AnyAsync(t => t.CrmLeadId == leadId 
                            && t.IsActive 
                            && !t.IsDeleted 
                            && t.Status < 2); // غیر تکمیل

            return hasActiveTask;
        }

        /// <summary>
        /// اعمال قانون "اقدام بعدی اجباری" برای Opportunity
        /// </summary>
        public async Task<bool> ValidateOpportunityHasNextActionAsync(int opportunityId)
        {
            // TODO: نیاز به بازنویسی با سیستم جدید CRM
            return true;
        }

        /// <summary>
        /// دریافت تسک‌های فعال یک Lead
        /// </summary>
        public async Task<List<Tasks>> GetActiveTasksForLeadAsync(int leadId)
        {
            return await _context.Tasks_Tbl
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Where(t => t.CrmLeadId == leadId 
                         && t.IsActive 
                         && !t.IsDeleted 
                         && t.Status < 2)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تسک‌های فعال یک Opportunity
        /// </summary>
        public async Task<List<Tasks>> GetActiveTasksForOpportunityAsync(int opportunityId)
        {
            return await _context.Tasks_Tbl
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.AssignedUser)
                .Where(t => t.CrmOpportunityId == opportunityId 
                         && t.IsActive 
                         && !t.IsDeleted 
                         && t.Status < 2)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        #endregion

        #region ========== Activity Logging ==========

        /// <summary>
        /// ثبت فعالیت در سیستم مرکزی
        /// </summary>
        public async Task<int> LogActivityAsync(
            string title,
            ModuleSourceType sourceModule,
            byte activityType,
            int branchId,
            string creatorUserId,
            string? description = null,
            int? relatedTaskId = null,
            int? relatedCrmId = null)
        {
            try
            {
                var activity = new ActivityBase
                {
                    Title = title,
                    Description = description,
                    ActivityType = activityType,
                    BranchId = branchId,
                    CreatorUserId = creatorUserId,
                    CreateDate = DateTime.Now,
                    Status = 0,
                    IsActive = true
                };

                _context.ActivityBase_Tbl.Add(activity);
                await _context.SaveChangesAsync();

                // ایجاد ارتباط با Task
                if (relatedTaskId.HasValue)
                {
                    var activityTask = new ActivityTask
                    {
                        ActivityId = activity.Id,
                        TaskId = relatedTaskId.Value,
                        RelationType = 1, // Task از Activity ایجاد شده
                        Description = $"ایجاد شده از ماژول {sourceModule}",
                        CreateDate = DateTime.Now,
                        CreatorUserId = creatorUserId,
                        IsActive = true
                    };

                    _context.ActivityTask_Tbl.Add(activityTask);
                }

                // ایجاد ارتباط با CRM
                if (relatedCrmId.HasValue)
                {
                    var activityCrm = new ActivityCRM
                    {
                        ActivityId = activity.Id,
                        CRMId = relatedCrmId.Value,
                        RelationType = 1,
                        Description = $"ایجاد شده از ماژول {sourceModule}",
                        CreateDate = DateTime.Now,
                        CreatorUserId = creatorUserId,
                        IsActive = true
                    };

                    _context.ActivityCRM_Tbl.Add(activityCrm);
                }

                await _context.SaveChangesAsync();

                return activity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت فعالیت");
                return 0;
            }
        }

        #endregion

        #region ========== Utility ==========

        /// <summary>
        /// دریافت اطلاعات منبع تسک
        /// </summary>
        public async Task<CrmTaskSourceInfo?> GetTaskSourceInfoAsync(int taskId)
        {
            var task = await _context.Tasks_Tbl
                .Include(t => t.CrmLead)
                .Include(t => t.CrmFollowUp)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.SourceModule != ModuleSourceType.CRM)
                return null;

            return new CrmTaskSourceInfo
            {
                SourceType = task.CrmSourceType ?? CrmTaskSourceType.Manual,
                LeadId = task.CrmLeadId,
                OpportunityId = task.CrmOpportunityId,
                FollowUpId = task.CrmFollowUpId,
                TicketId = task.CrmTicketId,
                ContractId = task.CrmContractId,
                CustomerId = task.CrmCustomerId,
                SourceDisplayName = task.CrmLead?.DisplayName,
                SourceUrl = task.CrmLeadId.HasValue 
                    ? $"/CrmArea/Leads/Details/{task.CrmLeadId}" 
                    : null
            };
        }

        /// <summary>
        /// بررسی اینکه آیا تسک از CRM آمده؟
        /// </summary>
        public async Task<bool> IsTaskFromCrmAsync(int taskId)
        {
            return await _context.Tasks_Tbl
                .AnyAsync(t => t.Id == taskId && t.SourceModule == ModuleSourceType.CRM);
        }

        #endregion

        #region ========== Private Helpers ==========

        private async Task<int?> CreateActivityForTaskAsync(Tasks task, CoreTaskRequest request)
        {
            try
            {
                var activity = new ActivityBase
                {
                    Title = task.Title,
                    Description = task.Description,
                    ActivityType = (byte)request.SourceModule, // نوع فعالیت بر اساس ماژول
                    BranchId = task.BranchId ?? 0,
                    CreatorUserId = task.CreatorUserId!,
                    CreateDate = DateTime.Now,
                    DueDate = task.DueDate,
                    Status = 0,
                    Priority = task.Priority,
                    IsActive = true
                };

                _context.ActivityBase_Tbl.Add(activity);
                await _context.SaveChangesAsync();

                // ایجاد ارتباط Activity ↔ Task
                var activityTask = new ActivityTask
                {
                    ActivityId = activity.Id,
                    TaskId = task.Id,
                    RelationType = 1, // Task از Activity ایجاد شده
                    Description = $"تسک ایجاد شده از {request.SourceModule}",
                    CreateDate = DateTime.Now,
                    CreatorUserId = task.CreatorUserId!,
                    IsActive = true
                };

                _context.ActivityTask_Tbl.Add(activityTask);
                await _context.SaveChangesAsync();

                return activity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد Activity برای تسک: TaskId={TaskId}", task.Id);
                return null;
            }
        }

        private async Task UpdateActivityStatusAsync(int taskId, byte newStatus)
        {
            var activityTask = await _context.ActivityTask_Tbl
                .Include(at => at.Activity)
                .FirstOrDefaultAsync(at => at.TaskId == taskId && at.IsActive);

            if (activityTask?.Activity != null)
            {
                activityTask.Activity.Status = newStatus;
                activityTask.Activity.LastUpdateDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        private string GetCrmActionTitle(CrmNextActionType actionType, string targetName)
        {
            return actionType switch
            {
                CrmNextActionType.Call => $"تماس با {targetName}",
                CrmNextActionType.Meeting => $"جلسه با {targetName}",
                CrmNextActionType.Email => $"ارسال ایمیل به {targetName}",
                CrmNextActionType.Sms => $"ارسال پیامک به {targetName}",
                CrmNextActionType.SendQuote => $"ارسال پیشنهاد قیمت به {targetName}",
                CrmNextActionType.FollowUpQuote => $"پیگیری پیشنهاد {targetName}",
                CrmNextActionType.Visit => $"بازدید {targetName}",
                CrmNextActionType.Demo => $"دمو برای {targetName}",
                _ => $"پیگیری {targetName}"
            };
        }

        #endregion
    }
}
