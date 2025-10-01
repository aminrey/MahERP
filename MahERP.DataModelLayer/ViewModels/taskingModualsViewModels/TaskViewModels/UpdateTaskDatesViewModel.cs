using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    public class UpdateTaskDatesViewModel
    {
        [Required]
        public int TaskId { get; set; }
        
        [Required]
        public string NewStartDate { get; set; }
        
        [Required]
        public string NewEndDate { get; set; }
    }
}