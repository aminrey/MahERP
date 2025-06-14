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
        }
    }
}
