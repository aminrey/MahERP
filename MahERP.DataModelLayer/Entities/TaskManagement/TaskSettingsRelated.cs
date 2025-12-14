using MahERP.DataModelLayer.Entities.AcControl;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// ⭐⭐⭐ تنظیمات پیش‌فرض دسته‌بندی تسک
    /// </summary>
    [Table("TaskCategoryDefaultSettings_Tbl")]
    public class TaskCategoryDefaultSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskCategoryId { get; set; }

        #region تنظیمات پیش‌فرض

        /// <summary>
        /// ⭐⭐⭐ تنظیم 0: چه کسانی می‌توانند تنظیمات را تغییر دهند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanEditSettingsRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanCommentRoles { get; set; } = "a,b,c,d,e";

        [Required]
        [MaxLength(50)]
        public string CanAddMembersRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanRemoveMembersRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanEditAfterCompletionRoles { get; set; } = "a,b";

        [Required]
        public bool CreatorCanEditDelete { get; set; } = false;

        #endregion

        #region متادیتا

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(450)]
        public string? LastModifiedByUserId { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        #endregion

        #region Navigation Properties

        [ForeignKey(nameof(TaskCategoryId))]
        public virtual TaskCategory TaskCategory { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers CreatedByUser { get; set; }

        [ForeignKey(nameof(LastModifiedByUserId))]
        public virtual AppUsers? LastModifiedByUser { get; set; }

        #endregion

        /// <summary>
        /// کپی تنظیمات به TaskSettings
        /// </summary>
        public TaskSettings ToTaskSettings(int taskId, string createdByUserId)
        {
            return new TaskSettings
            {
                TaskId = taskId,
                CanEditSettingsRoles = this.CanEditSettingsRoles,
                CanCommentRoles = this.CanCommentRoles,
                CanAddMembersRoles = this.CanAddMembersRoles,
                CanRemoveMembersRoles = this.CanRemoveMembersRoles,
                CanEditAfterCompletionRoles = this.CanEditAfterCompletionRoles,
                CreatorCanEditDelete = this.CreatorCanEditDelete,
                IsInherited = true,
                InheritedFrom = 2, // Category
                CreatedByUserId = createdByUserId,
                CreatedDate = DateTime.Now
            };
        }
    }

    /// <summary>
    /// ⭐⭐⭐ تنظیمات پیش‌فرض شعبه
    /// </summary>
    [Table("BranchDefaultTaskSettings_Tbl")]
    public class BranchDefaultTaskSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        #region تنظیمات پیش‌فرض

        /// <summary>
        /// ⭐⭐⭐ تنظیم 0: چه کسانی می‌توانند تنظیمات را تغییر دهند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanEditSettingsRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanCommentRoles { get; set; } = "a,b,c,d,e";

        [Required]
        [MaxLength(50)]
        public string CanAddMembersRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanRemoveMembersRoles { get; set; } = "a,b";

        [Required]
        [MaxLength(50)]
        public string CanEditAfterCompletionRoles { get; set; } = "a,b";

        [Required]
        public bool CreatorCanEditDelete { get; set; } = false;

        #endregion

        #region متادیتا

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(450)]
        public string? LastModifiedByUserId { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        #endregion

        #region Navigation Properties

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers CreatedByUser { get; set; }

        [ForeignKey(nameof(LastModifiedByUserId))]
        public virtual AppUsers? LastModifiedByUser { get; set; }

        #endregion

        /// <summary>
        /// کپی تنظیمات به TaskSettings
        /// </summary>
        public TaskSettings ToTaskSettings(int taskId, string createdByUserId)
        {
            return new TaskSettings
            {
                TaskId = taskId,
                CanEditSettingsRoles = this.CanEditSettingsRoles,
                CanCommentRoles = this.CanCommentRoles,
                CanAddMembersRoles = this.CanAddMembersRoles,
                CanRemoveMembersRoles = this.CanRemoveMembersRoles,
                CanEditAfterCompletionRoles = this.CanEditAfterCompletionRoles,
                CreatorCanEditDelete = this.CreatorCanEditDelete,
                IsInherited = true,
                InheritedFrom = 1, // Branch
                CreatedByUserId = createdByUserId,
                CreatedDate = DateTime.Now
            };
        }
    }

    /// <summary>
    /// ⭐⭐⭐ لاگ تغییرات تنظیمات تسک
    /// </summary>
    [Table("TaskSettingsChangeLog_Tbl")]
    public class TaskSettingsChangeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        /// <summary>
        /// نوع تنظیم: 1-5
        /// </summary>
        [Required]
        public byte SettingType { get; set; }

        /// <summary>
        /// مقدار قبلی (JSON)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// مقدار جدید (JSON)
        /// </summary>
        [Required]
        public string NewValue { get; set; }

        [Required]
        [MaxLength(450)]
        public string ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? ChangeReason { get; set; }

        #region Navigation Properties

        [ForeignKey(nameof(TaskId))]
        public virtual Tasks Task { get; set; }

        [ForeignKey(nameof(ChangedByUserId))]
        public virtual AppUsers ChangedByUser { get; set; }

        #endregion

        /// <summary>
        /// دریافت نام تنظیم
        /// </summary>
        public string GetSettingName() => SettingType switch
        {
            1 => "کامنت‌گذاری",
            2 => "افزودن عضو",
            3 => "حذف عضو",
            4 => "ویرایش پس از اتمام",
            5 => "حذف/ویرایش توسط سازنده",
            _ => "نامشخص"
        };
    }
}
