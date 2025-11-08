using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// دریافت‌کنندگان یک قالب اعلان
    /// این جدول زمانی استفاده می‌شود که RecipientMode = 1 یا 2 باشد
    /// </summary>
    [Table("NotificationTemplateRecipient_Tbl")]
    public class NotificationTemplateRecipient
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه قالب
        /// </summary>
        [Required]
        public int NotificationTemplateId { get; set; }

        /// <summary>
        /// نوع دریافت‌کننده
        /// 0 = Contact (شخص)
        /// 1 = Organization (سازمان)
        /// 2 = User (کاربر سیستم)
        /// </summary>
        [Required]
        public byte RecipientType { get; set; }

        #region 🔹 Foreign Keys بر اساس RecipientType

        /// <summary>
        /// شناسه شخص (RecipientType = 0)
        /// </summary>
        public int? ContactId { get; set; }

        /// <summary>
        /// شناسه سازمان (RecipientType = 1)
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// شناسه کاربر سیستم (RecipientType = 2)
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        #endregion

        #region 🔹 Audit

        /// <summary>
        /// تاریخ اضافه شدن
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// کاربر اضافه‌کننده
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        #endregion

        #region 🔹 Navigation Properties

        /// <summary>
        /// قالب مرتبط
        /// </summary>
        [ForeignKey(nameof(NotificationTemplateId))]
        public virtual NotificationTemplate? NotificationTemplate { get; set; }

        /// <summary>
        /// شخص (اگر RecipientType = 0)
        /// </summary>
        [ForeignKey(nameof(ContactId))]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// سازمان (اگر RecipientType = 1)
        /// </summary>
        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// کاربر سیستم (اگر RecipientType = 2)
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual AppUsers? User { get; set; }

        /// <summary>
        /// کاربر اضافه‌کننده
        /// </summary>
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers? CreatedBy { get; set; }

        #endregion

        #region 🔹 Helper Properties

        /// <summary>
        /// نام نوع دریافت‌کننده (برای نمایش)
        /// </summary>
        [NotMapped]
        public string RecipientTypeName => RecipientType switch
        {
            0 => "شخص",
            1 => "سازمان",
            2 => "کاربر سیستم",
            _ => "نامشخص"
        };

        /// <summary>
        /// نام دریافت‌کننده (برای نمایش)
        /// </summary>
        [NotMapped]
        public string RecipientName => RecipientType switch
        {
            0 => Contact?.FullName ?? "شخص نامشخص",
            1 => Organization?.Name ?? "سازمان نامشخص",
            2 => User != null ? $"{User.FirstName} {User.LastName}" : "کاربر نامشخص",
            _ => "نامشخص"
        };

        /// <summary>
        /// اطلاعات تماس (برای نمایش)
        /// </summary>
        [NotMapped]
        public string ContactInfo => RecipientType switch
        {
            0 => Contact?.PrimaryEmail ?? Contact?.DefaultPhone?.PhoneNumber ?? "-",
            1 => Organization?.Email ?? Organization?.PrimaryPhone ?? "-",
            2 => User?.Email ?? User?.PhoneNumber ?? "-",
            _ => "-"
        };

        #endregion
    }
}