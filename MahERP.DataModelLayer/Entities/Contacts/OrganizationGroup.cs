
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Entities.Organizations
{
    /// <summary>
    /// گروه‌بندی سازمان‌ها در سطح کل سیستم
    /// برای دسته‌بندی سازمان‌ها به گروه‌های مختلف (مشتریان، تامین‌کنندگان، شرکا و...)
    /// </summary>
    [Table("OrganizationGroup_Tbl")]
    [Index(nameof(Code), IsUnique = true, Name = "IX_OrganizationGroup_Code")]
    public class OrganizationGroup
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// کد گروه (یکتا) - مثل: "CUSTOMERS", "SUPPLIERS", "PARTNERS"
        /// </summary>
        [Required(ErrorMessage = "کد گروه الزامی است")]
        [MaxLength(50)]
        public string Code { get; set; }

        /// <summary>
        /// عنوان گروه
        /// </summary>
        [Required(ErrorMessage = "عنوان گروه الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات گروه
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// رنگ نمایش در UI (HEX)
        /// </summary>
        [MaxLength(7)]
        public string? ColorHex { get; set; }

        /// <summary>
        /// آیکون گروه (FontAwesome class)
        /// </summary>
        [MaxLength(50)]
        public string? IconClass { get; set; }

        /// <summary>
        /// ترتیب نمایش در فیلترها
        /// </summary>
        public int DisplayOrder { get; set; } = 1;

        /// <summary>
        /// آیا گروه سیستمی است؟ (قابل حذف نیست)
        /// </summary>
        public bool IsSystemGroup { get; set; } = false;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== System Info ==========
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(450)]
        public string CreatorUserId { get; set; }

        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUsers? Creator { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [MaxLength(450)]
        public string? LastUpdaterUserId { get; set; }

        [ForeignKey(nameof(LastUpdaterUserId))]
        public virtual AppUsers? LastUpdater { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// اعضای این گروه
        /// </summary>
        public virtual ICollection<OrganizationGroupMember> Members { get; set; } = new HashSet<OrganizationGroupMember>();

        // ========== Computed Properties ==========

        [NotMapped]
        public int ActiveMembersCount => Members?.Count(m => m.IsActive) ?? 0;

        [NotMapped]
        public string DisplayColorHex => ColorHex ?? "#6c757d";

        [NotMapped]
        public string DisplayIconClass => IconClass ?? "fa-building";
    }
}