using MahERP.DataModelLayer.Entities.AcControl;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskCategory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان دسته‌بندی را وارد کنید")]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }
        [ForeignKey("ParentCategoryId")]
        public virtual TaskCategory ParentCategory { get; set; }
        
        /// <summary>
        /// اولویت نمایش
        /// </summary>
        public byte DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Tasks> TaskList { get; set; }
        public virtual ICollection<TaskCategory> ChildCategories { get; set; }
        public virtual ICollection<BranchTaskCategoryStakeholder> BranchTaskCategoryStakeholders { get; set; }
    }
}
