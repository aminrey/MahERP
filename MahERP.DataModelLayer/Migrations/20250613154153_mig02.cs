using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "StakeholderContact_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "StakeholderContact_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "StakeholderCRM_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    LeadSource = table.Column<byte>(type: "tinyint", nullable: false),
                    SalesStage = table.Column<byte>(type: "tinyint", nullable: false),
                    LastContactDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PotentialValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreditRating = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Preferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalesRepUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderCRM_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_AspNetUsers_SalesRepUserId",
                        column: x => x.SalesRepUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskCRMDetails_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpNote = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCRMDetails_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_SalesRepUserId",
                table: "StakeholderCRM_Tbl",
                column: "SalesRepUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_StakeholderId",
                table: "StakeholderCRM_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_StakeholderContactId",
                table: "TaskCRMDetails_Tbl",
                column: "StakeholderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_TaskId",
                table: "TaskCRMDetails_Tbl",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StakeholderCRM_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCRMDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "StakeholderContact_Tbl");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "StakeholderContact_Tbl");
        }
    }
}
