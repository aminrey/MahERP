using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig13Stakeholder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "StakeholderContact_Tbl");

            migrationBuilder.RenameColumn(
                name: "ContactPriority",
                table: "StakeholderContact_Tbl",
                newName: "ImportanceLevel");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ContactType",
                table: "StakeholderContact_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDecisionMaker",
                table: "StakeholderContact_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "StakeholderContact_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Stakeholder_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stakeholder_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalCode",
                table: "Stakeholder_Tbl",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "Stakeholder_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                table: "Stakeholder_Tbl",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Stakeholder_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Stakeholder_Tbl",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Stakeholder_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Stakeholder_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyBrand",
                table: "Stakeholder_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EconomicCode",
                table: "Stakeholder_Tbl",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Gender",
                table: "Stakeholder_Tbl",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdaterUserId",
                table: "Stakeholder_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentative",
                table: "Stakeholder_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "PersonType",
                table: "Stakeholder_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddress",
                table: "Stakeholder_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "Stakeholder_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Stakeholder_Tbl",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Stakeholder_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StakeholderOrganization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentOrganizationId = table.Column<int>(type: "int", nullable: true),
                    ManagerContactId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_StakeholderContact_Tbl_ManagerContactId",
                        column: x => x.ManagerContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_StakeholderOrganization_Tbl_ParentOrganizationId",
                        column: x => x.ParentOrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderOrganizationPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganizationPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationPosition_Tbl_StakeholderOrganization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderOrganizationMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    IsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganizationMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderContact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderOrganizationPosition_Tbl_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StakeholderOrganizationPosition_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderOrganization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_LastUpdaterUserId",
                table: "Stakeholder_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_CreatorUserId",
                table: "StakeholderOrganization_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_LastUpdaterUserId",
                table: "StakeholderOrganization_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_ManagerContactId",
                table: "StakeholderOrganization_Tbl",
                column: "ManagerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_ParentOrganizationId",
                table: "StakeholderOrganization_Tbl",
                column: "ParentOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_StakeholderId",
                table: "StakeholderOrganization_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_ContactId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_CreatorUserId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_OrganizationId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_PositionId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationPosition_Tbl_CreatorUserId",
                table: "StakeholderOrganizationPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationPosition_Tbl_OrganizationId",
                table: "StakeholderOrganizationPosition_Tbl",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stakeholder_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "Stakeholder_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stakeholder_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganizationMember_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganizationPosition_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganization_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Stakeholder_Tbl_LastUpdaterUserId",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "ContactType",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "IsDecisionMaker",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "CompanyBrand",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "EconomicCode",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdaterUserId",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "LegalRepresentative",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "PersonType",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "RegisteredAddress",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Stakeholder_Tbl");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Stakeholder_Tbl");

            migrationBuilder.RenameColumn(
                name: "ImportanceLevel",
                table: "StakeholderContact_Tbl",
                newName: "ContactPriority");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalCode",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                table: "Stakeholder_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Stakeholder_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
