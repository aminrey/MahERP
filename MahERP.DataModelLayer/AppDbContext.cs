using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts; // ⭐ جدید
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.Email;
using MahERP.DataModelLayer.Entities.Organizations;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.Entities.Notifications;
using MahERP.DataModelLayer.Configurations; // ⭐ اضافه کردن این using

namespace MahERP.DataModelLayer
{
    public class AppDbContext : IdentityDbContext<AppUsers, AppRoles, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> Option) : base(Option)
        {
        }

        // Identity/Users
        public DbSet<RolePattern> RolePattern_Tbl { get; set; }
        public DbSet<RolePatternDetails> RolePatternDetails_Tbl { get; set; }
        public DbSet<UserRolePattern> UserRolePattern_Tbl { get; set; }
        public DbSet<PermissionLog> PermissionLog_Tbl { get; set; }

        // Organization
        public DbSet<Branch> Branch_Tbl { get; set; }
        public DbSet<BranchUser> BranchUser_Tbl { get; set; }
        public DbSet<Team> Team_Tbl { get; set; }
        public DbSet<TeamMember> TeamMember_Tbl { get; set; }
        public DbSet<TeamPosition> TeamPosition_Tbl { get; set; }
        public DbSet<TaskReminderSchedule> TaskReminderSchedule_Tbl { get; set; }
        public DbSet<TaskReminderEvent> TaskReminderEvent_Tbl { get; set; }
        public DbSet<TaskMyDay> TaskMyDay_Tbl { get; set; }

        // ⭐⭐⭐ NEW: Contacts & Organizations ⭐⭐⭐
        public DbSet<Contact> Contact_Tbl { get; set; }
        public DbSet<ContactPhone> ContactPhone_Tbl { get; set; }
        public DbSet<Organization> Organization_Tbl { get; set; }
        public DbSet<OrganizationDepartment> OrganizationDepartment_Tbl { get; set; }
        public DbSet<DepartmentPosition> DepartmentPosition_Tbl { get; set; }
        public DbSet<DepartmentMember> DepartmentMember_Tbl { get; set; }
        public DbSet<OrganizationContact> OrganizationContact_Tbl { get; set; }

        // ✅ NEW: Branch-Contact/Organization
        public DbSet<BranchContact> BranchContact_Tbl { get; set; }
        public DbSet<BranchOrganization> BranchOrganization_Tbl { get; set; }

        // Account Control (Old - Keep for migration compatibility)
        public DbSet<Stakeholder> Stakeholder_Tbl { get; set; }
        public DbSet<StakeholderBranch> StakeholderBranch_Tbl { get; set; }
        public DbSet<StakeholderContact> StakeholderContact_Tbl { get; set; }
        public DbSet<Contract> Contract_Tbl { get; set; }
        public DbSet<StakeholderOrganization> StakeholderOrganization_Tbl { get; set; }
        public DbSet<StakeholderOrganizationPosition> StakeholderOrganizationPosition_Tbl { get; set; }
        public DbSet<StakeholderOrganizationMember> StakeholderOrganizationMember_Tbl { get; set; }

        // Core Activities
        public DbSet<ActivityBase> ActivityBase_Tbl { get; set; }
        public DbSet<ActivityAttachment> ActivityAttachment_Tbl { get; set; }
        public DbSet<ActivityComment> ActivityComment_Tbl { get; set; }
        public DbSet<ActivityCRM> ActivityCRM_Tbl { get; set; }
        public DbSet<ActivityHistory> ActivityHistory_Tbl { get; set; }
        public DbSet<ActivityTask> ActivityTask_Tbl { get; set; }
        public DbSet<UserActivityLog> UserActivityLog_Tbl { get; set; }
        public DbSet<CoreNotification> CoreNotification_Tbl { get; set; }
        public DbSet<CoreNotificationDetail> CoreNotificationDetail_Tbl { get; set; }
        public DbSet<CoreNotificationDelivery> CoreNotificationDelivery_Tbl { get; set; }
        public DbSet<CoreNotificationSetting> CoreNotificationSetting_Tbl { get; set; }

        // Task Management
        public DbSet<Tasks> Tasks_Tbl { get; set; }
        public DbSet<TaskAssignment> TaskAssignment_Tbl { get; set; }
        public DbSet<TaskAttachment> TaskAttachment_Tbl { get; set; }
        public DbSet<TaskCategory> TaskCategory_Tbl { get; set; }
        public DbSet<TaskComment> TaskComment_Tbl { get; set; }
        public DbSet<TaskCommentAttachment> TaskCommentAttachment_Tbl { get; set; }
        public DbSet<TaskCommentMention> TaskCommentMention_Tbl { get; set; }
        public DbSet<TaskNotification> TaskNotification_Tbl { get; set; }
        public DbSet<TaskOperation> TaskOperation_Tbl { get; set; }
        public DbSet<TaskSchedule> TaskSchedule_Tbl { get; set; }
        public DbSet<TaskScheduleAssignment> TaskScheduleAssignment_Tbl { get; set; }
        public DbSet<TaskScheduleViewer> TaskScheduleViewer_Tbl { get; set; }
        public DbSet<TaskTemplate> TaskTemplate_Tbl { get; set; }
        public DbSet<TaskTemplateOperation> TaskTemplateOperation_Tbl { get; set; }
        public DbSet<TaskViewer> TaskViewer_Tbl { get; set; }
        public DbSet<PredefinedCopyDescription> PredefinedCopyDescription_Tbl { get; set; }
        public DbSet<BranchTaskCategoryStakeholder> BranchTaskCategoryStakeholder_Tbl { get; set; }
        public DbSet<TaskViewPermission> TaskViewPermission_Tbl { get; set; }
        public DbSet<TaskOperationWorkLog> TaskOperationWorkLog_Tbl { get; set; }
        public DbSet<TaskHistory> TaskHistory_Tbl { get; set; }
        public DbSet<TaskWorkLog> TaskWorkLog_Tbl { get; set; }

        /// <summary>
        /// ⭐⭐⭐ رونوشت تسک - ناظران دستی
        /// </summary>
        public DbSet<TaskCarbonCopy> TaskCarbonCopy_Tbl { get; set; }

        /// <summary>
        /// ⭐⭐⭐ زمان‌بندی ایجاد خودکار تسک‌ها
        /// </summary>
        public DbSet<ScheduledTaskCreation> ScheduledTaskCreation_Tbl { get; set; }

        // CRM
        public DbSet<CRMInteraction> CRMInteraction_Tbl { get; set; }
        public DbSet<CRMAttachment> CRMAttachment_Tbl { get; set; }
        public DbSet<CRMComment> CRMComment_Tbl { get; set; }
        public DbSet<CRMParticipant> CRMParticipant_Tbl { get; set; }
        public DbSet<CRMTeam> CRMTeam_Tbl { get; set; }
        public DbSet<StakeholderCRM> StakeholderCRM_Tbl { get; set; }
        public DbSet<TaskCRMDetails> TaskCRMDetails_Tbl { get; set; }
        // SMS Management
        public DbSet<SmsProvider> SmsProvider_Tbl { get; set; }
        public DbSet<SmsLog> SmsLog_Tbl { get; set; }
        public DbSet<SmsTemplate> SmsTemplate_Tbl { get; set; }
        public DbSet<SmsTemplateRecipient> SmsTemplateRecipient_Tbl { get; set; } // اضافه کردن DbSet
        // Email Management
        public DbSet<EmailLog> EmailLog_Tbl { get; set; }
        public DbSet<EmailTemplate> EmailTemplate_Tbl { get; set; }
        public DbSet<EmailTemplateRecipient> EmailTemplateRecipient_Tbl { get; set; }
        // Queue Management
        public DbSet<SmsQueue> SmsQueue_Tbl { get; set; }
        public DbSet<EmailQueue> EmailQueue_Tbl { get; set; }
        public DbSet<Settings> Settings_Tbl { get; set; }
        // ✅ NEW: Permission System
        public DbSet<Permission> Permission_Tbl { get; set; }
        public DbSet<Role> Role_Tbl { get; set; }
        public DbSet<RolePermission> RolePermission_Tbl { get; set; }
        public DbSet<UserRole> UserRole_Tbl { get; set; }
        public DbSet<UserPermission> UserPermission_Tbl { get; set; }
        public DbSet<PermissionChangeLog> PermissionChangeLog_Tbl { get; set; }


        /// <summary>
        /// گروه‌های افراد در سطح کل سیستم
        /// </summary>
        public DbSet<ContactGroup> ContactGroup_Tbl { get; set; }

        /// <summary>
        /// اعضای گروه‌های کل سیستم
        /// </summary>
        public DbSet<ContactGroupMember> ContactGroupMember_Tbl { get; set; }

        /// <summary>
        /// گروه‌های افراد در سطح شعبه
        /// </summary>
        public DbSet<BranchContactGroup> BranchContactGroup_Tbl { get; set; }

        /// <summary>
        /// اعضای گروه‌های شعبه
        /// </summary>
        public DbSet<BranchContactGroupMember> BranchContactGroupMember_Tbl { get; set; }

        // ⭐⭐⭐ NEW: Organization Grouping System ⭐⭐⭐
        
        /// <summary>
        /// گروه‌های سازمان‌ها در سطح کل سیستم
        /// </summary>
        public DbSet<OrganizationGroup> OrganizationGroup_Tbl { get; set; }
        
        /// <summary>
        /// اعضای گروه‌های کل سیستم
        /// </summary>
        public DbSet<OrganizationGroupMember> OrganizationGroupMember_Tbl { get; set; }
        
        /// <summary>
        /// گروه‌های سازمان‌ها در سطح شعبه
        /// </summary>
        public DbSet<BranchOrganizationGroup> BranchOrganizationGroup_Tbl { get; set; }
        
        /// <summary>
        /// اعضای گروه‌های شعبه
        /// </summary>
        public DbSet<BranchOrganizationGroupMember> BranchOrganizationGroupMember_Tbl { get; set; }

        // ⭐⭐⭐ NEW: Module Access Management
        public DbSet<UserModulePermission> UserModulePermission_Tbl { get; set; }
        public DbSet<TeamModulePermission> TeamModulePermission_Tbl { get; set; }
        public DbSet<BranchModulePermission> BranchModulePermission_Tbl { get; set; }
        public DbSet<UserModulePreference> UserModulePreference_Tbl { get; set; }

        // ⭐⭐⭐ Notification System Tables
        public DbSet<NotificationModuleConfig> NotificationModuleConfig_Tbl { get; set; }
        public DbSet<NotificationTypeConfig> NotificationTypeConfig_Tbl { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreference_Tbl { get; set; }
        public DbSet<NotificationBlacklist> NotificationBlacklist_Tbl { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipient_Tbl { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplate_Tbl { get; set; }
        public DbSet<NotificationTemplateRecipient> NotificationTemplateRecipient_Tbl { get; set; } // ⭐ جدید
        public DbSet<NotificationTemplateVariable> NotificationTemplateVariable_Tbl { get; set; }
        public DbSet<NotificationTemplateHistory> NotificationTemplateHistory_Tbl { get; set; }
        public DbSet<NotificationScheduledMessage> NotificationScheduledMessage_Tbl { get; set; }
        public DbSet<NotificationDeliveryStats> NotificationDeliveryStats_Tbl { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⭐ کانفیگ Entity Configurations
            NotificationEntitiesConfiguration.Configure(modelBuilder);
            ContactOrganizationConfiguration.Configure(modelBuilder);
            CoreEntitiesConfiguration.Configure(modelBuilder);
            
            // ⭐⭐⭐ کانفیگ زمان‌بندی تسک‌ها
            modelBuilder.ApplyConfiguration(new ScheduledTaskCreationConfiguration());
            
            // ⭐ کانفیگ Seed Data
            SeedDataConfiguration.Configure(modelBuilder);
        }
    }
}