using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.Notifications
{
    public class NotificationSettingsRepository : INotificationSettingsRepository
    {
        private readonly AppDbContext _context;

        public NotificationSettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== Module & Type Config ====================

        public async Task<List<NotificationModuleConfig>> GetAllModulesAsync()
        {
            return await _context.NotificationModuleConfig_Tbl
                .Where(m => m.IsActive)
                .Include(m => m.NotificationTypes)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<NotificationModuleConfig> GetModuleWithTypesAsync(int moduleId)
        {
            return await _context.NotificationModuleConfig_Tbl
                .Include(m => m.NotificationTypes.Where(t => t.IsActive))
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.IsActive);
        }

        public async Task<List<NotificationTypeConfig>> GetModuleNotificationTypesAsync(int moduleId)
        {
            return await _context.NotificationTypeConfig_Tbl
                .Where(t => t.ModuleConfigId == moduleId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<NotificationTypeConfig> GetNotificationTypeByIdAsync(int typeId)
        {
            return await _context.NotificationTypeConfig_Tbl
                .Include(t => t.ModuleConfig)
                .FirstOrDefaultAsync(t => t.Id == typeId);
        }

        public async Task<NotificationTypeConfig> GetNotificationTypeByCodeAsync(string typeCode)
        {
            return await _context.NotificationTypeConfig_Tbl
                .Include(t => t.ModuleConfig)
                .FirstOrDefaultAsync(t => t.TypeCode == typeCode);
        }

        public async Task<bool> ToggleNotificationTypeAsync(int typeId, bool isActive)
        {
            var type = await _context.NotificationTypeConfig_Tbl
                .FindAsync(typeId);

            if (type == null)
                return false;

            type.IsActive = isActive;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateNotificationTypeConfigAsync(NotificationTypeConfig config)
        {
            try
            {
                _context.NotificationTypeConfig_Tbl.Update(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== User Preferences ====================

        public async Task<List<UserNotificationPreference>> GetUserPreferencesAsync(string userId)
        {
            return await _context.UserNotificationPreference_Tbl
                .Include(p => p.NotificationTypeConfig)
                    .ThenInclude(t => t.ModuleConfig)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserNotificationPreference> GetUserPreferenceAsync(string userId, int typeId)
        {
            return await _context.UserNotificationPreference_Tbl
                .Include(p => p.NotificationTypeConfig)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationTypeConfigId == typeId);
        }

        public async Task<bool> SaveUserPreferenceAsync(UserNotificationPreference preference)
        {
            try
            {
                preference.CreatedAt = DateTime.Now;
                _context.UserNotificationPreference_Tbl.Add(preference);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserPreferenceAsync(UserNotificationPreference preference)
        {
            try
            {
                preference.LastModifiedAt = DateTime.Now;
                _context.UserNotificationPreference_Tbl.Update(preference);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateDefaultUserPreferencesAsync(string userId)
        {
            try
            {
                // دریافت تمام انواع اعلان که AllowUserCustomization = true
                var types = await _context.NotificationTypeConfig_Tbl
                    .Where(t => t.IsActive && t.AllowUserCustomization)
                    .ToListAsync();

                foreach (var type in types)
                {
                    // بررسی اینکه قبلاً وجود نداشته باشد
                    var exists = await _context.UserNotificationPreference_Tbl
                        .AnyAsync(p => p.UserId == userId && p.NotificationTypeConfigId == type.Id);

                    if (!exists)
                    {
                        var preference = new UserNotificationPreference
                        {
                            UserId = userId,
                            NotificationTypeConfigId = type.Id,
                            IsEnabled = true,
                            ReceiveBySystem = true,
                            ReceiveByEmail = type.SupportsEmail,
                            ReceiveBySms = false, // پیش‌فرض غیرفعال
                            ReceiveByTelegram = type.SupportsTelegram,
                            DeliveryMode = 0, // فوری
                            CreatedAt = DateTime.Now
                        };

                        _context.UserNotificationPreference_Tbl.Add(preference);
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

        // ==================== Blacklist ====================

        public async Task<List<NotificationBlacklist>> GetBlacklistAsync()
        {
            return await _context.NotificationBlacklist_Tbl
                .Include(b => b.User)
                .Include(b => b.NotificationTypeConfig)
                .Include(b => b.CreatedBy)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> AddToBlacklistAsync(NotificationBlacklist blacklist)
        {
            try
            {
                blacklist.CreatedDate = DateTime.Now;
                _context.NotificationBlacklist_Tbl.Add(blacklist);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromBlacklistAsync(int blacklistId)
        {
            try
            {
                var blacklist = await _context.NotificationBlacklist_Tbl
                    .FindAsync(blacklistId);

                if (blacklist == null)
                    return false;

                blacklist.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserBlacklistedAsync(string userId, int? typeId = null)
        {
            var query = _context.NotificationBlacklist_Tbl
                .Where(b => b.UserId == userId && b.IsActive);

            if (typeId.HasValue)
            {
                // بررسی برای نوع خاص یا همه انواع
                query = query.Where(b => b.NotificationTypeConfigId == typeId.Value || 
                                        b.NotificationTypeConfigId == null);
            }
            else
            {
                // بررسی برای همه انواع
                query = query.Where(b => b.NotificationTypeConfigId == null);
            }

            return await query.AnyAsync();
        }

        // ==================== Validation ====================

        public async Task<bool> CanUserReceiveNotificationAsync(string userId, int typeId, byte channel)
        {
            // 1. بررسی لیست سیاه
            if (await IsUserBlacklistedAsync(userId, typeId))
                return false;

            // 2. بررسی تنظیمات شخصی کاربر
            var preference = await GetUserPreferenceAsync(userId, typeId);

            if (preference == null)
                return true; // اگر تنظیمات نداشت، پیش‌فرض فعال است

            if (!preference.IsEnabled)
                return false;

            // 3. بررسی کانال خاص
            return channel switch
            {
                0 => preference.ReceiveBySystem,
                1 => preference.ReceiveByEmail,
                2 => preference.ReceiveBySms,
                3 => preference.ReceiveByTelegram,
                _ => false
            };
        }

        // ==================== ViewModels ====================

        public async Task<NotificationSettingsViewModel> GetSettingsViewModelAsync()
        {
            var modules = await GetAllModulesAsync();

            var viewModel = new NotificationSettingsViewModel
            {
                IsNotificationSystemEnabled = true,
                IsEmailEnabled = true,
                IsSmsEnabled = true,
                IsTelegramEnabled = true,
                Modules = new List<NotificationModuleViewModel>()
            };

            foreach (var module in modules)
            {
                var moduleVm = new NotificationModuleViewModel
                {
                    Id = module.Id,
                    ModuleCode = module.ModuleCode,
                    ModuleNameFa = module.ModuleNameFa,
                    ColorCode = module.ColorCode,
                    IsActive = module.IsActive,
                    Types = new List<NotificationTypeViewModel>()
                };

                foreach (var type in module.NotificationTypes.OrderBy(t => t.DisplayOrder))
                {
                    moduleVm.Types.Add(new NotificationTypeViewModel
                    {
                        Id = type.Id,
                        TypeCode = type.TypeCode,
                        TypeNameFa = type.TypeNameFa,
                        Description = type.Description,
                        IsActive = type.IsActive,
                        Priority = type.DefaultPriority,
                        SupportsEmail = type.SupportsEmail,
                        SupportsSms = type.SupportsSms,
                        SupportsTelegram = type.SupportsTelegram,
                        AllowUserCustomization = type.AllowUserCustomization
                    });
                }

                viewModel.Modules.Add(moduleVm);
            }

            return viewModel;
        }

        public async Task<UserNotificationSettingsViewModel> GetUserSettingsViewModelAsync(string userId)
        {
            var preferences = await GetUserPreferencesAsync(userId);

            var viewModel = new UserNotificationSettingsViewModel
            {
                UserId = userId,
                Preferences = new List<UserNotificationPreferenceViewModel>()
            };

            foreach (var pref in preferences)
            {
                viewModel.Preferences.Add(new UserNotificationPreferenceViewModel
                {
                    Id = pref.Id,
                    TypeId = pref.NotificationTypeConfigId,
                    TypeNameFa = pref.NotificationTypeConfig.TypeNameFa,
                    ModuleNameFa = pref.NotificationTypeConfig.ModuleConfig.ModuleNameFa,
                    IsEnabled = pref.IsEnabled,
                    ReceiveBySystem = pref.ReceiveBySystem,
                    ReceiveByEmail = pref.ReceiveByEmail,
                    ReceiveBySms = pref.ReceiveBySms,
                    ReceiveByTelegram = pref.ReceiveByTelegram,
                    DeliveryMode = pref.DeliveryMode,
                    PreferredDeliveryTime = pref.PreferredDeliveryTime?.ToString(@"hh\:mm"),
                    QuietHoursStart = pref.QuietHoursStart?.ToString(@"hh\:mm"),
                    QuietHoursEnd = pref.QuietHoursEnd?.ToString(@"hh\:mm")
                });
            }

            return viewModel;
        }
        // پیاده‌سازی در Repository:

        public async Task<NotificationTypeEditViewModel> GetEditTypeViewModelAsync(int typeId)
        {
            var type = await _context.NotificationTypeConfig_Tbl
                .Include(t => t.ModuleConfig)
                .FirstOrDefaultAsync(t => t.Id == typeId);

            if (type == null)
                return null;

            // ⭐⭐⭐ تغییر: استفاده از ChannelType به جای TemplateType
            // دریافت الگوهای ایمیل (ChannelType = 1)
            var emailTemplates = await _context.NotificationTemplate_Tbl
                .Where(t => t.Channel == 1 && t.IsActive)
                .Select(t => new TemplateSelectItem
                {
                    Id = t.Id,
                    Name = t.TemplateName,
                    Code = t.TemplateCode
                })
                .ToListAsync();

            // دریافت الگوهای پیامک (ChannelType = 2)
            var smsTemplates = await _context.NotificationTemplate_Tbl
                .Where(t => t.Channel == 2 && t.IsActive)
                .Select(t => new TemplateSelectItem
                {
                    Id = t.Id,
                    Name = t.TemplateName,
                    Code = t.TemplateCode
                })
                .ToListAsync();

            // دریافت الگوهای تلگرام (ChannelType = 3)
            var telegramTemplates = await _context.NotificationTemplate_Tbl
                .Where(t => t.Channel == 3 && t.IsActive)
                .Select(t => new TemplateSelectItem
                {
                    Id = t.Id,
                    Name = t.TemplateName,
                    Code = t.TemplateCode
                })
                .ToListAsync();

            // شمارش لیست سیاه
            var blacklistCount = await _context.NotificationBlacklist_Tbl
                .CountAsync(b => b.NotificationTypeConfigId == typeId && b.IsActive);

            return new NotificationTypeEditViewModel
            {
                Id = type.Id,
                TypeNameFa = type.TypeNameFa,
                TypeCode = type.TypeCode,
                Description = type.Description,
                IsActive = type.IsActive,
                DefaultPriority = type.DefaultPriority,
                AllowUserCustomization = type.AllowUserCustomization,
                SupportsEmail = type.SupportsEmail,
                SupportsSms = type.SupportsSms,
                SupportsTelegram = type.SupportsTelegram,
                AvailableEmailTemplates = emailTemplates,
                AvailableSmsTemplates = smsTemplates,
                AvailableTelegramTemplates = telegramTemplates,
                BlacklistCount = blacklistCount,
                ModuleName = type.ModuleConfig?.ModuleNameFa
            };
        }
        public async Task<List<NotificationTypeConfig>> GetAllNotificationTypesAsync()
        {
            return await _context.NotificationTypeConfig_Tbl
                .Include(t => t.ModuleConfig)
                .OrderBy(t => t.ModuleConfig.DisplayOrder)
                .ThenBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        #region Recipient Management Implementation

        public async Task<ManageRecipientsViewModel> GetManageRecipientsViewModelAsync(int typeId)
        {
            var type = await _context.NotificationTypeConfig_Tbl
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.User)
                .Include(t => t.Recipients)
                    .ThenInclude(r => r.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == typeId);

            if (type == null)
                return null;

            // دریافت کاربران فعلی در لیست
            var currentRecipients = type.Recipients
                .Where(r => r.IsActive)
                .Select(r => new RecipientUser
                {
                    RecipientId = r.Id,
                    UserId = r.UserId,
                    FullName = $"{r.User.FirstName} {r.User.LastName}",
                    Email = r.User.Email,
                    ProfileImagePath = r.User.ProfileImagePath,
                    Reason = r.Reason,
                    AddedDate = r.CreatedDate,
                    AddedByName = r.CreatedBy != null ? $"{r.CreatedBy.FirstName} {r.CreatedBy.LastName}" : "سیستم"
                })
                .ToList();

            // دریافت تمام کاربران فعال
            var allUsers = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .Select(u => new AvailableUser
                {
                    UserId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    UserName = u.UserName
                })
                .ToListAsync();

            return new ManageRecipientsViewModel
            {
                NotificationTypeId = type.Id,
                TypeName = type.TypeNameFa,
                Description = type.Description,
                SendMode = type.SendMode,
                CurrentRecipients = currentRecipients,
                AllUsers = allUsers
            };
        }

        public async Task<bool> UpdateSendModeAsync(int typeId, byte sendMode)
        {
            var type = await _context.NotificationTypeConfig_Tbl
                .FirstOrDefaultAsync(t => t.Id == typeId);

            if (type == null)
                return false;

            type.SendMode = sendMode;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddRecipientAsync(int typeId, string userId, string reason, string createdByUserId)
        {
            // بررسی تکراری نبودن
            var exists = await _context.NotificationRecipient_Tbl
                .AnyAsync(r => r.NotificationTypeConfigId == typeId && r.UserId == userId && r.IsActive);

            if (exists)
                return false;

            var recipient = new NotificationRecipient
            {
                NotificationTypeConfigId = typeId,
                UserId = userId,
                Reason = reason,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedByUserId = createdByUserId
            };

            _context.NotificationRecipient_Tbl.Add(recipient);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveRecipientAsync(int recipientId)
        {
            var recipient = await _context.NotificationRecipient_Tbl
                .FirstOrDefaultAsync(r => r.Id == recipientId);

            if (recipient == null)
                return false;

            // Soft delete
            recipient.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ShouldReceiveNotificationAsync(int typeId, string userId)
        {
            var type = await _context.NotificationTypeConfig_Tbl
                .Include(t => t.Recipients)
                .FirstOrDefaultAsync(t => t.Id == typeId);

            if (type == null || !type.IsActive)
                return false;

            switch (type.SendMode)
            {
                case 0: // AllUsers
                    return true;

                case 1: // SpecificUsers
                    return type.Recipients.Any(r => r.UserId == userId && r.IsActive);

                case 2: // AllExceptUsers
                    return !type.Recipients.Any(r => r.UserId == userId && r.IsActive);

                default:
                    return true;
            }
        }

        #endregion

        #region Template Management Implementation


        // Helper methods
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

        private string GetTemplateTypeName(byte templateType)
        {
            return templateType switch
            {
                0 => "داخل سیستم",
                1 => "ایمیل",
                2 => "پیامک",
                3 => "تلگرام",
                _ => "نامشخص"
            };
        }

        #endregion
    }
}