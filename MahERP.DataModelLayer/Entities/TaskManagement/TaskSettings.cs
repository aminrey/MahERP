using MahERP.DataModelLayer.Entities.AcControl;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// ⭐⭐⭐ تنظیمات دسترسی تسک - تعیین کننده اینکه چه کسانی چه کارهایی می‌توانند انجام دهند
    /// </summary>
    [Table("TaskSettings_Tbl")]
    public class TaskSettings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تسک
        /// </summary>
        [Required]
        public int TaskId { get; set; }

        #region تنظیمات دسترسی (Comma-Separated: "a,b,c,d,e")

        /// <summary>
        /// ⭐⭐⭐ تنظیم 0: چه کسانی می‌توانند تنظیمات را تغییر دهند
        /// پیش‌فرض: مدیر و سازنده (a,b)
        /// نکته: هر نقش فقط می‌تواند نقش‌های پایین‌تر از خود را مدیریت کند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanEditSettingsRoles { get; set; } = "a,b";

        /// <summary>
        /// تنظیم 1: چه کسانی می‌توانند کامنت بزنند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanCommentRoles { get; set; } = "a,b,c,d,e";

        /// <summary>
        /// تنظیم 2: چه کسانی می‌توانند عضو جدید اضافه کنند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanAddMembersRoles { get; set; } = "a,b";

        /// <summary>
        /// تنظیم 3: چه کسانی می‌توانند عضو حذف کنند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanRemoveMembersRoles { get; set; } = "a,b";

        /// <summary>
        /// تنظیم 4: پس از تکمیل تسک، چه کسانی می‌توانند ویرایش کنند
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CanEditAfterCompletionRoles { get; set; } = "a,b";

        /// <summary>
        /// تنظیم 5: آیا سازنده می‌تواند تسک را ویرایش/حذف کند؟
        /// </summary>
        [Required]
        public bool CreatorCanEditDelete { get; set; } = false;

        #endregion

        #region متادیتا

        /// <summary>
        /// آیا از تنظیمات پیش‌فرض استفاده می‌کند؟
        /// </summary>
        [Required]
        public bool IsInherited { get; set; } = true;

        /// <summary>
        /// منبع وراثت: 0=Global, 1=Branch, 2=Category
        /// </summary>
        public byte? InheritedFrom { get; set; }

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

        [ForeignKey(nameof(TaskId))]
        public virtual Tasks Task { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUsers CreatedByUser { get; set; }

        [ForeignKey(nameof(LastModifiedByUserId))]
        public virtual AppUsers? LastModifiedByUser { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ⭐⭐⭐ بررسی اینکه آیا نقش مشخص شده می‌تواند تنظیمات را تغییر دهد
        /// </summary>
        public bool CanEditSettings(TaskRole userRole) => HasRole(CanEditSettingsRoles, userRole);

        /// <summary>
        /// بررسی اینکه آیا نقش مشخص شده می‌تواند کامنت بزند
        /// </summary>
        public bool CanComment(TaskRole userRole) => HasRole(CanCommentRoles, userRole);

        /// <summary>
        /// بررسی اینکه آیا نقش مشخص شده می‌تواند عضو اضافه کند
        /// </summary>
        public bool CanAddMembers(TaskRole userRole) => HasRole(CanAddMembersRoles, userRole);

        /// <summary>
        /// بررسی اینکه آیا نقش مشخص شده می‌تواند عضو حذف کند
        /// </summary>
        public bool CanRemoveMembers(TaskRole userRole) => HasRole(CanRemoveMembersRoles, userRole);

        /// <summary>
        /// بررسی اینکه آیا نقش مشخص شده می‌تواند پس از اتمام ویرایش کند
        /// </summary>
        public bool CanEditAfterCompletion(TaskRole userRole) => HasRole(CanEditAfterCompletionRoles, userRole);

        /// <summary>
        /// بررسی وجود نقش در لیست
        /// </summary>
        private bool HasRole(string roles, TaskRole userRole)
        {
            if (string.IsNullOrWhiteSpace(roles)) return false;
            
            var rolesList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();
            
            return rolesList.Contains(GetRoleCode(userRole));
        }

        /// <summary>
        /// تبدیل enum به کد نقش
        /// </summary>
        private string GetRoleCode(TaskRole role) => role switch
        {
            TaskRole.HierarchyManager => "a",  // ⭐ جدید: 50
            TaskRole.Manager => "a",            // 100 (مدیر)
            TaskRole.Creator => "b",            // 200
            TaskRole.Member => "c",             // 300
            TaskRole.Supervisor => "d",         // 400
            TaskRole.CarbonCopy => "e",         // 500
            _ => string.Empty
        };

        /// <summary>
        /// اضافه کردن نقش به لیست
        /// </summary>
        public void AddRole(string settingName, TaskRole role)
        {
            var roleCode = GetRoleCode(role);
            var currentRoles = settingName switch
            {
                nameof(CanCommentRoles) => CanCommentRoles,
                nameof(CanAddMembersRoles) => CanAddMembersRoles,
                nameof(CanRemoveMembersRoles) => CanRemoveMembersRoles,
                nameof(CanEditAfterCompletionRoles) => CanEditAfterCompletionRoles,
                nameof(CanEditSettingsRoles) => CanEditSettingsRoles,
                _ => string.Empty
            };

            var rolesList = currentRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();

            if (!rolesList.Contains(roleCode))
            {
                rolesList.Add(roleCode);
                var newValue = string.Join(",", rolesList.OrderBy(r => r));

                switch (settingName)
                {
                    case nameof(CanCommentRoles):
                        CanCommentRoles = newValue;
                        break;
                    case nameof(CanAddMembersRoles):
                        CanAddMembersRoles = newValue;
                        break;
                    case nameof(CanRemoveMembersRoles):
                        CanRemoveMembersRoles = newValue;
                        break;
                    case nameof(CanEditAfterCompletionRoles):
                        CanEditAfterCompletionRoles = newValue;
                        break;
                    case nameof(CanEditSettingsRoles):
                        CanEditSettingsRoles = newValue;
                        break;
                }
            }
        }

        /// <summary>
        /// حذف نقش از لیست
        /// </summary>
        public void RemoveRole(string settingName, TaskRole role)
        {
            var roleCode = GetRoleCode(role);
            var currentRoles = settingName switch
            {
                nameof(CanCommentRoles) => CanCommentRoles,
                nameof(CanAddMembersRoles) => CanAddMembersRoles,
                nameof(CanRemoveMembersRoles) => CanRemoveMembersRoles,
                nameof(CanEditAfterCompletionRoles) => CanEditAfterCompletionRoles,
                nameof(CanEditSettingsRoles) => CanEditSettingsRoles,
                _ => string.Empty
            };

            var rolesList = currentRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => r != roleCode)
                .ToList();

            var newValue = string.Join(",", rolesList.OrderBy(r => r));

            switch (settingName)
            {
                case nameof(CanCommentRoles):
                    CanCommentRoles = newValue;
                    break;
                case nameof(CanAddMembersRoles):
                    CanAddMembersRoles = newValue;
                    break;
                case nameof(CanRemoveMembersRoles):
                    CanRemoveMembersRoles = newValue;
                    break;
                case nameof(CanEditAfterCompletionRoles):
                    CanEditAfterCompletionRoles = newValue;
                    break;
                case nameof(CanEditSettingsRoles):
                    CanEditSettingsRoles = newValue;
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// نوع عملیات برای بررسی دسترسی
    /// </summary>
    public enum TaskAction
    {
        Comment = 1,
        AddMember = 2,
        RemoveMember = 3,
        EditAfterCompletion = 4,
        EditOrDelete = 5
    }
}
