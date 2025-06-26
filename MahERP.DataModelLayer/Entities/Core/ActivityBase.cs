using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Core
{
    /// <summary>
    /// جدول پایه فعالیت‌ها - هسته مرکزی سیستم
    /// </summary>
    public class ActivityBase
    {
        public ActivityBase()
        {
            ActivityTasks = new HashSet<ActivityTask>();
            ActivityCRMs = new HashSet<ActivityCRM>();
            ActivityAttachments = new HashSet<ActivityAttachment>();
            ActivityComments = new HashSet<ActivityComment>();
            ActivityHistories = new HashSet<ActivityHistory>();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// عنوان فعالیت
        /// </summary>
        [Required(ErrorMessage = "عنوان فعالیت الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات کامل فعالیت
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// نوع فعالیت
        /// 0- عمومی
        /// 1- تسکینگ
        /// 2- CRM
        /// 3- مالی
        /// 4- منابع انسانی
        /// 5- پروژه
        /// </summary>
        public byte ActivityType { get; set; }

        /// <summary>
        /// اولویت فعالیت
        /// 0- عادی
        /// 1- متوسط
        /// 2- مهم
        /// 3- بحرانی
        /// </summary>
        public byte Priority { get; set; }

        /// <summary>
        /// وضعیت فعالیت
        /// 0- ایجاد شده
        /// 1- در حال انجام
        /// 2- تکمیل شده
        /// 3- تایید شده
        /// 4- رد شده
        /// 5- لغو شده
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// طرف حساب مرتبط (اختیاری)
        /// </summary>
        public int? StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder? Stakeholder { get; set; }

        /// <summary>
        /// قرارداد مرتبط (اختیاری)
        /// </summary>
        public int? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        /// <summary>
        /// شعبه مرتبط
        /// </summary>
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// تاریخ شروع فعالیت
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان فعالیت
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// مهلت انجام فعالیت
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// درصد پیشرفت فعالیت
        /// </summary>
        [Range(0, 100)]
        public int ProgressPercentage { get; set; } = 0;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// کاربری که آخرین بروزرسانی را انجام داده
        /// </summary>
        public string? LastUpdaterUserId { get; set; }
        [ForeignKey("LastUpdaterUserId")]
        public virtual AppUsers? LastUpdater { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// آیا رکورد حذف شده است (حذف منطقی)
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        /// <summary>
        /// ارتباطات با تسک‌ها
        /// </summary>
        public virtual ICollection<ActivityTask> ActivityTasks { get; set; }
        
        /// <summary>
        /// ارتباطات با تعاملات CRM
        /// </summary>
        public virtual ICollection<ActivityCRM> ActivityCRMs { get; set; }
        
        /// <summary>
        /// پیوست‌های مرتبط با فعالیت
        /// </summary>
        public virtual ICollection<ActivityAttachment> ActivityAttachments { get; set; }
        
        /// <summary>
        /// کامنت‌های ثبت شده برای فعالیت
        /// </summary>
        public virtual ICollection<ActivityComment> ActivityComments { get; set; }
        
        /// <summary>
        /// تاریخچه تغییرات فعالیت
        /// </summary>
        public virtual ICollection<ActivityHistory> ActivityHistories { get; set; }
    }
}
