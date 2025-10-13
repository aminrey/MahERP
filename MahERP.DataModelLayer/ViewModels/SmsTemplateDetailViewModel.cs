using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels
{
    public class SmsTemplateDetailViewModel
    {
        // اطلاعات قالب
        public int Id { get; set; }
        public string Title { get; set; }
        public string MessageTemplate { get; set; }
        public string Description { get; set; }
        public byte TemplateType { get; set; }
        public string TemplateTypeText { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatorName { get; set; }

        // آمار قالب
        public int TotalRecipients { get; set; }
        public int ContactRecipients { get; set; }
        public int OrganizationRecipients { get; set; }
        public int TotalSent { get; set; }
        public int SuccessfulSent { get; set; }
        public int FailedSent { get; set; }

        // لیست مخاطبین
        public List<RecipientItemViewModel> Recipients { get; set; } = new();

        // لاگ‌های ارسال اخیر
        public List<SmsLogItemViewModel> RecentLogs { get; set; } = new();
    }

    public class RecipientItemViewModel
    {
        public int Id { get; set; }
        public byte RecipientType { get; set; }
        public string RecipientTypeText { get; set; }
        public int? ContactId { get; set; }
        public int? OrganizationId { get; set; }
        public string RecipientName { get; set; }
        public string RecipientContact { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedByName { get; set; }
    }

    public class SmsLogItemViewModel
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string RecipientName { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int DeliveryStatus { get; set; }
        public string DeliveryStatusText { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime SendDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ProviderName { get; set; }
    }
}