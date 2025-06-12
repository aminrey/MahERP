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
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد یکتای فعالیت - شناسه قابل نمایش برای کاربر
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ActivityCode { get; set; }

        /// <summary>
        /// عنوان فعالیت
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات فعالیت - شرح کامل فعالیت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ماژول مربوطه
        /// 0- هیچکدام (فعالیت مستقل)
        /// 1- تسکینگ (مرتبط با وظایف)
        /// 2- CRM (مرتبط با مشتریان)
        /// 3- قراردادها (مرتبط با قراردادها)
        /// 4- سایر
        /// </summary>
        public byte ModuleType { get; set; }

        /// <summary>
        /// اولویت فعالیت
        /// 0- عادی
        /// 1- مهم
        /// 2- فوری
        /// </summary>
        public byte Priority { get; set; }

        /// <summary>
        /// وضعیت فعلی فعالیت
        /// 0- ایجاد شده
        /// 1- در حال انجام
        /// 2- تکمیل شده
        /// 3- لغو شده
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// تاریخ ایجاد فعالیت
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }

        /// <summary>
        /// مهلت انجام فعالیت
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// تاریخ تکمیل فعالیت
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// طرف حساب مرتبط با فعالیت
        /// </summary>
        public int? StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        /// <summary>
        /// قرارداد مرتبط با فعالیت
        /// </summary>
        public int? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        /// <summary>
        /// شعبه مربوط به فعالیت
        /// </summary>
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// آیا فعالیت حذف شده است (حذف منطقی)
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
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

        public ActivityBase()
        {
            ActivityTasks = new HashSet<ActivityTask>();
            ActivityCRMs = new HashSet<ActivityCRM>();
            ActivityAttachments = new HashSet<ActivityAttachment>();
            ActivityComments = new HashSet<ActivityComment>();
            ActivityHistories = new HashSet<ActivityHistory>();
        }
    }
}
