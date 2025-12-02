using MahERP.DataModelLayer.Entities.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.DataModelLayer.Enums;

namespace MahERP.DataModelLayer.ViewModels.Notifications
{
    /// <summary>
    /// ViewModel لیست الگوها
    /// </summary>
    public class NotificationTemplateListViewModel
    {
        public List<NotificationTemplateItemViewModel> Templates { get; set; }
            = new List<NotificationTemplateItemViewModel>();
    }

    /// <summary>
    /// ViewModel آیتم الگو در لیست
    /// </summary>
    public class NotificationTemplateItemViewModel
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; }
        
        // ⭐⭐⭐ تغییر به ساختار جدید
        public string? NotificationTypeName { get; set; } // نام نوع اعلان
        public string? ModuleName { get; set; } // نام ماژول
        public byte Channel { get; set; } // کانال ارسال
        public string? ChannelTypeName { get; set; } // نام کانال
        public byte RecipientMode { get; set; } // حالت دریافت‌کننده
        public int RecipientCount { get; set; } // تعداد دریافت‌کننده
        
        public string? Description { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatorName { get; set; }
        
        // ⭐⭐⭐ فیلدهای زمان‌بندی
        public bool IsScheduled { get; set; }
        public byte ScheduleType { get; set; }
        public string? ScheduledTime { get; set; }
        public string? ScheduledDaysOfWeek { get; set; }
        public int? ScheduledDayOfMonth { get; set; }
        public DateTime? LastExecutionDate { get; set; }
        public DateTime? NextExecutionDate { get; set; }
    }

    /// <summary>
    /// ViewModel فرم ایجاد/ویرایش الگو
    /// </summary>
    public class NotificationTemplateFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الگو الزامی است")]
        [Display(Name = "نام الگو")]
        [MaxLength(100)]
        public string TemplateName { get; set; }

        [Display(Name = "کد الگو")]
        [MaxLength(50)]
        public string? TemplateCode { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500)]
        public string? Description { get; set; }

        // ✅ تغییر نام: NotificationEventType به جای NotificationTypeConfigId
        [Required(ErrorMessage = "نوع رویداد الزامی است")]
        [Display(Name = "نوع رویداد")]
        public byte NotificationEventType { get; set; }

        // ✅ تغییر نام: Channel به جای ChannelType
        [Required(ErrorMessage = "کانال ارسال الزامی است")]
        [Display(Name = "کانال ارسال")]
        public byte Channel { get; set; }

        [Display(Name = "موضوع (برای ایمیل)")]
        [MaxLength(200)]
        public string? Subject { get; set; }

        // ✅ تغییر نام: MessageTemplate به جای Body
        [Required(ErrorMessage = "محتوای الگو الزامی است")]
        [Display(Name = "محتوای الگو")]
        public string MessageTemplate { get; set; }

        // ✅ Backward compatibility
        [Obsolete("استفاده از MessageTemplate")]
        public string? Body
        {
            get => MessageTemplate;
            set => MessageTemplate = value;
        }

        [Display(Name = "محتوای HTML")]
        public string? BodyHtml { get; set; }

        [Required(ErrorMessage = "حالت دریافت‌کنندگان الزامی است")]
        [Display(Name = "حالت دریافت‌کنندگان")]
        public byte RecipientMode { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "کاربران انتخاب شده")]
        public List<string>? SelectedUserIds { get; set; }

        // ⭐⭐⭐ تنظیمات زمان‌بندی
        [Display(Name = "زمان‌بندی فعال")]
        public bool IsScheduled { get; set; } = false;

        [Display(Name = "نوع زمان‌بندی")]
        public byte ScheduleType { get; set; } = 0;

        [Display(Name = "ساعت اجرا")]
        [MaxLength(5)]
        public string? ScheduledTime { get; set; }

        [Display(Name = "روزهای هفته")]
        [MaxLength(50)]
        public string? ScheduledDaysOfWeek { get; set; }

        [Display(Name = "روز ماه")]
        public int? ScheduledDayOfMonth { get; set; }

        /// <summary>
        /// ⭐⭐⭐ روزهای ماه برای اجرا (برای Monthly - چند روز)
        /// مثال: "10,15,25" = روزهای 10، 15، 25 هر ماه
        /// </summary>
        [Display(Name = "روزهای ماه")]
        [MaxLength(100)]
        public string? ScheduledDaysOfMonth { get; set; }

        [Display(Name = "آخرین اجرا")]
        public DateTime? LastExecutionDate { get; set; }

        [Display(Name = "اجرای بعدی")]
        public DateTime? NextExecutionDate { get; set; }

        // برای نمایش در فرم
        public List<NotificationTypeSelectItem> AvailableNotificationTypes { get; set; } = new();
        public List<SystemVariableViewModel> SystemVariables { get; set; } = new();
        public List<UserSelectItem> AvailableUsers { get; set; } = new();
    }

    /// <summary>
    /// آیتم نوع اعلان برای Dropdown
    /// </summary>
    public class NotificationTypeSelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ModuleName { get; set; }
        public string DisplayName => $"{ModuleName} - {Name}";
        
        /// <summary>
        /// آیا این نوع اعلان قابل زمان‌بندی است؟
        /// (فقط برای اعلان‌های دوره‌ای مثل DailyTaskDigest)
        /// </summary>
        public bool IsSchedulable { get; set; }
    }

    /// <summary>
    /// آیتم کاربر برای Dropdown
    /// </summary>
    public class UserSelectItem
    {
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// ⭐⭐⭐ متغیر سیستمی برای استفاده در الگوها
    /// </summary>
    public class SystemVariableViewModel
    {
        public string VariableName { get; set; }
        public string DisplayName { get; set; }
        public string? Description { get; set; }
        
        /// <summary>
        /// ⭐ دسته‌بندی‌های مربوط به این متغیر
        /// </summary>
        public List<NotificationVariableCategory> Categories { get; set; } = new();
        
        /// <summary>
        /// آیا این متغیر منسوخ شده؟
        /// </summary>
        public bool IsDeprecated { get; set; }
        
        /// <summary>
        /// مثال استفاده
        /// </summary>
        public string? ExampleValue { get; set; }
    }

    /// <summary>
    /// ViewModel پیش‌نمایش الگو
    /// </summary>
    public class NotificationTemplatePreviewViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public byte ChannelType { get; set; } // ⭐ تغییر از TemplateType
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? BodyHtml { get; set; }
        public string? PreviewContent { get; set; }
        public Dictionary<string, string>? SampleData { get; set; }
    }
}