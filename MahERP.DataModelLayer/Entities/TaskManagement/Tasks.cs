using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد یکتای تسک (عدد 7 رقمی)
        /// </summary>
        [Required(ErrorMessage = "کد تسک الزامی است")]
        //[MaxLength(7)]
        public string TaskCode { get; set; }

        /// <summary>
        /// 0- رزرو
        /// 1- تسک جهت اجرا
        /// 2- جهت اطلاع رسانی
        /// 3- تسک برای اجرا رزرو
        /// </summary>
        public byte TaskType { get; set; }

        /// <summary>
        /// 0- رزرو 
        /// 1- کاربر عادی نرم افزار ساخته 
        /// 2- اتوماتیک ساخته شده 
        /// 3- مشتری ساخته 
        /// </summary>
        public byte TaskTypeInput { get; set; }

        /// <summary>
        /// سطح دسترسی تسک
        /// 0- محرمانه (فقط سازنده و افراد اضافه شده به تسک)
        /// 1- خطی (کاربر تسک و مدیران بالادست)
        /// 2- تیمی (قابل مشاهده توسط تمام اعضای تیم)
        /// 3- آزاد (قابل مشاهده توسط همه)
        /// </summary>
        public byte VisibilityLevel { get; set; }

        /// <summary>
        /// آیا این تسک خصوصی است؟ (فقط سازنده و انجام‌دهنده می‌توانند ببینند)
        /// </summary>
        public bool IsPrivate { get; set; } = false;

        /// <summary>
        /// سطح نمایش تسک
        /// 0 = عمومی (طبق قوانین سیستم)
        /// 1 = خصوصی (فقط سازنده و انجام‌دهندگان)
        /// </summary>
        public byte DisplayLevel { get; set; } = 0;

        /// <summary>
        /// شناسه تیم (در صورتی که تسک تیمی باشد)
        /// </summary>
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        [Required(ErrorMessage = "عنوان تسک را وارد کنید")]
        public string Title { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// 0- عادی
        /// 1- مهم
        /// 2- فوری
        /// </summary>
        public byte Priority { get; set; }

        public bool Important { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        

        public bool? SupervisorApproved { get; set; }
        
        public DateTime? SupervisorApprovedDate { get; set; }
        
        public bool? ManagerApproved { get; set; }
        
        public DateTime? ManagerApprovedDate { get; set; }

        public string? Location { get; set; }

        /// <summary>
        /// وضعیت کلی تسک
        /// 0- ایجاد شده
        /// 1- در حال انجام
        /// 2- تکمیل شده
        /// 3- تأیید شده
        /// 4- رد شده
        /// 5- در انتظار
        /// </summary>
        public byte Status { get; set; }
        /// <summary>
        /// شناسه زمان‌بندی که این تسک را ایجاد کرده (برای تسک‌های خودکار)
        /// </summary>
        public int? ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual TaskSchedule? TaskSchedule { get; set; }

        /// <summary>
        /// 0- دستی
        /// 1- خودکار (ایجاد شده توسط زمان‌بندی)
        /// </summary>
        public byte CreationMode { get; set; }
  
        public int? ParentTaskId { get; set; }
        [ForeignKey("ParentTaskId")]
        public virtual Tasks? ParentTask { get; set; }

        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        public string? CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers? Creator { get; set; }

        /// <summary>
        /// اطلاعات کاربر حذف شده که به این تسک منتصب بوده (یوزرنیم، نام و نام خانوادگی)
        /// در صورتی که کاربر بطور کامل حذف شود، این فیلد پر می‌شود
        /// </summary>
        public string? DeletedUserInfo { get; set; }

        public int? StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder? Stakeholder { get; set; }

        public int? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        public int? TaskCategoryId { get; set; }
        [ForeignKey("TaskCategoryId")]
        public virtual TaskCategory? TaskCategory { get; set; }

        public bool IsActive { get; set; }

        public bool IsFavorite { get; set; }
        
        public bool IsArchived { get; set; }
        
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        // Navigation properties
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new HashSet<TaskAssignment>();
        public virtual ICollection<TaskComment> TaskComments { get; set; } = new HashSet<TaskComment>();
        public virtual ICollection<TaskAttachment> TaskAttachments { get; set; } = new HashSet<TaskAttachment>();
        public virtual ICollection<TaskNotification> TaskNotifications { get; set; } = new HashSet<TaskNotification>();
        public virtual ICollection<TaskOperation> TaskOperations { get; set; } = new HashSet<TaskOperation>();
        public virtual ICollection<TaskViewer> TaskViewers { get; set; } = new HashSet<TaskViewer>();
        /// <summary>
        /// گزارش کارهای انجام شده روی این تسک
        /// </summary>
        public virtual ICollection<TaskWorkLog> TaskWorkLogs { get; set; } = new HashSet<TaskWorkLog>();
    }
}

