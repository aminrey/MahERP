using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.AutoMapper
{
    public class AutoMapping : Profile
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

            // StakeholderContact mappings
            CreateMap<StakeholderContactViewModel, StakeholderContact>()
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            CreateMap<StakeholderContact, StakeholderContactViewModel>();

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
                .ForMember(dest => dest.AssignmentsTaskUsers, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            CreateMap<TaskViewModel, Tasks>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskNotifications, opt => opt.Ignore())
                .ForMember(dest => dest.TaskOperations, opt => opt.Ignore())
                .ForMember(dest => dest.TaskViewers, opt => opt.Ignore());

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

            // CRM Interaction Mappings
            CreateMap<CRMInteraction, CRMInteractionViewModel>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""))
                .ForMember(dest => dest.StakeholderName, opt => opt.MapFrom(src => src.Stakeholder != null ? $"{src.Stakeholder.FirstName} {src.Stakeholder.LastName}" : ""))
                .ForMember(dest => dest.StakeholderContactName, opt => opt.MapFrom(src => src.StakeholderContact != null ? $"{src.StakeholderContact.FirstName} {src.StakeholderContact.LastName}" : ""))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.ContractTitle, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.Title : ""))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.CRMAttachments))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.CRMComments))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.CRMParticipants));

            CreateMap<CRMInteractionViewModel, CRMInteraction>()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.StakeholderContact, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore())
                .ForMember(dest => dest.CRMAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.CRMComments, opt => opt.Ignore())
                .ForMember(dest => dest.CRMParticipants, opt => opt.Ignore())
                .ForMember(dest => dest.CRMTeams, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityCRMs, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore());

            // CRM Attachment Mappings
            CreateMap<CRMAttachment, CRMAttachmentViewModel>()
                .ForMember(dest => dest.UploaderName, opt => opt.MapFrom(src => src.Uploader != null ? $"{src.Uploader.FirstName} {src.Uploader.LastName}" : ""));

            CreateMap<CRMAttachmentViewModel, CRMAttachment>()
                .ForMember(dest => dest.Uploader, opt => opt.Ignore())
                .ForMember(dest => dest.CRMInteraction, opt => opt.Ignore());

            // CRM Comment Mappings
            CreateMap<CRMComment, CRMCommentViewModel>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            CreateMap<CRMCommentViewModel, CRMComment>()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CRMInteraction, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            // CRM Participant Mappings
            CreateMap<CRMParticipant, CRMParticipantViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""))
                .ForMember(dest => dest.StakeholderContactName, opt => opt.MapFrom(src => src.StakeholderContact != null ? $"{src.StakeholderContact.FirstName} {src.StakeholderContact.LastName}" : ""));

            CreateMap<CRMParticipantViewModel, CRMParticipant>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.StakeholderContact, opt => opt.Ignore())
                .ForMember(dest => dest.CRMInteraction, opt => opt.Ignore());

            // RolePattern mappings
            CreateMap<RolePattern, RolePatternViewModel>()
                .ForMember(dest => dest.AccessLevelText, opt => opt.MapFrom(src => GetAccessLevelText(src.AccessLevel)))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : "سیستم"))
                .ForMember(dest => dest.LastUpdaterName, opt => opt.MapFrom(src => src.LastUpdater != null ? $"{src.LastUpdater.FirstName} {src.LastUpdater.LastName}" : null))
                .ReverseMap()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.RolePatternDetails, opt => opt.Ignore())
                .ForMember(dest => dest.UserRolePatterns, opt => opt.Ignore());

            // RolePatternDetails mappings
            CreateMap<RolePatternDetails, RolePatternDetailsViewModel>()
                .ForMember(dest => dest.DataAccessLevelText, opt => opt.MapFrom(src => GetDataAccessLevelText(src.DataAccessLevel)))
                .ForMember(dest => dest.ControllerDisplayName, opt => opt.MapFrom(src => GetControllerDisplayName(src.ControllerName)))
                .ForMember(dest => dest.ActionDisplayName, opt => opt.MapFrom(src => GetActionDisplayName(src.ActionName)))
                .ReverseMap()
                .ForMember(dest => dest.RolePattern, opt => opt.Ignore());

            // UserRolePattern mappings
            CreateMap<UserRolePattern, UserRolePatternInfo>()
                .ForMember(dest => dest.PatternName, opt => opt.MapFrom(src => src.RolePattern.PatternName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.RolePattern.Description))
                .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => $"{src.AssignedByUser.FirstName} {src.AssignedByUser.LastName}"))
                .ForMember(dest => dest.AssignDate, opt => opt.MapFrom(src => src.AssignDate));

            // UserPermission mappings
            CreateMap<AppUsers, UserPermissionViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => "")) // This should be populated separately
                .ForMember(dest => dest.SystemRoles, opt => opt.Ignore()) // This should be populated separately
                .ForMember(dest => dest.RolePatterns, opt => opt.Ignore()); // This should be populated separately

            // AssignRolePattern mappings
            CreateMap<AssignRolePatternViewModel, UserRolePattern>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.RolePattern, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByUser, opt => opt.Ignore());
        }

        // Helper methods for mapping
        private static string GetAccessLevelText(byte accessLevel)
        {
            return accessLevel switch
            {
                1 => "مدیر سیستم",
                2 => "مدیر",
                3 => "سرپرست",
                4 => "کارشناس",
                5 => "کاربر عادی",
                _ => "نامشخص"
            };
        }

        private static string GetDataAccessLevelText(byte dataAccessLevel)
        {
            return dataAccessLevel switch
            {
                0 => "شخصی",
                1 => "شعبه",
                2 => "همه",
                _ => "نامشخص"
            };
        }

        private static string GetControllerDisplayName(string controllerName)
        {
            return controllerName switch
            {
                "Task" => "تسک‌ها",
                "CRM" => "مدیریت ارتباط با مشتری",
                "Stakeholder" => "طرف‌های حساب",
                "Contract" => "قراردادها",
                "User" => "کاربران",
                "RolePattern" => "الگوهای نقش",
                "Branch" => "شعب",
                "Team" => "تیم‌ها",
                _ => controllerName ?? ""
            };
        }

        private static string GetActionDisplayName(string actionName)
        {
            return actionName switch
            {
                "Index" => "لیست",
                "Create" => "ایجاد",
                "Edit" => "ویرایش",
                "Delete" => "حذف",
                "Details" => "جزئیات",
                "MyTasks" => "تسک‌های من",
                "ManagePermissions" => "مدیریت دسترسی‌ها",
                "*" => "همه عملیات",
                _ => actionName ?? ""
            };
        }
    }
}
