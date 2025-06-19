using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
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
        }
    }
}
