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
            
            // ⭐ CrmLeadSource - NEW
            ConfigureCrmLeadSource(modelBuilder);
            
            // ⭐ CrmLostReason - NEW
            ConfigureCrmLostReason(modelBuilder);
            
            // CrmLead
            ConfigureCrmLead(modelBuilder);
            
            // CrmLeadInteraction
            ConfigureCrmLeadInteraction(modelBuilder);
            
            // CrmFollowUp
            ConfigureCrmFollowUp(modelBuilder);

            // ⭐⭐⭐ NEW: Pipeline & Opportunity
            ConfigureCrmPipelineStage(modelBuilder);
            ConfigureCrmOpportunity(modelBuilder);
            ConfigureCrmOpportunityActivity(modelBuilder);
            ConfigureCrmOpportunityProduct(modelBuilder);
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

        // ⭐⭐⭐ NEW: Lead Source Configuration
        private static void ConfigureCrmLeadSource(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmLeadSource>(entity =>
            {
                entity.ToTable("CrmLeadSource_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.NameEnglish)
                    .HasMaxLength(100);

                entity.Property(e => e.Code)
                    .HasMaxLength(50);

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .HasDefaultValue("fa-globe");

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#6c757d");

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(1);

                entity.Property(e => e.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsSystem)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_CrmLeadSource_Name");

                entity.HasIndex(e => e.Code)
                    .HasDatabaseName("IX_CrmLeadSource_Code");

                entity.HasIndex(e => e.IsDefault)
                    .HasDatabaseName("IX_CrmLeadSource_IsDefault");

                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_CrmLeadSource_DisplayOrder");

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

        // ⭐⭐⭐ NEW: Lost Reason Configuration
        private static void ConfigureCrmLostReason(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmLostReason>(entity =>
            {
                entity.ToTable("CrmLostReason_Tbl");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.TitleEnglish)
                    .HasMaxLength(200);

                entity.Property(e => e.Code)
                    .HasMaxLength(50);

                entity.Property(e => e.AppliesTo)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.Category)
                    .HasDefaultValue((byte)5);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .HasDefaultValue("fa-times-circle");

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#dc3545");

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(1);

                entity.Property(e => e.IsSystem)
                    .HasDefaultValue(false);

                entity.Property(e => e.RequiresNote)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("IX_CrmLostReason_Title");

                entity.HasIndex(e => e.Code)
                    .HasDatabaseName("IX_CrmLostReason_Code");

                entity.HasIndex(e => e.AppliesTo)
                    .HasDatabaseName("IX_CrmLostReason_AppliesTo");

                entity.HasIndex(e => e.Category)
                    .HasDatabaseName("IX_CrmLostReason_Category");

                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_CrmLostReason_DisplayOrder");

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

                entity.Property(e => e.LostReasonNote)
                    .HasMaxLength(1000);

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

                entity.HasIndex(e => e.SourceId)
                    .HasDatabaseName("IX_CrmLead_SourceId");

                entity.HasIndex(e => e.LostReasonId)
                    .HasDatabaseName("IX_CrmLead_LostReasonId");

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

                // ⭐ NEW Relations
                entity.HasOne(e => e.LeadSource)
                    .WithMany(s => s.Leads)
                    .HasForeignKey(e => e.SourceId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.LostReason)
                    .WithMany(r => r.Leads)
                    .HasForeignKey(e => e.LostReasonId)
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

        // ⭐⭐⭐ NEW: Pipeline Stage Configuration
        private static void ConfigureCrmPipelineStage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmPipelineStage>(entity =>
            {
                entity.ToTable("CrmPipelineStages");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20)
                    .HasDefaultValue("#4285f4");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.BranchId)
                    .HasDatabaseName("IX_CrmPipelineStage_BranchId");

                entity.HasIndex(e => new { e.BranchId, e.DisplayOrder })
                    .HasDatabaseName("IX_CrmPipelineStage_Branch_Order");

                entity.HasIndex(e => new { e.BranchId, e.IsDefault })
                    .HasDatabaseName("IX_CrmPipelineStage_Branch_Default");

                // Relations
                entity.HasOne(e => e.Branch)
                    .WithMany()
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ⭐⭐⭐ NEW: Opportunity Configuration
        private static void ConfigureCrmOpportunity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmOpportunity>(entity =>
            {
                entity.ToTable("CrmOpportunities");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValue("IRR");

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.WeightedValue)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.LostReason)
                    .HasMaxLength(500);

                entity.Property(e => e.LostReasonNote)
                    .HasMaxLength(1000);

                entity.Property(e => e.WinningCompetitor)
                    .HasMaxLength(200);

                entity.Property(e => e.Source)
                    .HasMaxLength(100);

                entity.Property(e => e.Tags)
                    .HasMaxLength(500);

                entity.Property(e => e.Notes)
                    .HasMaxLength(2000);

                entity.Property(e => e.NextActionNote)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.BranchId)
                    .HasDatabaseName("IX_CrmOpportunity_BranchId");

                entity.HasIndex(e => e.StageId)
                    .HasDatabaseName("IX_CrmOpportunity_StageId");

                entity.HasIndex(e => e.AssignedUserId)
                    .HasDatabaseName("IX_CrmOpportunity_AssignedUserId");

                entity.HasIndex(e => e.ContactId)
                    .HasDatabaseName("IX_CrmOpportunity_ContactId");

                entity.HasIndex(e => e.OrganizationId)
                    .HasDatabaseName("IX_CrmOpportunity_OrganizationId");

                entity.HasIndex(e => e.SourceLeadId)
                    .HasDatabaseName("IX_CrmOpportunity_SourceLeadId");

                entity.HasIndex(e => e.LostReasonId)
                    .HasDatabaseName("IX_CrmOpportunity_LostReasonId");

                entity.HasIndex(e => e.ExpectedCloseDate)
                    .HasDatabaseName("IX_CrmOpportunity_ExpectedCloseDate");

                entity.HasIndex(e => e.NextActionDate)
                    .HasDatabaseName("IX_CrmOpportunity_NextActionDate");

                entity.HasIndex(e => new { e.BranchId, e.StageId, e.IsActive })
                    .HasDatabaseName("IX_CrmOpportunity_Branch_Stage_Active");

                entity.HasIndex(e => new { e.AssignedUserId, e.IsActive })
                    .HasDatabaseName("IX_CrmOpportunity_User_Active");

                // Relations
                entity.HasOne(e => e.Branch)
                    .WithMany()
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Stage)
                    .WithMany(s => s.Opportunities)
                    .HasForeignKey(e => e.StageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SourceLead)
                    .WithMany()
                    .HasForeignKey(e => e.SourceLeadId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Contact)
                    .WithMany()
                    .HasForeignKey(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ⭐ NEW: Lost Reason Navigation
                entity.HasOne(e => e.LostReasonNavigation)
                    .WithMany(r => r.Opportunities)
                    .HasForeignKey(e => e.LostReasonId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ⭐⭐⭐ NEW: Opportunity Activity Configuration
        private static void ConfigureCrmOpportunityActivity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmOpportunityActivity>(entity =>
            {
                entity.ToTable("CrmOpportunityActivities");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.ActivityDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.OpportunityId)
                    .HasDatabaseName("IX_CrmOpportunityActivity_OpportunityId");

                entity.HasIndex(e => new { e.OpportunityId, e.ActivityDate })
                    .HasDatabaseName("IX_CrmOpportunityActivity_Opp_Date");

                // Relations
                entity.HasOne(e => e.Opportunity)
                    .WithMany(o => o.Activities)
                    .HasForeignKey(e => e.OpportunityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ⭐⭐⭐ NEW: Opportunity Product Configuration
        private static void ConfigureCrmOpportunityProduct(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrmOpportunityProduct>(entity =>
            {
                entity.ToTable("CrmOpportunityProducts");
                
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18,4)")
                    .HasDefaultValue(1);

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,0)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes
                entity.HasIndex(e => e.OpportunityId)
                    .HasDatabaseName("IX_CrmOpportunityProduct_OpportunityId");

                // Relations
                entity.HasOne(e => e.Opportunity)
                    .WithMany(o => o.Products)
                    .HasForeignKey(e => e.OpportunityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
