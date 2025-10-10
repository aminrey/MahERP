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

        [Required(ErrorMessage = "انتخاب کاربر الزامی است")]
        public string SelectedUserId { get; set; }

        [Required(ErrorMessage = "انتخاب تیم الزامی است")]
        public int SelectedTeamId { get; set; }

        public string Description { get; set; }

        // برای نمایش در View
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