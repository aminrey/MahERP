using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// اطلاعات تکمیلی مشتری مختص ماژول CRM - اطلاعات فروش و بازاریابی مربوط به مشتری
    /// </summary>
    public class StakeholderCRM
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه طرف حساب مرتبط
        /// </summary>
        public int StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        /// <summary>
        /// منبع آشنایی با مشتری - نحوه آشنایی مشتری با سازمان
        /// 0- وب سایت
        /// 1- تبلیغات
        /// 2- معرفی
        /// 3- نمایشگاه
        /// 4- تماس مستقیم
        /// 5- سایر
        /// </summary>
        public byte LeadSource { get; set; }

        /// <summary>
        /// مرحله در چرخه فروش - وضعیت مشتری در فرآیند فروش
        /// 0- سرنخ اولیه
        /// 1- مذاکره
        /// 2- پیشنهاد قیمت
        /// 3- در حال قرارداد
        /// 4- مشتری فعال
        /// 5- مشتری غیرفعال
        /// </summary>
        public byte SalesStage { get; set; }

        /// <summary>
        /// تاریخ آخرین تماس با مشتری
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// ارزش بالقوه مشتری (به میلیون تومان) - ارزش مورد انتظار از مشتری
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PotentialValue { get; set; }

        /// <summary>
        /// رتبه‌بندی اعتباری مشتری (A, B, C, D)
        /// </summary>
        [MaxLength(1)]
        public string? CreditRating { get; set; }

        /// <summary>
        /// علاقه‌مندی‌ها و ترجیحات مشتری
        /// </summary>
        public string? Preferences { get; set; }

        /// <summary>
        /// صنعت یا حوزه فعالیت مشتری
        /// </summary>
        [MaxLength(100)]
        public string? Industry { get; set; }

        /// <summary>
        /// تعداد کارمندان شرکت مشتری
        /// </summary>
        public int? EmployeeCount { get; set; }

        /// <summary>
        /// گردش مالی سالانه (به میلیون تومان)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? AnnualRevenue { get; set; }

        /// <summary>
        /// کارشناس فروش اختصاصی - کارمندی که مسئول ارتباط با این مشتری است
        /// </summary>
        public string? SalesRepUserId { get; set; }
        [ForeignKey("SalesRepUserId")]
        public virtual AppUsers? SalesRep { get; set; }
        
        /// <summary>
        /// یادداشت‌های داخلی درباره مشتری - اطلاعاتی که فقط برای استفاده داخلی است
        /// </summary>
        public string? InternalNotes { get; set; }

        /// <summary>
        /// تاریخ ایجاد رکورد
        /// </summary>
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }
    }
}
