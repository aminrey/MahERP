using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddIsExpiredToTaskReminderSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDate",
                table: "TaskReminderSchedule_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpiredReason",
                table: "TaskReminderSchedule_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                table: "TaskReminderSchedule_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                table: "TaskReminderSchedule_Tbl");

            migrationBuilder.DropColumn(
                name: "ExpiredReason",
                table: "TaskReminderSchedule_Tbl");

            migrationBuilder.DropColumn(
                name: "IsExpired",
                table: "TaskReminderSchedule_Tbl");
        }
    }
}
