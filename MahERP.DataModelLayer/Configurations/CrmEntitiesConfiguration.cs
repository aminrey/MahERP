using MahERP.DataModelLayer.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کانفیگ Entity های CRM Lead Management
    /// </summary>
    public static class CrmEntitiesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            // CrmLeadStatus
            ConfigureCrmLeadStatus(modelBuilder);
            
            // CrmLead
            ConfigureCrmLead(modelBuilder);
            
            // CrmLeadInteraction
            ConfigureCrmLeadInteraction(modelBuilder);
            
            // CrmFollowUp
            ConfigureCrmFollowUp(modelBuilder);
        }

        private static void ConfigureCrmLeadStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmLeadStatus>(entity =>
            {
                entity.ToTable("CrmLeadStatus_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TitleEnglish)
                    .HasMaxLength(100);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#6c757d");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .HasDefaultValue("fa-circle");

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(1);

                entity.Property(e => e.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsFinal)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsPositive)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("IX_CrmLeadStatus_Title");

                entity.HasIndex(e => e.IsDefault)
                    .HasDatabaseName("IX_CrmLeadStatus_IsDefault");

                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_CrmLeadStatus_DisplayOrder");

                // Relations
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastUpdater)
                    .WithMany()
                    .HasForeignKey(e => e.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureCrmLead(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmLead>(entity =>
            {
                entity.ToTable("CrmLead_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Source)
                    .HasMaxLength(100);

                entity.Property(e => e.Score)
                    .HasDefaultValue(0);

                entity.Property(e => e.Notes)
                    .HasMaxLength(2000);

                entity.Property(e => e.Tags)
                    .HasMaxLength(500);

                entity.Property(e => e.EstimatedValue)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // ⭐ Check constraint: باید ContactId یا OrganizationId داشته باشد
                // این را در Repository اعمال می‌کنیم

                // Indexes
                entity.HasIndex(e => e.ContactId)
                    .HasDatabaseName("IX_CrmLead_ContactId");

                entity.HasIndex(e => e.OrganizationId)
                    .HasDatabaseName("IX_CrmLead_OrganizationId");

                entity.HasIndex(e => e.BranchId)
                    .HasDatabaseName("IX_CrmLead_BranchId");

                entity.HasIndex(e => e.AssignedUserId)
                    .HasDatabaseName("IX_CrmLead_AssignedUserId");

                entity.HasIndex(e => e.StatusId)
                    .HasDatabaseName("IX_CrmLead_StatusId");

                entity.HasIndex(e => e.NextFollowUpDate)
                    .HasDatabaseName("IX_CrmLead_NextFollowUpDate");

                entity.HasIndex(e => new { e.BranchId, e.StatusId })
                    .HasDatabaseName("IX_CrmLead_Branch_Status");

                // Relations
                entity.HasOne(e => e.Contact)
                    .WithMany()
                    .HasForeignKey(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Branch)
                    .WithMany()
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Status)
                    .WithMany(s => s.Leads)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastUpdater)
                    .WithMany()
                    .HasForeignKey(e => e.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureCrmLeadInteraction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmLeadInteraction>(entity =>
            {
                entity.ToTable("CrmLeadInteraction_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Subject)
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.Result)
                    .HasMaxLength(500);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(200);

                entity.Property(e => e.InteractionDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.LeadId)
                    .HasDatabaseName("IX_CrmLeadInteraction_LeadId");

                entity.HasIndex(e => e.InteractionType)
                    .HasDatabaseName("IX_CrmLeadInteraction_Type");

                entity.HasIndex(e => e.InteractionDate)
                    .HasDatabaseName("IX_CrmLeadInteraction_Date");

                entity.HasIndex(e => e.RelatedTaskId)
                    .HasDatabaseName("IX_CrmLeadInteraction_TaskId");

                entity.HasIndex(e => new { e.LeadId, e.InteractionDate })
                    .HasDatabaseName("IX_CrmLeadInteraction_Lead_Date");

                // Relations
                entity.HasOne(e => e.Lead)
                    .WithMany(l => l.Interactions)
                    .HasForeignKey(e => e.LeadId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RelatedTask)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedTaskId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastUpdater)
                    .WithMany()
                    .HasForeignKey(e => e.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureCrmFollowUp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmFollowUp>(entity =>
            {
                entity.ToTable("CrmFollowUp_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Priority)
                    .HasDefaultValue((byte)1);

                entity.Property(e => e.Status)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.CompletionResult)
                    .HasMaxLength(500);

                entity.Property(e => e.HasReminder)
                    .HasDefaultValue(true);

                entity.Property(e => e.ReminderSent)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.LeadId)
                    .HasDatabaseName("IX_CrmFollowUp_LeadId");

                entity.HasIndex(e => e.InteractionId)
                    .HasDatabaseName("IX_CrmFollowUp_InteractionId");

                entity.HasIndex(e => e.DueDate)
                    .HasDatabaseName("IX_CrmFollowUp_DueDate");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_CrmFollowUp_Status");

                entity.HasIndex(e => e.AssignedUserId)
                    .HasDatabaseName("IX_CrmFollowUp_AssignedUserId");

                entity.HasIndex(e => e.TaskId)
                    .HasDatabaseName("IX_CrmFollowUp_TaskId");

                entity.HasIndex(e => new { e.AssignedUserId, e.Status, e.DueDate })
                    .HasDatabaseName("IX_CrmFollowUp_User_Status_Due");

                entity.HasIndex(e => new { e.HasReminder, e.ReminderSent, e.ReminderDate })
                    .HasDatabaseName("IX_CrmFollowUp_Reminder")
                    .HasFilter("[HasReminder] = 1 AND [ReminderSent] = 0");

                // Relations
                entity.HasOne(e => e.Lead)
                    .WithMany(l => l.FollowUps)
                    .HasForeignKey(e => e.LeadId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Interaction)
                    .WithMany(i => i.FollowUps)
                    .HasForeignKey(e => e.InteractionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AssignedUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Task)
                    .WithMany()
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastUpdater)
                    .WithMany()
                    .HasForeignKey(e => e.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
