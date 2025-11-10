using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class dailytakstemolate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduleEnabled",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutionDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextExecutionDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ScheduleType",
                table: "NotificationTemplate_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledDayOfMonth",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduledDaysOfWeek",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduledTime",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "ارسال لیست تسک‌های انجام نشده هر روز (زمان‌بندی شده)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "IsScheduleEnabled",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "LastExecutionDate",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "NextExecutionDate",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduledDayOfMonth",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduledDaysOfWeek",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduledTime",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "ارسال لیست تسک‌های انجام نشده هر روز");
        }
    }
}
