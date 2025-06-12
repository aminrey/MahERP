using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
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

        }
    }
}
