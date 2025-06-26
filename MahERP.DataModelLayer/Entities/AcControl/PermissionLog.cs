using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class PermissionLog
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUsers User { get; set; }

        public string Action { get; set; }

        public string Controller { get; set; }

        /// <summary>
        /// نوع عملیات
        /// 0- مشاهده
        /// 1- ایجاد
        /// 2- ویرایش
        /// 3- حذف
        /// 4- تایید
        /// 5- رد
        /// </summary>
        public byte ActionType { get; set; }

        public bool AccessGranted { get; set; }

        public string? DenialReason { get; set; }

        public DateTime ActionDate { get; set; }

        public string? RequestData { get; set; }

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }
    }
}