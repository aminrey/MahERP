using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// افراد شرکت کننده در تعاملات CRM مانند تماس‌ها و جلسات
    /// </summary>
    public class CRMParticipant
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
        /// نوع شرکت کننده
        /// 0- کارمند شرکت
        /// 1- مشتری یا نماینده مشتری
        /// 2- سایر (متفرقه)
        /// </summary>
        public byte ParticipantType { get; set; }

        /// <summary>
        /// شناسه کاربر (اگر کارمند شرکت باشد)
        /// </summary>
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// شناسه فرد مرتبط با طرف حساب (اگر نماینده مشتری باشد)
        /// </summary>
        public int? StakeholderContactId { get; set; }
        [ForeignKey("StakeholderContactId")]
        public virtual StakeholderContact StakeholderContact { get; set; }

        /// <summary>
        /// نام شرکت کننده (برای افراد متفرقه)
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// سمت یا عنوان شرکت کننده
        /// </summary>
        [MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// اطلاعات تماس شرکت کننده (برای افراد متفرقه)
        /// </summary>
        [MaxLength(100)]
        public string ContactInfo { get; set; }

        /// <summary>
        /// توضیحات درباره نقش یا مشارکت فرد
        /// </summary>
        public string Notes { get; set; }
    }
}
