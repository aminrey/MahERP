using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    /// <summary>
    /// جدول واسط Many-to-Many بین Interaction و Goal
    /// یک تعامل می‌تواند برای چندین هدف باشد
    /// </summary>
    [Table("InteractionGoal_Tbl")]
    public class InteractionGoal
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه تعامل
        /// </summary>
        [Required]
        public int InteractionId { get; set; }

        [ForeignKey(nameof(InteractionId))]
        public virtual Interaction Interaction { get; set; } = null!;

        /// <summary>
        /// شناسه هدف
        /// </summary>
        [Required]
        public int GoalId { get; set; }

        [ForeignKey(nameof(GoalId))]
        public virtual Goal Goal { get; set; } = null!;

        /// <summary>
        /// یادداشت اختصاصی برای این هدف در این تعامل
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
