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

        // Organization
        public DbSet<Branch> Branch_Tbl { get; set; }
        public DbSet<BranchUser> BranchUser_Tbl { get; set; }
        public DbSet<Team> Team_Tbl { get; set; }
        public DbSet<TeamMember> TeamMember_Tbl { get; set; }

        // Account Control
        public DbSet<Stakeholder> Stakeholder_Tbl { get; set; }
        public DbSet<StakeholderBranch> StakeholderBranch_Tbl { get; set; }
        public DbSet<StakeholderContact> StakeholderContact_Tbl { get; set; }
        public DbSet<Contract> Contract_Tbl { get; set; }

        // Core Activities
        public DbSet<ActivityBase> ActivityBase_Tbl { get; set; }
        public DbSet<ActivityAttachment> ActivityAttachment_Tbl { get; set; }
        public DbSet<ActivityComment> ActivityComment_Tbl { get; set; }
        public DbSet<ActivityCRM> ActivityCRM_Tbl { get; set; }
        public DbSet<ActivityHistory> ActivityHistory_Tbl { get; set; }
        public DbSet<ActivityTask> ActivityTask_Tbl { get; set; }

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
        public DbSet<TaskTemplate> TaskTemplate_Tbl { get; set; }
        public DbSet<TaskTemplateOperation> TaskTemplateOperation_Tbl { get; set; }
        public DbSet<TaskViewer> TaskViewer_Tbl { get; set; }
        public DbSet<PredefinedCopyDescription> PredefinedCopyDescription_Tbl { get; set; }

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

            // Stakeholder relationships
            modelBuilder.Entity<Stakeholder>()
                .HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMInteraction relationships
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

            // Activity relationships
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

            // TeamMember relationships
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

            // اصلاح خطای مسیرهای چندگانه در TaskScheduleAssignment
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

            // اصلاح خطای مسیرهای چندگانه در CRMParticipant
            modelBuilder.Entity<CRMParticipant>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // اصلاح خطای مسیرهای چندگانه در TaskViewer
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

            // TaskCommentMention - اصلاح روابط با کاربران
            modelBuilder.Entity<TaskCommentMention>()
                .HasOne(tcm => tcm.MentionedUser)
                .WithMany()
                .HasForeignKey(tcm => tcm.MentionedUserId)
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

            // ======================== ACTIVITY RELATED RELATIONSHIPS ========================
            // ActivityComment relationships
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

            // ActivityTask relationships
            modelBuilder.Entity<ActivityTask>()
                .HasOne(at => at.Creator)
                .WithMany()
                .HasForeignKey(at => at.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityTask>()
                .HasOne(at => at.Task)
                .WithMany()
                .HasForeignKey(at => at.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityAttachment
            modelBuilder.Entity<ActivityAttachment>()
                .HasOne(aa => aa.Uploader)
                .WithMany()
                .HasForeignKey(aa => aa.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================== OTHER RELATIONSHIPS ========================
            // Branch self-referencing relationship
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.ParentBranch)
                .WithMany()
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Stakeholder-Contract relationship
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Stakeholder)
                .WithMany(s => s.Contracts)
                .HasForeignKey(c => c.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUsers self-referencing relationship
            modelBuilder.Entity<AppUsers>()
                .HasMany(u => u.ManagedUsers)
                .WithOne(u => u.DirectManager)
                .HasForeignKey(u => u.DirectManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // StakeholderContact relationship
            modelBuilder.Entity<StakeholderContact>()
                .HasOne(sc => sc.Stakeholder)
                .WithMany(s => s.StakeholderContacts)
                .HasForeignKey(sc => sc.StakeholderId)
                .OnDelete(DeleteBehavior.Restrict);

            // CRMComment relationship
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

            // CRMTeam
            modelBuilder.Entity<CRMTeam>()
                .HasOne(ct => ct.Creator)
                .WithMany()
                .HasForeignKey(ct => ct.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskNotification
            modelBuilder.Entity<TaskNotification>()
                .HasOne(tn => tn.Recipient)
                .WithMany()
                .HasForeignKey(tn => tn.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}