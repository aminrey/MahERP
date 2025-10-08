using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<TaskMyDay> TaskMyDay_Tbl { get; set; } // اضافه شده

        // Account Control
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
    
        // CRM
        public DbSet<CRMInteraction> CRMInteraction_Tbl { get; set; }
        public DbSet<CRMAttachment> CRMAttachment_Tbl { get; set; }
        public DbSet<CRMComment> CRMComment_Tbl { get; set; }
        public DbSet<CRMParticipant> CRMParticipant_Tbl { get; set; }
        public DbSet<CRMTeam> CRMTeam_Tbl { get; set; }
        public DbSet<StakeholderCRM> StakeholderCRM_Tbl { get; set; }
        public DbSet<TaskCRMDetails> TaskCRMDetails_Tbl { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================== SEED DEFAULT DATA ========================
            SeedDefaultBranches(modelBuilder);
            SeedDefaultRoles(modelBuilder);
            SeedDefaultTaskCategories(modelBuilder);
            SeedDefaultPredefinedCopyDescriptions(modelBuilder);
            SeedDefaultRolePatterns(modelBuilder);

            // ======================== ROLE PATTERN RELATIONSHIPS ========================
            // RolePattern relationships
            modelBuilder.Entity<RolePattern>()
                .HasOne(rp => rp.Creator)
                .WithMany()
                .HasForeignKey(rp => rp.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolePattern>()
                .HasOne(rp => rp.LastUpdater)
                .WithMany()
                .HasForeignKey(rp => rp.LastUpdaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolePatternDetails relationships
            modelBuilder.Entity<RolePatternDetails>()
                .HasOne(rpd => rpd.RolePattern)
                .WithMany(rp => rp.RolePatternDetails)
                .HasForeignKey(rpd => rpd.RolePatternId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserRolePattern relationships
            modelBuilder.Entity<UserRolePattern>()
                .HasOne(urp => urp.User)
                .WithMany()
                .HasForeignKey(urp => urp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRolePattern>()
                .HasOne(urp => urp.RolePattern)
                .WithMany(rp => rp.UserRolePatterns)
                .HasForeignKey(urp => urp.RolePatternId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRolePattern>()
                .HasOne(urp => urp.AssignedByUser)
                .WithMany()
                .HasForeignKey(urp => urp.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PermissionLog relationships
            modelBuilder.Entity<PermissionLog>()
                .HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================== USER RELATED RELATIONSHIPS ========================

            // Contract-specific relationships
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.LastUpdater)
                .WithMany()
                .HasForeignKey(c => c.LastUpdaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Stakeholder)
                .WithMany(s => s.Contracts)
                .HasForeignKey(c => c.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Stakeholder relationships
            modelBuilder.Entity<Stakeholder>()
                .HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // StakeholderBranch relationship
            modelBuilder.Entity<StakeholderBranch>()
                .HasOne(sb => sb.Stakeholder)
                .WithMany()
                .HasForeignKey(sb => sb.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StakeholderBranch>()
                .HasOne(sb => sb.Branch)
                .WithMany()
                .HasForeignKey(sb => sb.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StakeholderBranch>()
                .HasOne(sb => sb.Creator)
                .WithMany()
                .HasForeignKey(sb => sb.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // StakeholderContact relationship
            modelBuilder.Entity<StakeholderContact>()
                .HasOne(sc => sc.Stakeholder)
                .WithMany(s => s.StakeholderContacts)
                .HasForeignKey(sc => sc.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StakeholderContact>()
                .HasOne(sc => sc.Creator)  // تصحیح شده
                .WithMany()
                .HasForeignKey(sc => sc.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMInteraction relationships - اصلاح شده
            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.LastUpdater)
                .WithMany()
                .HasForeignKey(c => c.LastUpdaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.Stakeholder)
                .WithMany()
                .HasForeignKey(c => c.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.StakeholderContact)
                .WithMany()
                .HasForeignKey(c => c.StakeholderContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.Branch)
                .WithMany()
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMInteraction>()
                .HasOne(c => c.Contract)
                .WithMany()
                .HasForeignKey(c => c.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            // StakeholderCRM relationships
            modelBuilder.Entity<StakeholderCRM>()
                .HasOne(c => c.Stakeholder)
                .WithMany()
                .HasForeignKey(c => c.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StakeholderCRM>()
                .HasOne(c => c.SalesRep)
                .WithMany()
                .HasForeignKey(c => c.SalesRepUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskCRMDetails relationships
            modelBuilder.Entity<TaskCRMDetails>()
                .HasOne(t => t.Task)
                .WithMany()
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskCRMDetails>()
                .HasOne(t => t.StakeholderContact)
                .WithMany()
                .HasForeignKey(t => t.StakeholderContactId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity relationships - اصلاح شده
            modelBuilder.Entity<ActivityBase>()
                .HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityBase>()
                .HasOne(a => a.LastUpdater)
                .WithMany()
                .HasForeignKey(a => a.LastUpdaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityBase>()
                .HasOne(a => a.Stakeholder)
                .WithMany()
                .HasForeignKey(a => a.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityBase>()
                .HasOne(a => a.Contract)
                .WithMany()
                .HasForeignKey(a => a.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityBase>()
                .HasOne(a => a.Branch)
                .WithMany()
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityCRM relationships - اصلاح شده
            modelBuilder.Entity<ActivityCRM>()
                .HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityCRMs)
                .HasForeignKey(ac => ac.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityCRM>()
                .HasOne(ac => ac.CRMInteraction)
                .WithMany(c => c.ActivityCRMs)
                .HasForeignKey(ac => ac.CRMId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityCRM>()
                .HasOne(ac => ac.Creator)
                .WithMany()
                .HasForeignKey(ac => ac.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityTask relationships - اصلاح شده
            modelBuilder.Entity<ActivityTask>()
                .HasOne(at => at.Activity)
                .WithMany(a => a.ActivityTasks)
                .HasForeignKey(at => at.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityTask>()
                .HasOne(at => at.Task)
                .WithMany()
                .HasForeignKey(at => at.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityTask>()
                .HasOne(at => at.Creator)
                .WithMany()
                .HasForeignKey(at => at.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityComment relationships
            modelBuilder.Entity<ActivityComment>()
                .HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityComments)
                .HasForeignKey(ac => ac.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityComment>()
                .HasOne(ac => ac.Creator)
                .WithMany()
                .HasForeignKey(ac => ac.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityComment>()
                .HasOne(ac => ac.ParentComment)
                .WithMany()
                .HasForeignKey(ac => ac.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityAttachment relationships
            modelBuilder.Entity<ActivityAttachment>()
                .HasOne(aa => aa.Activity)
                .WithMany(a => a.ActivityAttachments)
                .HasForeignKey(aa => aa.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityAttachment>()
                .HasOne(aa => aa.Uploader)
                .WithMany()
                .HasForeignKey(aa => aa.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityHistory relationships
            modelBuilder.Entity<ActivityHistory>()
                .HasOne(ah => ah.Activity)
                .WithMany(a => a.ActivityHistories)
                .HasForeignKey(ah => ah.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityHistory>()
                .HasOne(ah => ah.Creator)
                .WithMany()
                .HasForeignKey(ah => ah.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Team relationships
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Manager)
                .WithMany()
                .HasForeignKey(t => t.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.LastUpdater)
                .WithMany()
                .HasForeignKey(t => t.LastUpdaterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.ParentTeam)
                .WithMany()
                .HasForeignKey(t => t.ParentTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Branch)
                .WithMany()
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // TeamMember relationships
            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMemberships)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.AddedByUser)
                .WithMany()
                .HasForeignKey(tm => tm.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // BranchUser relationships
            modelBuilder.Entity<BranchUser>()
                .HasOne(bu => bu.Branch)
                .WithMany()
                .HasForeignKey(bu => bu.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchUser>()
                .HasOne(bu => bu.User)
                .WithMany()
                .HasForeignKey(bu => bu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchUser>()
                .HasOne(bu => bu.AssignedByUser)
                .WithMany()
                .HasForeignKey(bu => bu.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskSchedule relationships
            modelBuilder.Entity<TaskSchedule>()
                .HasOne(ts => ts.Creator)
                .WithMany()
                .HasForeignKey(ts => ts.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskSchedule>()
                .HasOne(ts => ts.Modifier)
                .WithMany()
                .HasForeignKey(ts => ts.ModifierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskSchedule>()
                .HasOne(ts => ts.TaskTemplate)
                .WithMany()
                .HasForeignKey(ts => ts.TaskTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskScheduleAssignment relationships
            modelBuilder.Entity<TaskScheduleAssignment>()
                .HasOne(tsa => tsa.User)
                .WithMany()
                .HasForeignKey(tsa => tsa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskScheduleAssignment>()
                .HasOne(tsa => tsa.Creator)
                .WithMany()
                .HasForeignKey(tsa => tsa.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskScheduleAssignment>()
                .HasOne(tsa => tsa.Schedule)
                .WithMany()
                .HasForeignKey(tsa => tsa.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskScheduleAssignment>()
                .HasOne(tsa => tsa.PredefinedCopyDescription)
                .WithMany()
                .HasForeignKey(tsa => tsa.PredefinedCopyDescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskScheduleViewer relationships
            modelBuilder.Entity<TaskScheduleViewer>()
                .HasOne(tsv => tsv.User)
                .WithMany()
                .HasForeignKey(tsv => tsv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskScheduleViewer>()
                .HasOne(tsv => tsv.AddedByUser)
                .WithMany()
                .HasForeignKey(tsv => tsv.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskScheduleViewer>()
                .HasOne(tsv => tsv.Schedule)
                .WithMany()
                .HasForeignKey(tsv => tsv.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // CRM related relationships - اصلاح شده
            // CRMAttachment relationships
            modelBuilder.Entity<CRMAttachment>()
                .HasOne(ca => ca.CRMInteraction)
                .WithMany(c => c.CRMAttachments)
                .HasForeignKey(ca => ca.CRMInteractionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMAttachment>()
                .HasOne(ca => ca.Uploader)
                .WithMany()
                .HasForeignKey(ca => ca.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMComment relationships
            modelBuilder.Entity<CRMComment>()
                .HasOne(cc => cc.CRMInteraction)
                .WithMany(c => c.CRMComments)
                .HasForeignKey(cc => cc.CRMInteractionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMComment>()
                .HasOne(cc => cc.Creator)
                .WithMany()
                .HasForeignKey(cc => cc.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMComment>()
                .HasOne(cc => cc.ParentComment)
                .WithMany()
                .HasForeignKey(cc => cc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMParticipant relationships
            modelBuilder.Entity<CRMParticipant>()
                .HasOne(cp => cp.CRMInteraction)
                .WithMany(c => c.CRMParticipants)
                .HasForeignKey(cp => cp.CRMInteractionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMParticipant>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMParticipant>()
                .HasOne(cp => cp.StakeholderContact)
                .WithMany()
                .HasForeignKey(cp => cp.StakeholderContactId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMTeam relationships
            modelBuilder.Entity<CRMTeam>()
                .HasOne(ct => ct.CRMInteraction)
                .WithMany(c => c.CRMTeams)
                .HasForeignKey(ct => ct.CRMInteractionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMTeam>()
                .HasOne(ct => ct.Team)
                .WithMany()
                .HasForeignKey(ct => ct.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMTeam>()
                .HasOne(ct => ct.Creator)
                .WithMany()
                .HasForeignKey(ct => ct.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskViewer relationships
            modelBuilder.Entity<TaskViewer>()
                .HasOne(tv => tv.User)
                .WithMany()
                .HasForeignKey(tv => tv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskViewer>()
                .HasOne(tv => tv.AddedByUser)
                .WithMany()
                .HasForeignKey(tv => tv.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskViewer>()
                .HasOne(tv => tv.Task)
                .WithMany()
                .HasForeignKey(tv => tv.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================== TASK RELATED RELATIONSHIPS ========================
            // TaskAssignment relationships
            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.AssignedUser)
                .WithMany()
                .HasForeignKey(ta => ta.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.AssignerUser)
                .WithMany()
                .HasForeignKey(ta => ta.AssignerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.TaskAssignments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.PredefinedCopyDescription)
                .WithMany()
                .HasForeignKey(ta => ta.PredefinedCopyDescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskComment relationships
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Creator)
                .WithMany()
                .HasForeignKey(tc => tc.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskComments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.ParentComment)
                .WithMany()
                .HasForeignKey(tc => tc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskCommentAttachment relationships
            modelBuilder.Entity<TaskCommentAttachment>()
                .HasOne(tca => tca.Comment)
                .WithMany()
                .HasForeignKey(tca => tca.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskCommentAttachment>()
                .HasOne(tca => tca.Uploader)
                .WithMany()
                .HasForeignKey(tca => tca.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskCommentMention relationships
            modelBuilder.Entity<TaskCommentMention>()
                .HasOne(tcm => tcm.Comment)
                .WithMany()
                .HasForeignKey(tcm => tcm.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskCommentMention>()
                .HasOne(tcm => tcm.MentionedUser)
                .WithMany()
                .HasForeignKey(tcm => tcm.MentionedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskAttachment relationships
            modelBuilder.Entity<TaskAttachment>()
                .HasOne(ta => ta.Task)
                .WithMany()
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(ta => ta.Uploader)
                .WithMany()
                .HasForeignKey(ta => ta.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskOperation relationships
            modelBuilder.Entity<TaskOperation>()
                .HasOne(to => to.CompletedByUser)
                .WithMany()
                .HasForeignKey(to => to.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskOperation>()
                .HasOne(to => to.Creator)
                .WithMany()
                .HasForeignKey(to => to.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskOperation>()
                .HasOne(to => to.Task)
                .WithMany(t => t.TaskOperations)
                .HasForeignKey(to => to.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskNotification relationships
            modelBuilder.Entity<TaskNotification>()
                .HasOne(tn => tn.Task)
                .WithMany()
                .HasForeignKey(tn => tn.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskNotification>()
                .HasOne(tn => tn.Comment)
                .WithMany()
                .HasForeignKey(tn => tn.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskNotification>()
                .HasOne(tn => tn.Operation)
                .WithMany()
                .HasForeignKey(tn => tn.OperationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskNotification>()
                .HasOne(tn => tn.Recipient)
                .WithMany()
                .HasForeignKey(tn => tn.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task relationships
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Branch)
                .WithMany()
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.ParentTask)
                .WithMany()
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.TaskSchedule)
                .WithMany()
                .HasForeignKey(t => t.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Team)
                .WithMany()
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Stakeholder)
                .WithMany()
                .HasForeignKey(t => t.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Contract)
                .WithMany()
                .HasForeignKey(t => t.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.TaskCategory)
                .WithMany()
                .HasForeignKey(t => t.TaskCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskTemplate relationships
            modelBuilder.Entity<TaskTemplate>()
                .HasOne(tt => tt.Category)
                .WithMany()
                .HasForeignKey(tt => tt.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskTemplate>()
                .HasOne(tt => tt.Creator)
                .WithMany()
                .HasForeignKey(tt => tt.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskTemplateOperation relationships
            modelBuilder.Entity<TaskTemplateOperation>()
                .HasOne(tto => tto.Template)
                .WithMany()
                .HasForeignKey(tto => tto.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // ======================== OTHER RELATIONSHIPS ========================
            // Branch self-referencing relationship
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.ParentBranch)
                .WithMany()
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskCategory self-referencing relationship
            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.ParentCategory)
                .WithMany()
                .HasForeignKey(tc => tc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // BranchTaskCategoryStakeholder relationships
            modelBuilder.Entity<BranchTaskCategoryStakeholder>()
                .HasOne(btcs => btcs.Branch)
                .WithMany()
                .HasForeignKey(btcs => btcs.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchTaskCategoryStakeholder>()
                .HasOne(btcs => btcs.TaskCategory)
                .WithMany()
                .HasForeignKey(btcs => btcs.TaskCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchTaskCategoryStakeholder>()
                .HasOne(btcs => btcs.Stakeholder)
                .WithMany()
                .HasForeignKey(btcs => btcs.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchTaskCategoryStakeholder>()
                .HasOne(btcs => btcs.AssignedByUser)
                .WithMany()
                .HasForeignKey(btcs => btcs.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUsers self-referencing relationship
            modelBuilder.Entity<AppUsers>()
                .HasMany(u => u.ManagedUsers)
                .WithOne(u => u.DirectManager)
                .HasForeignKey(u => u.DirectManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================== UNIQUE CONSTRAINTS ========================
            // یکتا بودن نام کاربری در هر الگوی نقش
            modelBuilder.Entity<UserRolePattern>()
                .HasIndex(urp => new { urp.UserId, urp.RolePatternId })
                .IsUnique()
                .HasDatabaseName("IX_UserRolePattern_User_Pattern");

            // یکتا بودن نام الگوی نقش
            modelBuilder.Entity<RolePattern>()
                .HasIndex(rp => rp.PatternName)
                .IsUnique()
                .HasDatabaseName("IX_RolePattern_PatternName");
        }

        // ======================== PRIVATE SEED METHODS ========================

        private static void SeedDefaultBranches(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>().HasData(
                new Branch
                {
                    Id = 1,
                    Name = "شعبه رسنا",
                    Description = "شعبه برند رسنا",
                    IsActive = true,
                    IsMainBranch = true,
                    CreateDate = new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified)
                }
            );
        }

        private static void SeedDefaultRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppRoles>().HasData(
                new AppRoles
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "مدیر سیستم",
                    RoleLevel = "1",
                    ConcurrencyStamp = "8e446cc7-743a-4133-8241-0f374fcbbc0d"
                },
                new AppRoles
                {
                    Id = "2",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "مدیر",
                    RoleLevel = "2",
                    ConcurrencyStamp = "5b6877d1-6fe6-4f8c-92a4-33fdf65a391f"
                },
                new AppRoles
                {
                    Id = "3",
                    Name = "Supervisor",
                    NormalizedName = "SUPERVISOR",
                    Description = "سرپرست",
                    RoleLevel = "3",
                    ConcurrencyStamp = "8f4cee96-4bf9-4019-b589-4de5c0230e2c"
                },
                new AppRoles
                {
                    Id = "4",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    Description = "کارمند",
                    RoleLevel = "4",
                    ConcurrencyStamp = "523c9ab5-4b4c-43e2-84be-12c4b6f74eed"
                },
                new AppRoles
                {
                    Id = "5",
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "کاربر عادی",
                    RoleLevel = "5",
                    ConcurrencyStamp = "aa5d01a0-a905-44ef-9e53-9c694828dbff"
                }
            );
        }

        private static void SeedDefaultTaskCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { Id = 1, Title = "عمومی", Description = "دسته‌بندی عمومی برای تسک‌ها", DisplayOrder = 1, IsActive = true },
                new TaskCategory { Id = 2, Title = "اداری", Description = "تسک‌های مربوط به امور اداری", DisplayOrder = 2, IsActive = true },
                new TaskCategory { Id = 3, Title = "فروش", Description = "تسک‌های مربوط به فروش", DisplayOrder = 4, IsActive = true },
                new TaskCategory { Id = 4, Title = "خدمات حضوری", Description = "تسک‌های مربوط به خدمات مشتریان غیر حضوری", DisplayOrder = 5, IsActive = true },
                new TaskCategory { Id = 5, Title = "خدمات  غیر حضوری", Description = "تسک‌های مربوط به خدمات مشتریان حضوری", DisplayOrder = 5, IsActive = true },
                new TaskCategory { Id = 6, Title = "بازاریابی", Description = "تسک‌های بازاریابی و تبلیغات", DisplayOrder = 6, IsActive = true },
                new TaskCategory { Id = 7, Title = "مالی", Description = "تسک‌های مربوط به امور مالی", DisplayOrder = 7, IsActive = true },
                new TaskCategory { Id = 8, Title = "منابع انسانی", Description = "تسک‌های مربوط به HR", DisplayOrder = 8, IsActive = true },
                new TaskCategory { Id = 9, Title = "فوری", Description = "تسک‌های فوری و اضطراری", DisplayOrder = 10, IsActive = true }
            );
        }

        private static void SeedDefaultPredefinedCopyDescriptions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PredefinedCopyDescription>().HasData(
                new PredefinedCopyDescription { Id = 1, Title = "جهت اطلاع", Description = "جهت اطلاع و پیگیری", IsActive = true },
                new PredefinedCopyDescription { Id = 2, Title = "جهت اقدام", Description = "جهت انجام اقدامات لازم", IsActive = true },
                new PredefinedCopyDescription { Id = 3, Title = "جهت بررسی", Description = "جهت بررسی و اعلام نظر", IsActive = true },
                new PredefinedCopyDescription { Id = 4, Title = "جهت تایید", Description = "جهت تایید و ابلاغ", IsActive = true },
                new PredefinedCopyDescription { Id = 5, Title = "جهت نظارت", Description = "جهت نظارت و کنترل", IsActive = true },
                new PredefinedCopyDescription { Id = 6, Title = "جهت هماهنگی", Description = "جهت هماهنگی‌های لازم", IsActive = true },
                new PredefinedCopyDescription { Id = 7, Title = "جهت پیگیری", Description = "جهت پیگیری و گزارش", IsActive = true },
                new PredefinedCopyDescription { Id = 8, Title = "جهت اجرا", Description = "جهت اجرای دستورات", IsActive = true }
            );
        }

        private static void SeedDefaultRolePatterns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePattern>().HasData(
                new RolePattern
                {
                    Id = 1,
                    PatternName = "مدیریت کامل",
                    Description = "دسترسی کامل به تمام بخش‌ها",
                    AccessLevel = 1,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 2,
                    PatternName = "مدیر عملیات",
                    Description = "مدیریت عملیات و تسک‌ها",
                    AccessLevel = 2,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 3,
                    PatternName = "کارشناس فروش",
                    Description = "دسترسی به ماژول فروش و CRM",
                    AccessLevel = 4,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                },
                new RolePattern
                {
                    Id = 4,
                    PatternName = "کاربر عادی",
                    Description = "دسترسی محدود به تسک‌های شخصی",
                    AccessLevel = 5,
                    IsActive = true,
                    IsSystemPattern = true,
                    CreateDate = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    CreatorUserId = null
                }
            );

            modelBuilder.Entity<RolePatternDetails>().HasData(
                // الگوی مدیریت کامل - دسترسی کامل به همه چیز
                new RolePatternDetails { Id = 1, RolePatternId = 1, ControllerName = "Tasks", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 2, RolePatternId = 1, ControllerName = "CRM", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 3, RolePatternId = 1, ControllerName = "Stakeholder", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 4, RolePatternId = 1, ControllerName = "Contract", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 5, RolePatternId = 1, ControllerName = "User", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 6, RolePatternId = 1, ControllerName = "RolePattern", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 7, RolePatternId = 1, ControllerName = "UserPermission", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 8, RolePatternId = 1, ControllerName = "Branch", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 9, RolePatternId = 1, ControllerName = "Team", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },
                new RolePatternDetails { Id = 10, RolePatternId = 1, ControllerName = "TaskCategory", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 2, IsActive = true },

                // الگوی مدیر عملیات
                new RolePatternDetails { Id = 11, RolePatternId = 2, ControllerName = "Tasks", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = true, CanApprove = true, DataAccessLevel = 1, IsActive = true },
                new RolePatternDetails { Id = 12, RolePatternId = 2, ControllerName = "CRM", ActionName = "Index,Details,Create,Edit", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 1, IsActive = true },
                new RolePatternDetails { Id = 13, RolePatternId = 2, ControllerName = "Stakeholder", ActionName = "Index,Details", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 1, IsActive = true },

                // الگوی کارشناس فروش
                new RolePatternDetails { Id = 14, RolePatternId = 3, ControllerName = "CRM", ActionName = "*", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },
                new RolePatternDetails { Id = 15, RolePatternId = 3, ControllerName = "Stakeholder", ActionName = "Index,Details,Create,Edit", CanRead = true, CanCreate = true, CanEdit = true, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },
                new RolePatternDetails { Id = 16, RolePatternId = 3, ControllerName = "Tasks", ActionName = "Index,Details,MyTasks", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true },

                // الگوی کاربر عادی
                new RolePatternDetails { Id = 17, RolePatternId = 4, ControllerName = "Tasks", ActionName = "Index,Details,MyTasks", CanRead = true, CanCreate = false, CanEdit = false, CanDelete = false, CanApprove = false, DataAccessLevel = 0, IsActive = true }
            );
        }
    }
}