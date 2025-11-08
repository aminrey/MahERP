using MahERP.DataModelLayer.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کانفیگ Fluent API برای Entity های Notification
    /// </summary>
    public static class NotificationEntitiesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            ConfigureNotificationModuleConfig(modelBuilder);
            ConfigureNotificationTypeConfig(modelBuilder);
            ConfigureUserNotificationPreference(modelBuilder);
            ConfigureNotificationBlacklist(modelBuilder);
            ConfigureNotificationTemplateHistory(modelBuilder);
            ConfigureNotificationScheduledMessage(modelBuilder);
            ConfigureNotificationDeliveryStats(modelBuilder);
            ConfigureNotificationRecipient(modelBuilder); // Configure NotificationRecipient
        }

        #region NotificationModuleConfig

        private static void ConfigureNotificationModuleConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationModuleConfig>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ModuleCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ModuleNameFa)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ModuleNameEn)
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ColorCode)
                    .HasMaxLength(20);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Unique Index on ModuleCode
                entity.HasIndex(e => e.ModuleCode)
                    .IsUnique()
                    .HasDatabaseName("IX_NotificationModuleConfig_ModuleCode");

                // Index on IsActive
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_NotificationModuleConfig_IsActive");
            });
        }

        #endregion

        #region NotificationTypeConfig

        private static void ConfigureNotificationTypeConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationTypeConfig>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TypeCode)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TypeNameFa)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.AllowUserCustomization)
                    .HasDefaultValue(true);

                // Relationship
                entity.HasOne(e => e.ModuleConfig)
                    .WithMany(m => m.NotificationTypes)
                    .HasForeignKey(e => e.ModuleConfigId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(e => new { e.ModuleConfigId, e.TypeCode })
                    .IsUnique()
                    .HasDatabaseName("IX_NotificationTypeConfig_Module_TypeCode");

                // Index on IsActive
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_NotificationTypeConfig_IsActive");
            });
        }

        #endregion

        #region UserNotificationPreference

        private static void ConfigureUserNotificationPreference(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserNotificationPreference>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.IsEnabled)
                    .HasDefaultValue(true);

                entity.Property(e => e.ReceiveBySystem)
                    .HasDefaultValue(true);

                entity.Property(e => e.ReceiveByEmail)
                    .HasDefaultValue(true);

                entity.Property(e => e.ReceiveBySms)
                    .HasDefaultValue(false);

                entity.Property(e => e.ReceiveByTelegram)
                    .HasDefaultValue(true);

                entity.Property(e => e.DeliveryMode)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.OnlyUrgentNotifications)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.NotificationTypeConfig)
                    .WithMany()
                    .HasForeignKey(e => e.NotificationTypeConfigId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique Index
                entity.HasIndex(e => new { e.UserId, e.NotificationTypeConfigId })
                    .IsUnique()
                    .HasDatabaseName("IX_UserNotificationPreference_User_Type");
            });
        }

        #endregion

        #region NotificationBlacklist

        private static void ConfigureNotificationBlacklist(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationBlacklist>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NotificationTypeConfig)
                    .WithMany()
                    .HasForeignKey(e => e.NotificationTypeConfigId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index
                entity.HasIndex(e => new { e.UserId, e.IsActive })
                    .HasDatabaseName("IX_NotificationBlacklist_User_Active");
            });
        }

        #endregion



        #region NotificationTemplateHistory

        private static void ConfigureNotificationTemplateHistory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationTemplateHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.MessageTemplate)
                    .IsRequired();

                entity.Property(e => e.BodyHtml)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ChangeNote)
                    .HasMaxLength(1000);

                entity.Property(e => e.ChangeDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relationship
                entity.HasOne(e => e.Template)
                    .WithMany(t => t.History)
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ChangedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ChangedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index
                entity.HasIndex(e => new { e.TemplateId, e.Version })
                    .HasDatabaseName("IX_TemplateHistory_Template_Version");
            });
        }

        #endregion

        #region NotificationScheduledMessage

        private static void ConfigureNotificationScheduledMessage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationScheduledMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Body)
                    .IsRequired();

                entity.Property(e => e.BodyHtml)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.MessageType)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.Priority)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.SendBySystem)
                    .HasDefaultValue(true);

                entity.Property(e => e.SendByEmail)
                    .HasDefaultValue(false);

                entity.Property(e => e.SendBySms)
                    .HasDefaultValue(false);

                entity.Property(e => e.SendByTelegram)
                    .HasDefaultValue(false);

                entity.Property(e => e.RecipientUserIds)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.RecipientCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.SentCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.FailedCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Relationship
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_ScheduledMessage_Status");

                entity.HasIndex(e => e.ScheduledDateTime)
                    .HasDatabaseName("IX_ScheduledMessage_ScheduledDateTime");
            });
        }

        #endregion

        #region NotificationDeliveryStats

        private static void ConfigureNotificationDeliveryStats(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationDeliveryStats>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RetryAttempts)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationship
                entity.HasOne(e => e.Delivery)
                    .WithMany()
                    .HasForeignKey(e => e.CoreNotificationDeliveryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(e => e.CoreNotificationDeliveryId)
                    .IsUnique()
                    .HasDatabaseName("IX_DeliveryStats_DeliveryId");
            });
        }

        #endregion

        #region NotificationRecipient

        private static void ConfigureNotificationRecipient(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationRecipient>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.NotificationTypeConfig)
                    .WithMany(t => t.Recipients)
                    .HasForeignKey(e => e.NotificationTypeConfigId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.NotificationTypeConfigId, e.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_NotificationRecipient_TypeUser");
            });
        }

        #endregion

       
    }
}