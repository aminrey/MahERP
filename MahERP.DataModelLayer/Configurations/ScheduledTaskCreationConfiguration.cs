using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// پیکربندی Entity Framework برای جدول ScheduledTaskCreation
    /// </summary>
    public class ScheduledTaskCreationConfiguration : IEntityTypeConfiguration<ScheduledTaskCreation>
    {
        public void Configure(EntityTypeBuilder<ScheduledTaskCreation> builder)
        {
            builder.ToTable("ScheduledTaskCreation_Tbl");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ScheduleTitle)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ScheduleDescription)
                .HasMaxLength(500);

            builder.Property(e => e.TaskDataJson)
                .IsRequired()
                .HasColumnType("nvarchar(MAX)");

            builder.Property(e => e.ScheduleType)
                .IsRequired();

            builder.Property(e => e.ScheduledTime)
                .HasMaxLength(5);

            builder.Property(e => e.ScheduledDaysOfWeek)
                .HasMaxLength(50);

            builder.Property(e => e.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(e => e.ModifiedByUserId)
                .HasMaxLength(450);

            builder.Property(e => e.Notes)
                .HasColumnType("nvarchar(MAX)");

            // ⭐ Index برای بهبود کارایی
            builder.HasIndex(e => e.NextExecutionDate)
                .HasDatabaseName("IX_ScheduledTaskCreation_NextExecutionDate");

            builder.HasIndex(e => new { e.IsActive, e.IsScheduleEnabled, e.NextExecutionDate })
                .HasDatabaseName("IX_ScheduledTaskCreation_Active_Enabled_Next");

            builder.HasIndex(e => e.CreatedByUserId)
                .HasDatabaseName("IX_ScheduledTaskCreation_CreatedBy");

            builder.HasIndex(e => e.BranchId)
                .HasDatabaseName("IX_ScheduledTaskCreation_Branch");

            // مقادیر پیش‌فرض
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            builder.Property(e => e.IsScheduleEnabled)
                .HasDefaultValue(true);

            builder.Property(e => e.IsExecuted)
                .HasDefaultValue(false);

            builder.Property(e => e.IsRecurring)
                .HasDefaultValue(false);

            builder.Property(e => e.ExecutionCount)
                .HasDefaultValue(0);

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
