using MahERP.DataModelLayer.Entities.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Notifications
{
    /// <summary>
    /// آمار تفصیلی ارسال
    /// </summary>
    public class NotificationDeliveryStats
    {
        [Key]
        public int Id { get; set; }

        public int CoreNotificationDeliveryId { get; set; }
        [ForeignKey("CoreNotificationDeliveryId")]
        public virtual CoreNotificationDelivery Delivery { get; set; }

        // زمان‌بندی
        public DateTime? QueuedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // مدت زمان
        public int? ProcessingDurationSeconds { get; set; }
        public int? DeliveryDurationSeconds { get; set; }

        // اطلاعات تکنیکال
        public string? ServerResponse { get; set; }
        public string? ErrorDetails { get; set; }
        public int RetryAttempts { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}