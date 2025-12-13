using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;

namespace MahERP.DataModelLayer.Mappings
{
    /// <summary>
    /// AutoMapper Profile for CRM entities
    /// </summary>
    public class CRMMappingProfile : Profile
    {
        public CRMMappingProfile()
        {
            // CRMInteraction -> CRMInteractionViewModel
            CreateMap<CRMInteraction, CRMInteractionViewModel>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.StakeholderContactName, opt => opt.MapFrom(src => 
                    src.Contact != null ? src.Contact.FullName : null))
                .ForMember(dest => dest.StakeholderName, opt => opt.MapFrom(src => 
                    src.Organization != null ? src.Organization.DisplayName : null))
                .ForMember(dest => dest.ContractTitle, opt => opt.MapFrom(src => 
                    src.Contract != null ? src.Contract.Title : null))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => 
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.NextFollowUpDate, opt => opt.MapFrom(src => src.NextFollowUpDate))
                .ForMember(dest => dest.CRMTypeText, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.DirectionText, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.ResultText, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.StatusText, opt => opt.Ignore()) // Computed property
                // Ignore fields that are populated separately
                .ForMember(dest => dest.BranchesInitial, opt => opt.Ignore())
                .ForMember(dest => dest.ContactsInitial, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationsInitial, opt => opt.Ignore())
                .ForMember(dest => dest.ContactOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.UsersInitial, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFiles, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.Participants, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            // CRMInteractionViewModel -> CRMInteraction
            CreateMap<CRMInteractionViewModel, CRMInteraction>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.NextFollowUpDate, opt => opt.MapFrom(src => src.NextFollowUpDate))
                // Ignore navigation properties
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.StakeholderContact, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CRMAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.CRMParticipants, opt => opt.Ignore())
                .ForMember(dest => dest.CRMComments, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityCRMs, opt => opt.Ignore())
                .ForMember(dest => dest.CRMTeams, opt => opt.Ignore())
                // Ignore system fields
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore());
        }
    }
}
