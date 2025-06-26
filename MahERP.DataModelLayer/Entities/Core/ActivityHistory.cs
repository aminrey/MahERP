using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// تاریخچه تغییرات انجام شده در فعالیت‌ها
    /// </summary>
    public class ActivityHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه فعالیت مرتبط
        /// </summary>
        public int ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual ActivityBase Activity { get; set; }

        /// <summary>
        /// نوع تغییر
        /// 0- ایجاد فعالیت
        /// 1- ویرایش اطلاعات
        /// 2- تغییر وضعیت
        /// 3- تغییر اولویت
        /// 4- اختصاص دادن به کاربر/تیم
        /// 5- افزودن کامنت
        /// 6- افزودن فایل
        /// 7- ایجاد ارتباط با تسک
        /// 8- ایجاد ارتباط با CRM
        /// </summary>
        public byte ChangeType { get; set; }

        /// <summary>
        /// توضیحات تغییر - شرح تغییرات انجام شده
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// مقدار قدیمی (در صورت نیاز) - برای ثبت تاریخچه تغییرات
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// مقدار جدید (در صورت نیاز) - برای ثبت تاریخچه تغییرات
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// تاریخ انجام تغییر
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده تغییر
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
    }
}
