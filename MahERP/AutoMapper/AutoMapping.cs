using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.AutoMapper
{
    public class AutoMapping: Profile
    {

        public AutoMapping()
        {
            //App
            CreateMap<AppUsers, AddUserViewModel>().ReverseMap();
            CreateMap<AppUsers, EditUserViewModel>().ReverseMap();           

            //Stakeholder
            CreateMap<Stakeholder, StakeholderViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ReverseMap();
            CreateMap<StakeholderCRM, StakeholderCRMViewModel>().ReverseMap();

            // Branch -> BranchViewModel
            CreateMap<Branch, BranchViewModel>();

            // BranchViewModel -> Branch
            CreateMap<BranchViewModel, Branch>()
                .ForMember(dest => dest.BranchUsers, opt => opt.Ignore())
                .ForMember(dest => dest.TaskList, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholders, opt => opt.Ignore())
                .ForMember(dest => dest.ChildBranches, opt => opt.Ignore())
                .ForMember(dest => dest.ParentBranch, opt => opt.Ignore());

            // BranchUser -> BranchUserViewModel
            CreateMap<BranchUser, BranchUserViewModel>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name));

            // BranchUserViewModel -> BranchUser
            CreateMap<BranchUserViewModel, BranchUser>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByUser, opt => opt.Ignore());

            // Contract -> ContractViewModel
            CreateMap<Contract, ContractViewModel>()
                .ForMember(dest => dest.StartDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.EndDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.StakeholderFullName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorFullName, opt => opt.MapFrom(src =>
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""))
                .ForMember(dest => dest.LastUpdaterFullName, opt => opt.MapFrom(src =>
                    src.LastUpdater != null ? $"{src.LastUpdater.FirstName} {src.LastUpdater.LastName}" : ""));

            // ContractViewModel -> Contract
            CreateMap<ContractViewModel, Contract>()
                .ForMember(dest => dest.StartDate, opt => opt.Ignore()) // Handled manually in controller
                .ForMember(dest => dest.EndDate, opt => opt.Ignore()) // Handled manually in controller
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.TaskList, opt => opt.Ignore());
            // Tasks mapping
            CreateMap<Tasks, TaskViewModel>()
                .ForMember(dest => dest.CategoryTitle, opt => opt.MapFrom(src => src.TaskCategory != null ? src.TaskCategory.Title : null))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : null))
                .ForMember(dest => dest.StakeholderName, opt => opt.MapFrom(src => src.Stakeholder != null ? $"{src.Stakeholder.FirstName} {src.Stakeholder.LastName}" : null))
                .ForMember(dest => dest.ContractTitle, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.Title : null))
                .ForMember(dest => dest.Operations, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            CreateMap<TaskViewModel, Tasks>()
                .ForMember(dest => dest.TaskCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore())
                .ForMember(dest => dest.TaskOperations, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore());

            // Task operations mapping
            CreateMap<TaskOperation, TaskOperationViewModel>()
                .ForMember(dest => dest.CompletedByUserName, opt => opt.MapFrom(src => src.CompletedByUser != null ? $"{src.CompletedByUser.FirstName} {src.CompletedByUser.LastName}" : null));

            CreateMap<TaskOperationViewModel, TaskOperation>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            // Task assignments mapping
            CreateMap<TaskAssignment, TaskAssignmentViewModel>()
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : null))
                .ForMember(dest => dest.AssignerUserName, opt => opt.MapFrom(src => src.AssignerUser != null ? $"{src.AssignerUser.FirstName} {src.AssignerUser.LastName}" : null));

            CreateMap<TaskAssignmentViewModel, TaskAssignment>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.AssignerUser, opt => opt.Ignore());

            // Task categories mapping
            CreateMap<TaskCategory, TaskCategoryViewModel>()
                .ForMember(dest => dest.ParentCategoryTitle, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Title : null));

            CreateMap<TaskCategoryViewModel, TaskCategory>()
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore());


        }
    }
}
