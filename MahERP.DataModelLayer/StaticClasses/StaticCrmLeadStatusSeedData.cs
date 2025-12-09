using MahERP.DataModelLayer.Entities.Crm;
using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// داده‌های پایه (Seed Data) برای وضعیت‌های سرنخ CRM
    /// ⭐⭐⭐ این کلاس توسط SystemSeedDataBackgroundService استفاده می‌شود
    /// </summary>
    public static class StaticCrmLeadStatusSeedData
    {
        /// <summary>
        /// ⭐⭐⭐ وضعیت‌های پیش‌فرض سرنخ CRM
        /// </summary>
        public static List<CrmLeadStatus> DefaultStatuses => new()
        {
            // ========== وضعیت‌های اولیه ==========
            new CrmLeadStatus
            {
                Title = "سرنخ جدید",
                TitleEnglish = "New Lead",
                ColorCode = "#007bff",
                Icon = "fa-user-plus",
                DisplayOrder = 1,
                IsDefault = true,
                IsFinal = false,
                IsPositive = false,
                Description = "سرنخ جدید که هنوز تماسی با آن گرفته نشده",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "تماس اول",
                TitleEnglish = "First Contact",
                ColorCode = "#17a2b8",
                Icon = "fa-phone",
                DisplayOrder = 2,
                IsDefault = false,
                IsFinal = false,
                IsPositive = false,
                Description = "اولین تماس با سرنخ انجام شده",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "در حال پیگیری",
                TitleEnglish = "In Progress",
                ColorCode = "#ffc107",
                Icon = "fa-spinner",
                DisplayOrder = 3,
                IsDefault = false,
                IsFinal = false,
                IsPositive = false,
                Description = "در حال پیگیری و مذاکره",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "علاقه‌مند",
                TitleEnglish = "Interested",
                ColorCode = "#20c997",
                Icon = "fa-thumbs-up",
                DisplayOrder = 4,
                IsDefault = false,
                IsFinal = false,
                IsPositive = false,
                Description = "سرنخ علاقه‌مند به محصول/خدمات است",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "ارسال پیشنهاد",
                TitleEnglish = "Proposal Sent",
                ColorCode = "#6f42c1",
                Icon = "fa-file-invoice",
                DisplayOrder = 5,
                IsDefault = false,
                IsFinal = false,
                IsPositive = false,
                Description = "پیشنهاد قیمت یا قرارداد ارسال شده",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "در انتظار تصمیم",
                TitleEnglish = "Pending Decision",
                ColorCode = "#fd7e14",
                Icon = "fa-hourglass-half",
                DisplayOrder = 6,
                IsDefault = false,
                IsFinal = false,
                IsPositive = false,
                Description = "در انتظار تصمیم‌گیری مشتری",
                IsActive = true
            },
            
            // ========== وضعیت‌های نهایی مثبت ==========
            new CrmLeadStatus
            {
                Title = "مشتری شد",
                TitleEnglish = "Won",
                ColorCode = "#28a745",
                Icon = "fa-check-circle",
                DisplayOrder = 10,
                IsDefault = false,
                IsFinal = true,
                IsPositive = true,
                Description = "سرنخ به مشتری تبدیل شد",
                IsActive = true
            },
            
            // ========== وضعیت‌های نهایی منفی ==========
            new CrmLeadStatus
            {
                Title = "از دست رفته",
                TitleEnglish = "Lost",
                ColorCode = "#dc3545",
                Icon = "fa-times-circle",
                DisplayOrder = 11,
                IsDefault = false,
                IsFinal = true,
                IsPositive = false,
                Description = "سرنخ از دست رفت (رقیب برد یا منصرف شد)",
                IsActive = true
            },
            new CrmLeadStatus
            {
                Title = "غیرفعال",
                TitleEnglish = "Inactive",
                ColorCode = "#6c757d",
                Icon = "fa-ban",
                DisplayOrder = 12,
                IsDefault = false,
                IsFinal = true,
                IsPositive = false,
                Description = "سرنخ غیرفعال یا بدون پاسخ",
                IsActive = true
            }
        };

        /// <summary>
        /// منابع پیشنهادی سرنخ
        /// </summary>
        public static List<string> SuggestedSources => new()
        {
            "تبلیغات آنلاین",
            "تبلیغات تلویزیونی",
            "معرفی مشتری",
            "نمایشگاه",
            "شبکه‌های اجتماعی",
            "وب‌سایت",
            "تماس تلفنی",
            "بازاریابی ایمیلی",
            "همکاران",
            "سایر"
        };
    }
}
