using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// داده‌های اولیه برای جداول CRM
    /// </summary>
    public static class CrmSeedData
    {
        /// <summary>
        /// داده‌های اولیه وضعیت لید در قیف فروش
        /// </summary>
        public static List<LeadStageStatus> GetLeadStageStatuses()
        {
            return new List<LeadStageStatus>
            {
                new LeadStageStatus
                {
                    Id = 1,
                    StageType = LeadStageType.Awareness,
                    Title = "آگاهی",
                    TitleEnglish = "Awareness",
                    Description = "اولین تماس/آشنایی با محصول یا خدمت",
                    DisplayOrder = 1,
                    ColorCode = "#17a2b8",
                    Icon = "fa-eye",
                    IsActive = true
                },
                new LeadStageStatus
                {
                    Id = 2,
                    StageType = LeadStageType.Interest,
                    Title = "علاقه‌مندی",
                    TitleEnglish = "Interest",
                    Description = "نشان دادن علاقه به محصول/خدمت",
                    DisplayOrder = 2,
                    ColorCode = "#6f42c1",
                    Icon = "fa-heart",
                    IsActive = true
                },
                new LeadStageStatus
                {
                    Id = 3,
                    StageType = LeadStageType.Evaluation,
                    Title = "ارزیابی",
                    TitleEnglish = "Evaluation",
                    Description = "بررسی و مقایسه با گزینه‌های دیگر",
                    DisplayOrder = 3,
                    ColorCode = "#fd7e14",
                    Icon = "fa-balance-scale",
                    IsActive = true
                },
                new LeadStageStatus
                {
                    Id = 4,
                    StageType = LeadStageType.Decision,
                    Title = "تصمیم‌گیری",
                    TitleEnglish = "Decision",
                    Description = "آماده تصمیم نهایی برای خرید",
                    DisplayOrder = 4,
                    ColorCode = "#ffc107",
                    Icon = "fa-gavel",
                    IsActive = true
                },
                new LeadStageStatus
                {
                    Id = 5,
                    StageType = LeadStageType.Purchase,
                    Title = "خرید",
                    TitleEnglish = "Purchase",
                    Description = "انجام خرید - تبدیل به مشتری",
                    DisplayOrder = 5,
                    ColorCode = "#28a745",
                    Icon = "fa-shopping-cart",
                    IsActive = true
                }
            };
        }

        /// <summary>
        /// داده‌های اولیه وضعیت بعد از خرید
        /// </summary>
        public static List<PostPurchaseStage> GetPostPurchaseStages()
        {
            return new List<PostPurchaseStage>
            {
                new PostPurchaseStage
                {
                    Id = 1,
                    StageType = PostPurchaseStageType.Retention,
                    Title = "حفظ مشتری",
                    TitleEnglish = "Retention",
                    Description = "تعاملات برای نگهداشت و حفظ مشتری",
                    DisplayOrder = 1,
                    ColorCode = "#20c997",
                    Icon = "fa-user-shield",
                    IsActive = true
                },
                new PostPurchaseStage
                {
                    Id = 2,
                    StageType = PostPurchaseStageType.Referral,
                    Title = "ارجاع/توصیه",
                    TitleEnglish = "Referral",
                    Description = "مشتری کسی را به ما معرفی کرده است",
                    DisplayOrder = 2,
                    ColorCode = "#007bff",
                    Icon = "fa-share-alt",
                    IsActive = true
                },
                new PostPurchaseStage
                {
                    Id = 3,
                    StageType = PostPurchaseStageType.Loyalty,
                    Title = "وفادارسازی",
                    TitleEnglish = "Loyalty",
                    Description = "تعاملات برای افزایش وفاداری مشتری",
                    DisplayOrder = 3,
                    ColorCode = "#e83e8c",
                    Icon = "fa-medal",
                    IsActive = true
                },
                new PostPurchaseStage
                {
                    Id = 4,
                    StageType = PostPurchaseStageType.VIP,
                    Title = "VIP",
                    TitleEnglish = "VIP",
                    Description = "تعاملات ویژه با مشتریان خاص و VIP",
                    DisplayOrder = 4,
                    ColorCode = "#ffc107",
                    Icon = "fa-crown",
                    IsActive = true
                }
            };
        }
    }
}
