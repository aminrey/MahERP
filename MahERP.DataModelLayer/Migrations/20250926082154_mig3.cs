using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskReminderSchedule_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReminderType = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: true),
                    DaysBeforeDeadline = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "bit", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminderSchedule_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReminderSchedule_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderSchedule_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskReminderEvent_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EventType = table.Column<byte>(type: "tinyint", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    NotificationChannel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminderEvent_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_TaskReminderSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskReminderSchedule_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_CreatorUserId",
                table: "TaskReminderEvent_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_RecipientUserId",
                table: "TaskReminderEvent_Tbl",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_ScheduleId",
                table: "TaskReminderEvent_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_TaskId",
                table: "TaskReminderEvent_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderSchedule_Tbl_CreatorUserId",
                table: "TaskReminderSchedule_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderSchedule_Tbl_TaskId",
                table: "TaskReminderSchedule_Tbl",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReminderEvent_Tbl");

            migrationBuilder.DropTable(
                name: "TaskReminderSchedule_Tbl");
        }
    }
}
