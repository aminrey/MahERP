using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledTaskCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "Tasks_Tbl",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHardDeadline",
                table: "Tasks_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuggestedStartDate",
                table: "Tasks_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeNote",
                table: "Tasks_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduledTaskCreation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScheduleDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaskDataJson = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ScheduleType = table.Column<byte>(type: "tinyint", nullable: false),
                    ScheduledTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ScheduledDaysOfWeek = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScheduledDayOfMonth = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxOccurrences = table.Column<int>(type: "int", nullable: true),
                    ExecutionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsScheduleEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskCreation_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskCreation_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskCreation_Tbl_AspNetUsers_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScheduledTaskCreation_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskCreation_Active_Enabled_Next",
                table: "ScheduledTaskCreation_Tbl",
                columns: new[] { "IsActive", "IsScheduleEnabled", "NextExecutionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskCreation_Branch",
                table: "ScheduledTaskCreation_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskCreation_CreatedBy",
                table: "ScheduledTaskCreation_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskCreation_NextExecutionDate",
                table: "ScheduledTaskCreation_Tbl",
                column: "NextExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskCreation_Tbl_ModifiedByUserId",
                table: "ScheduledTaskCreation_Tbl",
                column: "ModifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledTaskCreation_Tbl");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "IsHardDeadline",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "SuggestedStartDate",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "TimeNote",
                table: "Tasks_Tbl");
        }
    }
}
