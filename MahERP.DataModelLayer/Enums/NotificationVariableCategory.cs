namespace MahERP.DataModelLayer.Enums
{
    /// <summary>
    /// دسته‌بندی متغیرهای قالب اعلان‌ها
    /// </summary>
    public enum NotificationVariableCategory
    {
        /// <summary>
        /// متغیرهای عمومی (تاریخ، زمان، ...)
        /// </summary>
        General = 0,

        /// <summary>
        /// اطلاعات کاربر گیرنده
        /// </summary>
        Recipient = 1,

        /// <summary>
        /// اطلاعات تسک خاص
        /// </summary>
        Task = 2,

        /// <summary>
        /// لیست تسک‌های انجام نشده
        /// </summary>
        TaskList = 3,

        /// <summary>
        /// اطلاعات کاربر ارسال‌کننده
        /// </summary>
        Sender = 4,

        /// <summary>
        /// اطلاعات یادآوری زمان‌بندی شده
        /// </summary>
        ReminderSchedule = 5
    }
}
