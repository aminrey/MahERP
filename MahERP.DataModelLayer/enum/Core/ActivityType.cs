namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// انواع فعالیت‌های کاربران در سیستم
    /// </summary>
    public enum ActivityType : byte
    {
        /// <summary>
        /// مشاهده اطلاعات
        /// </summary>
        View = 0,

        /// <summary>
        /// ایجاد رکورد جدید
        /// </summary>
        Create = 1,

        /// <summary>
        /// ویرایش رکورد موجود
        /// </summary>
        Edit = 2,

        /// <summary>
        /// حذف رکورد
        /// </summary>
        Delete = 3,

        /// <summary>
        /// تایید عملیات
        /// </summary>
        Approve = 4,

        /// <summary>
        /// رد عملیات
        /// </summary>
        Reject = 5,

        /// <summary>
        /// ورود به سیستم
        /// </summary>
        Login = 6,

        /// <summary>
        /// خروج از سیستم
        /// </summary>
        Logout = 7,

        /// <summary>
        /// دانلود فایل
        /// </summary>
        Download = 8,

        /// <summary>
        /// آپلود فایل
        /// </summary>
        Upload = 9,

        /// <summary>
        /// جستجو در سیستم
        /// </summary>
        Search = 10,

        /// <summary>
        /// چاپ اطلاعات
        /// </summary>
        Print = 11,

        /// <summary>
        /// ارسال ایمیل
        /// </summary>
        SendEmail = 12,

        /// <summary>
        /// ارسال پیامک
        /// </summary>
        SendSMS = 13,

        /// <summary>
        /// تغییر وضعیت
        /// </summary>
        ChangeStatus = 14,

        /// <summary>
        /// انتقال/انتصاب
        /// </summary>
        Assign = 15,

        /// <summary>
        /// تکمیل تسک
        /// </summary>
        Complete = 16,

        /// <summary>
        /// باز کردن مجدد
        /// </summary>
        Reopen = 17,

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        ChangePassword = 18,

        /// <summary>
        /// تغییر تنظیمات
        /// </summary>
        ChangeSetting = 19,

        /// <summary>
        /// اکسپورت داده
        /// </summary>
        Export = 20,

        /// <summary>
        /// ایمپورت داده
        /// </summary>
        Import = 21,

        /// <summary>
        /// بکاپ سیستم
        /// </summary>
        Backup = 22,

        /// <summary>
        /// ریستور سیستم
        /// </summary>
        Restore = 23
    }

    /// <summary>
    /// نتیجه عملیات
    /// </summary>
    public enum OperationResult : byte
    {
        /// <summary>
        /// موفق
        /// </summary>
        Success = 0,

        /// <summary>
        /// ناموفق
        /// </summary>
        Failed = 1,

        /// <summary>
        /// خطای سیستم
        /// </summary>
        Error = 2,

        /// <summary>
        /// عدم دسترسی
        /// </summary>
        AccessDenied = 3,

        /// <summary>
        /// اطلاعات نامعتبر
        /// </summary>
        InvalidData = 4,

        /// <summary>
        /// منقضی شده
        /// </summary>
        Expired = 5,

        /// <summary>
        /// لغو شده
        /// </summary>
        Cancelled = 6
    }

    /// <summary>
    /// سطح اهمیت لاگ
    /// </summary>
    public enum LogImportanceLevel : byte
    {
        /// <summary>
        /// عادی
        /// </summary>
        Normal = 0,

        /// <summary>
        /// مهم
        /// </summary>
        Important = 1,

        /// <summary>
        /// بحرانی
        /// </summary>
        Critical = 2
    }

    /// <summary>
    /// نوع دستگاه
    /// </summary>
    public enum DeviceType : byte
    {
        /// <summary>
        /// رایانه رومیزی
        /// </summary>
        Desktop = 0,

        /// <summary>
        /// تلفن همراه
        /// </summary>
        Mobile = 1,

        /// <summary>
        /// تبلت
        /// </summary>
        Tablet = 2,

        /// <summary>
        /// نامشخص
        /// </summary>
        Unknown = 3
    }
}