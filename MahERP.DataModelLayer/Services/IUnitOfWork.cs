using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository;
using System;

namespace MahERP.DataModelLayer.Services
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        bool HasActiveTransaction { get; }
        #region User and Role Management
        GenereicClass<AppUsers> UserManagerUW { get; }
        GenereicClass<AppRoles> RoleUW { get; }
        GenereicClass<RolePattern> RolePatternUW { get; }
        GenereicClass<RolePatternDetails> RolePatternDetailsUW { get; }
        GenereicClass<UserRolePattern> UserRolePatternUW { get; }
        GenereicClass<PermissionLog> PermissionLogUW { get; }
        #endregion

        #region Organization
        GenereicClass<Branch> BranchUW { get; }
        GenereicClass<BranchUser> BranchUserUW { get; }
        GenereicClass<BranchTaskCategoryStakeholder> BranchTaskCategoryUW { get; }
        GenereicClass<Team> TeamUW { get; }
        GenereicClass<TeamMember> TeamMemberUW { get; }
        GenereicClass<TeamPosition> TeamPositionUW { get; }
        #endregion

        #region Core Notification System - سیستم نوتیفیکیشن کلی
        GenereicClass<CoreNotification> CoreNotificationUW { get; }
        GenereicClass<CoreNotificationDetail> CoreNotificationDetailUW { get; }
        GenereicClass<CoreNotificationDelivery> CoreNotificationDeliveryUW { get; }
        GenereicClass<CoreNotificationSetting> CoreNotificationSettingUW { get; }
        GenereicClass<Settings> SettingsUW { get; }
        #endregion


        #region Stakeholder Management
        GenereicClass<Contact> ContactUW { get; }
        GenereicClass<ContactPhone> ContactPhoneUW { get; }
        GenereicClass<Organization> OrganizationUW { get; }
        GenereicClass<OrganizationDepartment> OrganizationDepartmentUW { get; }
        GenereicClass<DepartmentPosition> DepartmentPositionUW { get; }
        GenereicClass<DepartmentMember> DepartmentMemberUW { get; }
        GenereicClass<OrganizationContact> OrganizationContactUW { get; }

        GenereicClass<BranchContact> BranchContactUW { get; }
        GenereicClass<BranchOrganization> BranchOrganizationUW { get; }
        GenereicClass<Stakeholder> StakeholderUW { get; }
        GenereicClass<StakeholderCRM> StakeholderCRMUW { get; }
        GenereicClass<StakeholderContact> StakeholderContactUW { get; }
        GenereicClass<StakeholderBranch> StakeholderBranchUW { get; }
        GenereicClass<Contract> ContractUW { get; }
        // Stakeholder Organization
        GenereicClass<StakeholderOrganization> StakeholderOrganizationUW { get; }
        GenereicClass<StakeholderOrganizationPosition> StakeholderOrganizationPositionUW { get; }
        GenereicClass<StakeholderOrganizationMember> StakeholderOrganizationMemberUW { get; }

        #endregion

        #region Task Management
        GenereicClass<Tasks> TaskUW { get; }
        GenereicClass<TaskOperation> TaskOperationUW { get; }
        GenereicClass<TaskCategory> TaskCategoryUW { get; }
        GenereicClass<TaskAssignment> TaskAssignmentUW { get; }
        GenereicClass<TaskAttachment> TaskAttachmentUW { get; }
        GenereicClass<TaskComment> TaskCommentUW { get; }
        GenereicClass<TaskCommentAttachment> TaskCommentAttachmentUW { get; }
        GenereicClass<TaskCommentMention> TaskCommentMentionUW { get; }
        GenereicClass<TaskNotification> TaskNotificationUW { get; }
        GenereicClass<TaskSchedule> TaskScheduleUW { get; }
        GenereicClass<TaskScheduleAssignment> TaskScheduleAssignmentUW { get; }
        GenereicClass<TaskScheduleViewer> TaskScheduleViewerUW { get; }
        GenereicClass<TaskTemplate> TaskTemplateUW { get; }
        GenereicClass<TaskTemplateOperation> TaskTemplateOperationUW { get; }
        GenereicClass<TaskViewer> TaskViewerUW { get; }
        GenereicClass<PredefinedCopyDescription> PredefinedCopyDescriptionUW { get; }
        GenereicClass<TaskViewPermission> TaskViewPermissionUW { get; }
        GenereicClass<TaskReminderSchedule> TaskReminderScheduleUW { get; }
        GenereicClass<TaskReminderEvent> TaskReminderEventUW { get; }
        GenereicClass<TaskMyDay> TaskMyDayUW { get; }
        GenereicClass<TaskHistory> TaskHistoryUW { get; }

        #endregion

        #region CRM Management
        GenereicClass<CRMInteraction> CRMInteractionUW { get; }
        GenereicClass<CRMAttachment> CRMAttachmentUW { get; }
        GenereicClass<CRMComment> CRMCommentUW { get; }
        GenereicClass<CRMParticipant> CRMParticipantUW { get; }
        GenereicClass<CRMTeam> CRMTeamUW { get; }
        GenereicClass<TaskCRMDetails> TaskCRMDetailsUW { get; }
        #endregion

        int Save();
            Task<int> SaveAsync();

    }
}