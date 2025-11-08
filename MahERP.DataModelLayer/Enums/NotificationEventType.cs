namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// انواع رویدادهای سیستم که نیاز به اعلان دارند
    /// </summary>
    public enum NotificationEventType : byte
    {
        /// <summary>
        /// تخصیص تسک جدید به کاربر
        /// </summary>
        TaskAssigned = 1,

        /// <summary>
        /// تکمیل تسک توسط کاربر
        /// </summary>
        TaskCompleted = 2,

        /// <summary>
        /// یادآوری سررسید تسک
        /// </summary>
        TaskDeadlineReminder = 3,

        /// <summary>
        /// ثبت کامنت جدید در تسک
        /// </summary>
        TaskCommentAdded = 4,

        /// <summary>
        /// ویرایش تسک
        /// </summary>
        TaskUpdated = 5,

        /// <summary>
        /// تکمیل عملیات تسک
        /// </summary>
        TaskOperationCompleted = 6,

        /// <summary>
        /// حذف تسک
        /// </summary>
        TaskDeleted = 7,

        /// <summary>
        /// تایید یا رد تسک
        /// </summary>
        TaskStatusChanged = 8,

        /// <summary>
        /// تخصیص عملیات به کاربر
        /// </summary>
        OperationAssigned = 9,

        /// <summary>
        /// Mention در کامنت
        /// </summary>
        CommentMentioned = 10,

        /// <summary>
        /// تغییر اولویت تسک
        /// </summary>
        TaskPriorityChanged = 11,

        /// <summary>
        /// تخصیص مجدد تسک
        /// </summary>
        TaskReassigned = 12,

        /// <summary>
        /// اعلان روزانه تسک‌های انجام نشده
        /// </summary>
        DailyTaskDigest = 13,
        /// <summary>
        /// ثبت کار در تسک
        /// </summary>
        TaskWorkLog = 14
    }

    /// <summary>
    /// کانال‌های ارسال اعلان
    /// </summary>
    public enum NotificationChannel : byte
    {
        /// <summary>
        /// اعلان سیستمی (همیشه ارسال می‌شود)
        /// </summary>
        System = 0,

        /// <summary>
        /// ایمیل
        /// </summary>
        Email = 1,

        /// <summary>
        /// پیامک (SMS)
        /// </summary>
        Sms = 2,

        /// <summary>
        /// تلگرام
        /// </summary>
        Telegram = 3
    }
}