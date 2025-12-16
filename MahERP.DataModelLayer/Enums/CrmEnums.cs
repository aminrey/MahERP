namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// نوع Contact - تعیین وضعیت فرد در سیستم
    /// </summary>
    public enum ContactType : byte
    {
        /// <summary>
        /// سرنخ - هنوز خرید نکرده
        /// </summary>
        Lead = 0,

        /// <summary>
        /// مشتری - حداقل یک بار خرید کرده
        /// </summary>
        Customer = 1,

        /// <summary>
        /// شریک تجاری
        /// </summary>
        Partner = 2,

        /// <summary>
        /// سایر
        /// </summary>
        Other = 3
    }

    /// <summary>
    /// وضعیت لید در قیف فروش (Static - Seeded)
    /// </summary>
    public enum LeadStageType : byte
    {
        /// <summary>
        /// آگاهی - اولین تماس/آشنایی
        /// </summary>
        Awareness = 1,

        /// <summary>
        /// علاقه‌مندی - نشان دادن علاقه به محصول/خدمت
        /// </summary>
        Interest = 2,

        /// <summary>
        /// ارزیابی - بررسی و مقایسه
        /// </summary>
        Evaluation = 3,

        /// <summary>
        /// تصمیم‌گیری - آماده تصمیم نهایی
        /// </summary>
        Decision = 4,

        /// <summary>
        /// خرید - انجام خرید
        /// </summary>
        Purchase = 5
    }

    /// <summary>
    /// وضعیت مشتری بعد از خرید (Static - Seeded)
    /// </summary>
    public enum PostPurchaseStageType : byte
    {
        /// <summary>
        /// حفظ مشتری - تعاملات برای نگهداشت مشتری
        /// </summary>
        Retention = 1,

        /// <summary>
        /// ارجاع/توصیه - مشتری کسی را معرفی کرده
        /// </summary>
        Referral = 2,

        /// <summary>
        /// وفادارسازی - تعاملات برای افزایش وفاداری
        /// </summary>
        Loyalty = 3,

        /// <summary>
        /// VIP - تعاملات ویژه با مشتریان خاص
        /// </summary>
        VIP = 4
    }

    /// <summary>
    /// وضعیت نتیجه توصیه/ارجاع
    /// </summary>
    public enum ReferralStatus : byte
    {
        /// <summary>
        /// در انتظار - معرفی‌شده هنوز خرید نکرده
        /// </summary>
        Pending = 0,

        /// <summary>
        /// موفق - معرفی‌شده خرید کرد
        /// </summary>
        Successful = 1,

        /// <summary>
        /// ناموفق - معرفی‌شده خرید نکرد
        /// </summary>
        Failed = 2
    }
}
