using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixreminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSendCount",
                table: "TaskReminderSchedule_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SentCount",
                table: "TaskReminderSchedule_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationChannel",
                table: "TaskReminderEvent_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

          }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "MaxSendCount",
                table: "TaskReminderSchedule_Tbl");

            migrationBuilder.DropColumn(
                name: "SentCount",
                table: "TaskReminderSchedule_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationChannel",
                table: "TaskReminderEvent_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
