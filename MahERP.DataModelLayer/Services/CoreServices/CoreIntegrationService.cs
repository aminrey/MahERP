using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository.CrmRepository;
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
        private readonly ICrmLeadRepository _leadRepo;
        private readonly ICrmFollowUpRepository _followUpRepo;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        private readonly ILogger<CoreIntegrationService> _logger;

        public CoreIntegrationService(
            AppDbContext context,
            ICrmLeadRepository leadRepo,
            ICrmFollowUpRepository followUpRepo,
            TaskCodeGenerator taskCodeGenerator,
            ILogger<CoreIntegrationService> logger)
        {
            _context = context;
            _leadRepo = leadRepo;
            _followUpRepo = followUpRepo;
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

                    // ⭐ CRM Integration
                    SourceModule = request.SourceModule,
                    CrmSourceType = request.CrmSourceType,
                    CrmLeadId = request.CrmLeadId,
                    CrmOpportunityId = request.CrmOpportunityId,
                    CrmFollowUpId = request.CrmFollowUpId,
                    CrmTicketId = request.CrmTicketId,
                    CrmContractId = request.CrmContractId,
                    CrmCustomerId = request.CrmCustomerId,

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
            try
            {
                // دریافت اطلاعات Lead
                var lead = await _leadRepo.GetByIdAsync(request.LeadId, includeDetails: true);
                if (lead == null)
                    return CoreTaskResult.Failed("سرنخ یافت نشد");

                // تعیین کاربر انجام‌دهنده
                string assignedUserId = request.AssignedUserId ?? lead.AssignedUserId;

                // ساخت عنوان تسک
                string title = GetCrmActionTitle(request.ActionType, lead.DisplayName);

                // ساخت توضیحات
                string description = $"پیگیری سرنخ: {lead.DisplayName}";
                if (!string.IsNullOrEmpty(request.Note))
                    description += $"\n\nیادداشت: {request.Note}";

                // ایجاد درخواست تسک
                var taskRequest = new CoreTaskRequest
                {
                    Title = title,
                    Description = description,
                    AssignedUserId = assignedUserId,
                    BranchId = lead.BranchId,
                    CreatorUserId = request.CreatorUserId,
                    DueDate = request.DueDate,
                    Priority = (byte)request.Priority,
                    IsImportant = request.Priority >= CrmTaskPriority.High,

                    // CRM Info
                    SourceModule = ModuleSourceType.CRM,
                    CrmSourceType = CrmTaskSourceType.LeadFollowUp,
                    CrmLeadId = request.LeadId,

                    // Contact/Organization
                    ContactId = lead.ContactId,
                    OrganizationId = lead.OrganizationId,

                    NotifyAssignee = true
                };

                var result = await CreateTaskFromModuleAsync(taskRequest);

                // بروزرسانی NextFollowUpDate در Lead
                if (result.Success)
                {
                    await _leadRepo.UpdateNextFollowUpDateAsync(request.LeadId, request.DueDate);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد تسک از سرنخ CRM: LeadId={LeadId}", request.LeadId);
                return CoreTaskResult.Failed($"خطا: {ex.Message}");
            }
        }

        /// <summary>
        /// ایجاد تسک از فرصت فروش CRM
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromCrmOpportunityAsync(CrmOpportunityTaskRequest request)
        {
            // TODO: پیاده‌سازی بعد از ایجاد Entity CrmOpportunity
            return CoreTaskResult.Failed("این قابلیت هنوز پیاده‌سازی نشده است");
        }

        /// <summary>
        /// ایجاد تسک از پیگیری CRM (تبدیل FollowUp به Task)
        /// </summary>
        public async Task<CoreTaskResult> CreateTaskFromCrmFollowUpAsync(CrmFollowUpTaskRequest request)
        {
            try
            {
                // دریافت اطلاعات FollowUp
                var followUp = await _followUpRepo.GetByIdAsync(request.FollowUpId);
                if (followUp == null)
                    return CoreTaskResult.Failed("پیگیری یافت نشد");

                // دریافت اطلاعات Lead
                var lead = await _leadRepo.GetByIdAsync(followUp.LeadId, includeDetails: true);
                if (lead == null)
                    return CoreTaskResult.Failed("سرنخ مرتبط یافت نشد");

                // تعیین عنوان و توضیحات
                string title = request.UseFollowUpData
                    ? followUp.DisplayTitle
                    : request.CustomTitle ?? followUp.DisplayTitle;

                string description = request.UseFollowUpData
                    ? $"پیگیری: {followUp.Description}\n\nسرنخ: {lead.DisplayName}"
                    : request.CustomDescription ?? "";

                DateTime dueDate = request.UseFollowUpData
                    ? followUp.DueDate
                    : request.CustomDueDate ?? followUp.DueDate;

                // ایجاد درخواست تسک
                var taskRequest = new CoreTaskRequest
                {
                    Title = title,
                    Description = description,
                    AssignedUserId = followUp.AssignedUserId,
                    BranchId = lead.BranchId,
                    CreatorUserId = request.CreatorUserId,
                    DueDate = dueDate,
                    Priority = followUp.Priority,
                    IsImportant = followUp.Priority >= 2,

                    // CRM Info
                    SourceModule = ModuleSourceType.CRM,
                    CrmSourceType = CrmTaskSourceType.LeadFollowUp,
                    CrmLeadId = followUp.LeadId,
                    CrmFollowUpId = followUp.Id,

                    // Contact/Organization
                    ContactId = lead.ContactId,
                    OrganizationId = lead.OrganizationId,

                    NotifyAssignee = true
                };

                var result = await CreateTaskFromModuleAsync(taskRequest);

                // بروزرسانی FollowUp با TaskId
                if (result.Success && result.TaskId.HasValue)
                {
                    await _followUpRepo.ConvertToTaskAsync(followUp.Id, result.TaskId.Value, request.CreatorUserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد تسک از پیگیری CRM: FollowUpId={FollowUpId}", request.FollowUpId);
                return CoreTaskResult.Failed($"خطا: {ex.Message}");
            }
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
                    .Include(t => t.CrmLead)
                    .Include(t => t.CrmFollowUp)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null) return;

                // اگر تسک از CRM آمده
                if (task.SourceModule == ModuleSourceType.CRM)
                {
                    // بروزرسانی CrmFollowUp
                    if (task.CrmFollowUpId.HasValue)
                    {
                        await _followUpRepo.CompleteAsync(task.CrmFollowUpId.Value, completionNote, completedByUserId);
                    }

                    // بروزرسانی تاریخ آخرین تماس در Lead
                    if (task.CrmLeadId.HasValue)
                    {
                        await _leadRepo.UpdateLastContactDateAsync(task.CrmLeadId.Value, DateTime.Now);
                        await _leadRepo.UpdateNextFollowUpDateAsync(task.CrmLeadId.Value);
                    }

                    _logger.LogInformation(
                        "تسک CRM تکمیل شد: TaskId={TaskId}, LeadId={LeadId}, FollowUpId={FollowUpId}",
                        taskId, task.CrmLeadId, task.CrmFollowUpId);
                }

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

                // اگر تسک از CRM آمده، FollowUp را آزاد کن
                if (task.CrmFollowUpId.HasValue)
                {
                    var followUp = await _context.CrmFollowUp_Tbl.FindAsync(task.CrmFollowUpId.Value);
                    if (followUp != null)
                    {
                        followUp.TaskId = null;
                        await _context.SaveChangesAsync();
                    }
                }

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
            // بررسی وجود تسک فعال
            var hasActiveTask = await _context.Tasks_Tbl
                .AnyAsync(t => t.CrmLeadId == leadId 
                            && t.IsActive 
                            && !t.IsDeleted 
                            && t.Status < 2); // غیر تکمیل

            if (hasActiveTask) return true;

            // بررسی وجود FollowUp فعال
            var hasActiveFollowUp = await _context.CrmFollowUp_Tbl
                .AnyAsync(f => f.LeadId == leadId 
                            && f.IsActive 
                            && f.Status == 0); // در انتظار

            return hasActiveFollowUp;
        }

        /// <summary>
        /// اعمال قانون "اقدام بعدی اجباری" برای Opportunity
        /// </summary>
        public async Task<bool> ValidateOpportunityHasNextActionAsync(int opportunityId)
        {
            // TODO: پیاده‌سازی بعد از ایجاد Entity CrmOpportunity
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
