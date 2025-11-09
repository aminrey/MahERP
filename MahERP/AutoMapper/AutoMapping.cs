using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts; // ⭐ NEW
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels; // ⭐ NEW
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;
using MahERP.DataModelLayer.ViewModels.RoleViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
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


            CreateMap<Role, RoleViewModel>().ReverseMap();

            // Permission Mappings
            CreateMap<Permission, PermissionTreeViewModel>()
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());
            // BranchContact -> BranchContactViewModel
            CreateMap<BranchContact, BranchContactViewModel>()
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.ContactName,
                    opt => opt.MapFrom(src => src.Contact != null ? src.Contact.FullName : ""))
                .ForMember(dest => dest.ContactPhone,
                    opt => opt.MapFrom(src => src.Contact != null && src.Contact.DefaultPhone != null
                        ? src.Contact.DefaultPhone.FormattedNumber
                        : ""))
                .ForMember(dest => dest.AssignDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.AssignDate, "yyyy/MM/dd")));

            // BranchContactViewModel -> BranchContact
            CreateMap<BranchContactViewModel, BranchContact>()
                .ForMember(dest => dest.AssignDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.AssignDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.AssignDatePersian)
                        : DateTime.Now))
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedBy, opt => opt.Ignore());

            // BranchOrganization -> BranchOrganizationViewModel
            CreateMap<BranchOrganization, BranchOrganizationViewModel>()
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Organization != null ? src.Organization.DisplayName : ""))
                .ForMember(dest => dest.AssignDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.AssignDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.DepartmentsCount,
                    opt => opt.MapFrom(src => src.Organization != null
                        ? src.Organization.Departments.Count(d => d.IsActive)
                        : 0))
                .ForMember(dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.Organization != null
                        ? src.Organization.Departments
                            .SelectMany(d => d.Members)
                            .Count(m => m.IsActive)
                        : 0));

            // BranchOrganizationViewModel -> BranchOrganization
            CreateMap<BranchOrganizationViewModel, BranchOrganization>()
                .ForMember(dest => dest.AssignDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.AssignDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.AssignDatePersian)
                        : DateTime.Now))
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedBy, opt => opt.Ignore());
            // Branch -> BranchViewModel
            CreateMap<Branch, BranchViewModel>();

            // BranchViewModel -> Branch
            CreateMap<BranchViewModel, Branch>()
                .ForMember(dest => dest.BranchUsers, opt => opt.Ignore())
                .ForMember(dest => dest.TaskList, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholders, opt => opt.Ignore())
                .ForMember(dest => dest.ChildBranches, opt => opt.Ignore())
                .ForMember(dest => dest.ParentBranch, opt => opt.Ignore())
                                .ReverseMap();


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
            // Mapping Task به TaskViewModel
            CreateMap<Tasks, TaskViewModel>()
                .ForMember(dest => dest.Operations, opt => opt.MapFrom(src =>
                    src.TaskOperations != null ? src.TaskOperations.Where(o => !o.IsDeleted).ToList() : new List<TaskOperation>()))
                .ForMember(dest => dest.AssignmentsTaskUser, opt => opt.MapFrom(src => src.TaskAssignments))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => CalculateProgress(src)))
                .ForMember(dest => dest.WorkLogs, opt => opt.MapFrom(src => src.TaskWorkLogs))
                .ForMember(dest => dest.TaskCode, opt => opt.MapFrom(src => src.TaskCode))
    .ForMember(dest => dest.IsIndependentCompletion, opt => opt.MapFrom(src => src.IsIndependentCompletion)) 
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Creator.FirstName} {src.Creator.LastName}" ))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComments));
            CreateMap<TaskViewModel, Tasks>()
                     .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => src.SelectedContactId))
    .ForMember(dest => dest.OrganizationId,
         opt => opt.MapFrom(src => src.SelectedOrganizationId))

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
            // Mapping TaskOperation به TaskOperationViewModel
            CreateMap<TaskOperation, TaskOperationViewModel>()
                .ForMember(dest => dest.WorkLogs, opt => opt.MapFrom(src => src.WorkLogs))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
                .ForMember(dest => dest.IsStarred, opt => opt.MapFrom(src => src.IsStarred));


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

            // اضافه کردن mapping جدید برای TaskCategoryItemViewModel
            CreateMap<OperationWorkLogViewModel, TaskOperationWorkLog>().ReverseMap();
            CreateMap<TaskCategory, TaskCategoryItemViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<TaskCategoryItemViewModel, TaskCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.Ignore());
              

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

            // BranchTaskCategory Mappings - انتصاب دسته‌بندی تسک به شعبه و طرف حساب
            CreateMap<BranchTaskCategoryStakeholder, BranchTaskCategoryStakeholderViewModel>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.TaskCategoryTitle, opt => opt.MapFrom(src => src.TaskCategory != null ? src.TaskCategory.Title : ""))
                .ForMember(dest => dest.StakeholderName, opt => opt.MapFrom(src => src.Stakeholder != null ? $"{src.Stakeholder.FirstName} {src.Stakeholder.LastName}" : ""))
                .ForMember(dest => dest.AssignedByUserName, opt => opt.MapFrom(src => src.AssignedByUser != null ? $"{src.AssignedByUser.FirstName} {src.AssignedByUser.LastName}" : ""));

            CreateMap<BranchTaskCategoryStakeholderViewModel, BranchTaskCategoryStakeholder>()
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByUser, opt => opt.Ignore());

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

            // NEW TEAM MAPPINGS
            // Team -> TeamViewModel
            CreateMap<Team, TeamViewModel>()
                .ForMember(dest => dest.ParentTeamTitle, opt => opt.MapFrom(src => src.ParentTeam != null ? src.ParentTeam.Title : null))
                .ForMember(dest => dest.ManagerFullName, opt => opt.MapFrom(src => src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}" : null))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
                .ForMember(dest => dest.AccessLevelText, opt => opt.MapFrom(src => GetTeamAccessLevelText(src.AccessLevel)))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : null))
                .ForMember(dest => dest.LastUpdaterName, opt => opt.MapFrom(src => src.LastUpdater != null ? $"{src.LastUpdater.FirstName} {src.LastUpdater.LastName}" : null))
                .ForMember(dest => dest.CreatorUserId, opt => opt.MapFrom(src => src.CreatorUserId))
                .ForMember(dest => dest.ChildTeams, opt => opt.Ignore())
                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                
                .ReverseMap()
                                .ForMember(dest => dest.ChildTeams, opt => opt.Ignore())
                                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore());

            // TeamViewModel -> Team
            CreateMap<TeamViewModel, Team>()
                .ForMember(dest => dest.ParentTeam, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.ChildTeams, opt => opt.Ignore())
                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore());

            // TeamMember -> TeamMemberViewModel
            CreateMap<TeamMember, TeamMemberViewModel>()
                .ForMember(dest => dest.TeamTitle, opt => opt.MapFrom(src => src.Team != null ? src.Team.Title : null))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
                .ForMember(dest => dest.MembershipTypeText, opt => opt.MapFrom(src => GetMembershipTypeText(src.MembershipType)))
                .ForMember(dest => dest.AddedByUserName, opt => opt.MapFrom(src => src.AddedByUser != null ? $"{src.AddedByUser.FirstName} {src.AddedByUser.LastName}" : null));

            // TeamMemberViewModel -> TeamMember
            CreateMap<TeamPositionViewModel, TeamPosition>().ReverseMap();
            CreateMap<TeamMemberViewModel, TeamMember>()
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.AddedByUser, opt => opt.Ignore());






            // Stakeholder Mappings
            CreateMap<Stakeholder, StakeholderViewModel>()
                .ForMember(dest => dest.BirthDate,
                    opt => opt.MapFrom(src => src.BirthDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.BirthDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.RegistrationDate,
                    opt => opt.MapFrom(src => src.RegistrationDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.RegistrationDate.Value, "yyyy/MM/dd")
                        : null))
                .ReverseMap()
                .ForMember(dest => dest.BirthDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.BirthDate)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.BirthDate)
                        : (DateTime?)null))
                .ForMember(dest => dest.RegistrationDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.RegistrationDate)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.RegistrationDate)
                        : (DateTime?)null));

            // StakeholderContact Mappings
            CreateMap<StakeholderContact, StakeholderContactViewModel>()
                .ReverseMap();

            // StakeholderOrganization Mappings
            CreateMap<StakeholderOrganization, StakeholderOrganizationViewModel>()
                .ForMember(dest => dest.ManagerName,
                    opt => opt.MapFrom(src => src.ManagerContact != null
                        ? $"{src.ManagerContact.FirstName} {src.ManagerContact.LastName}"
                        : null))
                .ForMember(dest => dest.ParentOrganizationTitle,
                    opt => opt.MapFrom(src => src.ParentOrganization != null
                        ? src.ParentOrganization.Title
                        : null))
                .ReverseMap()
                // ✅ مطمئن شوید StakeholderId به درستی map می‌شود
                .ForMember(dest => dest.StakeholderId, opt => opt.MapFrom(src => src.StakeholderId)) // ✅ صریح
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ParentOrganizationId, opt => opt.MapFrom(src => src.ParentOrganizationId))
                .ForMember(dest => dest.ManagerContactId, opt => opt.MapFrom(src => src.ManagerContactId))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.ChildOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Positions, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholder, opt => opt.Ignore())
                .ForMember(dest => dest.ParentOrganization, opt => opt.Ignore())
                .ForMember(dest => dest.ManagerContact, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore());

            // StakeholderOrganizationPosition Mappings
            CreateMap<StakeholderOrganizationPosition, StakeholderOrganizationPositionViewModel>()
                .ReverseMap();

            // StakeholderOrganizationMember Mappings
            CreateMap<StakeholderOrganizationMember, StakeholderOrganizationMemberViewModel>()
                .ForMember(dest => dest.JoinDate,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.JoinDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.LeaveDate,
                    opt => opt.MapFrom(src => src.LeaveDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.LeaveDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.ContactName,
                    opt => opt.MapFrom(src => src.Contact != null
                        ? $"{src.Contact.FirstName} {src.Contact.LastName}"
                        : null))
                .ForMember(dest => dest.PositionTitle,
                    opt => opt.MapFrom(src => src.Position != null
                        ? src.Position.Title
                        : null))
                .ReverseMap()
                .ForMember(dest => dest.JoinDate,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertShamsiToMiladi(src.JoinDate)))
                .ForMember(dest => dest.LeaveDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.LeaveDate)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.LeaveDate)
                        : (DateTime?)null));

            // ==================== CONTACT MAPPINGS ====================

            // Contact -> ContactViewModel
            CreateMap<Contact, ContactViewModel>()
                .ForMember(dest => dest.BirthDatePersian,
                    opt => opt.MapFrom(src => src.BirthDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.BirthDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.CreatedDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd HH:mm")))
                .ForMember(dest => dest.LastUpdateDatePersian,
                    opt => opt.MapFrom(src => src.LastUpdateDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.LastUpdateDate.Value, "yyyy/MM/dd HH:mm")
                        : null))
                .ForMember(dest => dest.CreatorName,
                    opt => opt.MapFrom(src => src.Creator != null
                        ? $"{src.Creator.FirstName} {src.Creator.LastName}"
                        : ""))
                .ForMember(dest => dest.Phones,
                    opt => opt.MapFrom(src => src.Phones.Where(p => p.IsActive)))
                .ForMember(dest => dest.DepartmentMemberships,
                    opt => opt.MapFrom(src => src.DepartmentMemberships.Where(dm => dm.IsActive)))
                .ForMember(dest => dest.OrganizationRelations,
                    opt => opt.MapFrom(src => src.OrganizationRelations.Where(or => or.IsActive)))
                .ForMember(dest => dest.FullName,
        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
    .ForMember(dest => dest.PrimaryPhone,
        opt => opt.MapFrom(src => src.Phones
            .Where(p => p.IsDefault && p.IsActive)
            .Select(p => p.PhoneNumber)
            .FirstOrDefault()));

            // ContactViewModel -> Contact
            CreateMap<ContactViewModel, Contact>()
                .ForMember(dest => dest.BirthDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.BirthDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.BirthDatePersian)
                        : (DateTime?)null))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentMemberships, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationRelations, opt => opt.Ignore())
                .ForMember(dest => dest.ManagedDepartments, opt => opt.Ignore());

            // ==================== CONTACT PHONE MAPPINGS ====================

            // ContactPhone -> ContactPhoneViewModel
            CreateMap<ContactPhone, ContactPhoneViewModel>()
                .ForMember(dest => dest.FormattedNumber, opt => opt.MapFrom(src => src.FormattedNumber))
                .ForMember(dest => dest.DisplayText, opt => opt.MapFrom(src => src.DisplayText));

            // ContactPhoneViewModel -> ContactPhone
            CreateMap<ContactPhoneViewModel, ContactPhone>()
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.VerifiedDate, opt => opt.Ignore());

            // ==================== ORGANIZATION MAPPINGS ====================

            // در بخش Organization Mappings:

            CreateMap<Organization, OrganizationViewModel>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.LogoPath, opt => opt.MapFrom(src => src.LogoPath)) // ✅ اضافه شده
                .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.RegistrationNumber))
                .ForMember(dest => dest.EconomicCode, opt => opt.MapFrom(src => src.EconomicCode))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate)) // ✅ اضافه شده
                .ForMember(dest => dest.LegalRepresentative, opt => opt.MapFrom(src => src.LegalRepresentative))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone))
                .ForMember(dest => dest.SecondaryPhone, opt => opt.MapFrom(src => src.SecondaryPhone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OrganizationType, opt => opt.MapFrom(src => src.OrganizationType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastUpdateDate, opt => opt.MapFrom(src => src.LastUpdateDate)) // ✅ اضافه شده
                .ForMember(dest => dest.RegistrationDatePersian, opt => opt.MapFrom(src =>
                    src.RegistrationDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.RegistrationDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.CreatedDatePersian, opt => opt.MapFrom(src =>
                    ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.LastUpdateDatePersian, opt => opt.MapFrom(src =>
                    src.LastUpdateDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.LastUpdateDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src =>
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : null))
                .ForMember(dest => dest.Departments, opt => opt.MapFrom(src => src.Departments)) // ✅ اضافه شده
                .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts)); // ✅ اضافه شده

            CreateMap<OrganizationViewModel, Organization>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.LogoPath, opt => opt.MapFrom(src => src.LogoPath)) // ✅ اضافه شده
                .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.RegistrationNumber))
                .ForMember(dest => dest.EconomicCode, opt => opt.MapFrom(src => src.EconomicCode))
                .ForMember(dest => dest.LegalRepresentative, opt => opt.MapFrom(src => src.LegalRepresentative))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone))
                .ForMember(dest => dest.SecondaryPhone, opt => opt.MapFrom(src => src.SecondaryPhone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OrganizationType, opt => opt.MapFrom(src => src.OrganizationType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.RegistrationDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.RegistrationDatePersian)
                        : src.RegistrationDate))
                // فیلدهای سیستمی را ignore می‌کنیم
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.Departments, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore());
            // ==================== ORGANIZATION DEPARTMENT MAPPINGS ====================

            // OrganizationDepartment -> OrganizationDepartmentViewModel
            CreateMap<OrganizationDepartment, OrganizationDepartmentViewModel>()
                .ForMember(dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Organization != null ? src.Organization.DisplayName : ""))
                .ForMember(dest => dest.ParentDepartmentTitle,
                    opt => opt.MapFrom(src => src.ParentDepartment != null ? src.ParentDepartment.Title : null))
                .ForMember(dest => dest.ManagerName,
                    opt => opt.MapFrom(src => src.ManagerContact != null
                        ? $"{src.ManagerContact.FirstName} {src.ManagerContact.LastName}"
                        : null))
                .ForMember(dest => dest.FullPath, opt => opt.MapFrom(src => src.FullPath))
                .ForMember(dest => dest.ActiveMembersCount, opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.ActivePositionsCount, opt => opt.MapFrom(src => src.ActivePositionsCount))
                .ForMember(dest => dest.ChildDepartments,
                    opt => opt.MapFrom(src => src.ChildDepartments.Where(cd => cd.IsActive)))
                .ForMember(dest => dest.Positions,
                    opt => opt.MapFrom(src => src.Positions.Where(p => p.IsActive)))
                .ForMember(dest => dest.Members,
                    opt => opt.MapFrom(src => src.Members.Where(m => m.IsActive)));

            // OrganizationDepartmentViewModel -> OrganizationDepartment
            CreateMap<OrganizationDepartmentViewModel, OrganizationDepartment>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.ParentDepartment, opt => opt.Ignore())
                .ForMember(dest => dest.ManagerContact, opt => opt.Ignore())
                .ForMember(dest => dest.ChildDepartments, opt => opt.Ignore())
                .ForMember(dest => dest.Positions, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore());

            // ==================== DEPARTMENT POSITION MAPPINGS ====================

            // DepartmentPosition -> DepartmentPositionViewModel
            CreateMap<DepartmentPosition, DepartmentPositionViewModel>()
                .ForMember(dest => dest.DepartmentTitle,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Title : ""))
                .ForMember(dest => dest.ActiveMembersCount, opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.SalaryRangeText, opt => opt.MapFrom(src => src.SalaryRangeText));

            // DepartmentPositionViewModel -> DepartmentPosition
            CreateMap<DepartmentPositionViewModel, DepartmentPosition>()
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // ==================== DEPARTMENT MEMBER MAPPINGS ====================

            // DepartmentMember -> DepartmentMemberViewModel
            CreateMap<DepartmentMember, DepartmentMemberViewModel>()
                .ForMember(dest => dest.DepartmentTitle,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Title : ""))
                .ForMember(dest => dest.ContactName,
                    opt => opt.MapFrom(src => src.Contact != null
                        ? $"{src.Contact.FirstName} {src.Contact.LastName}"
                        : ""))
                .ForMember(dest => dest.ContactPhone,
                    opt => opt.MapFrom(src => src.Contact != null && src.Contact.DefaultPhone != null
                        ? src.Contact.DefaultPhone.FormattedNumber
                        : ""))
                .ForMember(dest => dest.PositionTitle,
                    opt => opt.MapFrom(src => src.Position != null ? src.Position.Title : ""))
                .ForMember(dest => dest.JoinDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.JoinDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.LeaveDatePersian,
                    opt => opt.MapFrom(src => src.LeaveDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.LeaveDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.ServiceDurationText, opt => opt.MapFrom(src => src.ServiceDurationText));

            // DepartmentMemberViewModel -> DepartmentMember
            CreateMap<DepartmentMemberViewModel, DepartmentMember>()
                .ForMember(dest => dest.JoinDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.JoinDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.JoinDatePersian)
                        : DateTime.Now))
                .ForMember(dest => dest.LeaveDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.LeaveDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.LeaveDatePersian)
                        : (DateTime?)null))
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // ==================== ORGANIZATION CONTACT MAPPINGS ====================

            // OrganizationContact -> OrganizationContactViewModel
            CreateMap<OrganizationContact, OrganizationContactViewModel>()
                .ForMember(dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Organization != null ? src.Organization.DisplayName : ""))
                .ForMember(dest => dest.ContactName,
                    opt => opt.MapFrom(src => src.Contact != null
                        ? $"{src.Contact.FirstName} {src.Contact.LastName}"
                        : ""))
                .ForMember(dest => dest.ContactPhone,
                    opt => opt.MapFrom(src => src.Contact != null && src.Contact.DefaultPhone != null
                        ? src.Contact.DefaultPhone.FormattedNumber
                        : ""))
                .ForMember(dest => dest.StartDatePersian,
                    opt => opt.MapFrom(src => src.StartDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.StartDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.EndDatePersian,
                    opt => opt.MapFrom(src => src.EndDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.EndDate.Value, "yyyy/MM/dd")
                        : null));

            // OrganizationContactViewModel -> OrganizationContact
            CreateMap<OrganizationContactViewModel, OrganizationContact>()
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.StartDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.StartDatePersian)
                        : (DateTime?)null))
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EndDatePersian)
                        ? ConvertDateTime.ConvertShamsiToMiladi(src.EndDatePersian)
                        : (DateTime?)null))
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // ==================== STATISTICS MAPPINGS ====================

            // ContactStatisticsViewModel (no reverse mapping needed)
            CreateMap<ContactStatisticsViewModel, ContactStatisticsViewModel>();

            // OrganizationStatisticsViewModel (no reverse mapping needed)
            CreateMap<OrganizationStatisticsViewModel, OrganizationStatisticsViewModel>();

            CreateMap<SmsProvider, SmsProviderViewModel>().ReverseMap();

            // ContactGroup -> ContactGroupViewModel
            CreateMap<ContactGroup, ContactGroupViewModel>()
                .ForMember(dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.CreatedDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.CreatorName,
                    opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""));

            // ContactGroupViewModel -> ContactGroup
            CreateMap<ContactGroupViewModel, ContactGroup>()
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // BranchContactGroup -> BranchContactGroupViewModel
            CreateMap<BranchContactGroup, BranchContactGroupViewModel>()
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.CreatedDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd")));

            // BranchContactGroupViewModel -> BranchContactGroup
            CreateMap<BranchContactGroupViewModel, BranchContactGroup>()
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // OrganizationGroup -> OrganizationGroupViewModel
            CreateMap<OrganizationGroup, OrganizationGroupViewModel>()
                .ForMember(dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.CreatedDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd")))
                .ForMember(dest => dest.CreatorName,
                    opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""));

            // OrganizationGroupViewModel -> OrganizationGroup
            CreateMap<OrganizationGroupViewModel, OrganizationGroup>()
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // BranchOrganizationGroup -> BranchOrganizationGroupViewModel
            CreateMap<BranchOrganizationGroup, BranchOrganizationGroupViewModel>()
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : ""))
                .ForMember(dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.ActiveMembersCount))
                .ForMember(dest => dest.CreatedDatePersian,
                    opt => opt.MapFrom(src => ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd")));

            // BranchOrganizationGroupViewModel -> BranchOrganizationGroup
            CreateMap<BranchOrganizationGroupViewModel, BranchOrganizationGroup>()
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

            // در انتهای متد AutoMapping() این mapping را اضافه کنید:

            // ==================== TASK COMMENT MAPPINGS ====================

            // TaskComment -> TaskCommentViewModel
            CreateMap<TaskComment, TaskCommentViewModel>()
                .ForMember(dest => dest.CreatorName,
                    opt => opt.MapFrom(src => src.Creator != null
                        ? $"{src.Creator.FirstName} {src.Creator.LastName}"
                        : "نامشخص"))
                .ForMember(dest => dest.CreatorProfileImage,
                    opt => opt.MapFrom(src => src.Creator != null
                        ? (src.Creator.ProfileImagePath ?? "/images/default-avatar.png")
                        : "/images/default-avatar.png"))
               
                .ForMember(dest => dest.Attachments,
                    opt => opt.MapFrom(src => src.Attachments));

            // TaskCommentViewModel -> TaskComment
            CreateMap<TaskCommentViewModel, TaskComment>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.MentionedUsers, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.EditDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsEdited, opt => opt.Ignore());

            // TaskCommentAttachment -> TaskCommentAttachmentViewModel
            CreateMap<TaskCommentAttachment, TaskCommentAttachmentViewModel>();

            // TaskCommentAttachmentViewModel -> TaskCommentAttachment
            CreateMap<TaskCommentAttachmentViewModel, TaskCommentAttachment>()
                .ForMember(dest => dest.Comment, opt => opt.Ignore())
                .ForMember(dest => dest.Uploader, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.UploaderUserId, opt => opt.Ignore());



            CreateMap<TaskWorkLog, TaskWorkLogViewModel>()
    .ForMember(dest => dest.UserName,
        opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
    .ForMember(dest => dest.UserProfileImage,
        opt => opt.MapFrom(src => src.User.ProfileImagePath ?? "/images/default-avatar.png"));




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
                "Contact" => "افراد", // ⭐ NEW
                "Organization" => "سازمان‌ها", // ⭐ NEW
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
                "ManagePhones" => "مدیریت شماره‌ها", // ⭐ NEW
                "OrganizationChart" => "چارت سازمانی", // ⭐ NEW
                "*" => "همه عملیات",
                _ => actionName ?? ""
            };
        }

        private static string GetTeamAccessLevelText(byte accessLevel)
        {
            return accessLevel switch
            {
                0 => "عمومی",
                1 => "محدود",
                _ => "نامشخص"
            };
        }

        private static string GetMembershipTypeText(byte membershipType)
        {
            return membershipType switch
            {
                0 => "عضو عادی",
                1 => "عضو ویژه",
                2 => "مدیر تیم",
                _ => "نامشخص"
            };
        }

        // Helper method for calculating progress
        private static int CalculateProgress(Tasks task)
        {
            if (task.TaskOperations == null || !task.TaskOperations.Any())
                return 0;

            var activeOps = task.TaskOperations.Where(o => !o.IsDeleted).ToList();
            if (!activeOps.Any()) return 0;

            var completedOps = activeOps.Count(o => o.IsCompleted);
            return (int)Math.Round((double)completedOps / activeOps.Count * 100);
        }
    }
}
