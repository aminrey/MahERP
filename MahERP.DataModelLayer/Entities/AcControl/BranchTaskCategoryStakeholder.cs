using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    /// <summary>
    /// انتصاب دسته‌بندی تسک به شعبه با در نظر گیری طرف حساب مشخص
    /// هر شعبه برای هر طرف حساب می‌تواند چندین دسته‌بندی تسک داشته باشد
    /// </summary>
    public class BranchTaskCategoryStakeholder
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه شعبه
        /// </summary>
        [Required(ErrorMessage = "شناسه شعبه الزامی است")]
        public int BranchId { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی تسک
        /// </summary>
        [Required(ErrorMessage = "شناسه دسته‌بندی تسک الزامی است")]
        public int TaskCategoryId { get; set; }

        /// <summary>
        /// شناسه طرف حساب (اجباری)
        /// </summary>
        [Required(ErrorMessage = "شناسه طرف حساب الزامی است")]
        public int StakeholderId { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ تخصیص
        /// </summary>
        [Required]
        public DateTime AssignDate { get; set; }

        /// <summary>
        /// شناسه کاربر تخصیص دهنده
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر تخصیص دهنده الزامی است")]
        [MaxLength(450)]
        public string AssignedByUserId { get; set; }

        // Navigation Properties
        /// <summary>
        /// شعبه مرتبط
        /// </summary>
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        /// <summary>
        /// دسته‌بندی تسک مرتبط
        /// </summary>
        [ForeignKey("TaskCategoryId")]
        public virtual TaskCategory TaskCategory { get; set; }

        /// <summary>
        /// طرف حساب مرتبط
        /// </summary>
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        /// <summary>
        /// کاربر تخصیص دهنده
        /// </summary>
        [ForeignKey("AssignedByUserId")]
        public virtual AppUsers AssignedByUser { get; set; }
    }
}