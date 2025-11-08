using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.Notifications
{
    /// <summary>
    /// ViewModel برای صفحه تنظیمات کلی اعلان‌ها (Admin)
    /// </summary>
    public class NotificationSettingsViewModel
    {
        public bool IsNotificationSystemEnabled { get; set; }
        
        public bool IsEmailEnabled { get; set; }
        public bool IsSmsEnabled { get; set; }
        public bool IsTelegramEnabled { get; set; }
        
        public int MaxEmailPerUserPerDay { get; set; } = 50;
        public int MaxSmsPerUserPerDay { get; set; } = 10;
        public int MaxTelegramPerUserPerDay { get; set; } = 100;
        
        public string DailyDigestTime { get; set; } = "08:00";
        
        public List<NotificationModuleViewModel> Modules { get; set; }
            = new List<NotificationModuleViewModel>();
    }

    public class NotificationModuleViewModel
    {
        public int Id { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleNameFa { get; set; }
        public string ColorCode { get; set; }
        public bool IsActive { get; set; }
        
        public List<NotificationTypeViewModel> Types { get; set; }
            = new List<NotificationTypeViewModel>();
    }

    public class NotificationTypeViewModel
    {
        public int Id { get; set; }
        public string TypeCode { get; set; }
        public string TypeNameFa { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public byte Priority { get; set; }
        
        public bool SupportsEmail { get; set; }
        public bool SupportsSms { get; set; }
        public bool SupportsTelegram { get; set; }
        
        public bool AllowUserCustomization { get; set; }
    }

    /// <summary>
    /// ViewModel برای تنظیمات شخصی کاربر
    /// </summary>
    public class UserNotificationSettingsViewModel
    {
        public string UserId { get; set; }
        
        public List<UserNotificationPreferenceViewModel> Preferences { get; set; }
            = new List<UserNotificationPreferenceViewModel>();
    }

    public class UserNotificationPreferenceViewModel
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string TypeNameFa { get; set; }
        public string ModuleNameFa { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public bool ReceiveBySystem { get; set; }
        public bool ReceiveByEmail { get; set; }
        public bool ReceiveBySms { get; set; }
        public bool ReceiveByTelegram { get; set; }
        
        public byte DeliveryMode { get; set; }
        public string PreferredDeliveryTime { get; set; }
        
        public string QuietHoursStart { get; set; }
        public string QuietHoursEnd { get; set; }
    }
}