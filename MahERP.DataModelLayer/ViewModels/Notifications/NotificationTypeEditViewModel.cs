using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.Notifications
{
    /// <summary>
    /// ViewModel برای ویرایش نوع اعلان
    /// </summary>
    public class NotificationTypeEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام فارسی الزامی است")]
        [MaxLength(200)]
        public string TypeNameFa { get; set; }

        public string? TypeCode { get; set; } // فقط نمایش (غیرقابل ویرایش)

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [Range(0, 2)]
        public byte DefaultPriority { get; set; } // 0=Low, 1=Normal, 2=High

        public bool AllowUserCustomization { get; set; }

        // کانال‌ها
        public bool SupportsEmail { get; set; }
        public bool SupportsSms { get; set; }
        public bool SupportsTelegram { get; set; }

        // الگوهای مرتبط (برای هر کانال)
        public int? EmailTemplateId { get; set; }
        public int? SmsTemplateId { get; set; }
        public int? TelegramTemplateId { get; set; }

        // لیست الگوهای موجود
        public List<TemplateSelectItem> AvailableEmailTemplates { get; set; } = new();
        public List<TemplateSelectItem> AvailableSmsTemplates { get; set; } = new();
        public List<TemplateSelectItem> AvailableTelegramTemplates { get; set; } = new();

        // آمار لیست سیاه
        public int BlacklistCount { get; set; }

        // نام ماژول
        public string? ModuleName { get; set; }
    }

    public class TemplateSelectItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}