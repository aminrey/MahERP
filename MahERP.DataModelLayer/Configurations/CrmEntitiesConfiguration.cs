using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.StaticClasses;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کانفیگ Entity های CRM جدید
    /// </summary>
    public static class CrmEntitiesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            // LeadStageStatus (استاتیک)
            ConfigureLeadStageStatus(modelBuilder);
            
            // PostPurchaseStage (استاتیک)
            ConfigurePostPurchaseStage(modelBuilder);
            
            // InteractionType (CRUD)
            ConfigureInteractionType(modelBuilder);
            
            // Goal
            ConfigureGoal(modelBuilder);
            
            // Interaction
            ConfigureInteraction(modelBuilder);
            
            // InteractionGoal (M:N)
            ConfigureInteractionGoal(modelBuilder);
            
            // Referral
            ConfigureReferral(modelBuilder);
            
            // Seed Data
            SeedCrmData(modelBuilder);
        }

        private static void ConfigureLeadStageStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeadStageStatus>(entity =>
            {
                entity.ToTable("LeadStageStatus_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TitleEnglish)
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#6c757d");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.StageType)
                    .HasDatabaseName("IX_LeadStageStatus_StageType")
                    .IsUnique();

                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_LeadStageStatus_DisplayOrder");
            });
        }

        private static void ConfigurePostPurchaseStage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostPurchaseStage>(entity =>
            {
                entity.ToTable("PostPurchaseStage_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TitleEnglish)
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#6c757d");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.StageType)
                    .HasDatabaseName("IX_PostPurchaseStage_StageType")
                    .IsUnique();

                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_PostPurchaseStage_DisplayOrder");
            });
        }

        private static void ConfigureInteractionType(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InteractionType>(entity =>
            {
                entity.ToTable("InteractionType_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20);

                entity.Property(e => e.Icon)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relations
                entity.HasOne(e => e.LeadStageStatus)
                    .WithMany(s => s.InteractionTypes)
                    .HasForeignKey(e => e.LeadStageStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("IX_InteractionType_Title");

                entity.HasIndex(e => e.LeadStageStatusId)
                    .HasDatabaseName("IX_InteractionType_LeadStageStatusId");
            });
        }

        private static void ConfigureGoal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.ToTable("Goal_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.ProductName)
                    .HasMaxLength(200);

                entity.Property(e => e.EstimatedValue)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.ActualValue)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.IsConverted)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relations
                entity.HasOne(e => e.Contact)
                    .WithMany()
                    .HasForeignKey(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CurrentLeadStageStatus)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentLeadStageStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.ContactId)
                    .HasDatabaseName("IX_Goal_ContactId");

                entity.HasIndex(e => e.OrganizationId)
                    .HasDatabaseName("IX_Goal_OrganizationId");

                entity.HasIndex(e => e.IsConverted)
                    .HasDatabaseName("IX_Goal_IsConverted");
            });
        }

        private static void ConfigureInteraction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interaction>(entity =>
            {
                entity.ToTable("Interaction_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Subject)
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.Result)
                    .HasMaxLength(1000);

                entity.Property(e => e.NextAction)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.HasReferral)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsReferred)
                    .HasDefaultValue(false);

                entity.Property(e => e.InteractionDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relations
                entity.HasOne(e => e.Contact)
                    .WithMany()
                    .HasForeignKey(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InteractionType)
                    .WithMany(t => t.Interactions)
                    .HasForeignKey(e => e.InteractionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PostPurchaseStage)
                    .WithMany(s => s.Interactions)
                    .HasForeignKey(e => e.PostPurchaseStageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.ContactId)
                    .HasDatabaseName("IX_Interaction_ContactId");

                entity.HasIndex(e => e.InteractionTypeId)
                    .HasDatabaseName("IX_Interaction_InteractionTypeId");

                entity.HasIndex(e => e.InteractionDate)
                    .HasDatabaseName("IX_Interaction_InteractionDate");

                entity.HasIndex(e => e.HasReferral)
                    .HasDatabaseName("IX_Interaction_HasReferral");
            });
        }

        private static void ConfigureInteractionGoal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InteractionGoal>(entity =>
            {
                entity.ToTable("InteractionGoal_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Note)
                    .HasMaxLength(500);

                // Relations
                entity.HasOne(e => e.Interaction)
                    .WithMany(i => i.InteractionGoals)
                    .HasForeignKey(e => e.InteractionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Goal)
                    .WithMany(g => g.InteractionGoals)
                    .HasForeignKey(e => e.GoalId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(e => new { e.InteractionId, e.GoalId })
                    .HasDatabaseName("IX_InteractionGoal_Unique")
                    .IsUnique();
            });
        }

        private static void ConfigureReferral(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Referral>(entity =>
            {
                entity.ToTable("Referral_Tbl");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .HasDefaultValue(Enums.ReferralStatus.Pending);

                entity.Property(e => e.ReferralType)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.MarketerUserId)
                    .HasMaxLength(450);

                entity.Property(e => e.ReferralDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relations
                entity.HasOne(e => e.ReferrerContact)
                    .WithMany()
                    .HasForeignKey(e => e.ReferrerContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReferredContact)
                    .WithMany()
                    .HasForeignKey(e => e.ReferredContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReferrerInteraction)
                    .WithMany(i => i.ReferralsAsReferrer)
                    .HasForeignKey(e => e.ReferrerInteractionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReferredInteraction)
                    .WithMany(i => i.ReferralsAsReferred)
                    .HasForeignKey(e => e.ReferredInteractionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.ReferrerContactId)
                    .HasDatabaseName("IX_Referral_ReferrerContactId");

                entity.HasIndex(e => e.ReferredContactId)
                    .HasDatabaseName("IX_Referral_ReferredContactId");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Referral_Status");

                entity.HasIndex(e => e.ReferralDate)
                    .HasDatabaseName("IX_Referral_ReferralDate");
            });
        }

        private static void SeedCrmData(ModelBuilder modelBuilder)
        {
            // Seed LeadStageStatus
            modelBuilder.Entity<LeadStageStatus>().HasData(
                CrmSeedData.GetLeadStageStatuses()
            );

            // Seed PostPurchaseStage
            modelBuilder.Entity<PostPurchaseStage>().HasData(
                CrmSeedData.GetPostPurchaseStages()
            );
        }
    }
}
