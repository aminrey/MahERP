using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.Repository;
using System;

namespace MahERP.DataModelLayer.Services
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly AppDbContext _Context;

        public UnitOfWork(AppDbContext Db)
        {
            _Context = Db;
        }

        #region User and Role Management
        private GenereicClass<AppUsers> _userManager;
        private GenereicClass<AppRoles> _RoleManager;
        private GenereicClass<RolePattern> _rolePattern;
        private GenereicClass<RolePatternDetails> _rolePatterDetails;
        private GenereicClass<UserRolePattern> _userRolePattern;
        private GenereicClass<PermissionLog> _permissionLog;
        #endregion

        #region Organization
        private GenereicClass<Branch> _branch;
        private GenereicClass<BranchUser> _branchUser;
        private GenereicClass<Team> _team;
        private GenereicClass<TeamMember> _teamMember;
        #endregion

        #region Stakeholder Management
        private GenereicClass<Stakeholder> _stakeholder;
        private GenereicClass<StakeholderCRM> _stakeholderCRM;
        private GenereicClass<StakeholderContact> _stakeholderContact;
        private GenereicClass<StakeholderBranch> _stakeholderBranch;
        private GenereicClass<Contract> _contract;
        #endregion

        #region Task Management
        private GenereicClass<Tasks> _taskRepository;
        private GenereicClass<TaskOperation> _taskOperationRepository;
        private GenereicClass<TaskCategory> _taskCategoryRepository;
        private GenereicClass<TaskAssignment> _taskAssignmentRepository;
        private GenereicClass<TaskAttachment> _taskAttachmentRepository;
        private GenereicClass<TaskComment> _taskCommentRepository;
        private GenereicClass<TaskCommentAttachment> _taskCommentAttachmentRepository;
        private GenereicClass<TaskCommentMention> _taskCommentMentionRepository;
        private GenereicClass<TaskNotification> _taskNotificationRepository;
        private GenereicClass<TaskSchedule> _taskScheduleRepository;
        private GenereicClass<TaskScheduleAssignment> _taskScheduleAssignmentRepository;
        private GenereicClass<TaskScheduleViewer> _taskScheduleViewerRepository;
        private GenereicClass<TaskTemplate> _taskTemplateRepository;
        private GenereicClass<TaskTemplateOperation> _taskTemplateOperationRepository;
        private GenereicClass<TaskViewer> _taskViewerRepository;
        private GenereicClass<PredefinedCopyDescription> _predefinedCopyDescriptionRepository;
        #endregion

        #region CRM Management
        private GenereicClass<CRMInteraction> _crmInteractionRepository;
        private GenereicClass<CRMAttachment> _crmAttachmentRepository;
        private GenereicClass<CRMComment> _crmCommentRepository;
        private GenereicClass<CRMParticipant> _crmParticipantRepository;
        private GenereicClass<CRMTeam> _crmTeamRepository;
        private GenereicClass<TaskCRMDetails> _taskCRMDetailsRepository;
        #endregion

        #region User and Role Management Properties

        public GenereicClass<AppUsers> UserManagerUW
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = new GenereicClass<AppUsers>(_Context);
                }
                return _userManager;
            }
        }

        public GenereicClass<AppRoles> RoleUW
        {
            get
            {
                if (_RoleManager == null)
                {
                    _RoleManager = new GenereicClass<AppRoles>(_Context);
                }
                return _RoleManager;
            }
        }

        public GenereicClass<RolePattern> RolePatternUW
        {
            get
            {
                if (_rolePattern == null)
                {
                    _rolePattern = new GenereicClass<RolePattern>(_Context);
                }
                return _rolePattern;
            }
        }

        public GenereicClass<RolePatternDetails> RolePatternDetailsUW
        {
            get
            {
                if (_rolePatterDetails == null)
                {
                    _rolePatterDetails = new GenereicClass<RolePatternDetails>(_Context);
                }
                return _rolePatterDetails;
            }
        }

        public GenereicClass<UserRolePattern> UserRolePatternUW
        {
            get
            {
                if (_userRolePattern == null)
                {
                    _userRolePattern = new GenereicClass<UserRolePattern>(_Context);
                }
                return _userRolePattern;
            }
        }

        public GenereicClass<PermissionLog> PermissionLogUW
        {
            get
            {
                if (_permissionLog == null)
                {
                    _permissionLog = new GenereicClass<PermissionLog>(_Context);
                }
                return _permissionLog;
            }
        }

        #endregion

        #region Organization Properties

        public GenereicClass<Branch> BranchUW
        {
            get
            {
                if (_branch == null)
                {
                    _branch = new GenereicClass<Branch>(_Context);
                }
                return _branch;
            }
        }

        public GenereicClass<BranchUser> BranchUserUW
        {
            get
            {
                if (_branchUser == null)
                {
                    _branchUser = new GenereicClass<BranchUser>(_Context);
                }
                return _branchUser;
            }
        }

        public GenereicClass<Team> TeamUW
        {
            get
            {
                if (_team == null)
                {
                    _team = new GenereicClass<Team>(_Context);
                }
                return _team;
            }
        }

        public GenereicClass<TeamMember> TeamMemberUW
        {
            get
            {
                if (_teamMember == null)
                {
                    _teamMember = new GenereicClass<TeamMember>(_Context);
                }
                return _teamMember;
            }
        }

        #endregion

        #region Stakeholder Management Properties

        public GenereicClass<Stakeholder> StakeholderUW
        {
            get
            {
                if (_stakeholder == null)
                {
                    _stakeholder = new GenereicClass<Stakeholder>(_Context);
                }
                return _stakeholder;
            }
        }

        public GenereicClass<StakeholderCRM> StakeholderCRMUW
        {
            get
            {
                if (_stakeholderCRM == null)
                {
                    _stakeholderCRM = new GenereicClass<StakeholderCRM>(_Context);
                }
                return _stakeholderCRM;
            }
        }

        public GenereicClass<StakeholderContact> StakeholderContactUW
        {
            get
            {
                if (_stakeholderContact == null)
                {
                    _stakeholderContact = new GenereicClass<StakeholderContact>(_Context);
                }
                return _stakeholderContact;
            }
        }

        public GenereicClass<StakeholderBranch> StakeholderBranchUW
        {
            get
            {
                if (_stakeholderBranch == null)
                {
                    _stakeholderBranch = new GenereicClass<StakeholderBranch>(_Context);
                }
                return _stakeholderBranch;
            }
        }

        public GenereicClass<Contract> ContractUW
        {
            get
            {
                if (_contract == null)
                {
                    _contract = new GenereicClass<Contract>(_Context);
                }
                return _contract;
            }
        }

        #endregion

        #region Task Management Properties

        public GenereicClass<Tasks> TaskUW
        {
            get
            {
                if (_taskRepository == null)
                {
                    _taskRepository = new GenereicClass<Tasks>(_Context);
                }
                return _taskRepository;
            }
        }

        public GenereicClass<TaskOperation> TaskOperationUW
        {
            get
            {
                if (_taskOperationRepository == null)
                {
                    _taskOperationRepository = new GenereicClass<TaskOperation>(_Context);
                }
                return _taskOperationRepository;
            }
        }

        public GenereicClass<TaskCategory> TaskCategoryUW
        {
            get
            {
                if (_taskCategoryRepository == null)
                {
                    _taskCategoryRepository = new GenereicClass<TaskCategory>(_Context);
                }
                return _taskCategoryRepository;
            }
        }

        public GenereicClass<TaskAssignment> TaskAssignmentUW
        {
            get
            {
                if (_taskAssignmentRepository == null)
                {
                    _taskAssignmentRepository = new GenereicClass<TaskAssignment>(_Context);
                }
                return _taskAssignmentRepository;
            }
        }

        public GenereicClass<TaskAttachment> TaskAttachmentUW
        {
            get
            {
                if (_taskAttachmentRepository == null)
                {
                    _taskAttachmentRepository = new GenereicClass<TaskAttachment>(_Context);
                }
                return _taskAttachmentRepository;
            }
        }

        public GenereicClass<TaskComment> TaskCommentUW
        {
            get
            {
                if (_taskCommentRepository == null)
                {
                    _taskCommentRepository = new GenereicClass<TaskComment>(_Context);
                }
                return _taskCommentRepository;
            }
        }

        public GenereicClass<TaskCommentAttachment> TaskCommentAttachmentUW
        {
            get
            {
                if (_taskCommentAttachmentRepository == null)
                {
                    _taskCommentAttachmentRepository = new GenereicClass<TaskCommentAttachment>(_Context);
                }
                return _taskCommentAttachmentRepository;
            }
        }

        public GenereicClass<TaskCommentMention> TaskCommentMentionUW
        {
            get
            {
                if (_taskCommentMentionRepository == null)
                {
                    _taskCommentMentionRepository = new GenereicClass<TaskCommentMention>(_Context);
                }
                return _taskCommentMentionRepository;
            }
        }

        public GenereicClass<TaskNotification> TaskNotificationUW
        {
            get
            {
                if (_taskNotificationRepository == null)
                {
                    _taskNotificationRepository = new GenereicClass<TaskNotification>(_Context);
                }
                return _taskNotificationRepository;
            }
        }

        public GenereicClass<TaskSchedule> TaskScheduleUW
        {
            get
            {
                if (_taskScheduleRepository == null)
                {
                    _taskScheduleRepository = new GenereicClass<TaskSchedule>(_Context);
                }
                return _taskScheduleRepository;
            }
        }

        public GenereicClass<TaskScheduleAssignment> TaskScheduleAssignmentUW
        {
            get
            {
                if (_taskScheduleAssignmentRepository == null)
                {
                    _taskScheduleAssignmentRepository = new GenereicClass<TaskScheduleAssignment>(_Context);
                }
                return _taskScheduleAssignmentRepository;
            }
        }

        public GenereicClass<TaskScheduleViewer> TaskScheduleViewerUW
        {
            get
            {
                if (_taskScheduleViewerRepository == null)
                {
                    _taskScheduleViewerRepository = new GenereicClass<TaskScheduleViewer>(_Context);
                }
                return _taskScheduleViewerRepository;
            }
        }

        public GenereicClass<TaskTemplate> TaskTemplateUW
        {
            get
            {
                if (_taskTemplateRepository == null)
                {
                    _taskTemplateRepository = new GenereicClass<TaskTemplate>(_Context);
                }
                return _taskTemplateRepository;
            }
        }

        public GenereicClass<TaskTemplateOperation> TaskTemplateOperationUW
        {
            get
            {
                if (_taskTemplateOperationRepository == null)
                {
                    _taskTemplateOperationRepository = new GenereicClass<TaskTemplateOperation>(_Context);
                }
                return _taskTemplateOperationRepository;
            }
        }

        public GenereicClass<TaskViewer> TaskViewerUW
        {
            get
            {
                if (_taskViewerRepository == null)
                {
                    _taskViewerRepository = new GenereicClass<TaskViewer>(_Context);
                }
                return _taskViewerRepository;
            }
        }

        public GenereicClass<PredefinedCopyDescription> PredefinedCopyDescriptionUW
        {
            get
            {
                if (_predefinedCopyDescriptionRepository == null)
                {
                    _predefinedCopyDescriptionRepository = new GenereicClass<PredefinedCopyDescription>(_Context);
                }
                return _predefinedCopyDescriptionRepository;
            }
        }

        #endregion

        #region CRM Management Properties

        public GenereicClass<CRMInteraction> CRMInteractionUW
        {
            get
            {
                if (_crmInteractionRepository == null)
                {
                    _crmInteractionRepository = new GenereicClass<CRMInteraction>(_Context);
                }
                return _crmInteractionRepository;
            }
        }

        public GenereicClass<CRMAttachment> CRMAttachmentUW
        {
            get
            {
                if (_crmAttachmentRepository == null)
                {
                    _crmAttachmentRepository = new GenereicClass<CRMAttachment>(_Context);
                }
                return _crmAttachmentRepository;
            }
        }

        public GenereicClass<CRMComment> CRMCommentUW
        {
            get
            {
                if (_crmCommentRepository == null)
                {
                    _crmCommentRepository = new GenereicClass<CRMComment>(_Context);
                }
                return _crmCommentRepository;
            }
        }

        public GenereicClass<CRMParticipant> CRMParticipantUW
        {
            get
            {
                if (_crmParticipantRepository == null)
                {
                    _crmParticipantRepository = new GenereicClass<CRMParticipant>(_Context);
                }
                return _crmParticipantRepository;
            }
        }

        public GenereicClass<CRMTeam> CRMTeamUW
        {
            get
            {
                if (_crmTeamRepository == null)
                {
                    _crmTeamRepository = new GenereicClass<CRMTeam>(_Context);
                }
                return _crmTeamRepository;
            }
        }

        public GenereicClass<TaskCRMDetails> TaskCRMDetailsUW
        {
            get
            {
                if (_taskCRMDetailsRepository == null)
                {
                    _taskCRMDetailsRepository = new GenereicClass<TaskCRMDetails>(_Context);
                }
                return _taskCRMDetailsRepository;
            }
        }

        #endregion

        public int Save()
        {
            return _Context.SaveChanges();
        }

        public void Dispose()
        {
            _Context.Dispose();
        }
    }
}