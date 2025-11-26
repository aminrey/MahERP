using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <summary>
    /// Migration برای اضافه کردن فیلد ScheduledDaysOfMonth به TaskReminderSchedule
    /// جهت پشتیبانی از یادآوری ماهانه با انتخاب چند روز
    /// </summary>
    public partial class AddScheduledDaysOfMonthToReminder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduledDaysOfMonth",
                table: "TaskReminderSchedule_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "روزهای ماه برای یادآوری ماهانه (فرمت: \"10,20,25\")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDaysOfMonth",
                table: "TaskReminderSchedule_Tbl");
        }
    }
}
