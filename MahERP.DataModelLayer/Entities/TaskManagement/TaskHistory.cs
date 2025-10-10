using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// تاریخچه تغییرات تسک - نسخه گسترش یافته
    /// </summary>
    public class TaskHistory
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        /// <summary>
        /// نوع تغییر:
        /// 0 - ایجاد تسک
        /// 1 - ویرایش تسک
        /// 2 - تغییر وضعیت
        /// 3 - اضافه کردن کاربر
        /// 4 - حذف کاربر
        /// 5 - اضافه کردن عملیات
        /// 6 - ویرایش عملیات
        /// 7 - تکمیل عملیات
        /// 8 - حذف عملیات
        /// 9 - ثبت گزارش کار روی عملیات
        /// 10 - افزودن یادآوری
        /// 11 - ویرایش یادآوری
        /// 12 - حذف یادآوری
        /// 13 - افزودن پیوست
        /// 14 - حذف پیوست
        /// 15 - تایید تسک (Supervisor)
        /// 16 - تایید تسک (Manager)
        /// 17 - رد تسک
        /// </summary>
        [Required]
        public byte ActionType { get; set; }

        /// <summary>
        /// عنوان تغییر (برای نمایش سریع)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات کامل تغییر
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// شناسه آیتم مرتبط (مثل OperationId، WorkLogId، ReminderId)
        /// </summary>
        public int? RelatedItemId { get; set; }

        /// <summary>
        /// نوع آیتم مرتبط (TaskOperation, WorkLog, Reminder, etc.)
        /// </summary>
        [MaxLength(100)]
        public string RelatedItemType { get; set; }

        /// <summary>
        /// اطلاعات قبلی (JSON)
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// اطلاعات جدید (JSON)
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// تاریخ انجام تغییر
        /// </summary>
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// IP کاربر (اختیاری)
        /// </summary>
        [MaxLength(50)]
        public string UserIp { get; set; }

        /// <summary>
        /// User Agent (اختیاری)
        /// </summary>
        [MaxLength(500)]
        public string UserAgent { get; set; }
    }
    /// <summary>
    /// انواع تغییرات قابل ثبت در تاریخچه تسک
    /// </summary>
    public enum TaskHistoryActionType : byte
    {
        /// <summary>0 - ایجاد تسک</summary>
        TaskCreated = 0,

        /// <summary>1 - ویرایش تسک</summary>
        TaskEdited = 1,

        /// <summary>2 - تغییر وضعیت</summary>
        StatusChanged = 2,

        /// <summary>3 - اضافه کردن کاربر</summary>
        UserAssigned = 3,

        /// <summary>4 - حذف کاربر</summary>
        UserRemoved = 4,

        /// <summary>5 - اضافه کردن عملیات</summary>
        OperationAdded = 5,

        /// <summary>6 - ویرایش عملیات</summary>
        OperationEdited = 6,

        /// <summary>7 - تکمیل عملیات</summary>
        OperationCompleted = 7,

        /// <summary>8 - حذف عملیات</summary>
        OperationDeleted = 8,

        /// <summary>9 - ثبت گزارش کار روی عملیات</summary>
        WorkLogAdded = 9,

        /// <summary>10 - افزودن یادآوری</summary>
        ReminderAdded = 10,

        /// <summary>11 - ویرایش یادآوری</summary>
        ReminderEdited = 11,

        /// <summary>12 - حذف یادآوری</summary>
        ReminderDeleted = 12,

        /// <summary>13 - افزودن پیوست</summary>
        AttachmentAdded = 13,

        /// <summary>14 - حذف پیوست</summary>
        AttachmentDeleted = 14,

        /// <summary>15 - تایید سرپرست</summary>
        SupervisorApproved = 15,

        /// <summary>16 - تایید مدیر</summary>
        ManagerApproved = 16,

        /// <summary>17 - رد تسک</summary>
        TaskRejected = 17
    }
}
