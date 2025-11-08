using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// تاریخچه تغییرات الگوها
    /// </summary>
    public class NotificationTemplateHistory
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual NotificationTemplate Template { get; set; }

        public int Version { get; set; }

        public string? Subject { get; set; }
        public string MessageTemplate { get; set; }
        public string? BodyHtml { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public string? ChangedByUserId { get; set; }
        [ForeignKey("ChangedByUserId")]
        public virtual AppUsers? ChangedBy { get; set; }

        [MaxLength(1000)]
        public string? ChangeNote { get; set; }
    }
}