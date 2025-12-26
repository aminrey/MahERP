using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
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

            // ==================== TEAM MAPPINGS ====================
            // Team -> TeamViewModel
            CreateMap<Team, TeamViewModel>()
                .ForMember(dest => dest.ParentTeamTitle, opt => opt.MapFrom(src =>
                    src.ParentTeam != null ? src.ParentTeam.Title : null))
                .ForMember(dest => dest.ManagerFullName, opt => opt.MapFrom(src =>
                    src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}" : null))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src =>
                    src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}" : null))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src =>
                    src.Branch != null ? src.Branch.Name : null))
                .ForMember(dest => dest.AccessLevelText, opt => opt.MapFrom(src =>
                    GetTeamAccessLevelText(src.AccessLevel)))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src =>
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : null))
                .ForMember(dest => dest.LastUpdaterName, opt => opt.MapFrom(src =>
                    src.LastUpdater != null ? $"{src.LastUpdater.FirstName} {src.LastUpdater.LastName}" : null))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src =>
                    src.TeamMembers != null ? src.TeamMembers.Count(tm => tm.IsActive) : 0))
                .ForMember(dest => dest.ChildTeams, opt => opt.Ignore())
                .ForMember(dest => dest.TeamPositions, opt => opt.Ignore())
                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore());

            // TeamViewModel -> Team
            CreateMap<TeamViewModel, Team>()
                .ForMember(dest => dest.ParentTeam, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.ChildTeams, opt => opt.Ignore())
                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore())
                .ForMember(dest => dest.TeamPositions, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore());

            // TeamMember -> TeamMemberViewModel
            CreateMap<TeamMember, TeamMemberViewModel>()
                .ForMember(dest => dest.TeamTitle, opt => opt.MapFrom(src =>
                    src.Team != null ? src.Team.Title : null))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
                .ForMember(dest => dest.PositionTitle, opt => opt.MapFrom(src =>
                    src.Position != null ? src.Position.Title : null))
                .ForMember(dest => dest.PowerLevel, opt => opt.MapFrom(src =>
                    src.Position != null ? (int?)src.Position.PowerLevel : null))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src =>
                    src.Position != null ? src.Position.Title : "عضو"))
                .ForMember(dest => dest.MembershipTypeText, opt => opt.MapFrom(src =>
                    GetMembershipTypeText(src.MembershipType)))
                .ForMember(dest => dest.AddedByUserName, opt => opt.MapFrom(src =>
                    src.AddedByUser != null ? $"{src.AddedByUser.FirstName} {src.AddedByUser.LastName}" : null));

            // TeamMemberViewModel -> TeamMember
            CreateMap<TeamMemberViewModel, TeamMember>()
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.AddedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore());

            // ==================== BRANCH MAPPINGS ====================
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
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => 
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : ""))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComments))
                // ⭐⭐⭐ NEW - Mapping Contact و Organization
                .ForMember(dest => dest.SelectedContactId, opt => opt.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.SelectedOrganizationId, opt => opt.MapFrom(src => src.OrganizationId))
                .ForMember(dest => dest.ContactFullName, opt => opt.MapFrom(src => 
                    src.Contact != null ? $"{src.Contact.FirstName} {src.Contact.LastName}" : null))
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => 
                    src.Organization != null ? src.Organization.DisplayName : null))
                // ⭐⭐⭐ Mapping فیلدهای زمان‌بندی
                .ForMember(dest => dest.SuggestedStartDate, opt => opt.MapFrom(src => src.SuggestedStartDate))
                .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
                .ForMember(dest => dest.IsHardDeadline, opt => opt.MapFrom(src => src.IsHardDeadline))
                .ForMember(dest => dest.TimeNote, opt => opt.MapFrom(src => src.TimeNote))
                .ForMember(dest => dest.ExistingAttachments, opt => opt.MapFrom(src => src.TaskAttachments))
                // ⭐⭐⭐ Mapping فیلدهای اضافی
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.Important, opt => opt.MapFrom(src => src.Important))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VisibilityLevel, opt => opt.MapFrom(src => src.VisibilityLevel))
                .ForMember(dest => dest.TaskTypeInput, opt => opt.MapFrom(src => src.TaskTypeInput))
                .ForMember(dest => dest.CreationMode, opt => opt.MapFrom(src => src.CreationMode))
                .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
                .ForMember(dest => dest.CompletionMode, opt => opt.MapFrom(src => src.CompletionMode))
                // ⭐⭐⭐ Ignore فیلدهای فقط-خواندنی و محاسباتی
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskOperationsJson, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRemindersJson, opt => opt.Ignore())
                .ForMember(dest => dest.branchListInitial, opt => opt.Ignore())
                .ForMember(dest => dest.UsersInitial, opt => opt.Ignore())
                .ForMember(dest => dest.StakeholdersInitial, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategoryInitial, opt => opt.Ignore())
                .ForMember(dest => dest.TeamsInitial, opt => opt.Ignore())
                .ForMember(dest => dest.AssignmentsCopyCarbonUsers, opt => opt.Ignore())
                .ForMember(dest => dest.AssignmentsSelectedTaskUserArraysString, opt => opt.Ignore())
                .ForMember(dest => dest.AssignmentsSelectedTeamIds, opt => opt.Ignore())
                .ForMember(dest => dest.AssignmentsSelectedCopyCarbonUsersArraysString, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCodeSettings, opt => opt.Ignore())
                .ForMember(dest => dest.UserTeamAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.UserTeamAssignmentsJson, opt => opt.Ignore())
                .ForMember(dest => dest.ContactsInitial, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationsInitial, opt => opt.Ignore())
                .ForMember(dest => dest.ContactOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.IsAssignedToCurrentUser, opt => opt.Ignore())
                .ForMember(dest => dest.IsCreator, opt => opt.Ignore())
                .ForMember(dest => dest.IsManager, opt => opt.Ignore())
                .ForMember(dest => dest.IsSupervisor, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompletedByMe, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserContext, opt => opt.Ignore())
                .ForMember(dest => dest.TaskSchedule, opt => opt.Ignore())
                .ForMember(dest => dest.IsFocused, opt => opt.Ignore())
                .ForMember(dest => dest.FocusedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsInMyDay, opt => opt.Ignore())
                .ForMember(dest => dest.FromList, opt => opt.Ignore())
                .ForMember(dest => dest.SuggestedStartDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.BranchIdSelected, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategoryIdSelected, opt => opt.Ignore())
                .ForMember(dest => dest.IsManualTaskCode, opt => opt.Ignore())
                .ForMember(dest => dest.ManualTaskCode, opt => opt.Ignore())
                .ForMember(dest => dest.EnableDefaultReminder, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryTitle, opt => opt.MapFrom(src => 
                    src.TaskCategory != null ? src.TaskCategory.Title : null))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.StakeholderName, opt => opt.Ignore())
                .ForMember(dest => dest.ContractTitle, opt => opt.MapFrom(src => 
                    src.Contract != null ? src.Contract.Title : null));
            CreateMap<TaskViewModel, Tasks>()
                .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => src.SelectedContactId))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.SelectedOrganizationId))
                // ⭐⭐⭐ Mapping فیلدهای زمان‌بندی
                .ForMember(dest => dest.SuggestedStartDate, opt => opt.MapFrom(src => src.SuggestedStartDate))
                .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
                .ForMember(dest => dest.IsHardDeadline, opt => opt.MapFrom(src => src.IsHardDeadline))
                .ForMember(dest => dest.TimeNote, opt => opt.MapFrom(src => src.TimeNote))
                // ⭐⭐⭐ Mapping فیلدهای اضافی
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.Important, opt => opt.MapFrom(src => src.Important))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VisibilityLevel, opt => opt.MapFrom(src => src.VisibilityLevel))
                .ForMember(dest => dest.TaskTypeInput, opt => opt.MapFrom(src => src.TaskTypeInput))
                .ForMember(dest => dest.CreationMode, opt => opt.MapFrom(src => src.CreationMode))
                .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
                .ForMember(dest => dest.CompletionMode, opt => opt.MapFrom(src => src.CompletionMode))
                .ForMember(dest => dest.IsIndependentCompletion, opt => opt.MapFrom(src => src.IsIndependentCompletion))
                // ⭐⭐⭐ Ignore navigation properties
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.Schedule, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskNotifications, opt => opt.Ignore())
                .ForMember(dest => dest.TaskOperations, opt => opt.Ignore())
                .ForMember(dest => dest.TaskViewers, opt => opt.Ignore())
                .ForMember(dest => dest.TaskWorkLogs, opt => opt.Ignore())
                .ForMember(dest => dest.CarbonCopies, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.Contract, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategory, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedUserInfo, opt => opt.Ignore())
                .ForMember(dest => dest.ScheduleId, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayLevel, opt => opt.Ignore());
            // Mapping TaskOperation به TaskOperationViewModel
            CreateMap<TaskOperation, TaskOperationViewModel>()
                .ForMember(dest => dest.WorkLogs, opt => opt.MapFrom(src => src.WorkLogs))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
                .ForMember(dest => dest.IsStarred, opt => opt.MapFrom(src => src.IsStarred))
                .ForMember(dest => dest.CompletedByUserName, opt => opt.MapFrom(src => 
                    src.CompletedByUser != null ? $"{src.CompletedByUser.FirstName} {src.CompletedByUser.LastName}" : null))
                .ForMember(dest => dest.CreatorUserName, opt => opt.MapFrom(src => 
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : null))
                .ForMember(dest => dest.CreatedDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionDatePersian, opt => opt.Ignore());

            CreateMap<TaskOperationViewModel, TaskOperation>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.WorkLogs, opt => opt.Ignore());

            // ⭐⭐⭐ NEW - Mapping برای TaskOperationWorkLog
            CreateMap<TaskOperationWorkLog, OperationWorkLogViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int?)src.Id))
                .ForMember(dest => dest.TaskOperationId, opt => opt.MapFrom(src => src.TaskOperationId))
                .ForMember(dest => dest.WorkDescription, opt => opt.MapFrom(src => src.WorkDescription))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage))
                .ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => (DateTime?)src.WorkDate))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.WorkDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.OperationTitle, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTitle, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsAlreadyWorkedOn, opt => opt.Ignore())
                .ForMember(dest => dest.RecentWorkLogs, opt => opt.Ignore())
                .ForMember(dest => dest.TotalWorkLogsCount, opt => opt.Ignore())
                .ForMember(dest => dest.CanEdit, opt => opt.Ignore())
                .ForMember(dest => dest.CanDelete, opt => opt.Ignore());

            CreateMap<OperationWorkLogViewModel, TaskOperationWorkLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.TaskOperationId, opt => opt.MapFrom(src => src.TaskOperationId))
                .ForMember(dest => dest.WorkDescription, opt => opt.MapFrom(src => src.WorkDescription))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage))
                .ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => src.WorkDate ?? DateTime.Now))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.TaskOperation, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // ⭐⭐⭐ NEW - Mapping برای TaskWorkLog (سطح کلی تسک)
            CreateMap<TaskWorkLog, TaskWorkLogViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int?)src.Id))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ForMember(dest => dest.WorkDescription, opt => opt.MapFrom(src => src.WorkDescription))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage))
                .ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => src.WorkDate))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.WorkDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDatePersian, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTitle, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsAlreadyWorkedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfileImage, opt => opt.Ignore());

            CreateMap<TaskWorkLogViewModel, TaskWorkLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ForMember(dest => dest.WorkDescription, opt => opt.MapFrom(src => src.WorkDescription))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage))
                .ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => src.WorkDate))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());


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

            // StakeholderOrganization مappings
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

            // ⭐ NEW: CRM Contact ViewModels
            CreateMap<ContactCreateViewModel, Contact>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentMemberships, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationRelations, opt => opt.Ignore())
                .ForMember(dest => dest.ManagedDepartments, opt => opt.Ignore())
                .ForMember(dest => dest.BirthDate, opt => opt.Ignore())
                .ForMember(dest => dest.ContactType, opt => opt.Ignore());

            CreateMap<Contact, ContactEditViewModel>();
            
            CreateMap<ContactEditViewModel, Contact>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentMemberships, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationRelations, opt => opt.Ignore())
                .ForMember(dest => dest.ManagedDepartments, opt => opt.Ignore())
                .ForMember(dest => dest.BirthDate, opt => opt.Ignore())
                .ForMember(dest => dest.ContactType, opt => opt.Ignore());

            // ==================== ORGANIZATION MAPPINGS ====================

            // ⭐ NEW: CRM Organization ViewModels
            CreateMap<OrganizationCreateViewModel, Organization>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore())
                .ForMember(dest => dest.Departments, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore());

            CreateMap<Organization, OrganizationEditViewModel>();
            
            CreateMap<OrganizationEditViewModel, Organization>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdater, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore())
                .ForMember(dest => dest.Departments, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore());


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
                // ⭐⭐⭐ اصلاح شده: گرفتن شماره از DefaultPhone
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src =>
                    src.Phones.Where(p => p.IsDefault && p.IsActive)
                        .Select(p => p.FormattedNumber)
                        .FirstOrDefault() ?? src.PrimaryPhone)) // fallback به فیلد قدیمی
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
                .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts)) // ✅ اضافه شده
                .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.Phones.Where(p => p.IsActive))); // ⭐ NEW

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
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Phones, opt => opt.Ignore()); // ⭐ NEW

            // ==================== ORGANIZATION PHONE MAPPINGS ========== ⭐ NEW

            // OrganizationPhone -> OrganizationPhoneViewModel
            CreateMap<OrganizationPhone, OrganizationPhoneViewModel>()
                .ForMember(dest => dest.PhoneTypeText, opt => opt.MapFrom(src => src.PhoneTypeText))
                .ForMember(dest => dest.FormattedNumber, opt => opt.MapFrom(src => src.FormattedNumber));

            // OrganizationPhoneViewModel -> OrganizationPhone
            CreateMap<OrganizationPhoneViewModel, OrganizationPhone>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore());

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

            // Task assignments mapping
            CreateMap<TaskAssignment, TaskAssignmentViewModel>()
                .ForMember(dest => dest.CompletionNote, opt => opt.MapFrom(src => src.UserReport))
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src =>
                    src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : null))
                .ForMember(dest => dest.AssignerUserName, opt => opt.MapFrom(src =>
                    src.AssignerUser != null ? $"{src.AssignerUser.FirstName} {src.AssignerUser.LastName}" : null))
                .ForMember(dest => dest.AssignedUserProfileImage, opt => opt.MapFrom(src =>
                    src.AssignedUser != null ? (src.AssignedUser.ProfileImagePath ?? "/images/default-avatar.png") : "/images/default-avatar.png"))
                .ForMember(dest => dest.IsFocused, opt => opt.Ignore())
                .ForMember(dest => dest.FocusedDate, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedInTeamName, opt => opt.Ignore())
                .ForMember(dest => dest.PersonalStartDate, opt => opt.Ignore())
                .ForMember(dest => dest.PersonalDueDate, opt => opt.Ignore());

            CreateMap<TaskAssignmentViewModel, TaskAssignment>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.AssignerUser, opt => opt.Ignore())
                .ForMember(dest => dest.UserReport, opt => opt.MapFrom(src => src.CompletionNote));

            // ⭐⭐⭐ NEW - Task Comment Mappings
            CreateMap<TaskComment, TaskCommentViewModel>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src =>
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : "نامشخص"))
                .ForMember(dest => dest.CreatorProfileImage, opt => opt.MapFrom(src =>
                    src.Creator != null ? (src.Creator.ProfileImagePath ?? "/images/default-avatar.png") : "/images/default-avatar.png"))
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

            CreateMap<TaskCommentViewModel, TaskComment>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.MentionedUsers, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // ⭐⭐⭐ NEW - Task Comment Attachment Mappings
            CreateMap<TaskReminderSchedule, TaskReminderViewModel>()
                .ForMember(dest => dest.StartDatePersian, opt => opt.MapFrom(src =>
                    src.StartDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.StartDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.EndDatePersian, opt => opt.MapFrom(src =>
                    src.EndDate.HasValue
                        ? ConvertDateTime.ConvertMiladiToShamsi(src.EndDate.Value, "yyyy/MM/dd")
                        : null))
                .ForMember(dest => dest.NotificationTime, opt => opt.MapFrom(src => src.NotificationTime))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null))
                .ForMember(dest => dest.TaskCode, opt => opt.MapFrom(src => src.Task != null ? src.Task.TaskCode : null))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src =>
                    src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : "سیستم"))
                .ForMember(dest => dest.CreatedDatePersian, opt => opt.MapFrom(src =>
                    ConvertDateTime.ConvertMiladiToShamsi(src.CreatedDate, "yyyy/MM/dd HH:mm")));

            CreateMap<TaskReminderViewModel, TaskReminderSchedule>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId)) // ⭐⭐⭐ اضافه شد
                .ForMember(dest => dest.StartDate, opt => opt.Ignore()) // در controller handle می‌شود
                .ForMember(dest => dest.EndDate, opt => opt.Ignore()) // در controller handle می‌شود
                .ForMember(dest => dest.NotificationTime, opt => opt.MapFrom(src => src.NotificationTime))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ReminderType, opt => opt.MapFrom(src => src.ReminderType))
                .ForMember(dest => dest.IntervalDays, opt => opt.MapFrom(src => src.IntervalDays))
                .ForMember(dest => dest.DaysBeforeDeadline, opt => opt.MapFrom(src => src.DaysBeforeDeadline))
                .ForMember(dest => dest.ScheduledDaysOfMonth, opt => opt.MapFrom(src => src.ScheduledDaysOfMonth))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                // ⭐⭐⭐ Ignore فیلدهای سیستمی
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDefault, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastExecuted, opt => opt.Ignore())
                .ForMember(dest => dest.SentCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsExpired, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiredReason, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiredDate, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.GeneratedEvents, opt => opt.Ignore());

            // ⭐⭐⭐ Task Comment Attachment Mappings
            CreateMap<TaskCommentAttachment, TaskCommentAttachmentViewModel>()
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.FileSize) ? long.Parse(src.FileSize) : 0));

            CreateMap<TaskCommentAttachmentViewModel, TaskCommentAttachment>()
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize.ToString()))
                .ForMember(dest => dest.Comment, opt => opt.Ignore())
                .ForMember(dest => dest.Uploader, opt => opt.Ignore())
                .ForMember(dest => dest.UploaderUserId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.FileUUID, opt => opt.Ignore());
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
