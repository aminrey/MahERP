using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت آمار و فیلترهای تسک‌ها
    /// شامل: محاسبه آمار، اعمال فیلترها، بررسی فیلترهای فعال
    /// </summary>
    public partial class TaskRepository
    {
        
            #region Permission Checks

            /// <summary>
            /// بررسی اینکه آیا کاربر می‌تواند عمل خاصی را انجام دهد
            /// </summary>
            public async Task<bool> CanUserPerformActionAsync(int taskId, string userId, TaskAction action)
            {
                // دریافت نقش کاربر
                var userRole = await GetUserRoleInTaskAsync(taskId, userId);
                if (userRole == null)
                    return false;

                // دریافت تنظیمات
                var settings = await GetTaskSettingsAsync(taskId);

                // بررسی بر اساس نوع عمل
                return action switch
                {
                    TaskAction.Comment => settings.CanComment(userRole.Value),
                    TaskAction.AddMember => settings.CanAddMembers(userRole.Value),
                    TaskAction.RemoveMember => settings.CanRemoveMembers(userRole.Value),
                    TaskAction.EditAfterCompletion => await CheckEditAfterCompletionAsync(taskId, userRole.Value, settings),
                    TaskAction.EditOrDelete => await CheckEditOrDeleteAsync(taskId, userRole.Value, settings),
                    _ => false
                };
            }

            /// <summary>
            /// دریافت بالاترین نقش کاربر در تسک
            /// </summary>
            public async Task<TaskRole?> GetUserRoleInTaskAsync(int taskId, string userId)
            {
                var roles = await GetUserRolesInTaskAsync(taskId, userId);

                // بالاترین نقش (کمترین عدد) را برگردان
                return roles.Any() ? roles.Min() : null;
            }

            /// <summary>
            /// دریافت تمام نقش‌های کاربر در تسک
            /// </summary>
            public async Task<List<TaskRole>> GetUserRolesInTaskAsync(int taskId, string userId)
            {
                var roles = new List<TaskRole>();

                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskAssignments)
                    .Include(t => t.Team)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    return roles;

                // ⭐⭐⭐ دریافت assignment کاربر در این تسک
                var userAssignment = task.TaskAssignments?
                    .FirstOrDefault(a => a.AssignedUserId == userId);

                // ⭐⭐⭐ تیم صحیح:
                // - اگر کاربر منتصب است: تیمی که در آن منتصب شده (AssignedInTeamId)
                // - اگر کاربر منتصب نیست (مثلاً فقط سازنده): تیم اصلی تسک (task.TeamId)
                int? relevantTeamId = userAssignment?.AssignedInTeamId ?? task.TeamId;

                // 1. بررسی مدیر تیم
                if (relevantTeamId.HasValue)
                {
                    // اگر از assignment آمده، تیم را جداگانه بگیر
                    // اگر از task.TeamId آمده، team از include موجود است
                    string? teamManagerId = null;
                    
                    if (userAssignment?.AssignedInTeamId != null)
                    {
                        var assignedTeam = await _context.Team_Tbl
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == userAssignment.AssignedInTeamId);
                        teamManagerId = assignedTeam?.ManagerUserId;
                    }
                    else
                    {
                        teamManagerId = task.Team?.ManagerUserId;
                    }

                    if (teamManagerId == userId)
                    {
                        roles.Add(TaskRole.Manager);
                    }
                }

                // 2. بررسی سازنده
                if (task.CreatorUserId == userId)
                {
                    roles.Add(TaskRole.Creator);
                }

                // 3. بررسی عضو (اگر assignment دارد)
                // ⭐⭐⭐ AssignmentType: 0=اجراکننده، 1=رونوشت، 2=ناظر
                // همه assignment ها (حتی رونوشت و ناظر) باید دسترسی Member داشته باشند
                if (userAssignment != null)
                {
                    roles.Add(TaskRole.Member);
                }

                // 4. بررسی ناظر (از TeamMember با MembershipType = 1)
                if (relevantTeamId.HasValue)
                {
                    var isSupervisor = await _context.TeamMember_Tbl
                        .AnyAsync(tm => tm.TeamId == relevantTeamId.Value &&
                                       tm.UserId == userId &&
                                       tm.MembershipType == 1 &&
                                       tm.IsActive);

                    if (isSupervisor)
                    {
                        roles.Add(TaskRole.Supervisor);
                    }
                }

                // 5. بررسی رونوشت
                var isCarbonCopy = await _context.TaskCarbonCopy_Tbl
                    .AnyAsync(cc => cc.TaskId == taskId &&
                                   cc.UserId == userId &&
                                   cc.IsActive);

                if (isCarbonCopy)
                {
                    roles.Add(TaskRole.CarbonCopy);
                }

                return roles;
            }

            /// <summary>
            /// بررسی اینکه آیا کاربر می‌تواند تنظیمات را ویرایش کند
            /// ⭐⭐⭐ بر اساس تنظیم CanEditSettingsRoles و Authority Level
            /// </summary>
            public async Task<bool> CanUserEditSettingsAsync(int taskId, string userId)
            {
                // ⭐⭐⭐ ابتدا بررسی Admin سیستم
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user?.IsAdmin == true)
                {
                    return true;
                }

                // ⭐⭐⭐ دریافت نقش کاربر در تسک
                var userRole = await GetUserRoleInTaskAsync(taskId, userId);
                if (userRole == null)
                {
                    return false;
                }

                // ⭐⭐⭐ دریافت تنظیمات تسک
                var settings = await GetTaskSettingsAsync(taskId);

                // ⭐⭐⭐ بررسی آیا نقش کاربر در لیست CanEditSettingsRoles است
                if (!settings.CanEditSettings(userRole.Value))
                {
                    return false;
                }

                // ⭐⭐⭐ بررسی HierarchyManager (مدیر تیم‌های بالاتر)
                var task = await _context.Tasks_Tbl
                    .Include(t => t.Team)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task?.TeamId != null && userRole.Value != TaskRole.Manager)
                {
                    var isHierarchyManager = await IsUserHierarchyManagerOfTaskAsync(userId, task.TeamId.Value);
                    if (isHierarchyManager)
                    {
                        return true;
                    }
                }

                return true;
            }

            /// <summary>
            /// بررسی Authority Level
            /// </summary>
            public bool CanManageRole(TaskRole userRole, TaskRole targetRole)
            {
                // نمی‌توان نقش بالاتر از خود را مدیریت کرد
                return (int)userRole <= (int)targetRole;
            }

            #endregion

            #region Private Helper Methods

            /// <summary>
            /// بررسی دسترسی ویرایش پس از اتمام
            /// </summary>
            private async Task<bool> CheckEditAfterCompletionAsync(
                int taskId,
                TaskRole userRole,
                TaskSettings settings)
            {
                // بررسی اینکه آیا تسک تکمیل شده است
                // تسک زمانی تکمیل است که همه assignment ها تکمیل شده باشند
                var hasCompletedAssignments = await _context.TaskAssignment_Tbl
                    .AnyAsync(a => a.TaskId == taskId && a.CompletionDate.HasValue);

                if (!hasCompletedAssignments)
                {
                    // تسک تکمیل نشده، پس نیازی به این چک نیست
                    return true;
                }

                // تسک تکمیل شده، بررسی تنظیمات
                return settings.CanEditAfterCompletion(userRole);
            }

            /// <summary>
            /// بررسی دسترسی حذف/ویرایش
            /// </summary>
            private async Task<bool> CheckEditOrDeleteAsync(
                int taskId,
                TaskRole userRole,
                TaskSettings settings)
            {
                // مدیر همیشه می‌تواند ویرایش/حذف کند
                if (userRole == TaskRole.Manager)
                    return true;

                // سازنده بر اساس تنظیمات
                if (userRole == TaskRole.Creator)
                    return settings.CreatorCanEditDelete;

                // سایر نقش‌ها نمی‌توانند
                return false;
            }

            #endregion

            #region Settings Change Log

            /// <summary>
            /// ثبت تغییر در لاگ
            /// </summary>
            public async Task LogSettingChangeAsync(TaskSettingsChangeLog log)
            {
                await _context.TaskSettingsChangeLog_Tbl.AddAsync(log);
                await _context.SaveChangesAsync();
            }

            /// <summary>
            /// دریافت تاریخچه تغییرات
            /// </summary>
            public async Task<List<TaskSettingsChangeLog>> GetSettingsChangeHistoryAsync(int taskId)
            {
                return await _context.TaskSettingsChangeLog_Tbl
                    .Where(l => l.TaskId == taskId)
                    .OrderByDescending(l => l.ChangeDate)
                    .Take(50) // محدود به 50 تغییر آخر
                    .ToListAsync();
            }

        #endregion

  

       

            #region Task Settings CRUD

            /// <summary>
            /// دریافت تنظیمات یک تسک با سیستم وراثت
            /// Priority: Task → Category → Branch → Global
            /// </summary>
            public async Task<TaskSettings> GetTaskSettingsAsync(int taskId)
            {
                // 1. بررسی تنظیمات خود تسک
                var taskSettings = await GetTaskSettingsRawAsync(taskId);

                if (taskSettings != null && !taskSettings.IsInherited)
                {
                    return taskSettings;
                }

                // 2. دریافت اطلاعات تسک برای وراثت
                var task = await _context.Tasks_Tbl
                    .AsNoTracking()
                    .Where(t => t.Id == taskId)
                    .Select(t => new { t.TaskCategoryId, t.BranchId })
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    throw new InvalidOperationException($"تسک با شناسه {taskId} یافت نشد");
                }

                // 3. بررسی تنظیمات دسته‌بندی
                if (task.TaskCategoryId.HasValue)
                {
                    var categorySettings = await GetCategoryDefaultSettingsAsync(task.TaskCategoryId.Value);
                    if (categorySettings != null)
                    {
                        return categorySettings.ToTaskSettings(taskId, "system");
                    }
                }

                // 4. بررسی تنظیمات شعبه
                if (task.BranchId.HasValue)
                {
                    var branchSettings = await GetBranchDefaultSettingsAsync(task.BranchId.Value);
                    if (branchSettings != null)
                    {
                        return branchSettings.ToTaskSettings(taskId, "system");
                    }
                }

                // 5. تنظیمات پیش‌فرض سیستم (Global)
                return GetGlobalDefaultSettings();
            }

            /// <summary>
            /// دریافت تنظیمات خام تسک (بدون وراثت)
            /// </summary>
            public async Task<TaskSettings?> GetTaskSettingsRawAsync(int taskId)
            {
                return await _context.TaskSettings_Tbl
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.TaskId == taskId);
            }

            /// <summary>
            /// ذخیره تنظیمات تسک
            /// </summary>
            public async Task<TaskSettings> SaveTaskSettingsAsync(TaskSettings settings, string userId)
            {
                var existingSettings = await _context.TaskSettings_Tbl
                    .FirstOrDefaultAsync(s => s.TaskId == settings.TaskId);

                if (existingSettings != null)
                {
                    // بروزرسانی
                    var oldSettings = existingSettings.Clone();

                    existingSettings.CanCommentRoles = settings.CanCommentRoles;
                    existingSettings.CanAddMembersRoles = settings.CanAddMembersRoles;
                    existingSettings.CanRemoveMembersRoles = settings.CanRemoveMembersRoles;
                    existingSettings.CanEditAfterCompletionRoles = settings.CanEditAfterCompletionRoles;
                    existingSettings.CreatorCanEditDelete = settings.CreatorCanEditDelete;
                    existingSettings.IsInherited = settings.IsInherited;
                    existingSettings.InheritedFrom = settings.InheritedFrom;
                    existingSettings.LastModifiedByUserId = userId;
                    existingSettings.LastModifiedDate = DateTime.Now;

                    _context.TaskSettings_Tbl.Update(existingSettings);

                    // ثبت لاگ تغییرات
                    await LogAllChangesAsync(settings.TaskId, oldSettings, existingSettings, userId);
                }
                else
                {
                    // ایجاد جدید
                    settings.CreatedByUserId = userId;
                    settings.CreatedDate = DateTime.Now;

                    await _context.TaskSettings_Tbl.AddAsync(settings);
                }

                await _context.SaveChangesAsync();
                return settings;
            }

            /// <summary>
            /// حذف تنظیمات سفارشی تسک (بازگشت به وراثت)
            /// </summary>
            public async Task<bool> ResetTaskSettingsAsync(int taskId)
            {
                var settings = await _context.TaskSettings_Tbl
                    .FirstOrDefaultAsync(s => s.TaskId == taskId);

                if (settings == null)
                    return false;

                _context.TaskSettings_Tbl.Remove(settings);
                await _context.SaveChangesAsync();

                return true;
            }

            /// <summary>
            /// بررسی وجود تنظیمات سفارشی
            /// </summary>
            public async Task<bool> HasCustomSettingsAsync(int taskId)
            {
                return await _context.TaskSettings_Tbl
                    .AnyAsync(s => s.TaskId == taskId && !s.IsInherited);
            }

            #endregion

            #region Branch Default Settings

            /// <summary>
            /// دریافت تنظیمات پیش‌فرض شعبه
            /// </summary>
            public async Task<BranchDefaultTaskSettings?> GetBranchDefaultSettingsAsync(int branchId)
            {
                return await _context.BranchDefaultTaskSettings_Tbl
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.BranchId == branchId);
            }

            /// <summary>
            /// ذخیره تنظیمات پیش‌فرض شعبه
            /// </summary>
            public async Task<BranchDefaultTaskSettings> SaveBranchDefaultSettingsAsync(
                BranchDefaultTaskSettings settings,
                string userId)
            {
                var existing = await _context.BranchDefaultTaskSettings_Tbl
                    .FirstOrDefaultAsync(s => s.BranchId == settings.BranchId);

                if (existing != null)
                {
                    existing.CanCommentRoles = settings.CanCommentRoles;
                    existing.CanAddMembersRoles = settings.CanAddMembersRoles;
                    existing.CanRemoveMembersRoles = settings.CanRemoveMembersRoles;
                    existing.CanEditAfterCompletionRoles = settings.CanEditAfterCompletionRoles;
                    existing.CreatorCanEditDelete = settings.CreatorCanEditDelete;
                    existing.LastModifiedByUserId = userId;
                    existing.LastModifiedDate = DateTime.Now;

                    _context.BranchDefaultTaskSettings_Tbl.Update(existing);
                }
                else
                {
                    settings.CreatedByUserId = userId;
                    settings.CreatedDate = DateTime.Now;
                    await _context.BranchDefaultTaskSettings_Tbl.AddAsync(settings);
                }

                await _context.SaveChangesAsync();
                return settings;
            }

            /// <summary>
            /// بررسی وجود تنظیمات شعبه
            /// </summary>
            public async Task<bool> BranchHasDefaultSettingsAsync(int branchId)
            {
                return await _context.BranchDefaultTaskSettings_Tbl
                    .AnyAsync(s => s.BranchId == branchId);
            }

            #endregion

            #region Category Default Settings

            /// <summary>
            /// دریافت تنظیمات پیش‌فرض دسته‌بندی
            /// </summary>
            public async Task<TaskCategoryDefaultSettings?> GetCategoryDefaultSettingsAsync(int categoryId)
            {
                return await _context.TaskCategoryDefaultSettings_Tbl
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.TaskCategoryId == categoryId);
            }

            /// <summary>
            /// ذخیره تنظیمات پیش‌فرض دسته‌بندی
            /// </summary>
            public async Task<TaskCategoryDefaultSettings> SaveCategoryDefaultSettingsAsync(
                TaskCategoryDefaultSettings settings,
                string userId)
            {
                var existing = await _context.TaskCategoryDefaultSettings_Tbl
                    .FirstOrDefaultAsync(s => s.TaskCategoryId == settings.TaskCategoryId);

                if (existing != null)
                {
                    existing.CanCommentRoles = settings.CanCommentRoles;
                    existing.CanAddMembersRoles = settings.CanAddMembersRoles;
                    existing.CanRemoveMembersRoles = settings.CanRemoveMembersRoles;
                    existing.CanEditAfterCompletionRoles = settings.CanEditAfterCompletionRoles;
                    existing.CreatorCanEditDelete = settings.CreatorCanEditDelete;
                    existing.LastModifiedByUserId = userId;
                    existing.LastModifiedDate = DateTime.Now;

                    _context.TaskCategoryDefaultSettings_Tbl.Update(existing);
                }
                else
                {
                    settings.CreatedByUserId = userId;
                    settings.CreatedDate = DateTime.Now;
                    await _context.TaskCategoryDefaultSettings_Tbl.AddAsync(settings);
                }

                await _context.SaveChangesAsync();
                return settings;
            }

            /// <summary>
            /// بررسی وجود تنظیمات دسته‌بندی
            /// </summary>
            public async Task<bool> CategoryHasDefaultSettingsAsync(int categoryId)
            {
                return await _context.TaskCategoryDefaultSettings_Tbl
                    .AnyAsync(s => s.TaskCategoryId == categoryId);
            }

            #endregion

            #region Helper Methods

            /// <summary>
            /// تنظیمات پیش‌فرض سیستم (Global)
            /// </summary>
            public TaskSettings GetGlobalDefaultSettings()
            {
                return new TaskSettings
                {
                    TaskId = 0,
                    CanEditSettingsRoles = "a,b", // ⭐⭐⭐ پیش‌فرض: مدیر و سازنده
                    CanCommentRoles = "a,b,c,d,e",
                    CanAddMembersRoles = "a,b",
                    CanRemoveMembersRoles = "a,b",
                    CanEditAfterCompletionRoles = "a,b",
                    CreatorCanEditDelete = false,
                    IsInherited = true,
                    InheritedFrom = 0, // Global
                    CreatedByUserId = "system",
                    CreatedDate = DateTime.Now
                };
            }

            /// <summary>
            /// ثبت تمام تغییرات در لاگ
            /// </summary>
            private async Task LogAllChangesAsync(
                int taskId,
                TaskSettings oldSettings,
                TaskSettings newSettings,
                string userId)
            {
                var changes = new List<TaskSettingsChangeLog>();

                // تنظیم 1: کامنت
                if (oldSettings.CanCommentRoles != newSettings.CanCommentRoles)
                {
                    changes.Add(new TaskSettingsChangeLog
                    {
                        TaskId = taskId,
                        SettingType = 1,
                        OldValue = oldSettings.CanCommentRoles,
                        NewValue = newSettings.CanCommentRoles,
                        ChangedByUserId = userId,
                        ChangeDate = DateTime.Now
                    });
                }

                // تنظیم 2: افزودن عضو
                if (oldSettings.CanAddMembersRoles != newSettings.CanAddMembersRoles)
                {
                    changes.Add(new TaskSettingsChangeLog
                    {
                        TaskId = taskId,
                        SettingType = 2,
                        OldValue = oldSettings.CanAddMembersRoles,
                        NewValue = newSettings.CanAddMembersRoles,
                        ChangedByUserId = userId,
                        ChangeDate = DateTime.Now
                    });
                }

                // تنظیم 3: حذف عضو
                if (oldSettings.CanRemoveMembersRoles != newSettings.CanRemoveMembersRoles)
                {
                    changes.Add(new TaskSettingsChangeLog
                    {
                        TaskId = taskId,
                        SettingType = 3,
                        OldValue = oldSettings.CanRemoveMembersRoles,
                        NewValue = newSettings.CanRemoveMembersRoles,
                        ChangedByUserId = userId,
                        ChangeDate = DateTime.Now
                    });
                }

                // تنظیم 4: ویرایش پس از اتمام
                if (oldSettings.CanEditAfterCompletionRoles != newSettings.CanEditAfterCompletionRoles)
                {
                    changes.Add(new TaskSettingsChangeLog
                    {
                        TaskId = taskId,
                        SettingType = 4,
                        OldValue = oldSettings.CanEditAfterCompletionRoles,
                        NewValue = newSettings.CanEditAfterCompletionRoles,
                        ChangedByUserId = userId,
                        ChangeDate = DateTime.Now
                    });
                }

                // تنظیم 5: حذف/ویرایش سازنده
                if (oldSettings.CreatorCanEditDelete != newSettings.CreatorCanEditDelete)
                {
                    changes.Add(new TaskSettingsChangeLog
                    {
                        TaskId = taskId,
                        SettingType = 5,
                        OldValue = oldSettings.CreatorCanEditDelete.ToString(),
                        NewValue = newSettings.CreatorCanEditDelete.ToString(),
                        ChangedByUserId = userId,
                        ChangeDate = DateTime.Now
                    });
                }

                if (changes.Any())
                {
                    await _context.TaskSettingsChangeLog_Tbl.AddRangeAsync(changes);
                }
            }

            #endregion
        

        

        #region Bulk Operations

        /// <summary>
        /// اعمال تنظیمات شعبه به تمام تسک‌های آن شعبه
        /// </summary>
        public async Task<int> ApplyBranchSettingsToAllTasksAsync(int branchId, string userId)
        {
            var branchSettings = await GetBranchDefaultSettingsAsync(branchId);
            if (branchSettings == null)
                return 0;

            var tasks = await _context.Tasks_Tbl
                .Where(t => t.BranchId == branchId && !t.IsDeleted)
                .ToListAsync();

            int updatedCount = 0;

            foreach (var task in tasks)
            {
                var taskSettings = new TaskSettings
                {
                    TaskId = task.Id,
                    CanCommentRoles = branchSettings.CanCommentRoles,
                    CanAddMembersRoles = branchSettings.CanAddMembersRoles,
                    CanRemoveMembersRoles = branchSettings.CanRemoveMembersRoles,
                    CanEditAfterCompletionRoles = branchSettings.CanEditAfterCompletionRoles,
                    CreatorCanEditDelete = branchSettings.CreatorCanEditDelete,
                    IsInherited = false,
                    InheritedFrom = 2 // Branch
                };

                await SaveTaskSettingsAsync(taskSettings, userId);
                updatedCount++;
            }

            return updatedCount;
        }

        /// <summary>
        /// اعمال تنظیمات دسته‌بندی به تمام تسک‌های آن دسته
        /// </summary>
        public async Task<int> ApplyCategorySettingsToAllTasksAsync(int categoryId, string userId)
        {
            var categorySettings = await GetCategoryDefaultSettingsAsync(categoryId);
            if (categorySettings == null)
                return 0;

            var tasks = await _context.Tasks_Tbl
                .Where(t => t.TaskCategoryId == categoryId && !t.IsDeleted)
                .ToListAsync();

            int updatedCount = 0;

            foreach (var task in tasks)
            {
                var taskSettings = new TaskSettings
                {
                    TaskId = task.Id,
                    CanCommentRoles = categorySettings.CanCommentRoles,
                    CanAddMembersRoles = categorySettings.CanAddMembersRoles,
                    CanRemoveMembersRoles = categorySettings.CanRemoveMembersRoles,
                    CanEditAfterCompletionRoles = categorySettings.CanEditAfterCompletionRoles,
                    CreatorCanEditDelete = categorySettings.CreatorCanEditDelete,
                    IsInherited = false,
                    InheritedFrom = 1 // Category
                };

                await SaveTaskSettingsAsync(taskSettings, userId);
                updatedCount++;
            }

            return updatedCount;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت آمار استفاده از تنظیمات
        /// </summary>
        public async Task<TaskSettingsStatisticsViewModel> GetSettingsStatisticsAsync(int? branchId = null, int? categoryId = null)
        {
            var query = _context.Tasks_Tbl.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.TaskCategoryId == categoryId.Value);

            var totalTasks = await query.CountAsync();
            
            var tasksWithCustomSettings = await _context.TaskSettings_Tbl
                .Where(s => !s.IsInherited)
                .Select(s => s.TaskId)
                .ToListAsync();

            var customSettingsCount = await query
                .CountAsync(t => tasksWithCustomSettings.Contains(t.Id));

            var inheritedCount = totalTasks - customSettingsCount;

            return new TaskSettingsStatisticsViewModel
            {
                TotalTasks = totalTasks,
                TasksWithCustomSettings = customSettingsCount,
                TasksWithInheritedSettings = inheritedCount,
                CustomSettingsPercentage = totalTasks > 0 
                    ? (customSettingsCount * 100.0 / totalTasks) 
                    : 0
            };
        }

        #endregion

        #region Mapping Methods

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// ⭐⭐⭐ نکته: settings.TaskId ممکن است 0 باشد (برای Global Settings)
        /// بنابراین taskId اصلی را جداگانه می‌گیریم
        /// </summary>
        public async Task<TaskSettingsViewModel> MapEntityToViewModelAsync(TaskSettings settings, string currentUserId, int? originalTaskId = null)
        {
            // ⭐⭐⭐ استفاده از taskId اصلی (اگر داده شده) یا settings.TaskId
            int taskId = originalTaskId ?? settings.TaskId;
            
            var task = await _context.Tasks_Tbl
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);

            var viewModel = new TaskSettingsViewModel
            {
                TaskId = taskId,
                TaskTitle = task?.Title ?? "نامشخص",
                TaskCode = task?.TaskCode ?? "",
                IsInherited = settings.IsInherited,
                InheritedFromText = GetInheritedFromText(settings.InheritedFrom)
            };

            // ⭐⭐⭐ تنظیم 0: تغییر تنظیمات (فقط مدیر می‌بیند)
            viewModel.EditSettingsSetting = CreateSettingItem(
                0,
                "دسترسی به تنظیمات",
                "چه کسانی می‌توانند تنظیمات تسک را تغییر دهند",
                settings.CanEditSettingsRoles);

            // تنظیم 1: کامنت
            viewModel.CommentSetting = CreateSettingItem(
                1,
                "کامنت‌گذاری",
                "چه کسانی می‌توانند در تسک کامنت بگذارند",
                settings.CanCommentRoles);

            // تنظیم 2: افزودن عضو
            viewModel.AddMembersSetting = CreateSettingItem(
                2,
                "افزودن عضو جدید",
                "چه کسانی می‌توانند افراد جدید را به تسک اضافه کنند",
                settings.CanAddMembersRoles);

            // تنظیم 3: حذف عضو
            viewModel.RemoveMembersSetting = CreateSettingItem(
                3,
                "حذف عضو",
                "چه کسانی می‌توانند اعضا را از تسک حذف کنند",
                settings.CanRemoveMembersRoles);

            // تنظیم 4: ویرایش پس از اتمام
            viewModel.EditAfterCompletionSetting = CreateSettingItem(
                4,
                "ویرایش پس از تکمیل",
                "پس از تکمیل تسک، چه کسانی می‌توانند آن را ویرایش کنند",
                settings.CanEditAfterCompletionRoles);

            // تنظیم 5: حذف/ویرایش سازنده
            viewModel.CreatorEditDeleteSetting = new CreatorEditDeleteSettingViewModel
            {
                SettingId = 5,
                Title = "مجوز حذف/ویرایش برای سازنده",
                Description = "آیا سازنده تسک می‌تواند آن را حذف یا ویرایش کند؟",
                IsEnabled = settings.CreatorCanEditDelete,
                IsReadOnly = false
            };

            // ⭐⭐⭐ بررسی Admin سیستم
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user?.IsAdmin == true)
            {
                viewModel.CurrentUserRole = TaskRole.Manager;
                viewModel.CurrentUserRoleText = "مدیر سیستم";
                viewModel.CanEdit = true;
            }
            else
            {
                // ⭐⭐⭐ بررسی نقش کاربر در تسک - با taskId اصلی
                var userRole = await GetUserRoleInTaskAsync(taskId, currentUserId);
                viewModel.CurrentUserRole = userRole ?? TaskRole.CarbonCopy;
                viewModel.CurrentUserRoleText = GetRoleText(userRole);
                viewModel.CanEdit = await CanUserEditSettingsAsync(taskId, currentUserId);
            }

            return viewModel;
        }

        /// <summary>
        /// دریافت متن نقش کاربر
        /// </summary>
        private string GetRoleText(TaskRole? role)
        {
            return role switch
            {
                TaskRole.Manager => "مدیر تیم",
                TaskRole.Creator => "سازنده تسک",
                TaskRole.Supervisor => "سرپرست",
                TaskRole.Member => "عضو تسک",
                TaskRole.CarbonCopy => "ناظر (رونوشت)",
                _ => "بدون نقش"
            };
        }

        /// <summary>
        /// تبدیل ViewModel به Entity
        /// </summary>
        public TaskSettings MapViewModelToEntity(TaskSettingsViewModel viewModel, int taskId, string userId)
        {
            return new TaskSettings
            {
                TaskId = taskId,
                CanCommentRoles = NormalizeRoles(string.Join(",", viewModel.CommentSetting.SelectedRoles)),
                CanAddMembersRoles = NormalizeRoles(string.Join(",", viewModel.AddMembersSetting.SelectedRoles)),
                CanRemoveMembersRoles = NormalizeRoles(string.Join(",", viewModel.RemoveMembersSetting.SelectedRoles)),
                CanEditAfterCompletionRoles = NormalizeRoles(string.Join(",", viewModel.EditAfterCompletionSetting.SelectedRoles)),
                CreatorCanEditDelete = viewModel.CreatorEditDeleteSetting.IsEnabled,
                IsInherited = false,
                InheritedFrom = 3, // Task-specific
                CreatedByUserId = userId,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد آیتم تنظیم
        /// </summary>
        private SettingItemViewModel CreateSettingItem(
            int settingId,
            string title,
            string description,
            string selectedRoles)
        {
            var allRoles = new List<RoleCheckboxItem>
            {
                new() { RoleCode = "a", RoleText = "مدیر", AuthorityLevel = 1, IsChecked = selectedRoles.Contains("a"), IsDisabled = false },
                new() { RoleCode = "b", RoleText = "سازنده", AuthorityLevel = 2, IsChecked = selectedRoles.Contains("b"), IsDisabled = false },
                new() { RoleCode = "c", RoleText = "عضو", AuthorityLevel = 3, IsChecked = selectedRoles.Contains("c"), IsDisabled = false },
                new() { RoleCode = "d", RoleText = "ناظر", AuthorityLevel = 4, IsChecked = selectedRoles.Contains("d"), IsDisabled = false },
                new() { RoleCode = "e", RoleText = "رونوشت", AuthorityLevel = 5, IsChecked = selectedRoles.Contains("e"), IsDisabled = false }
            };

            return new SettingItemViewModel
            {
                SettingId = settingId,
                Title = title,
                Description = description,
                AvailableRoles = allRoles,
                IsReadOnly = false,
                SelectedRoles = selectedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            };
        }

        /// <summary>
        /// Normalize کردن نقش‌ها
        /// </summary>
        private string NormalizeRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return string.Empty;

            var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            return string.Join(",", roleList);
        }

        /// <summary>
        /// دریافت متن منبع وراثت
        /// </summary>
        private string GetInheritedFromText(int? inheritedFrom)
        {
            return inheritedFrom switch
            {
                0 => "تنظیمات سیستم",
                1 => "تنظیمات دسته‌بندی",
                2 => "تنظیمات شعبه",
                3 => "تنظیمات سفارشی",
                _ => "نامشخص"
            };
        }

        #endregion
    }
    /// <summary>
    /// Extension Methods برای Clone کردن
    /// </summary>
    internal static class TaskSettingsExtensions
    {
        public static TaskSettings Clone(this TaskSettings source)
        {
            return new TaskSettings
            {
                TaskId = source.TaskId,
                CanCommentRoles = source.CanCommentRoles,
                CanAddMembersRoles = source.CanAddMembersRoles,
                CanRemoveMembersRoles = source.CanRemoveMembersRoles,
                CanEditAfterCompletionRoles = source.CanEditAfterCompletionRoles,
                CreatorCanEditDelete = source.CreatorCanEditDelete,
                IsInherited = source.IsInherited,
                InheritedFrom = source.InheritedFrom
            };
        }
    }

}
