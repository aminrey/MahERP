using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly AppDbContext _Context;

        // ⭐ فیلدهای Transaction Management
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        public UnitOfWork(AppDbContext Db)
        {
            _Context = Db ?? throw new ArgumentNullException(nameof(Db));
        }

        #region Transaction Management Implementation

        /// <summary>
        /// شروع تراکنش جدید
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException(
                    "تراکنش قبلاً شروع شده است. لطفاً ابتدا تراکنش فعلی را Commit یا Rollback کنید.");
            }

            _transaction = await _Context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// تأیید و ذخیره تراکنش
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "هیچ تراکنش فعالی برای Commit وجود ندارد.");
            }

            try
            {
                // ابتدا تغییرات را ذخیره کن
                await _Context.SaveChangesAsync();

                // سپس تراکنش را Commit کن
                await _transaction.CommitAsync();
            }
            catch
            {
                // در صورت خطا، Rollback خودکار
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                // پاکسازی تراکنش
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// لغو و برگشت تراکنش
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    // لاگ خطا در صورت نیاز
                    System.Diagnostics.Debug.WriteLine($"خطا در Rollback تراکنش: {ex.Message}");
                    throw;
                }
                finally
                {
                    await DisposeTransactionAsync();
                }
            }
        }

        /// <summary>
        /// بررسی وجود تراکنش فعال
        /// </summary>
        public bool HasActiveTransaction => _transaction != null;

        /// <summary>
        /// پاکسازی تراکنش
        /// </summary>
        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

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
        private GenereicClass<BranchTaskCategoryStakeholder> _branchTaskCategory;
        private GenereicClass<Team> _team;
        private GenereicClass<TeamMember> _teamMember;
        public GenereicClass<TeamPosition> _TeamPosition;
        #endregion

        #region Core Notification System - سیستم نوتیفیکیشن کلی
        private GenereicClass<CoreNotification> _coreNotification;
        private GenereicClass<CoreNotificationDetail> _coreNotificationDetail;
        private GenereicClass<CoreNotificationDelivery> _coreNotificationDelivery;
        private GenereicClass<CoreNotificationSetting> _coreNotificationSetting;
        #endregion

        #region Stakeholder Management
        private GenereicClass<Stakeholder> _stakeholder;
        private GenereicClass<StakeholderCRM> _stakeholderCRM;
        private GenereicClass<StakeholderContact> _stakeholderContact;
        private GenereicClass<StakeholderBranch> _stakeholderBranch;
        private GenereicClass<Contract> _contract;
        private GenereicClass<StakeholderOrganization> _stakeholderOrganization;
        private GenereicClass<StakeholderOrganizationPosition> _stakeholderOrganizationPosition;
        private GenereicClass<StakeholderOrganizationMember> _stakeholderOrganizationMember;
        #endregion
        // ⭐⭐⭐ NEW: Contacts & Organizations ⭐⭐⭐
        private GenereicClass<Contact> _contactUW;
        private GenereicClass<ContactPhone> _contactPhoneUW;
        private GenereicClass<Organization> _organizationUW;
        private GenereicClass<OrganizationDepartment> _organizationDepartmentUW;
        private GenereicClass<DepartmentPosition> _departmentPositionUW;
        private GenereicClass<DepartmentMember> _departmentMemberUW;
        private GenereicClass<OrganizationContact> _organizationContactUW;

        private GenereicClass<BranchContact> _branchContactUW;
        private GenereicClass<BranchOrganization> _branchOrganizationUW;
        private GenereicClass<Settings> _SettingsUW;

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
        private GenereicClass<TaskViewPermission> _taskViewPermissionRepository;
        private GenereicClass<PredefinedCopyDescription> _predefinedCopyDescriptionRepository;
        private GenereicClass<TaskReminderSchedule> _taskReminderScheduleRepository;
        private GenereicClass<TaskReminderEvent> _taskReminderEventRepository;
        private GenereicClass<TaskMyDay> _taskMyDayRepository;
        private GenereicClass<TaskHistory> _taskHistory;

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

        public GenereicClass<TeamPosition> TeamPositionUW
        {
            get
            {
                if (_TeamPosition == null)
                {
                    _TeamPosition = new GenereicClass<TeamPosition>(_Context);
                }
                return _TeamPosition;
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

        public GenereicClass<BranchTaskCategoryStakeholder> BranchTaskCategoryUW
        {
            get
            {
                if (_branchTaskCategory == null)
                {
                    _branchTaskCategory = new GenereicClass<BranchTaskCategoryStakeholder>(_Context);
                }
                return _branchTaskCategory;
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

        #region Core Notification System Properties

        public GenereicClass<Settings> SettingsUW
        {
            get
            {
                if (_SettingsUW == null)
                {
                    _SettingsUW = new GenereicClass<Settings>(_Context);
                }
                return _SettingsUW;
            }
        }
        public GenereicClass<CoreNotification> CoreNotificationUW
        {
            get
            {
                if (_coreNotification == null)
                {
                    _coreNotification = new GenereicClass<CoreNotification>(_Context);
                }
                return _coreNotification;
            }
        }

        public GenereicClass<CoreNotificationDetail> CoreNotificationDetailUW
        {
            get
            {
                if (_coreNotificationDetail == null)
                {
                    _coreNotificationDetail = new GenereicClass<CoreNotificationDetail>(_Context);
                }
                return _coreNotificationDetail;
            }
        }

        public GenereicClass<CoreNotificationDelivery> CoreNotificationDeliveryUW
        {
            get
            {
                if (_coreNotificationDelivery == null)
                {
                    _coreNotificationDelivery = new GenereicClass<CoreNotificationDelivery>(_Context);
                }
                return _coreNotificationDelivery;
            }
        }

        public GenereicClass<CoreNotificationSetting> CoreNotificationSettingUW
        {
            get
            {
                if (_coreNotificationSetting == null)
                {
                    _coreNotificationSetting = new GenereicClass<CoreNotificationSetting>(_Context);
                }
                return _coreNotificationSetting;
            }
        }

        #endregion

        #region Stakeholder Management Properties



        // ⭐⭐⭐ NEW: Contacts & Organizations Properties ⭐⭐⭐
        public GenereicClass<Contact> ContactUW
        {
            get
            {
                if (_contactUW == null)
                    _contactUW = new GenereicClass<Contact>(_Context);
                return _contactUW;
            }
        }

        public GenereicClass<ContactPhone> ContactPhoneUW
        {
            get
            {
                if (_contactPhoneUW == null)
                    _contactPhoneUW = new GenereicClass<ContactPhone>(_Context);
                return _contactPhoneUW;
            }
        }

        public GenereicClass<Organization> OrganizationUW
        {
            get
            {
                if (_organizationUW == null)
                    _organizationUW = new GenereicClass<Organization>(_Context);
                return _organizationUW;
            }
        }

        public GenereicClass<OrganizationDepartment> OrganizationDepartmentUW
        {
            get
            {
                if (_organizationDepartmentUW == null)
                    _organizationDepartmentUW = new GenereicClass<OrganizationDepartment>(_Context);
                return _organizationDepartmentUW;
            }
        }

        public GenereicClass<DepartmentPosition> DepartmentPositionUW
        {
            get
            {
                if (_departmentPositionUW == null)
                    _departmentPositionUW = new GenereicClass<DepartmentPosition>(_Context);
                return _departmentPositionUW;
            }
        }

        public GenereicClass<DepartmentMember> DepartmentMemberUW
        {
            get
            {
                if (_departmentMemberUW == null)
                    _departmentMemberUW = new GenereicClass<DepartmentMember>(_Context);
                return _departmentMemberUW;
            }
        }

        public GenereicClass<OrganizationContact> OrganizationContactUW
        {
            get
            {
                if (_organizationContactUW == null)
                    _organizationContactUW = new GenereicClass<OrganizationContact>(_Context);
                return _organizationContactUW;
            }
        }
        public GenereicClass<BranchContact> BranchContactUW
        {
            get
            {
                if (_branchContactUW == null)
                    _branchContactUW = new GenereicClass<BranchContact>(_Context);
                return _branchContactUW;
            }
        }

        public GenereicClass<BranchOrganization> BranchOrganizationUW
        {
            get
            {
                if (_branchOrganizationUW == null)
                    _branchOrganizationUW = new GenereicClass<BranchOrganization>(_Context);
                return _branchOrganizationUW;
            }
        }

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

        public GenereicClass<StakeholderOrganization> StakeholderOrganizationUW
        {
            get
            {
                if (_stakeholderOrganization == null)
                {
                    _stakeholderOrganization = new GenereicClass<StakeholderOrganization>(_Context);
                }
                return _stakeholderOrganization;
            }
        }

        public GenereicClass<StakeholderOrganizationPosition> StakeholderOrganizationPositionUW
        {
            get
            {
                if (_stakeholderOrganizationPosition == null)
                {
                    _stakeholderOrganizationPosition = new GenereicClass<StakeholderOrganizationPosition>(_Context);
                }
                return _stakeholderOrganizationPosition;
            }
        }

        public GenereicClass<StakeholderOrganizationMember> StakeholderOrganizationMemberUW
        {
            get
            {
                if (_stakeholderOrganizationMember == null)
                {
                    _stakeholderOrganizationMember = new GenereicClass<StakeholderOrganizationMember>(_Context);
                }
                return _stakeholderOrganizationMember;
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

        public GenereicClass<TaskViewPermission> TaskViewPermissionUW
        {
            get
            {
                if (_taskViewPermissionRepository == null)
                {
                    _taskViewPermissionRepository = new GenereicClass<TaskViewPermission>(_Context);
                }
                return _taskViewPermissionRepository;
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

        public GenereicClass<TaskReminderSchedule> TaskReminderScheduleUW
        {
            get
            {
                if (_taskReminderScheduleRepository == null)
                {
                    _taskReminderScheduleRepository = new GenereicClass<TaskReminderSchedule>(_Context);
                }
                return _taskReminderScheduleRepository;
            }
        }

        public GenereicClass<TaskReminderEvent> TaskReminderEventUW
        {
            get
            {
                if (_taskReminderEventRepository == null)
                {
                    _taskReminderEventRepository = new GenereicClass<TaskReminderEvent>(_Context);
                }
                return _taskReminderEventRepository;
            }
        }

        public GenereicClass<TaskMyDay> TaskMyDayUW
        {
            get
            {
                if (_taskMyDayRepository == null)
                {
                    _taskMyDayRepository = new GenereicClass<TaskMyDay>(_Context);
                }
                return _taskMyDayRepository;
            }
        }
        public GenereicClass<TaskHistory> TaskHistoryUW
        {
            get
            {
                if (_taskHistory == null)
                {
                    _taskHistory = new GenereicClass<TaskHistory>(_Context);
                }
                return _taskHistory;
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

        #region Save Methods

        /// <summary>
        /// ذخیره تغییرات (Sync)
        /// </summary>
        public int Save()
        {
            return _Context.SaveChanges();
        }

        /// <summary>
        /// ذخیره تغییرات (Async)
        /// </summary>
        public async Task<int> SaveAsync()
        {
            return await _Context.SaveChangesAsync();
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// پاکسازی منابع
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// پاکسازی منابع (Protected)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // پاکسازی تراکنش در صورت وجود
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    // پاکسازی DbContext
                    _Context?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer برای اطمینان از پاکسازی
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }

        #endregion
    }
}