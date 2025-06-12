using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// کاربران مجاز به مشاهده تسک (بدون دسترسی ویرایش)
    /// لیست اشتراک و نمایش
    /// </summary>
    public class TaskViewer
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
        /// نوع دسترسی
        /// 0- اضافه شده دستی
        /// 1- دسترسی بر اساس سلسله مراتب (مدیر)
        /// 2- دسترسی بر اساس عضویت در تیم
        /// 3- دسترسی عمومی
        /// </summary>
        public byte AccessType { get; set; }

        /// <summary>
        /// شناسه تیم مرتبط (اگر AccessType برابر با 2 باشد)
        /// </summary>
        public int? TeamId { get; set; }

        public string AddedByUserId { get; set; }
        [ForeignKey("AddedByUserId")]
        public virtual AppUsers AddedByUser { get; set; }

        public DateTime AddedDate { get; set; }

        /// <summary>
        /// آیا کاربر این تسک را مشاهده کرده است
        /// </summary>
        public bool IsViewed { get; set; }

        /// <summary>
        /// تاریخ مشاهده تسک
        /// </summary>
        public DateTime? ViewDate { get; set; }

        /// <summary>
        /// توضیحات اضافی درباره دلیل اضافه شدن کاربر به لیست مشاهده کنندگان
        /// </summary>
        public string Description { get; set; }
    }
}

