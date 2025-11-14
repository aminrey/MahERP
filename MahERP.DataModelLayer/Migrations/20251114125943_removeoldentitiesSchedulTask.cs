using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class removeoldentitiesSchedulTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_TaskSchedule_Tbl_ScheduleId",
                table: "Tasks_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleExecution");

            migrationBuilder.DropTable(
                name: "TaskScheduleViewer_Tbl");

            migrationBuilder.DropTable(
                name: "TaskTemplateOperation_Tbl");

            migrationBuilder.DropTable(
                name: "TaskSchedule_Tbl");

            migrationBuilder.DropTable(
                name: "TaskTemplate_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Tbl_ScheduleId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Tasks_Tbl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AddBranchManagerAsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DefaultDurationDays = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    TaskType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplate_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplate_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskTemplate_Tbl_TaskCategory_Tbl_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskSchedule_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifierUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaskTemplateId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastRunErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRunStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthDay = table.Column<byte>(type: "tinyint", nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecurrenceInterval = table.Column<int>(type: "int", nullable: false),
                    RecurrenceType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeekDays = table.Column<byte>(type: "tinyint", nullable: false),
                    YearMonth = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSchedule_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_AspNetUsers_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_TaskTemplate_Tbl_TaskTemplateId",
                        column: x => x.TaskTemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplateOperation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplateOperation_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplateOperation_Tbl_TaskTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleAssignment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PredefinedCopyDescriptionId = table.Column<int>(type: "int", nullable: true),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignmentType = table.Column<byte>(type: "tinyint", nullable: false),
                    CopyDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleAssignment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                        column: x => x.PredefinedCopyDescriptionId,
                        principalTable: "PredefinedCopyDescription_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleExecution",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTaskId = table.Column<int>(type: "int", nullable: true),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionDuration = table.Column<double>(type: "float", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleExecution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleExecution_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleExecution_Tasks_Tbl_CreatedTaskId",
                        column: x => x.CreatedTaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleViewer_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleViewer_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ScheduleId",
                table: "Tasks_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_CreatorUserId",
                table: "TaskSchedule_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_ModifierUserId",
                table: "TaskSchedule_Tbl",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_TaskTemplateId",
                table: "TaskSchedule_Tbl",
                column: "TaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_CreatorUserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment_Tbl",
                column: "PredefinedCopyDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_ScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_UserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleExecution_CreatedTaskId",
                table: "TaskScheduleExecution",
                column: "CreatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleExecution_ScheduleId",
                table: "TaskScheduleExecution",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_AddedByUserId",
                table: "TaskScheduleViewer_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_ScheduleId",
                table: "TaskScheduleViewer_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_UserId",
                table: "TaskScheduleViewer_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplate_Tbl_CategoryId",
                table: "TaskTemplate_Tbl",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplate_Tbl_CreatorUserId",
                table: "TaskTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateOperation_Tbl_TemplateId",
                table: "TaskTemplateOperation_Tbl",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_TaskSchedule_Tbl_ScheduleId",
                table: "Tasks_Tbl",
                column: "ScheduleId",
                principalTable: "TaskSchedule_Tbl",
                principalColumn: "Id");
        }
    }
}
