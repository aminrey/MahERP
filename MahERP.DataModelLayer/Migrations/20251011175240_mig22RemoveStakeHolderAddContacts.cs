using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig22RemoveStakeHolderAddContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "Tasks_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId1",
                table: "Tasks_Tbl");

            migrationBuilder.RenameColumn(
                name: "StakeholderId1",
                table: "Tasks_Tbl",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Tbl_StakeholderId1",
                table: "Tasks_Tbl",
                newName: "IX_Tasks_Tbl_OrganizationId");

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrimaryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecondaryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contact_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contact_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Organization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    EconomicCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrganizationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organization_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organization_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BranchContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ContactPhone_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PhoneType = table.Column<byte>(type: "tinyint", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPhone_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPhone_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchOrganization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    IncludeAllMembers = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsDecisionMaker = table.Column<bool>(type: "bit", nullable: false),
                    ImportanceLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationDepartment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ManagerContactId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationDepartment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_Contact_Tbl_ManagerContactId",
                        column: x => x.ManagerContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_OrganizationDepartment_Tbl_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    MinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CanHireSubordinates = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentPosition_Tbl_OrganizationDepartment_Tbl_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmploymentType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                        column: x => x.PositionId,
                        principalTable: "DepartmentPosition_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_OrganizationDepartment_Tbl_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ContactId",
                table: "Tasks_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Branch_Contact",
                table: "BranchContact_Tbl",
                columns: new[] { "BranchId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Tbl_AssignedByUserId",
                table: "BranchContact_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Tbl_ContactId",
                table: "BranchContact_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Branch_Organization",
                table: "BranchOrganization_Tbl",
                columns: new[] { "BranchId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Tbl_AssignedByUserId",
                table: "BranchOrganization_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Tbl_OrganizationId",
                table: "BranchOrganization_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_NationalCode",
                table: "Contact_Tbl",
                column: "NationalCode",
                unique: true,
                filter: "[NationalCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Tbl_CreatorUserId",
                table: "Contact_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Tbl_LastUpdaterUserId",
                table: "Contact_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_Contact_Number",
                table: "ContactPhone_Tbl",
                columns: new[] { "ContactId", "PhoneNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Department_Contact",
                table: "DepartmentMember_Tbl",
                columns: new[] { "DepartmentId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_ContactId",
                table: "DepartmentMember_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_CreatorUserId",
                table: "DepartmentMember_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_PositionId",
                table: "DepartmentMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPosition_Tbl_CreatorUserId",
                table: "DepartmentPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPosition_Tbl_DepartmentId",
                table: "DepartmentPosition_Tbl",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_EconomicCode",
                table: "Organization_Tbl",
                column: "EconomicCode",
                unique: true,
                filter: "[EconomicCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_RegistrationNumber",
                table: "Organization_Tbl",
                column: "RegistrationNumber",
                unique: true,
                filter: "[RegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Tbl_CreatorUserId",
                table: "Organization_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Tbl_LastUpdaterUserId",
                table: "Organization_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Org_Contact_Type",
                table: "OrganizationContact_Tbl",
                columns: new[] { "OrganizationId", "ContactId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Tbl_ContactId",
                table: "OrganizationContact_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Tbl_CreatorUserId",
                table: "OrganizationContact_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_CreatorUserId",
                table: "OrganizationDepartment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_LastUpdaterUserId",
                table: "OrganizationDepartment_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_ManagerContactId",
                table: "OrganizationDepartment_Tbl",
                column: "ManagerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_OrganizationId",
                table: "OrganizationDepartment_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_ParentDepartmentId",
                table: "OrganizationDepartment_Tbl",
                column: "ParentDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_Contact_Tbl_ContactId",
                table: "Tasks_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_Organization_Tbl_OrganizationId",
                table: "Tasks_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "Tasks_Tbl",
                column: "StakeholderId",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_Contact_Tbl_ContactId",
                table: "Tasks_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_Organization_Tbl_OrganizationId",
                table: "Tasks_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "Tasks_Tbl");

            migrationBuilder.DropTable(
                name: "BranchContact_Tbl");

            migrationBuilder.DropTable(
                name: "BranchOrganization_Tbl");

            migrationBuilder.DropTable(
                name: "ContactPhone_Tbl");

            migrationBuilder.DropTable(
                name: "DepartmentMember_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationContact_Tbl");

            migrationBuilder.DropTable(
                name: "DepartmentPosition_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationDepartment_Tbl");

            migrationBuilder.DropTable(
                name: "Contact_Tbl");

            migrationBuilder.DropTable(
                name: "Organization_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Tbl_ContactId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Tasks_Tbl");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Tasks_Tbl",
                newName: "StakeholderId1");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Tbl_OrganizationId",
                table: "Tasks_Tbl",
                newName: "IX_Tasks_Tbl_StakeholderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "Tasks_Tbl",
                column: "StakeholderId",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId1",
                table: "Tasks_Tbl",
                column: "StakeholderId1",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id");
        }
    }
}
