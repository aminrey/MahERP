using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig20tasklog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "Tasks_Tbl");

            migrationBuilder.AddColumn<DateTime>(
                name: "FocusedDate",
                table: "TaskAssignment_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFocused",
                table: "TaskAssignment_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "Branch_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotTokenName",
                table: "Branch_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskWorkLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskWorkLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskWorkLog_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Branch_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "TelegramBotToken", "TelegramBotTokenName" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkLog_Tbl_TaskId",
                table: "TaskWorkLog_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkLog_Tbl_UserId",
                table: "TaskWorkLog_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskWorkLog_Tbl");

            migrationBuilder.DropColumn(
                name: "FocusedDate",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "IsFocused",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "Branch_Tbl");

            migrationBuilder.DropColumn(
                name: "TelegramBotTokenName",
                table: "Branch_Tbl");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "Tasks_Tbl",
                type: "datetime2",
                nullable: true);
        }
    }
}
