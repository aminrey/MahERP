using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// متغیرهای قابل استفاده در الگوها
    /// </summary>
    public class NotificationTemplateVariable
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual NotificationTemplate Template { get; set; }

        /// <summary>
        /// نام متغیر (بدون براکت) - مثال: UserName
        /// </summary>
        [Required, MaxLength(100)]
        public string VariableName { get; set; }

        /// <summary>
        /// نام نمایشی فارسی
        /// </summary>
        [Required, MaxLength(200)]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// مقدار پیش‌فرض
        /// </summary>
        [MaxLength(200)]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// آیا الزامی است؟
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// نوع داده: 0=متن، 1=عدد، 2=تاریخ، 3=لینک
        /// </summary>
        public byte DataType { get; set; } = 0;

        public int DisplayOrder { get; set; }
    }
}