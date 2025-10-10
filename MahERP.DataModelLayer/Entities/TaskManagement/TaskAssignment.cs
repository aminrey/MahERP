using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organization;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskAssignment
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        /// <summary>
        /// کاربرانی که این تسک به آن‌ها تخصیص داده شده است.
        /// <para>این ویژگی شناسه کاربری را که تسک به او اختصاص داده شده نگهداری می‌کند.</para>
        /// </summary>
        public string? AssignedUserId { get; set; }
        [ForeignKey("AssignedUserId")]
        public virtual AppUsers? AssignedUser { get; set; }
        /// <summary>
        /// تیم هایی که این تسک به آن‌ها تخصیص داده شده است.
        /// <para>این ویژگی شناسه تیمی را که تسک به او اختصاص داده شده نگهداری می‌کند.</para>
        /// </summary>
        public int? AssignedTeamId { get; set; }
        [ForeignKey("AssignedTeamId")]
        public virtual Team? AssignedTeam { get; set; }

        /// <summary>
        /// کاربری که این تسک را به کاربر دیگر تخصیص داده است.
        /// <para>این ویژگی شناسه کاربری را که تسک را به کاربر دیگر اختصاص داده نگهداری می‌کند.</para>
        /// </summary>
        public string? AssignerUserId { get; set; }
        [ForeignKey("AssignerUserId")]
        public virtual AppUsers? AssignerUser { get; set; }

        /// <summary>
        /// اطلاعات کاربر حذف شده که این تسک به او تخصیص داده شده بود (یوزرنیم، نام و نام خانوادگی)
        /// در صورتی که کاربر تخصیص‌یافته بطور کامل حذف شود، این فیلد پر می‌شود
        /// </summary>
        public string? DeletedAssignedUserInfo { get; set; }

        /// <summary>
        /// اطلاعات کاربر حذف شده که این تسک را تخصیص داده بود (یوزرنیم، نام و نام خانوادگی)
        /// در صورتی که کاربر تخصیص‌دهنده بطور کامل حذف شود، این فیلد پر می‌شود
        /// </summary>
        public string? DeletedAssignerUserInfo { get; set; }

        /// <summary>
        /// 0- اصلی (اجراکننده)
        /// 1- رونوشت - سازنده تسک
        /// 2- ناظر 
        /// 
        /// viwer که در اینجا نیست قدرت تغییر ندارد و صرفا فقط بیننده است 
        /// <para>نوع تخصیص را مشخص می‌کند (اصلی، رونوشت، ناظر).</para>
        /// </summary>
        public byte AssignmentType { get; set; }

        /// <summary>
        /// توضیحات رونوشت (برای حالت رونوشت)
        /// <para>در صورت انتخاب حالت رونوشت، توضیحات مربوطه در این ویژگی ذخیره می‌شود.</para>
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// شناسه توضیح رونوشت پیش‌فرض (در صورت استفاده از توضیحات آماده)
        /// <para>در صورت استفاده از توضیحات آماده، شناسه آن در این ویژگی ذخیره می‌شود.</para>
        /// </summary>
        public int? PredefinedCopyDescriptionId { get; set; }
        [ForeignKey("PredefinedCopyDescriptionId")]
        public virtual PredefinedCopyDescription? PredefinedCopyDescription { get; set; }

        /// <summary>
        /// تاریخ تخصیص تسک به کاربر
        /// <para>زمانی که تسک به کاربر تخصیص داده می‌شود، این ویژگی مقداردهی می‌شود.</para>
        /// </summary>
        public DateTime AssignmentDate { get; set; }

        /// <summary>
        /// تاریخ شروع انجام تسک توسط کاربر
        /// <para>در صورتی که کاربر شروع به انجام تسک کند، این ویژگی مقداردهی می‌شود.</para>
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ سررسید انجام تسک
        /// <para>تاریخ پایان مهلت انجام تسک در این ویژگی ذخیره می‌شود.</para>
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// تاریخ تکمیل شدن تسک توسط کاربر
        /// <para>در صورت تکمیل تسک توسط کاربر، این ویژگی مقداردهی می‌شود.</para>
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// تاریخ شروع شخصی که کاربر برای خود تعیین کرده است
        /// <para>کاربر می‌تواند مستقل از تاریخ پیشنهادی سازنده، تاریخ شروع شخصی خود را تعیین کند</para>
        /// </summary>
        [Display(Name = "تاریخ شروع شخصی")]
        public DateTime? PersonalStartDate { get; set; }

        /// <summary>
        /// تاریخ پایان شخصی که کاربر برای خود تعیین کرده است
        /// <para>کاربر می‌تواند مستقل از ددلاین اصلی، تاریخ پایان شخصی خود را تعیین کند</para>
        /// </summary>
        [Display(Name = "تاریخ پایان شخصی")]
        public DateTime? PersonalEndDate { get; set; }

        /// <summary>
        /// یادداشت شخصی کاربر در مورد زمان‌بندی
        /// <para>کاربر می‌تواند توضیحات در مورد زمان‌بندی شخصی خود ارائه دهد</para>
        /// </summary>
        [Display(Name = "یادداشت زمان‌بندی شخصی")]
        [MaxLength(500)]
        public string? PersonalTimeNote { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی تاریخ‌های شخصی
        /// <para>زمانی که کاربر تاریخ‌های شخصی خود را تغییر می‌دهد، این فیلد بروزرسانی می‌شود</para>
        /// </summary>
        public DateTime? PersonalDatesUpdatedDate { get; set; }

        /// <summary>
        /// 0- تخصیص داده شده
        /// 1- مشاهده شده
        /// 2- در حال انجام
        /// 3- تکمیل شده
        /// 4- تایید شده توسط ناظر
        /// 5- تایید شده توسط مدیر
        /// 6- رد شده
        /// <para>وضعیت فعلی تخصیص تسک به کاربر را مشخص می‌کند.</para>
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// آیا این تسک به علاقه‌مندی‌های کاربر اضافه شده است یا خیر
        /// <para>در صورت علاقه‌مندی کاربر به تسک، این ویژگی مقدار true می‌گیرد.</para>
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// آیا این تسک در بخش "روز من" کاربر قرار دارد یا خیر
        /// <para>در صورت قرار گرفتن تسک در بخش "روز من"، این ویژگی مقدار true می‌گیرد.</para>
        /// </summary>
        public bool IsMyDay { get; set; }

        /// <summary>
        /// آیا کاربر این تسک را مشاهده کرده است یا خیر
        /// <para>در صورت مشاهده تسک توسط کاربر، این ویژگی مقدار true می‌گیرد.</para>
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// تاریخ مشاهده شدن تسک توسط کاربر
        /// <para>در صورت مشاهده تسک توسط کاربر، زمان مشاهده در این ویژگی ذخیره می‌شود.</para>
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// گزارش کاربر در رابطه با انجام تسک
        /// <para>کاربر می‌تواند گزارش مربوط به انجام تسک را در این ویژگی ثبت کند.</para>
        /// </summary>
        public string? UserReport { get; set; }

        /// <summary>
        /// تاریخ ثبت گزارش توسط کاربر
        /// <para>زمانی که کاربر گزارش را ثبت می‌کند، این ویژگی مقداردهی می‌شود.</para>
        /// </summary>
        public DateTime? ReportDate { get; set; }

        /// <summary>
        /// شناسه تیمی که کاربر در آن منتصب شده
        /// (کاربر ممکن است در چند تیم باشد، این مشخص می‌کند در کدام تیم این تسک را دارد)
        /// </summary>
        public int? AssignedInTeamId { get; set; }

        [ForeignKey("AssignedInTeamId")]
        public virtual Team? AssignedInTeam { get; set; }

        /// <summary>
        /// آیا این تسک در حال حاضر فوکوس کاربر است؟
        /// فقط یک تسک از تسک‌های کاربر می‌تواند فوکوس باشد
        /// </summary>
        public bool IsFocused { get; set; } = false;

        /// <summary>
        /// تاریخ فوکوس شدن تسک
        /// </summary>
        public DateTime? FocusedDate { get; set; }

        /// <summary>
        /// ⭐ رکوردهای "روز من" مرتبط با این assignment
        /// یک assignment می‌تواند در چند روز مختلف قرار بگیرد
        /// </summary>
        public virtual ICollection<TaskMyDay> MyDayRecords { get; set; } = new List<TaskMyDay>();
    }
}
