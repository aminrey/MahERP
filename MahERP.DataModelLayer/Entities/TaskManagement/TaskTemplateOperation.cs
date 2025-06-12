using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    /// <summary>
    /// عملیات‌های قالب تسک
    /// </summary>
    public class TaskTemplateOperation
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual TaskTemplate Template { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public int OperationOrder { get; set; }
    }
}
