namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// نوع ماژول منبع درخواست
    /// </summary>
    public enum ModuleSourceType : byte
    {
        /// <summary>
        /// سیستم (خودکار)
        /// </summary>
        System = 0,

        /// <summary>
        /// ماژول تسکینگ
        /// </summary>
        Tasking = 1,

        /// <summary>
        /// ماژول CRM
        /// </summary>
        CRM = 2,

        /// <summary>
        /// ماژول حسابداری (آینده)
        /// </summary>
        Accounting = 3,

        /// <summary>
        /// ماژول انبارداری (آینده)
        /// </summary>
        Inventory = 4,

        /// <summary>
        /// ماژول منابع انسانی (آینده)
        /// </summary>
        HR = 5,

        /// <summary>
        /// ماژول پروژه (آینده)
        /// </summary>
        Project = 6
    }

    /// <summary>
    /// نوع منبع تسک در CRM
    /// </summary>
    public enum CrmTaskSourceType : byte
    {
        /// <summary>
        /// تسک دستی (بدون منبع CRM)
        /// </summary>
        Manual = 0,

        /// <summary>
        /// پیگیری سرنخ
        /// </summary>
        LeadFollowUp = 1,

        /// <summary>
        /// اقدام بعدی فرصت فروش
        /// </summary>
        OpportunityNextAction = 2,

        /// <summary>
        /// تیکت پشتیبانی
        /// </summary>
        Ticket = 3,

        /// <summary>
        /// تمدید قرارداد
        /// </summary>
        ContractRenewal = 4,

        /// <summary>
        /// تماس دوره‌ای با مشتری
        /// </summary>
        CustomerCall = 5,

        /// <summary>
        /// یادآوری پرداخت
        /// </summary>
        PaymentReminder = 6
    }

    /// <summary>
    /// نوع درخواست به Core
    /// </summary>
    public enum CoreRequestType : byte
    {
        /// <summary>
        /// ایجاد تسک
        /// </summary>
        CreateTask = 1,

        /// <summary>
        /// بروزرسانی تسک
        /// </summary>
        UpdateTask = 2,

        /// <summary>
        /// ایجاد نوتیفیکیشن
        /// </summary>
        CreateNotification = 3,

        /// <summary>
        /// ثبت فعالیت
        /// </summary>
        LogActivity = 4,

        /// <summary>
        /// ارسال پیامک
        /// </summary>
        SendSms = 5,

        /// <summary>
        /// ارسال ایمیل
        /// </summary>
        SendEmail = 6
    }

    /// <summary>
    /// وضعیت درخواست Core
    /// </summary>
    public enum CoreRequestStatus : byte
    {
        /// <summary>
        /// در انتظار پردازش
        /// </summary>
        Pending = 0,

        /// <summary>
        /// در حال پردازش
        /// </summary>
        Processing = 1,

        /// <summary>
        /// تکمیل شده
        /// </summary>
        Completed = 2,

        /// <summary>
        /// خطا
        /// </summary>
        Failed = 3,

        /// <summary>
        /// لغو شده
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// اولویت تسک CRM
    /// </summary>
    public enum CrmTaskPriority : byte
    {
        /// <summary>
        /// کم
        /// </summary>
        Low = 0,

        /// <summary>
        /// عادی
        /// </summary>
        Normal = 1,

        /// <summary>
        /// زیاد
        /// </summary>
        High = 2,

        /// <summary>
        /// فوری
        /// </summary>
        Urgent = 3
    }

    /// <summary>
    /// نوع اقدام بعدی در CRM
    /// </summary>
    public enum CrmNextActionType : byte
    {
        /// <summary>
        /// تماس تلفنی
        /// </summary>
        Call = 0,

        /// <summary>
        /// جلسه
        /// </summary>
        Meeting = 1,

        /// <summary>
        /// ارسال ایمیل
        /// </summary>
        Email = 2,

        /// <summary>
        /// ارسال پیامک
        /// </summary>
        Sms = 3,

        /// <summary>
        /// ارسال پیشنهاد قیمت
        /// </summary>
        SendQuote = 4,

        /// <summary>
        /// پیگیری پیشنهاد
        /// </summary>
        FollowUpQuote = 5,

        /// <summary>
        /// بازدید
        /// </summary>
        Visit = 6,

        /// <summary>
        /// دمو
        /// </summary>
        Demo = 7,

        /// <summary>
        /// سایر
        /// </summary>
        Other = 99
    }
}
