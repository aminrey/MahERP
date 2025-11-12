using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services.BackgroundServices
{
    /// <summary>
    /// سرویس پس‌زمینه برای ساخت خودکار تسک‌های زمان‌بندی شده
    /// </summary>
    public class ScheduledTaskCreationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledTaskCreationBackgroundService> _logger;
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        public ScheduledTaskCreationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledTaskCreationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 ScheduledTaskCreationBackgroundService شروع شد");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledTasksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطا در پردازش تسک‌های زمان‌بندی شده");
                }

                // چک هر 1 دقیقه
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("⏹️ ScheduledTaskCreationBackgroundService متوقف شد");
        }

        private async Task ProcessScheduledTasksAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var scheduledTaskRepo = scope.ServiceProvider.GetRequiredService<IScheduledTaskCreationRepository>();
            var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var taskHistoryRepo = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();

            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            _logger.LogDebug($"🕐 زمان فعلی ایران: {nowIran:yyyy-MM-dd HH:mm:ss}");

            // دریافت زمان‌بندی‌های آماده برای اجرا
            var dueSchedules = await scheduledTaskRepo.GetDueScheduledTasksAsync();

            if (!dueSchedules.Any())
            {
                _logger.LogTrace("ℹ️ هیچ تسک زمان‌بندی شده‌ای برای اجرا وجود ندارد");
                return;
            }

            _logger.LogInformation($"⏰ {dueSchedules.Count} تسک زمان‌بندی شده آماده اجرا است");

            foreach (var schedule in dueSchedules)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                // ⭐⭐⭐ Double-check: جلوگیری از اجرای مکرر
                if (schedule.LastExecutionDate.HasValue &&
                    (DateTime.UtcNow - schedule.LastExecutionDate.Value).TotalMinutes < 1)
                {
                    _logger.LogWarning($"⚠️ زمان‌بندی #{schedule.Id} - {schedule.ScheduleTitle} در کمتر از 1 دقیقه پیش اجرا شده است. Skip.");
                    continue;
                }

                await ExecuteScheduledTaskAsync(schedule, scheduledTaskRepo, taskRepo, context, taskHistoryRepo);
            }
        }

        private async Task ExecuteScheduledTaskAsync(
            ScheduledTaskCreation schedule,
            IScheduledTaskCreationRepository scheduledTaskRepo,
            ITaskRepository taskRepo,
            AppDbContext context,
            ITaskHistoryRepository taskHistoryRepo)
        {
            _logger.LogInformation($"📤 شروع ساخت تسک زمان‌بندی شده: {schedule.ScheduleTitle}");

            try
            {
                // Deserialize TaskViewModel از JSON
                var taskModel = scheduledTaskRepo.DeserializeTaskData(schedule.TaskDataJson);
                if (taskModel == null)
                {
                    _logger.LogError($"❌ خطا در Deserialize داده تسک #{schedule.Id}");
                    await scheduledTaskRepo.UpdateExecutionStatusAsync(
                        schedule.Id,
                        false,
                        "خطا در خواندن اطلاعات تسک از JSON");
                    return;
                }

                // ⭐ ایجاد تسک با استفاده از transaction
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // ایجاد Entity تسک
                    var task = new Tasks
                    {
                        TaskCode = await GenerateTaskCodeAsync(context),
                        Title = taskModel.Title,
                        Description = taskModel.Description,
                        TaskType = taskModel.TaskType,
                        Priority = taskModel.Priority,
                        Important = taskModel.Important,
                        Status = 0,
                        BranchId = taskModel.BranchIdSelected,
                        CreatorUserId = schedule.CreatedByUserId,
                        ContactId = taskModel.SelectedContactId,
                        OrganizationId = taskModel.SelectedOrganizationId,
                        TaskCategoryId = taskModel.TaskCategoryIdSelected,
                        SuggestedStartDate = taskModel.SuggestedStartDate,
                        StartDate = taskModel.StartDate,
                        DueDate = taskModel.DueDate,
                        EstimatedHours = taskModel.EstimatedHours,
                        IsHardDeadline = taskModel.IsHardDeadline,
                        TimeNote = taskModel.TimeNote,
                        IsIndependentCompletion = taskModel.IsIndependentCompletion,
                        VisibilityLevel = taskModel.VisibilityLevel,
                        IsPrivate = taskModel.IsPrivate,
                        TeamId = taskModel.AssignmentsSelectedTeamIds?.FirstOrDefault(),
                        CreateDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        CreationMode = 1, // ⭐⭐⭐ خودکار
                        TaskTypeInput = 2  // ⭐⭐⭐ ساخته شده اتوماتیک
                    };

                    context.Tasks_Tbl.Add(task);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"✅ تسک با کد {task.TaskCode} ساخته شد");

                    // ⭐ ذخیره Assignments
                    if (taskModel.AssignmentsSelectedTaskUserArraysString != null && taskModel.AssignmentsSelectedTaskUserArraysString.Any())
                    {
                        await SaveTaskAssignmentsAsync(task, taskModel, context);
                    }

                    // ⭐ ذخیره Operations
                    if (!string.IsNullOrEmpty(taskModel.TaskOperationsJson))
                    {
                        await taskRepo.SaveTaskOperationsAndRemindersAsync(task.Id, taskModel);
                    }

                    // ⭐ ثبت در تاریخچه
                    await taskHistoryRepo.LogTaskCreatedAsync(
                        task.Id,
                        schedule.CreatedByUserId,
                        task.Title,
                        task.TaskCode);

                    await transaction.CommitAsync();

                    // ⭐⭐⭐ ارسال اعلان
                    NotificationProcessingBackgroundService.EnqueueTaskNotification(
                        task.Id,
                        schedule.CreatedByUserId,
                        NotificationEventType.TaskAssigned,
                        priority: 1
                    );

                    _logger.LogInformation($"✅ تسک #{task.Id} - {task.TaskCode} از زمان‌بندی #{schedule.Id} ساخته شد");

                    // بروزرسانی وضعیت زمان‌بندی
                    await scheduledTaskRepo.UpdateExecutionStatusAsync(
                        schedule.Id,
                        true,
                        $"تسک با موفقیت ساخته شد - TaskId: {task.Id}, TaskCode: {task.TaskCode}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌ خطا در ساخت تسک از زمان‌بندی #{schedule.Id}");

                    await scheduledTaskRepo.UpdateExecutionStatusAsync(
                        schedule.Id,
                        false,
                        $"خطا: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا کلی در اجرای زمان‌بندی #{schedule.Id}");
            }
        }

        private async Task SaveTaskAssignmentsAsync(Tasks task, dynamic taskModel, AppDbContext context)
        {
            var userTeamMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(
                taskModel.UserTeamAssignmentsJson ?? "{}");

            foreach (var userId in taskModel.AssignmentsSelectedTaskUserArraysString)
            {
                var cleanUserId = userId.Replace("[", "").Replace("]", "").Replace("/", "").Trim();
                
                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    AssignedUserId = cleanUserId,
                    AssignerUserId = task.CreatorUserId,
                    AssignmentDate = DateTime.UtcNow,
                    AssignedInTeamId = userTeamMap.ContainsKey(cleanUserId) ? userTeamMap[cleanUserId] : (int?)null
                };

                context.TaskAssignment_Tbl.Add(assignment);
            }

            await context.SaveChangesAsync();
        }

        private async Task<string> GenerateTaskCodeAsync(AppDbContext context)
        {
            var lastTask = await context.Tasks_Tbl
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (lastTask == null || string.IsNullOrEmpty(lastTask.TaskCode))
            {
                return "TSK-0000001";
            }

            if (lastTask.TaskCode.Contains("-"))
            {
                var parts = lastTask.TaskCode.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out var lastNumber))
                {
                    return $"TSK-{(lastNumber + 1):D7}";
                }
            }

            return $"TSK-{DateTime.UtcNow.Ticks % 10000000:D7}";
        }
    }
}
