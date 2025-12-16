using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System.ComponentModel.DataAnnotations;
namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{

    public class AssignUserToTaskViewModel
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskCode { get; set; }
        public int BranchId { get; set; }

        // ⭐⭐⭐ برای پشتیبانی از چندین کاربر (مشابه CreateNewTask)
        [Required(ErrorMessage = "حداقل یک کاربر باید انتخاب شود")]
        public string AssignmentsSelectedTaskUserArraysString { get; set; }
        
        public string UserTeamAssignmentsJson { get; set; }

        // ⭐ برای نمایش در View
        public List<BranchUserViewModel> AvailableUsers { get; set; }
        public List<TeamViewModel> AvailableTeams { get; set; }
    }

    public class RemoveAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskCode { get; set; }
        public string UserName { get; set; }
    }
}