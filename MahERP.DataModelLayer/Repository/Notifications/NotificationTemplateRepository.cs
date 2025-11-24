using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.ViewModels.Notifications;
using MahERP.DataModelLayer.Enums; // ⭐ اضافه شد
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

                // ⭐⭐⭐ بروزرسانی فیلدهای زمان‌بندی
                existing.IsScheduled = template.IsScheduled;
                existing.ScheduleType = template.ScheduleType;
                existing.ScheduledTime = template.ScheduledTime;
                existing.ScheduledDaysOfWeek = template.ScheduledDaysOfWeek;
                existing.ScheduledDayOfMonth = template.ScheduledDayOfMonth;
                existing.IsScheduleEnabled = template.IsScheduleEnabled;
                
                // ⭐ محاسبه زمان اجرای بعدی اگر زمان‌بندی فعال شد
                if (template.IsScheduled && !string.IsNullOrEmpty(template.ScheduledTime))
                {
                    existing.NextExecutionDate = CalculateNextExecutionDate(existing);
                }
                else
                {
                    existing.NextExecutionDate = null;
                }

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

        /// <summary>
        /// محاسبه زمان اجرای بعدی بر اساس تنظیمات زمان‌بندی
        /// </summary>
        private DateTime? CalculateNextExecutionDate(NotificationTemplate template)
        {
            if (string.IsNullOrEmpty(template.ScheduledTime))
                return null;

            var now = DateTime.Now;
            var timeParts = template.ScheduledTime.Split(':');

            if (timeParts.Length != 2 ||
                !int.TryParse(timeParts[0], out int hour) ||
                !int.TryParse(timeParts[1], out int minute))
            {
                return null;
            }

            DateTime nextExecution;

            switch (template.ScheduleType)
            {
                case 1: // روزانه
                    nextExecution = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
                    if (nextExecution <= now)
                    {
                        nextExecution = nextExecution.AddDays(1);
                    }
                    break;

                case 2: // هفتگی
                    if (string.IsNullOrEmpty(template.ScheduledDaysOfWeek))
                        return null;

                    var daysOfWeek = template.ScheduledDaysOfWeek
                        .Split(',')
                        .Select(d => int.Parse(d.Trim()))
                        .OrderBy(d => d)
                        .ToList();

                    nextExecution = FindNextWeeklyExecution(now, hour, minute, daysOfWeek);
                    break;

                case 3: // ماهانه
                    if (!template.ScheduledDayOfMonth.HasValue)
                        return null;

                    nextExecution = FindNextMonthlyExecution(now, hour, minute, template.ScheduledDayOfMonth.Value);
                    break;

                default:
                    return null;
            }

            return nextExecution;
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی هفتگی
        /// </summary>
        private DateTime FindNextWeeklyExecution(DateTime now, int hour, int minute, List<int> daysOfWeek)
        {
            var currentDayOfWeek = (int)now.DayOfWeek;

            // چک کردن امروز
            var todayExecution = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (daysOfWeek.Contains(currentDayOfWeek) && todayExecution > now)
            {
                return todayExecution;
            }

            // پیدا کردن روز بعدی
            for (int i = 1; i <= 7; i++)
            {
                var nextDate = now.AddDays(i);
                var nextDayOfWeek = (int)nextDate.DayOfWeek;

                if (daysOfWeek.Contains(nextDayOfWeek))
                {
                    return new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, hour, minute, 0);
                }
            }

            return now.AddDays(7);
        }

        /// <summary>
        /// پیدا کردن زمان بعدی برای زمان‌بندی ماهانه
        /// </summary>
        private DateTime FindNextMonthlyExecution(DateTime now, int hour, int minute, int dayOfMonth)
        {
            // چک کردن این ماه
            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var targetDay = Math.Min(dayOfMonth, daysInMonth);

            var thisMonthExecution = new DateTime(now.Year, now.Month, targetDay, hour, minute, 0);
            if (thisMonthExecution > now)
            {
                return thisMonthExecution;
            }

            // ماه بعد
            var nextMonth = now.AddMonths(1);
            daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            targetDay = Math.Min(dayOfMonth, daysInMonth);

            return new DateTime(nextMonth.Year, nextMonth.Month, targetDay, hour, minute, 0);
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
            // متغیرهای سیستمی با دسته‌بندی
            return new List<SystemVariableViewModel>
            {
                // ⭐⭐⭐ متغیرهای عمومی - برای همه انواع
                new() { 
                    VariableName = "Date", 
                    DisplayName = "تاریخ جاری", 
                    Description = "تاریخ شمسی فعلی", 
                    ExampleValue = "1403/08/15",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.General 
                    }
                },
                new() { 
                    VariableName = "Time", 
                    DisplayName = "ساعت جاری", 
                    Description = "ساعت فعلی", 
                    ExampleValue = "14:30",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.General 
                    }
                },
                
                // ⭐⭐⭐ اطلاعات کاربر دریافت‌کننده - برای همه انواع
                new() { 
                    VariableName = "RecipientFullName", 
                    DisplayName = "نام کامل گیرنده", 
                    Description = "نام و نام خانوادگی کاربر دریافت‌کننده", 
                    ExampleValue = "احمد محمدی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                new() { 
                    VariableName = "RecipientFirstName", 
                    DisplayName = "نام گیرنده", 
                    Description = "نام کوچک کاربر", 
                    ExampleValue = "احمد",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                new() { 
                    VariableName = "RecipientLastName", 
                    DisplayName = "نام خانوادگی گیرنده", 
                    Description = "نام خانوادگی کاربر", 
                    ExampleValue = "محمدی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                new() { 
                    VariableName = "RecipientUserName", 
                    DisplayName = "نام کاربری", 
                    Description = "نام کاربری سیستم", 
                    ExampleValue = "ahmad.mohammadi",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                new() { 
                    VariableName = "RecipientEmail", 
                    DisplayName = "ایمیل گیرنده", 
                    Description = "آدرس ایمیل کاربر", 
                    ExampleValue = "ahmad@example.com",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                new() { 
                    VariableName = "RecipientPhone", 
                    DisplayName = "تلفن گیرنده", 
                    Description = "شماره موبایل کاربر", 
                    ExampleValue = "09123456789",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    }
                },
                
                // ⭐⭐⭐ اطلاعات تسک خاص - فقط برای اعلان‌های مرتبط با تسک
                new() { 
                    VariableName = "TaskTitle", 
                    DisplayName = "عنوان تسک", 
                    Description = "عنوان تسک خاص", 
                    ExampleValue = "بررسی پیشنهاد مالی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskCode", 
                    DisplayName = "کد تسک", 
                    Description = "کد یکتای تسک", 
                    ExampleValue = "T-2024-001",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskDescription", 
                    DisplayName = "توضیحات تسک", 
                    Description = "توضیحات کامل تسک", 
                    ExampleValue = "بررسی دقیق پیشنهاد مالی و ارائه گزارش",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskStartDate", 
                    DisplayName = "تاریخ شروع تسک", 
                    Description = "تاریخ شروع تسک", 
                    ExampleValue = "1403/08/10",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskDueDate", 
                    DisplayName = "مهلت تسک", 
                    Description = "تاریخ سررسید تسک", 
                    ExampleValue = "1403/08/20",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskPriority", 
                    DisplayName = "اولویت تسک", 
                    Description = "سطح اولویت (عادی، متوسط، بالا، فوری)", 
                    ExampleValue = "فوری",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskCategory", 
                    DisplayName = "دسته‌بندی تسک", 
                    Description = "دسته‌بندی تسک", 
                    ExampleValue = "امور مالی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskStakeholder", 
                    DisplayName = "طرف حساب", 
                    Description = "شخص یا سازمان مرتبط با تسک", 
                    ExampleValue = "شرکت ABC",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskBranch", 
                    DisplayName = "شعبه تسک", 
                    Description = "شعبه مرتبط با تسک", 
                    ExampleValue = "دفتر مرکزی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                new() { 
                    VariableName = "TaskCreatorName", 
                    DisplayName = "سازنده تسک", 
                    Description = "نام کاربری که تسک را ایجاد کرده", 
                    ExampleValue = "علی رضایی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    }
                },
                
                // ⭐⭐⭐ اطلاعات کاربر ارسال‌کننده - فقط برای اعلان‌های تسک
                new() { 
                    VariableName = "SenderName", 
                    DisplayName = "ارسال‌کننده اعلان", 
                    Description = "نام کاربری که اعلان را ارسال کرده", 
                    ExampleValue = "محمد حسینی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Sender 
                    }
                },
                
                // ⭐⭐⭐ لیست تسک‌های انجام نشده - فقط برای اعلان‌های دوره‌ای
                new() { 
                    VariableName = "PendingTasks", 
                    DisplayName = "لیست تسک‌های انجام نشده", 
                    Description = "لیست فرمت‌شده تمام تسک‌های در حال انجام کاربر با جزئیات کامل (عنوان، توضیح، تاریخ، اولویت، پیشرفت)", 
                    ExampleValue = "📌 تسک‌های در حال انجام:\n1️⃣ بررسی پیشنهاد...",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.TaskList 
                    }
                },
                
                // ⭐⭐⭐ متغیرهای یادآوری زمان‌بندی شده
                new() { 
                    VariableName = "Title", 
                    DisplayName = "عنوان اعلان", 
                    Description = "عنوان دینامیک اعلان (از قالب یادآوری یا پیش‌فرض)", 
                    ExampleValue = "یادآوری تسک‌های امروز",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.ReminderSchedule 
                    }
                },
                new() { 
                    VariableName = "Message", 
                    DisplayName = "متن اعلان", 
                    Description = "متن دینامیک اعلان", 
                    ExampleValue = "یادآوری برای بررسی تسک‌های در حال انجام",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.ReminderSchedule 
                    }
                },
                new() { 
                    VariableName = "ActionUrl", 
                    DisplayName = "لینک عملیات", 
                    Description = "لینک مستقیم به صفحه مربوطه", 
                    ExampleValue = "/TaskingArea/Tasks/Details/123",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.ReminderSchedule 
                    }
                },
                
                // ⭐⭐⭐ متغیرهای منسوخ شده (Deprecated)
                new() { 
                    VariableName = "UserName", 
                    DisplayName = "[منسوخ] نام کاربر", 
                    Description = "❌ از RecipientFullName استفاده کنید", 
                    ExampleValue = "احمد محمدی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    },
                    IsDeprecated = true
                },
                new() { 
                    VariableName = "FirstName", 
                    DisplayName = "[منسوخ] نام", 
                    Description = "❌ از RecipientFirstName استفاده کنید", 
                    ExampleValue = "احمد",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    },
                    IsDeprecated = true
                },
                new() { 
                    VariableName = "LastName", 
                    DisplayName = "[منسوخ] نام خانوادگی", 
                    Description = "❌ از RecipientLastName استفاده کنید", 
                    ExampleValue = "محمدی",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    },
                    IsDeprecated = true
                },
                new() { 
                    VariableName = "Email", 
                    DisplayName = "[منسوخ] ایمیل", 
                    Description = "❌ از RecipientEmail استفاده کنید", 
                    ExampleValue = "ahmad@example.com",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    },
                    IsDeprecated = true
                },
                new() { 
                    VariableName = "PhoneNumber", 
                    DisplayName = "[منسوخ] شماره تماس", 
                    Description = "❌ از RecipientPhone استفاده کنید", 
                    ExampleValue = "09123456789",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Recipient 
                    },
                    IsDeprecated = true
                },
                new() { 
                    VariableName = "DueDate", 
                    DisplayName = "[منسوخ] مهلت", 
                    Description = "❌ از TaskDueDate استفاده کنید", 
                    ExampleValue = "1403/08/20",
                    Categories = new List<NotificationVariableCategory> { 
                        NotificationVariableCategory.Task 
                    },
                    IsDeprecated = true
                }
            };
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت متغیرهای مناسب برای یک نوع خاص اعلان
        /// </summary>
        public async Task<List<SystemVariableViewModel>> GetVariablesForEventTypeAsync(byte eventType)
        {
            var allVariables = await GetSystemVariablesAsync();
            var requiredCategories = GetRequiredCategoriesForEventType(eventType);

            // فیلتر متغیرها بر اساس دسته‌های مورد نیاز
            return allVariables
                .Where(v => v.Categories.Any(c => requiredCategories.Contains(c)))
                .OrderBy(v => v.IsDeprecated) // غیرمنسوخ‌ها اول
                .ThenBy(v => v.DisplayName)
                .ToList();
        }

        /// <summary>
        /// ⭐⭐⭐ تعیین دسته‌های مورد نیاز برای هر نوع اعلان
        /// </summary>
        private List<NotificationVariableCategory> GetRequiredCategoriesForEventType(byte eventType)
        {
            var categories = new List<NotificationVariableCategory>
            {
                // همیشه متغیرهای عمومی و گیرنده
                NotificationVariableCategory.General,
                NotificationVariableCategory.Recipient
            };

            // بر اساس NotificationEventType
            switch (eventType)
            {
                case 13: // DailyTaskDigest - اعلان روزانه
                    categories.Add(NotificationVariableCategory.TaskList);
                    break;

                case 1:  // TaskAssigned
                case 2:  // TaskCompleted
                case 3:  // TaskDeadlineReminder
                case 4:  // TaskCommentAdded
                case 5:  // TaskUpdated
                case 6:  // TaskOperationCompleted
                case 7:  // TaskDeleted
                case 8:  // TaskStatusChanged
                case 10: // OperationAssigned
                case 11: // TaskPriorityChanged
                case 12: // TaskReassigned
                case 14: // TaskWorkLog
                case 15: // CustomReminder
                    // اعلان‌های مرتبط با تسک خاص
                    categories.Add(NotificationVariableCategory.Task);
                    categories.Add(NotificationVariableCategory.Sender);
                    
                    // اگر یادآوری است
                    if (eventType == 3 || eventType == 15)
                    {
                        categories.Add(NotificationVariableCategory.ReminderSchedule);
                    }
                    break;

                default:
                    // پیش‌فرض: فقط عمومی و گیرنده
                    break;
            }

            return categories;
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
                            : "سیستم",
                        
                        // ⭐⭐⭐ فیلدهای زمان‌بندی
                        IsScheduled = t.IsScheduled,
                        ScheduleType = t.ScheduleType,
                        ScheduledTime = t.ScheduledTime,
                        ScheduledDaysOfWeek = t.ScheduledDaysOfWeek,
                        ScheduledDayOfMonth = t.ScheduledDayOfMonth,
                        LastExecutionDate = t.LastExecutionDate,
                        NextExecutionDate = t.NextExecutionDate
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

                // ⭐⭐⭐ Debug: لاگ مقادیر زمان‌بندی
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] Template ID: {template.Id}");
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] IsScheduled: {template.IsScheduled}");
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] ScheduleType: {template.ScheduleType}");
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] ScheduledTime: {template.ScheduledTime}");
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] ScheduledDaysOfWeek: {template.ScheduledDaysOfWeek}");
                System.Diagnostics.Debug.WriteLine($"[GetTemplateFormViewModelAsync] ScheduledDayOfMonth: {template.ScheduledDayOfMonth}");

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
                    
                    // ⭐⭐⭐ FIX: متغیرهای فیلتر شده بر اساس نوع اعلان
                    SystemVariables = await GetVariablesForEventTypeAsync(template.NotificationEventType),
                    AvailableUsers = await GetUsersForSelectAsync(),
                    AvailableNotificationTypes = await GetAvailableNotificationTypesAsync()
                };
            }

            // الگوی جدید
            return new NotificationTemplateFormViewModel
            {
                IsActive = true,
                RecipientMode = 0,
                SystemVariables = await GetSystemVariablesAsync(), // همه متغیرها
                AvailableUsers = await GetUsersForSelectAsync(),
                AvailableNotificationTypes = await GetAvailableNotificationTypesAsync()
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