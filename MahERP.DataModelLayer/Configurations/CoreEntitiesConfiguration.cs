using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کانفیگ Fluent API برای Entity های Core
    /// شامل: Branch, Team, TeamMember, TeamPosition, ActivityBase و...
    /// </summary>
    public static class CoreEntitiesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            ConfigureBranch(modelBuilder);
            ConfigureBranchUser(modelBuilder);
            ConfigureTeam(modelBuilder);
            ConfigureTeamMember(modelBuilder);
            ConfigureTeamPosition(modelBuilder);
            ConfigureActivityBase(modelBuilder);
            ConfigureActivityComment(modelBuilder);
            ConfigureActivityAttachment(modelBuilder);
            ConfigureActivityHistory(modelBuilder);
            ConfigureActivityTask(modelBuilder);
            ConfigureActivityCRM(modelBuilder);
            ConfigureCoreNotification(modelBuilder);
            ConfigureCoreNotificationDetail(modelBuilder);
            ConfigureCoreNotificationDelivery(modelBuilder);
            ConfigureCoreNotificationSetting(modelBuilder);
            ConfigureUserActivityLog(modelBuilder);
        }

        #region Branch Configuration

        private static void ConfigureBranch(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>(entity =>
            {
                // Primary Key
                entity.HasKey(b => b.Id);

                // Properties
                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(b => b.Description)
                    .HasMaxLength(500);

                entity.Property(b => b.Address)
                    .HasMaxLength(500);

                entity.Property(b => b.Phone)
                    .HasMaxLength(15);

                entity.Property(b => b.IsMainBranch)
                    .HasDefaultValue(false);

                entity.Property(b => b.IsActive)
                    .HasDefaultValue(true);

                // Self-referencing relationship (Parent Branch)
                entity.HasOne(b => b.ParentBranch)
                    .WithMany()
                    .HasForeignKey(b => b.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(b => b.Name)
                    .HasDatabaseName("IX_Branch_Name");

                entity.HasIndex(b => b.ParentId)
                    .HasDatabaseName("IX_Branch_ParentId");
            });
        }

        private static void ConfigureBranchUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchUser>(entity =>
            {
                // Primary Key
                entity.HasKey(bu => bu.Id);

                // Branch relationship
                entity.HasOne(bu => bu.Branch)
                    .WithMany()
                    .HasForeignKey(bu => bu.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // User relationship
                entity.HasOne(bu => bu.User)
                    .WithMany()
                    .HasForeignKey(bu => bu.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Assigner relationship
                entity.HasOne(bu => bu.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(bu => bu.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(bu => new { bu.BranchId, bu.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_BranchUser_Branch_User");
            });
        }

        #endregion

        #region Team Configuration

        private static void ConfigureTeam(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity =>
            {
                // Primary Key
                entity.HasKey(t => t.Id);

                // Properties
                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(t => t.ParentTeam)
                    .WithMany(t => t.ChildTeams)
                    .HasForeignKey(t => t.ParentTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Manager)
                    .WithMany()
                    .HasForeignKey(t => t.ManagerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Branch)
                    .WithMany()
                    .HasForeignKey(t => t.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Creator)
                    .WithMany()
                    .HasForeignKey(t => t.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.LastUpdater)
                    .WithMany()
                    .HasForeignKey(t => t.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(t => t.BranchId)
                    .HasDatabaseName("IX_Team_BranchId");

                entity.HasIndex(t => t.ManagerUserId)
                    .HasDatabaseName("IX_Team_ManagerUserId");
            });
        }

        private static void ConfigureTeamMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamMember>(entity =>
            {
                // Primary Key
                entity.HasKey(tm => tm.Id);

                // Properties
                entity.Property(tm => tm.IsActive)
                    .HasDefaultValue(true);

                entity.Property(tm => tm.RoleDescription)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(tm => tm.Team)
                    .WithMany(t => t.TeamMembers)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tm => tm.User)
                    .WithMany(u => u.TeamMemberships)
                    .HasForeignKey(tm => tm.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tm => tm.Position)
                    .WithMany(tp => tp.TeamMembers)
                    .HasForeignKey(tm => tm.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tm => tm.AddedByUser)
                    .WithMany()
                    .HasForeignKey(tm => tm.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(tm => new { tm.TeamId, tm.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_TeamMember_Team_User");
            });
        }

        private static void ConfigureTeamPosition(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamPosition>(entity =>
            {
                // Primary Key
                entity.HasKey(tp => tp.Id);

                // Properties
                entity.Property(tp => tp.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(tp => tp.Description)
                    .HasMaxLength(500);

                entity.Property(tp => tp.IsActive)
                    .HasDefaultValue(true);

                entity.Property(tp => tp.IsDefault)
                    .HasDefaultValue(false);

                // Relationships
                entity.HasOne(tp => tp.Team)
                    .WithMany(t => t.TeamPositions)
                    .HasForeignKey(tp => tp.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tp => tp.Creator)
                    .WithMany()
                    .HasForeignKey(tp => tp.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tp => tp.LastUpdater)
                    .WithMany()
                    .HasForeignKey(tp => tp.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(tp => new { tp.TeamId, tp.Title })
                    .IsUnique()
                    .HasDatabaseName("IX_TeamPosition_Team_Title");

                entity.HasIndex(tp => tp.PowerLevel)
                    .HasDatabaseName("IX_TeamPosition_PowerLevel");
            });
        }

        #endregion

        #region Activity Configuration

        private static void ConfigureActivityBase(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityBase>(entity =>
            {
                // Primary Key
                entity.HasKey(a => a.Id);

                // Properties
                entity.Property(a => a.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(a => a.Description)
                    .HasMaxLength(2000);

                entity.Property(a => a.IsActive)
                    .HasDefaultValue(true);

                entity.Property(a => a.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(a => a.ProgressPercentage)
                    .HasDefaultValue(0);

                // Relationships
                entity.HasOne(a => a.Creator)
                    .WithMany()
                    .HasForeignKey(a => a.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.LastUpdater)
                    .WithMany()
                    .HasForeignKey(a => a.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Stakeholder)
                    .WithMany()
                    .HasForeignKey(a => a.StakeholderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Contract)
                    .WithMany()
                    .HasForeignKey(a => a.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Branch)
                    .WithMany()
                    .HasForeignKey(a => a.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(a => a.Status)
                    .HasDatabaseName("IX_ActivityBase_Status");

                entity.HasIndex(a => new { a.BranchId, a.ActivityType })
                    .HasDatabaseName("IX_ActivityBase_Branch_Type");
            });
        }

        private static void ConfigureActivityComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityComment>(entity =>
            {
                // Primary Key
                entity.HasKey(ac => ac.Id);

                // Properties
                entity.Property(ac => ac.CommentText)
                    .IsRequired()
                    .HasMaxLength(2000);

                // Relationships
                entity.HasOne(ac => ac.Activity)
                    .WithMany(a => a.ActivityComments)
                    .HasForeignKey(ac => ac.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ac => ac.Creator)
                    .WithMany()
                    .HasForeignKey(ac => ac.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ac => ac.ParentComment)
                    .WithMany(ac => ac.Replies)
                    .HasForeignKey(ac => ac.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(ac => ac.ActivityId)
                    .HasDatabaseName("IX_ActivityComment_ActivityId");
            });
        }

        private static void ConfigureActivityAttachment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityAttachment>(entity =>
            {
                // Primary Key
                entity.HasKey(aa => aa.Id);

                // Properties
                entity.Property(aa => aa.FileName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(aa => aa.FileType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(aa => aa.FilePath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(aa => aa.Description)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(aa => aa.Activity)
                    .WithMany(a => a.ActivityAttachments)
                    .HasForeignKey(aa => aa.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(aa => aa.Uploader)
                    .WithMany()
                    .HasForeignKey(aa => aa.UploaderUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(aa => aa.ActivityId)
                    .HasDatabaseName("IX_ActivityAttachment_ActivityId");
            });
        }

        private static void ConfigureActivityHistory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityHistory>(entity =>
            {
                // Primary Key
                entity.HasKey(ah => ah.Id);

                // Properties
                entity.Property(ah => ah.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(ah => ah.OldValue)
                    .HasMaxLength(500);

                entity.Property(ah => ah.NewValue)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(ah => ah.Activity)
                    .WithMany(a => a.ActivityHistories)
                    .HasForeignKey(ah => ah.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ah => ah.Creator)
                    .WithMany()
                    .HasForeignKey(ah => ah.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(ah => new { ah.ActivityId, ah.CreateDate })
                    .HasDatabaseName("IX_ActivityHistory_Activity_Date");
            });
        }

        private static void ConfigureActivityTask(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityTask>(entity =>
            {
                // Primary Key
                entity.HasKey(at => at.Id);

                // Properties
                entity.Property(at => at.Description)
                    .HasMaxLength(1000);

                entity.Property(at => at.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(at => at.Activity)
                    .WithMany(a => a.ActivityTasks)
                    .HasForeignKey(at => at.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(at => at.Task)
                    .WithMany()
                    .HasForeignKey(at => at.TaskId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(at => at.Creator)
                    .WithMany()
                    .HasForeignKey(at => at.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(at => new { at.ActivityId, at.TaskId })
                    .IsUnique()
                    .HasDatabaseName("IX_ActivityTask_Activity_Task");
            });
        }

        private static void ConfigureActivityCRM(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityCRM>(entity =>
            {
                // Primary Key
                entity.HasKey(ac => ac.Id);

                // Properties
                entity.Property(ac => ac.Description)
                    .HasMaxLength(1000);

                entity.Property(ac => ac.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(ac => ac.Activity)
                    .WithMany(a => a.ActivityCRMs)
                    .HasForeignKey(ac => ac.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ac => ac.CRMInteraction)
                    .WithMany(c => c.ActivityCRMs)
                    .HasForeignKey(ac => ac.CRMId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ac => ac.Creator)
                    .WithMany()
                    .HasForeignKey(ac => ac.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(ac => new { ac.ActivityId, ac.CRMId })
                    .IsUnique()
                    .HasDatabaseName("IX_ActivityCRM_Activity_CRM");
            });
        }

        #endregion

        #region Core Notification Configuration

        private static void ConfigureCoreNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreNotification>(entity =>
            {
                // Primary Key
                entity.HasKey(cn => cn.Id);

                // Properties
                entity.Property(cn => cn.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(cn => cn.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(cn => cn.SystemName)
                    .IsRequired()
                    .HasMaxLength(100);

                // Relationships
                entity.HasOne(cn => cn.Recipient)
                    .WithMany()
                    .HasForeignKey(cn => cn.RecipientUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cn => cn.Sender)
                    .WithMany()
                    .HasForeignKey(cn => cn.SenderUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cn => cn.Branch)
                    .WithMany()
                    .HasForeignKey(cn => cn.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(cn => new { cn.SystemId, cn.CreateDate })
                    .HasDatabaseName("IX_CoreNotification_System_Date");

                entity.HasIndex(cn => new { cn.RecipientUserId, cn.IsRead })
                    .HasDatabaseName("IX_CoreNotification_Recipient_Read");
            });
        }

        private static void ConfigureCoreNotificationDetail(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreNotificationDetail>(entity =>
            {
                // Primary Key
                entity.HasKey(cnd => cnd.Id);

                // Relationships
                entity.HasOne(cnd => cnd.CoreNotification)
                    .WithMany(cn => cn.Details)
                    .HasForeignKey(cnd => cnd.CoreNotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(cnd => cnd.CoreNotificationId)
                    .HasDatabaseName("IX_CoreNotificationDetail_NotificationId");
            });
        }

        private static void ConfigureCoreNotificationDelivery(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreNotificationDelivery>(entity =>
            {
                // Primary Key
                entity.HasKey(cnd => cnd.Id);

                // Relationships
                entity.HasOne(cnd => cnd.CoreNotification)
                    .WithMany(cn => cn.Deliveries)
                    .HasForeignKey(cnd => cnd.CoreNotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(cnd => new { cnd.CoreNotificationId, cnd.DeliveryStatus })
                    .HasDatabaseName("IX_CoreNotificationDelivery_Notification_Status");

                entity.HasIndex(cnd => cnd.DeliveryMethod)
                    .HasDatabaseName("IX_CoreNotificationDelivery_Method");
            });
        }

        private static void ConfigureCoreNotificationSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreNotificationSetting>(entity =>
            {
                // Primary Key
                entity.HasKey(cns => cns.Id);

                // Relationships
                entity.HasOne(cns => cns.User)
                    .WithMany()
                    .HasForeignKey(cns => cns.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique Index
                entity.HasIndex(cns => new { cns.UserId, cns.SystemId, cns.NotificationTypeGeneral })
                    .IsUnique()
                    .HasDatabaseName("IX_CoreNotificationSetting_User_System_Type");
            });
        }

        #endregion

        #region User Activity Log

        private static void ConfigureUserActivityLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                // Primary Key
                entity.HasKey(ual => ual.Id);

                // Properties
                entity.Property(ual => ual.ModuleName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(ual => ual.ActionName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(ual => ual.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(ual => ual.IpAddress)
                    .HasMaxLength(50);

                entity.Property(ual => ual.UserAgent)
                    .HasMaxLength(500);

                entity.Property(ual => ual.RequestUrl)
                    .HasMaxLength(500);

                entity.Property(ual => ual.HttpMethod)
                    .HasMaxLength(10);

                // Relationships
                entity.HasOne(ual => ual.User)
                    .WithMany()
                    .HasForeignKey(ual => ual.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ual => ual.Branch)
                    .WithMany()
                    .HasForeignKey(ual => ual.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(ual => new { ual.UserId, ual.ActivityDateTime })
                    .HasDatabaseName("IX_UserActivityLog_User_Date");

                entity.HasIndex(ual => ual.ActivityType)
                    .HasDatabaseName("IX_UserActivityLog_ActivityType");

                entity.HasIndex(ual => new { ual.ModuleName, ual.ActivityDateTime })
                    .HasDatabaseName("IX_UserActivityLog_Module_Date");

                entity.HasIndex(ual => ual.ResultStatus)
                    .HasDatabaseName("IX_UserActivityLog_ResultStatus");
            });
        }

        #endregion
    }
}