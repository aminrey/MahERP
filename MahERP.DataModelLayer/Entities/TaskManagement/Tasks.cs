using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
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
        
        /// <summary>
        /// تاریخ شروع پیشنهادی توسط سازنده تسک
        /// </summary>
        [Display(Name = "تاریخ شروع پیشنهادی")]
        public DateTime? SuggestedStartDate { get; set; }

        /// <summary>
        /// مدت زمان تخمینی انجام (به ساعت)
        /// </summary>
        [Display(Name = "مدت زمان تخمینی (ساعت)")]
        public decimal? EstimatedHours { get; set; }

        /// <summary>
        /// آیا مهلت سخت است؟
        /// </summary>
        [Display(Name = "مهلت سخت")]
        public bool IsHardDeadline { get; set; }

        /// <summary>
        /// یادداشت زمان‌بندی
        /// </summary>
        [Display(Name = "یادداشت زمان‌بندی")]
        [MaxLength(500)]
        public string? TimeNote { get; set; }

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
        /// 0- دستی
        /// 1- خودکار (ایجاد شده توسط زمان‌بندی)
        /// </summary>
        public byte CreationMode { get; set; }

        /// <summary>
        /// ⭐⭐⭐ نوع تکمیل تسک (false=مشترک, true=مستقل)
        /// </summary>
        [Display(Name = "نوع تکمیل تسک")]
        public bool IsIndependentCompletion { get; set; } = false;
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

        // ⭐ حفظ فیلد قدیمی برای backward compatibility
        [Obsolete("Use ContactId and OrganizationId instead")]
        public int? StakeholderId { get; set; }
        
        // ⭐⭐⭐ فیلدهای جدید
        /// <summary>
        /// ⭐⭐⭐ شناسه فرد (Contact) مرتبط با تسک
        /// برای تسک‌هایی که یک فرد خاص طرف حساب است
        /// </summary>
        [Display(Name = "شناسه فرد")]
        public int? ContactId { get; set; }

        /// <summary>
        /// ⭐⭐⭐ شناسه سازمان (Organization) مرتبط با تسک
        /// برای تسک‌هایی که یک شرکت/سازمان طرف حساب است
        /// </summary>
        [Display(Name = "شناسه سازمان")]
        public int? OrganizationId { get; set; }
        
        // Navigation Properties
        /// <summary>
        /// فرد مرتبط با تسک (نسخه جدید)
        /// </summary>
        [ForeignKey("ContactId")]
        public virtual Contact Contact { get; set; }
        
        /// <summary>
        /// سازمان مرتبط با تسک (نسخه جدید)
        /// </summary>
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

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
        /// ⭐⭐⭐ نوع تکمیل تسک
        /// 0 = مشترک (Shared): یک نفر تکمیل کند، برای همه تکمیل می‌شود
        /// 1 = مستقل (Independent): هر نفر باید جداگانه تکمیل کند
        /// </summary>
        [Display(Name = "نوع تکمیل تسک")]
        public byte CompletionMode { get; set; } = 0; // پیش‌فرض: مشترک

        /// <summary>
        /// ⭐⭐⭐ شناسه زمان‌بندی (Schedule) که این تسک از آن ساخته شده
        /// null = تسک دستی
        /// مقدار = تسک خودکار از Schedule
        /// </summary>
        [Display(Name = "شناسه زمان‌بندی")]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Navigation Property به زمان‌بندی
        /// </summary>
        [ForeignKey("ScheduleId")]
        public virtual ScheduledTaskCreation? Schedule { get; set; }

        // ========== ⭐⭐⭐ CRM Integration Fields ==========

        /// <summary>
        /// ماژول منبع که این تسک را ایجاد کرده
        /// 0 = System, 1 = Tasking, 2 = CRM, 3 = Accounting, ...
        /// </summary>
        [Display(Name = "ماژول منبع")]
        public ModuleSourceType SourceModule { get; set; } = ModuleSourceType.Tasking;

        /// <summary>
        /// نوع منبع CRM (اگر از CRM آمده باشد)
        /// 0 = Manual, 1 = LeadFollowUp, 2 = OpportunityNextAction, ...
        /// </summary>
        [Display(Name = "نوع منبع CRM")]
        public CrmTaskSourceType? CrmSourceType { get; set; }

        /// <summary>
        /// شناسه سرنخ CRM مرتبط
        /// </summary>
        [Display(Name = "سرنخ CRM")]
        public int? CrmLeadId { get; set; }

        /// <summary>
        /// Navigation به CrmLead
        /// </summary>
        [ForeignKey("CrmLeadId")]
        public virtual CrmLead? CrmLead { get; set; }

        /// <summary>
        /// شناسه فرصت فروش CRM مرتبط (آینده)
        /// </summary>
        [Display(Name = "فرصت فروش CRM")]
        public int? CrmOpportunityId { get; set; }

        /// <summary>
        /// شناسه پیگیری CRM مرتبط
        /// </summary>
        [Display(Name = "پیگیری CRM")]
        public int? CrmFollowUpId { get; set; }

        /// <summary>
        /// Navigation به CrmFollowUp
        /// </summary>
        [ForeignKey("CrmFollowUpId")]
        public virtual CrmFollowUp? CrmFollowUp { get; set; }

        /// <summary>
        /// شناسه تیکت پشتیبانی CRM مرتبط (آینده)
        /// </summary>
        [Display(Name = "تیکت CRM")]
        public int? CrmTicketId { get; set; }

        /// <summary>
        /// شناسه قرارداد CRM مرتبط (آینده)
        /// </summary>
        [Display(Name = "قرارداد CRM")]
        public int? CrmContractId { get; set; }

        /// <summary>
        /// شناسه مشتری CRM مرتبط (آینده)
        /// </summary>
        [Display(Name = "مشتری CRM")]
        public int? CrmCustomerId { get; set; }

        // ========== END CRM Integration Fields ==========

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

        /// <summary>
        /// ⭐⭐⭐ ناظران رونوشت شده (دستی) این تسک
        /// </summary>
        public virtual ICollection<TaskCarbonCopy> CarbonCopies { get; set; } = new HashSet<TaskCarbonCopy>();
    }
}

