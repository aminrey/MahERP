using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Notifications
{
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly AppDbContext _context;

        public NotificationTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== Template CRUD ====================

        public async Task<List<NotificationTemplate>> GetAllTemplatesAsync(int? notificationTypeId = null, byte? channelType = null)
        {
            var query = _context.NotificationTemplate_Tbl
                .Include(t => t.CreatedBy)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.User)
                .AsQueryable();

            // ✅ فیلتر بر اساس NotificationEventType (نه NotificationTypeConfigId)
            if (notificationTypeId.HasValue)
            {
                query = query.Where(t => t.NotificationEventType == (byte)notificationTypeId.Value);
            }

            // ✅ فیلتر بر اساس Channel (نه ChannelType)
            if (channelType.HasValue)
            {
                query = query.Where(t => t.Channel == channelType.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<NotificationTemplate> GetTemplateByIdAsync(int templateId)
        {
            return await _context.NotificationTemplate_Tbl
                .Include(t => t.CreatedBy)
                .Include(t => t.LastModifiedBy)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.User)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Contact)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.Organization)
                .Include(t => t.History)
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }

        public async Task<NotificationTemplate> GetTemplateByCodeAsync(string templateCode)
        {
            if (string.IsNullOrEmpty(templateCode))
                return null;

            return await _context.NotificationTemplate_Tbl
                .FirstOrDefaultAsync(t => t.TemplateCode == templateCode);
        }

        public async Task<int> CreateTemplateAsync(NotificationTemplate template, string userId)
        {
            template.CreatedByUserId = userId;
            template.CreatedDate = DateTime.Now;
            template.Version = 1;
            template.UsageCount = 0;

            _context.NotificationTemplate_Tbl.Add(template);
            await _context.SaveChangesAsync();

            // ⭐ ذخیره در تاریخچه
            await SaveTemplateHistoryAsync(template, userId, "ایجاد الگوی جدید");

            return template.Id;
        }

        public async Task<bool> UpdateTemplateAsync(NotificationTemplate template, string userId, string changeNote = null)
        {
            try
            {
                var existing = await GetTemplateByIdAsync(template.Id);

                if (existing == null || existing.IsSystemTemplate)
                    return false;

                // افزایش نسخه
                existing.Version++;

                // ✅ بروزرسانی فیلدهای جدید
                existing.TemplateName = template.TemplateName;
                existing.TemplateCode = template.TemplateCode;
                existing.Description = template.Description;
                existing.NotificationEventType = template.NotificationEventType; // ✅ جدید
                existing.Channel = template.Channel; // ✅ جدید
                existing.Subject = template.Subject;
                existing.MessageTemplate = template.MessageTemplate; // ✅ جدید
                existing.BodyHtml = template.BodyHtml;
                existing.RecipientMode = template.RecipientMode;
                existing.IsActive = template.IsActive;
                existing.LastModifiedDate = DateTime.Now;
                existing.LastModifiedByUserId = userId;

                _context.NotificationTemplate_Tbl.Update(existing);
                await _context.SaveChangesAsync();

                // ⭐ ذخیره در تاریخچه
                await SaveTemplateHistoryAsync(existing, userId, changeNote ?? "ویرایش الگو");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _context.NotificationTemplate_Tbl
                    .Include(t => t.Recipients)
                    .FirstOrDefaultAsync(t => t.Id == templateId);

                if (template == null || template.IsSystemTemplate)
                    return false;

                // حذف دریافت‌کنندگان
                _context.NotificationTemplateRecipient_Tbl.RemoveRange(template.Recipients);

                // حذف الگو
                _context.NotificationTemplate_Tbl.Remove(template);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleTemplateStatusAsync(int templateId, bool isActive)
        {
            try
            {
                var template = await _context.NotificationTemplate_Tbl
                    .FindAsync(templateId);

                if (template == null)
                    return false;

                template.IsActive = isActive;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== Recipients Management ====================

        /// <summary>
        /// افزودن دریافت‌کننده با پشتیبانی از RecipientType
        /// </summary>
        public async Task<bool> AddRecipientAsync(
            int templateId,
            byte recipientType,
            int? contactId,
            int? organizationId,
            string userId,
            string currentUserId)
        {
            try
            {
                // بررسی تکراری
                var exists = await _context.NotificationTemplateRecipient_Tbl
                    .AnyAsync(r => r.NotificationTemplateId == templateId &&
                                  r.RecipientType == recipientType &&
                                  (recipientType == 0 ? r.ContactId == contactId :
                                   recipientType == 1 ? r.OrganizationId == organizationId :
                                   r.UserId == userId) &&
                                  r.IsActive);

                if (exists)
                    return false;

                var recipient = new NotificationTemplateRecipient
                {
                    NotificationTemplateId = templateId,
                    RecipientType = recipientType,
                    ContactId = contactId,
                    OrganizationId = organizationId,
                    UserId = userId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = currentUserId
                };

                _context.NotificationTemplateRecipient_Tbl.Add(recipient);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveRecipientAsync(int recipientId)
        {
            try
            {
                var recipient = await _context.NotificationTemplateRecipient_Tbl
                    .FindAsync(recipientId);

                if (recipient == null)
                    return false;

                // Soft delete
                recipient.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateRecipientsAsync(int templateId, List<string> userIds, string currentUserId)
        {
            try
            {
                // حذف دریافت‌کنندگان فعلی
                var existingRecipients = await _context.NotificationTemplateRecipient_Tbl
                    .Where(r => r.NotificationTemplateId == templateId)
                    .ToListAsync();

                _context.NotificationTemplateRecipient_Tbl.RemoveRange(existingRecipients);

                // اضافه کردن دریافت‌کنندگان جدید
                if (userIds != null && userIds.Any())
                {
                    foreach (var userId in userIds)
                    {
                        var recipient = new NotificationTemplateRecipient
                        {
                            NotificationTemplateId = templateId,
                            RecipientType = 2, // User
                            UserId = userId,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedByUserId = currentUserId
                        };

                        _context.NotificationTemplateRecipient_Tbl.Add(recipient);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== Template Variables ====================

        public async Task<List<NotificationTemplateVariable>> GetTemplateVariablesAsync(int templateId)
        {
            return await _context.NotificationTemplateVariable_Tbl
                .Where(v => v.TemplateId == templateId)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<SystemVariableViewModel>> GetSystemVariablesAsync()
        {
            // متغیرهای سیستمی عمومی
            return new List<SystemVariableViewModel>
            {
                // ⭐ متغیرهای پایه
                new() { VariableName = "Title", DisplayName = "عنوان", Description = "عنوان اعلان" },
                new() { VariableName = "Message", DisplayName = "پیام", Description = "متن پیام" },
                new() { VariableName = "ActionUrl", DisplayName = "لینک", Description = "لینک عملیات" },
                new() { VariableName = "Date", DisplayName = "تاریخ", Description = "تاریخ جاری" },
                new() { VariableName = "Time", DisplayName = "ساعت", Description = "ساعت جاری" },
                
                // ⭐ اطلاعات کاربر دریافت‌کننده
                new() { VariableName = "RecipientFullName", DisplayName = "نام کامل گیرنده", Description = "نام و نام خانوادگی گیرنده" },
                new() { VariableName = "RecipientFirstName", DisplayName = "نام گیرنده", Description = "نام کوچک گیرنده" },
                new() { VariableName = "RecipientLastName", DisplayName = "نام خانوادگی گیرنده", Description = "نام خانوادگی گیرنده" },
                new() { VariableName = "RecipientUserName", DisplayName = "نام کاربری", Description = "نام کاربری سیستم" },
                new() { VariableName = "RecipientEmail", DisplayName = "ایمیل گیرنده", Description = "آدرس ایمیل گیرنده" },
                new() { VariableName = "RecipientPhone", DisplayName = "تلفن گیرنده", Description = "شماره موبایل گیرنده" },
                
                // ⭐ اطلاعات تسک
                new() { VariableName = "TaskTitle", DisplayName = "عنوان تسک", Description = "عنوان تسک" },
                new() { VariableName = "TaskCode", DisplayName = "کد تسک", Description = "کد یکتای تسک" },
                new() { VariableName = "TaskDescription", DisplayName = "توضیحات تسک", Description = "توضیحات تسک" },
                new() { VariableName = "TaskStartDate", DisplayName = "تاریخ شروع", Description = "تاریخ شروع تسک" },
                new() { VariableName = "TaskDueDate", DisplayName = "مهلت انجام", Description = "تاریخ سررسید تسک" },
                new() { VariableName = "TaskPriority", DisplayName = "اولویت", Description = "اولویت تسک (عادی/متوسط/بالا/فوری)" },
                new() { VariableName = "TaskCategory", DisplayName = "دسته‌بندی", Description = "دسته‌بندی تسک" },
                new() { VariableName = "TaskStakeholder", DisplayName = "طرف حساب", Description = "شخص یا سازمان مرتبط" },
                new() { VariableName = "TaskBranch", DisplayName = "شعبه", Description = "شعبه مربوط به تسک" },
                new() { VariableName = "TaskCreatorName", DisplayName = "سازنده تسک", Description = "نام سازنده تسک" },
                new() { VariableName = "SenderName", DisplayName = "ارسال‌کننده", Description = "نام ارسال‌کننده اعلان" },
                
                // ⭐⭐⭐ NEW: لیست تسک‌های انجام نشده
                new() { 
                    VariableName = "PendingTasks", 
                    DisplayName = "لیست تسک‌های انجام نشده", 
                    Description = "لیست کامل تسک‌های در حال انجام کاربر با جزئیات (عنوان، توضیح، تاریخ، اولویت، پیشرفت)" 
                },
                
                // ⭐ متغیرهای قدیمی (حفظ برای سازگاری)
                new() { VariableName = "UserName", DisplayName = "[قدیمی] نام کاربر", Description = "استفاده از RecipientFullName پیشنهاد می‌شود" },
                new() { VariableName = "FirstName", DisplayName = "[قدیمی] نام", Description = "استفاده از RecipientFirstName پیشنهاد می‌شود" },
                new() { VariableName = "LastName", DisplayName = "[قدیمی] نام خانوادگی", Description = "استفاده از RecipientLastName پیشنهاد می‌شود" },
                new() { VariableName = "Email", DisplayName = "[قدیمی] ایمیل", Description = "استفاده از RecipientEmail پیشنهاد می‌شود" },
                new() { VariableName = "PhoneNumber", DisplayName = "[قدیمی] شماره تماس", Description = "استفاده از RecipientPhone پیشنهاد می‌شود" },
                new() { VariableName = "SystemName", DisplayName = "نام سیستم", Description = "نام سیستم ERP" },
                new() { VariableName = "CompanyName", DisplayName = "نام شرکت", Description = "نام شرکت" },
                new() { VariableName = "BranchName", DisplayName = "نام شعبه", Description = "نام شعبه فعلی" },
                new() { VariableName = "DueDate", DisplayName = "[قدیمی] مهلت", Description = "استفاده از TaskDueDate پیشنهاد می‌شود" }
            };
        }

        public async Task<bool> AddVariableToTemplateAsync(NotificationTemplateVariable variable)
        {
            try
            {
                _context.NotificationTemplateVariable_Tbl.Add(variable);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveVariableFromTemplateAsync(int variableId)
        {
            try
            {
                var variable = await _context.NotificationTemplateVariable_Tbl
                    .FindAsync(variableId);

                if (variable == null)
                    return false;

                _context.NotificationTemplateVariable_Tbl.Remove(variable);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== Template History ====================

        public async Task<List<NotificationTemplateHistory>> GetTemplateHistoryAsync(int templateId)
        {
            return await _context.NotificationTemplateHistory_Tbl
                .Include(h => h.ChangedBy)
                .Where(h => h.TemplateId == templateId)
                .OrderByDescending(h => h.ChangeDate)
                .ToListAsync();
        }

        public async Task<bool> RestoreTemplateVersionAsync(int templateId, int version, string userId)
        {
            try
            {
                var history = await _context.NotificationTemplateHistory_Tbl
                    .FirstOrDefaultAsync(h => h.TemplateId == templateId && h.Version == version);

                if (history == null)
                    return false;

                var template = await GetTemplateByIdAsync(templateId);

                if (template == null || template.IsSystemTemplate)
                    return false;

                // بازگرداندن محتوا
                template.Subject = history.Subject;
                template.MessageTemplate = history.MessageTemplate; // ✅ اصلاح
                template.BodyHtml = history.BodyHtml;
                template.Version++;
                template.LastModifiedDate = DateTime.Now;
                template.LastModifiedByUserId = userId;

                _context.NotificationTemplate_Tbl.Update(template);
                await _context.SaveChangesAsync();

                // ذخیره در تاریخچه
                await SaveTemplateHistoryAsync(template, userId, $"بازگرداندن به نسخه {version}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task SaveTemplateHistoryAsync(NotificationTemplate template, string userId, string changeNote)
        {
            var history = new NotificationTemplateHistory
            {
                TemplateId = template.Id,
                Version = template.Version,
                Subject = template.Subject,
                MessageTemplate = template.MessageTemplate, // ✅ اصلاح
                BodyHtml = template.BodyHtml,
                ChangeDate = DateTime.Now,
                ChangedByUserId = userId,
                ChangeNote = changeNote
            };

            _context.NotificationTemplateHistory_Tbl.Add(history);
            await _context.SaveChangesAsync();
        }

        // ==================== Template Preview ====================

        public async Task<string> PreviewTemplateAsync(int templateId, Dictionary<string, string> sampleData = null)
        {
            var template = await GetTemplateByIdAsync(templateId);

            if (template == null)
                return string.Empty;

            // داده‌های نمونه پیش‌فرض
            if (sampleData == null)
            {
                sampleData = new Dictionary<string, string>
                {
                    { "Title", "عنوان نمونه" },
                    { "Message", "این یک پیام تستی است" },
                    { "ActionUrl", "/TaskingArea/Tasks/Details/1" },
                    { "Date", DateTime.Now.ToString("yyyy/MM/dd") },
                    { "Time", DateTime.Now.ToString("HH:mm") },
                    { "UserName", "احمد محمدی" },
                    { "FirstName", "احمد" },
                    { "LastName", "محمدی" },
                    { "Email", "ahmad@example.com" },
                    { "PhoneNumber", "09123456789" },
                    { "SystemName", "سیستم مدیریت" },
                    { "CompanyName", "شرکت نمونه" },
                    { "BranchName", "دفتر مرکزی" },
                    { "TaskTitle", "تسک نمونه" },
                    { "TaskCode", "T-001" },
                    { "DueDate", "1403/08/20" }
                };
            }

            // استفاده از BodyHtml اگر موجود باشد
            var content = !string.IsNullOrEmpty(template.BodyHtml)
                ? template.BodyHtml
                : template.MessageTemplate; // ✅ اصلاح

            return ReplaceTemplateVariables(content, sampleData);
        }

        public string ReplaceTemplateVariables(string content, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(content) || variables == null)
                return content;

            var result = content;

            foreach (var variable in variables)
            {
                // جایگزینی {VariableName}
                var pattern = $@"\{{{variable.Key}\}}";
                result = Regex.Replace(result, pattern, variable.Value, RegexOptions.IgnoreCase);
            }

            return result;
        }

        // ==================== ViewModels ====================

        public async Task<NotificationTemplateListViewModel> GetTemplateListViewModelAsync(int? notificationTypeId = null, byte? channelType = null)
        {
            var templates = await GetAllTemplatesAsync(notificationTypeId, channelType);

            // ⭐⭐⭐ دریافت نام فارسی از NotificationTypeConfig
            var typeConfigs = await _context.NotificationTypeConfig_Tbl
                .Include(t => t.ModuleConfig) // ⭐ Include ModuleName
                .Where(t => t.IsActive)
                .ToListAsync();

            return new NotificationTemplateListViewModel
            {
                Templates = templates.Select(t =>
                {
                    // ⭐ پیدا کردن TypeConfig که این EventType را پوشش می‌دهد
                    var typeConfig = typeConfigs.FirstOrDefault(tc =>
                        tc.EventTypesList.Contains(t.NotificationEventType));

                    return new NotificationTemplateItemViewModel
                    {
                        Id = t.Id,
                        TemplateCode = t.TemplateCode,
                        TemplateName = t.TemplateName,
                        // ⭐⭐⭐ استفاده از نام فارسی از Config
                        NotificationTypeName = typeConfig?.TypeNameFa ?? t.EventTypeName,
                        ModuleName = typeConfig?.ModuleConfig?.ModuleNameFa ?? "تسکینگ",
                        Channel = t.Channel,
                        ChannelTypeName = t.ChannelName,
                        RecipientMode = t.RecipientMode,
                        RecipientCount = t.Recipients.Count(r => r.IsActive),
                        Description = t.Description,
                        IsSystemTemplate = t.IsSystemTemplate,
                        IsActive = t.IsActive,
                        Version = t.Version,
                        UsageCount = t.UsageCount,
                        LastUsedDate = t.LastUsedDate,
                        CreatedDate = t.CreatedDate,
                        CreatorName = t.CreatedBy != null
                            ? $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}"
                            : "سیستم"
                    };
                }).ToList()
            };
        }
        // در کلاس NotificationTemplateRepository

        // ⭐ متد جدید برای دریافت لیست انواع اعلان
        private async Task<List<NotificationTypeSelectItem>> GetAvailableNotificationTypesAsync()
        {
            var configs = await _context.NotificationTypeConfig_Tbl
                .Where(t => t.IsActive)
                .OrderBy(t => t.ModuleConfigId)
                .ThenBy(t => t.TypeNameFa).Include(t=> t.ModuleConfig)
                .ToListAsync();

            return configs.Select(t => new NotificationTypeSelectItem
            {
                // ⭐⭐⭐ استفاده از اولین EventType به جای Id
                Id = t.EventTypesList.Any() ? t.EventTypesList.First() : 0,
                Name = t.TypeNameFa,
                ModuleName = t.ModuleConfig.ModuleNameFa,
                // ⭐⭐⭐ فقط اعلان‌های دوره‌ای قابل زمان‌بندی هستند
                IsSchedulable = t.EventTypesList.Contains(13) // DailyTaskDigest
            }).ToList();
        }

        // ⭐ اصلاح متد GetTemplateFormViewModelAsync
        public async Task<NotificationTemplateFormViewModel> GetTemplateFormViewModelAsync(int? templateId = null)
        {
            if (templateId.HasValue)
            {
                var template = await GetTemplateByIdAsync(templateId.Value);

                if (template == null)
                    return null;

                var selectedUserIds = template.Recipients
                    .Where(r => r.IsActive && r.RecipientType == 2)
                    .Select(r => r.UserId)
                    .ToList();

                return new NotificationTemplateFormViewModel
                {
                    Id = template.Id,
                    NotificationEventType = template.NotificationEventType,
                    Channel = template.Channel,
                    TemplateCode = template.TemplateCode,
                    TemplateName = template.TemplateName,
                    Description = template.Description,
                    Subject = template.Subject,
                    MessageTemplate = template.MessageTemplate,
                    BodyHtml = template.BodyHtml,
                    RecipientMode = template.RecipientMode,
                    SelectedUserIds = selectedUserIds,
                    IsActive = template.IsActive,
                    
                    // ⭐⭐⭐ فیلدهای زمان‌بندی
                    IsScheduled = template.IsScheduled,
                    ScheduleType = template.ScheduleType,
                    ScheduledTime = template.ScheduledTime,
                    ScheduledDaysOfWeek = template.ScheduledDaysOfWeek,
                    ScheduledDayOfMonth = template.ScheduledDayOfMonth,
                    LastExecutionDate = template.LastExecutionDate,
                    NextExecutionDate = template.NextExecutionDate,
                    
                    SystemVariables = await GetSystemVariablesAsync(),
                    AvailableUsers = await GetUsersForSelectAsync(),
                    AvailableNotificationTypes = await GetAvailableNotificationTypesAsync() // ✅ اضافه شد
                };
            }

            // الگوی جدید
            return new NotificationTemplateFormViewModel
            {
                IsActive = true,
                RecipientMode = 0,
                SystemVariables = await GetSystemVariablesAsync(),
                AvailableUsers = await GetUsersForSelectAsync(),
                AvailableNotificationTypes = await GetAvailableNotificationTypesAsync() // ✅ اضافه شد
            };
        }

        // ==================== Helper Methods ====================

        private async Task<List<UserSelectItem>> GetUsersForSelectAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .OrderBy(u => u.FirstName)
                .Select(u => new UserSelectItem
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email
                })
                .ToListAsync();
        }

        private string GetChannelTypeName(byte channelType)
        {
            return channelType switch
            {
                0 => "داخل سیستم",
                1 => "ایمیل",
                2 => "پیامک",
                3 => "تلگرام",
                _ => "نامشخص"
            };
        }
    }
}