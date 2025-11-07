using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Configurations
{
    /// <summary>
    /// کانفیگ Fluent API برای Entity های افراد و سازمان‌ها
    /// </summary>
    public static class ContactOrganizationConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            ConfigureContact(modelBuilder);
            ConfigureContactPhone(modelBuilder);
            ConfigureOrganization(modelBuilder);
            ConfigureOrganizationDepartment(modelBuilder);
            ConfigureDepartmentPosition(modelBuilder);
            ConfigureDepartmentMember(modelBuilder);
            ConfigureOrganizationContact(modelBuilder);
            ConfigureContactGroup(modelBuilder);
            ConfigureContactGroupMember(modelBuilder);
            ConfigureBranchContactGroup(modelBuilder);
            ConfigureBranchContactGroupMember(modelBuilder);
            ConfigureOrganizationGroup(modelBuilder);
            ConfigureOrganizationGroupMember(modelBuilder);
            ConfigureBranchOrganizationGroup(modelBuilder);
            ConfigureBranchOrganizationGroupMember(modelBuilder);
        }

        #region Contact Configuration

        private static void ConfigureContact(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>(entity =>
            {
                // Primary Key
                entity.HasKey(c => c.Id);

                // Properties
                entity.Property(c => c.FirstName)
                    .HasMaxLength(100);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.NationalCode)
                    .HasMaxLength(10);

                entity.Property(c => c.PrimaryEmail)
                    .HasMaxLength(200);

                entity.Property(c => c.SecondaryEmail)
                    .HasMaxLength(200);

                entity.Property(c => c.PrimaryAddress)
                    .HasMaxLength(500);

                entity.Property(c => c.SecondaryAddress)
                    .HasMaxLength(500);

                entity.Property(c => c.PrimaryPostalCode)
                    .HasMaxLength(20);

                entity.Property(c => c.SecondaryPostalCode)
                    .HasMaxLength(20);

                entity.Property(c => c.ProfileImagePath)
                    .HasMaxLength(500);

                entity.Property(c => c.Notes)
                    .HasMaxLength(2000);

                entity.Property(c => c.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(c => c.Creator)
                    .WithMany()
                    .HasForeignKey(c => c.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.LastUpdater)
                    .WithMany()
                    .HasForeignKey(c => c.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(c => c.NationalCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Contact_NationalCode")
                    .HasFilter("[NationalCode] IS NOT NULL");

                // ✅ اصلاح شده: تبدیل Property به Index
                entity.HasIndex(c => c.PrimaryEmail)
                    .HasDatabaseName("IX_Contact_PrimaryEmail");

                entity.HasIndex(c => new { c.FirstName, c.LastName })
                    .HasDatabaseName("IX_Contact_FullName");
            });
        }

        private static void ConfigureContactPhone(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactPhone>(entity =>
            {
                // Primary Key
                entity.HasKey(cp => cp.Id);

                // Properties
                entity.Property(cp => cp.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(cp => cp.Extension)
                    .HasMaxLength(10);

                entity.Property(cp => cp.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(cp => cp.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(cp => cp.Contact)
                    .WithMany(c => c.Phones)
                    .HasForeignKey(cp => cp.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cp => cp.Creator)
                    .WithMany()
                    .HasForeignKey(cp => cp.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(cp => new { cp.ContactId, cp.PhoneType })
                    .HasDatabaseName("IX_ContactPhone_Contact_Type");

                entity.HasIndex(cp => cp.PhoneNumber)
                    .HasDatabaseName("IX_ContactPhone_PhoneNumber");

                // Unique constraint for default phone
                entity.HasIndex(cp => new { cp.ContactId, cp.IsDefault })
                    .HasDatabaseName("IX_ContactPhone_Contact_Default")
                    .HasFilter("[IsDefault] = 1 AND [IsActive] = 1");
            });
        }

        #endregion

        #region Organization Configuration

        private static void ConfigureOrganization(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>(entity =>
            {
                // Primary Key
                entity.HasKey(o => o.Id);

                // Properties
                entity.Property(o => o.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(o => o.Brand)
                    .HasMaxLength(100);

                entity.Property(o => o.RegistrationNumber)
                    .HasMaxLength(11);

                entity.Property(o => o.EconomicCode)
                    .HasMaxLength(12);

                entity.Property(o => o.Website)
                    .HasMaxLength(200);

                entity.Property(o => o.LegalRepresentative)
                    .HasMaxLength(200);

                entity.Property(o => o.LogoPath)
                    .HasMaxLength(500);

                entity.Property(o => o.PrimaryPhone)
                    .HasMaxLength(15);

                entity.Property(o => o.SecondaryPhone)
                    .HasMaxLength(15);

                entity.Property(o => o.Email)
                    .HasMaxLength(200);

                entity.Property(o => o.Address)
                    .HasMaxLength(500);

                entity.Property(o => o.PostalCode)
                    .HasMaxLength(20);

                entity.Property(o => o.Description)
                    .HasMaxLength(2000);

                entity.Property(o => o.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(o => o.Creator)
                    .WithMany()
                    .HasForeignKey(o => o.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.LastUpdater)
                    .WithMany()
                    .HasForeignKey(o => o.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(o => o.RegistrationNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Organization_RegistrationNumber")
                    .HasFilter("[RegistrationNumber] IS NOT NULL");

                entity.HasIndex(o => o.EconomicCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Organization_EconomicCode")
                    .HasFilter("[EconomicCode] IS NOT NULL");

                entity.HasIndex(o => o.Name)
                    .HasDatabaseName("IX_Organization_Name");

                entity.HasIndex(o => o.OrganizationType)
                    .HasDatabaseName("IX_Organization_Type");
            });
        }

        private static void ConfigureOrganizationDepartment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationDepartment>(entity =>
            {
                // Primary Key
                entity.HasKey(od => od.Id);

                // Properties
                entity.Property(od => od.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(od => od.Code)
                    .HasMaxLength(50);

                entity.Property(od => od.Description)
                    .HasMaxLength(1000);

                entity.Property(od => od.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(od => od.Organization)
                    .WithMany(o => o.Departments)
                    .HasForeignKey(od => od.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.ParentDepartment)
                    .WithMany(od => od.ChildDepartments)
                    .HasForeignKey(od => od.ParentDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.ManagerContact)
                    .WithMany(c => c.ManagedDepartments)
                    .HasForeignKey(od => od.ManagerContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.Creator)
                    .WithMany()
                    .HasForeignKey(od => od.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.LastUpdater)
                    .WithMany()
                    .HasForeignKey(od => od.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(od => new { od.OrganizationId, od.Code })
                    .IsUnique()
                    .HasDatabaseName("IX_OrganizationDepartment_Org_Code")
                    .HasFilter("[Code] IS NOT NULL");

                entity.HasIndex(od => od.ParentDepartmentId)
                    .HasDatabaseName("IX_OrganizationDepartment_ParentId");

                entity.HasIndex(od => new { od.OrganizationId, od.Level })
                    .HasDatabaseName("IX_OrganizationDepartment_Org_Level");
            });
        }

        private static void ConfigureDepartmentPosition(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentPosition>(entity =>
            {
                // Primary Key
                entity.HasKey(dp => dp.Id);

                // Properties
                entity.Property(dp => dp.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(dp => dp.Description)
                    .HasMaxLength(500);

                entity.Property(dp => dp.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(dp => dp.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(dp => dp.Department)
                    .WithMany(od => od.Positions)
                    .HasForeignKey(dp => dp.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(dp => dp.Creator)
                    .WithMany()
                    .HasForeignKey(dp => dp.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(dp => new { dp.DepartmentId, dp.Title })
                    .IsUnique()
                    .HasDatabaseName("IX_DepartmentPosition_Dept_Title");

                entity.HasIndex(dp => new { dp.DepartmentId, dp.PowerLevel })
                    .HasDatabaseName("IX_DepartmentPosition_Dept_PowerLevel");

                // Unique constraint for default position per department
                entity.HasIndex(dp => new { dp.DepartmentId, dp.IsDefault })
                    .HasDatabaseName("IX_DepartmentPosition_Dept_Default")
                    .HasFilter("[IsDefault] = 1 AND [IsActive] = 1");
            });
        }

        private static void ConfigureDepartmentMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentMember>(entity =>
            {
                // Primary Key
                entity.HasKey(dm => dm.Id);

                // Properties
                entity.Property(dm => dm.Notes)
                    .HasMaxLength(500);

                entity.Property(dm => dm.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(dm => dm.Department)
                    .WithMany(od => od.Members)
                    .HasForeignKey(dm => dm.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dm => dm.Contact)
                    .WithMany(c => c.DepartmentMemberships)
                    .HasForeignKey(dm => dm.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dm => dm.Position)
                    .WithMany(dp => dp.Members)
                    .HasForeignKey(dm => dm.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dm => dm.Creator)
                    .WithMany()
                    .HasForeignKey(dm => dm.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(dm => new { dm.DepartmentId, dm.ContactId })
                    .IsUnique()
                    .HasDatabaseName("IX_DepartmentMember_Department_Contact");

                entity.HasIndex(dm => dm.PositionId)
                    .HasDatabaseName("IX_DepartmentMember_PositionId");

                entity.HasIndex(dm => new { dm.DepartmentId, dm.IsActive })
                    .HasDatabaseName("IX_DepartmentMember_Dept_Active");
            });
        }

        private static void ConfigureOrganizationContact(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationContact>(entity =>
            {
                // Primary Key
                entity.HasKey(oc => oc.Id);

                // Properties
                entity.Property(oc => oc.JobTitle)
                    .HasMaxLength(100);

                entity.Property(oc => oc.Department)
                    .HasMaxLength(100);

                entity.Property(oc => oc.Notes)
                    .HasMaxLength(1000);

                entity.Property(oc => oc.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(oc => oc.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(oc => oc.Organization)
                    .WithMany(o => o.Contacts)
                    .HasForeignKey(oc => oc.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oc => oc.Contact)
                    .WithMany(c => c.OrganizationRelations)
                    .HasForeignKey(oc => oc.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oc => oc.Creator)
                    .WithMany()
                    .HasForeignKey(oc => oc.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(oc => new { oc.OrganizationId, oc.ContactId, oc.RelationType })
                    .IsUnique()
                    .HasDatabaseName("IX_OrganizationContact_Org_Contact_Type");

                entity.HasIndex(oc => new { oc.OrganizationId, oc.IsPrimary })
                    .HasDatabaseName("IX_OrganizationContact_Org_Primary");

                entity.HasIndex(oc => oc.RelationType)
                    .HasDatabaseName("IX_OrganizationContact_RelationType");
            });
        }

        #endregion

        #region Contact Group Configuration

        private static void ConfigureContactGroup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactGroup>(entity =>
            {
                // Primary Key
                entity.HasKey(cg => cg.Id);

                // Properties
                entity.Property(cg => cg.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(cg => cg.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(cg => cg.Description)
                    .HasMaxLength(1000);

                entity.Property(cg => cg.ColorHex)
                    .HasMaxLength(7);

                entity.Property(cg => cg.IconClass)
                    .HasMaxLength(50);

                entity.Property(cg => cg.IsSystemGroup)
                    .HasDefaultValue(false);

                entity.Property(cg => cg.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(cg => cg.Creator)
                    .WithMany()
                    .HasForeignKey(cg => cg.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cg => cg.LastUpdater)
                    .WithMany()
                    .HasForeignKey(cg => cg.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(cg => cg.Code)
                    .IsUnique()
                    .HasDatabaseName("IX_ContactGroup_Code");

                entity.HasIndex(cg => cg.DisplayOrder)
                    .HasDatabaseName("IX_ContactGroup_DisplayOrder");
            });
        }

        private static void ConfigureContactGroupMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactGroupMember>(entity =>
            {
                // Primary Key
                entity.HasKey(cgm => cgm.Id);

                // Properties
                entity.Property(cgm => cgm.Notes)
                    .HasMaxLength(500);

                entity.Property(cgm => cgm.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(cgm => cgm.Group)
                    .WithMany(cg => cg.Members)
                    .HasForeignKey(cgm => cgm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cgm => cgm.Contact)
                    .WithMany()
                    .HasForeignKey(cgm => cgm.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cgm => cgm.AddedByUser)
                    .WithMany()
                    .HasForeignKey(cgm => cgm.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(cgm => new { cgm.GroupId, cgm.ContactId })
                    .IsUnique()
                    .HasDatabaseName("IX_ContactGroupMember_Group_Contact");
            });
        }

        private static void ConfigureBranchContactGroup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchContactGroup>(entity =>
            {
                // Primary Key
                entity.HasKey(bcg => bcg.Id);

                // Properties
                entity.Property(bcg => bcg.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(bcg => bcg.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(bcg => bcg.Description)
                    .HasMaxLength(1000);

                entity.Property(bcg => bcg.ColorHex)
                    .HasMaxLength(7);

                entity.Property(bcg => bcg.IconClass)
                    .HasMaxLength(50);

                entity.Property(bcg => bcg.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(bcg => bcg.Branch)
                    .WithMany()
                    .HasForeignKey(bcg => bcg.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bcg => bcg.Creator)
                    .WithMany()
                    .HasForeignKey(bcg => bcg.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bcg => bcg.LastUpdater)
                    .WithMany()
                    .HasForeignKey(bcg => bcg.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(bcg => new { bcg.BranchId, bcg.Code })
                    .IsUnique()
                    .HasDatabaseName("IX_BranchContactGroup_Branch_Code");

                entity.HasIndex(bcg => bcg.DisplayOrder)
                    .HasDatabaseName("IX_BranchContactGroup_DisplayOrder");
            });
        }
        private static void ConfigureBranchContactGroupMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchContactGroupMember>(entity =>
            {
                // Primary Key
                entity.HasKey(bcgm => bcgm.Id);

                // Properties
                entity.Property(bcgm => bcgm.Notes)
                    .HasMaxLength(500);

                entity.Property(bcgm => bcgm.IsActive)
                    .HasDefaultValue(true);

                // ✅ اصلاح شده: استفاده از property های صحیح
                // Relationships
                entity.HasOne(bcgm => bcgm.BranchGroup)
                    .WithMany(bcg => bcg.Members)
                    .HasForeignKey(bcgm => bcgm.BranchGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bcgm => bcgm.BranchContact)
                    .WithMany()
                    .HasForeignKey(bcgm => bcgm.BranchContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bcgm => bcgm.AddedByUser)
                    .WithMany()
                    .HasForeignKey(bcgm => bcgm.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(bcgm => new { bcgm.BranchGroupId, bcgm.BranchContactId })
                    .IsUnique()
                    .HasDatabaseName("IX_BranchContactGroupMember_Group_Contact");
            });
        }

        #endregion

        #region Organization Group Configuration

        private static void ConfigureOrganizationGroup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationGroup>(entity =>
            {
                // Primary Key
                entity.HasKey(og => og.Id);

                // Properties
                entity.Property(og => og.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(og => og.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(og => og.Description)
                    .HasMaxLength(1000);

                entity.Property(og => og.ColorHex)
                    .HasMaxLength(7);

                entity.Property(og => og.IconClass)
                    .HasMaxLength(50);

                entity.Property(og => og.IsSystemGroup)
                    .HasDefaultValue(false);

                entity.Property(og => og.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(og => og.Creator)
                    .WithMany()
                    .HasForeignKey(og => og.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(og => og.LastUpdater)
                    .WithMany()
                    .HasForeignKey(og => og.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(og => og.Code)
                    .IsUnique()
                    .HasDatabaseName("IX_OrganizationGroup_Code");

                entity.HasIndex(og => og.DisplayOrder)
                    .HasDatabaseName("IX_OrganizationGroup_DisplayOrder");
            });
        }
        private static void ConfigureOrganizationGroupMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationGroupMember>(entity =>
            {
                // Primary Key
                entity.HasKey(ogm => ogm.Id);

                // Properties
                entity.Property(ogm => ogm.Notes)
                    .HasMaxLength(500);

                entity.Property(ogm => ogm.IsActive)
                    .HasDefaultValue(true);

                // ✅ اصلاح شده: استفاده از property های صحیح
                // Relationships
                entity.HasOne(ogm => ogm.Group)
                    .WithMany(og => og.Members)
                    .HasForeignKey(ogm => ogm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ogm => ogm.Organization)
                    .WithMany()
                    .HasForeignKey(ogm => ogm.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ogm => ogm.AddedByUser)
                    .WithMany()
                    .HasForeignKey(ogm => ogm.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(ogm => new { ogm.GroupId, ogm.OrganizationId })
                    .IsUnique()
                    .HasDatabaseName("IX_OrganizationGroupMember_Group_Organization");
            });
        }
        private static void ConfigureBranchOrganizationGroup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchOrganizationGroup>(entity =>
            {
                // Primary Key
                entity.HasKey(bog => bog.Id);

                // Properties
                entity.Property(bog => bog.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(bog => bog.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(bog => bog.Description)
                    .HasMaxLength(1000);

                entity.Property(bog => bog.ColorHex)
                    .HasMaxLength(7);

                entity.Property(bog => bog.IconClass)
                    .HasMaxLength(50);

                entity.Property(bog => bog.IsActive)
                    .HasDefaultValue(true);

                // Relationships
                entity.HasOne(bog => bog.Branch)
                    .WithMany()
                    .HasForeignKey(bog => bog.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bog => bog.Creator)
                    .WithMany()
                    .HasForeignKey(bog => bog.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bog => bog.LastUpdater)
                    .WithMany()
                    .HasForeignKey(bog => bog.LastUpdaterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(bog => new { bog.BranchId, bog.Code })
                    .IsUnique()
                    .HasDatabaseName("IX_BranchOrganizationGroup_Branch_Code");

                entity.HasIndex(bog => bog.DisplayOrder)
                    .HasDatabaseName("IX_BranchOrganizationGroup_DisplayOrder");
            });
        }
        private static void ConfigureBranchOrganizationGroupMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchOrganizationGroupMember>(entity =>
            {
                // Primary Key
                entity.HasKey(bogm => bogm.Id);

                // Properties
                entity.Property(bogm => bogm.Notes)
                    .HasMaxLength(500);

                entity.Property(bogm => bogm.IsActive)
                    .HasDefaultValue(true);

                // ✅ اصلاح شده: استفاده از property های صحیح
                // Relationships
                entity.HasOne(bogm => bogm.BranchGroup)
                    .WithMany(bog => bog.Members)
                    .HasForeignKey(bogm => bogm.BranchGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bogm => bogm.BranchOrganization)
                    .WithMany()
                    .HasForeignKey(bogm => bogm.BranchOrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bogm => bogm.AddedByUser)
                    .WithMany()
                    .HasForeignKey(bogm => bogm.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(bogm => new { bogm.BranchGroupId, bogm.BranchOrganizationId })
                    .IsUnique()
                    .HasDatabaseName("IX_BranchOrganizationGroupMember_Group_Organization");
            });
        }

        #endregion
    }
}