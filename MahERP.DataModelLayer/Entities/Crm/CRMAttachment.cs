using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// پیوست‌های مربوط به تعاملات CRM
    /// </summary>
    public class CRMAttachment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// تعامل CRM مرتبط
        /// </summary>
        public int CRMInteractionId { get; set; }
        [ForeignKey("CRMInteractionId")]
        public virtual CRMInteraction CRMInteraction { get; set; }

        /// <summary>
        /// نام فایل پیوست
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        /// <summary>
        /// نوع فایل (مانند pdf، docx و...)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FileType { get; set; }

        /// <summary>
        /// مسیر ذخیره فایل در سرور
        /// </summary>
        [Required]
        public string FilePath { get; set; }

        /// <summary>
        /// توضیحات پیوست
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// حجم فایل به بایت
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// تاریخ آپلود فایل
        /// </summary>
        public DateTime UploadDate { get; set; }

        /// <summary>
        /// کاربر آپلود کننده فایل
        /// </summary>
        public string UploaderUserId { get; set; }
        [ForeignKey("UploaderUserId")]
        public virtual AppUsers Uploader { get; set; }
    }
}
