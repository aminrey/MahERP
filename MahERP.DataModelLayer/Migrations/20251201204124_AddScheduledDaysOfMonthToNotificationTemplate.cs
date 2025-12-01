using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledDaysOfMonthToNotificationTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "NotificationModuleConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ScheduledDaysOfMonth",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDaysOfMonth",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.InsertData(
                table: "NotificationModuleConfig_Tbl",
                columns: new[] { "Id", "ColorCode", "Description", "DisplayOrder", "IsActive", "ModuleCode", "ModuleNameEn", "ModuleNameFa" },
                values: new object[] { 1, "#2196F3", "سیستم مدیریت تسک‌ها و پروژه‌ها", 1, true, "TASKING", "Tasking Module", "ماژول تسکینگ" });

            migrationBuilder.InsertData(
                table: "NotificationTypeConfig_Tbl",
                columns: new[] { "Id", "AllowUserCustomization", "CoreNotificationTypeGeneral", "CoreNotificationTypeSpecific", "DefaultEmailTemplateId", "DefaultPriority", "DefaultSmsTemplateId", "DefaultSystemNotificationTemplateId", "DefaultTelegramTemplateId", "Description", "DisplayOrder", "IsActive", "ModuleConfigId", "RelatedEventTypes", "SendMode", "SupportsEmail", "SupportsSms", "SupportsTelegram", "TypeCode", "TypeNameFa" },
                values: new object[,]
                {
                    { 1, true, (byte)0, (byte)0, null, (byte)0, null, null, null, "ارسال پیام زمان بندی شده)", 1, true, 1, "[13]", (byte)0, true, false, true, "TASK_DAILY_DIGEST", "اعلان زمانبدی شده" },
                    { 2, true, (byte)9, (byte)1, null, (byte)1, null, null, null, "اعلان هنگام تخصیص تسک جدید به کاربر", 2, true, 1, "[1,12]", (byte)0, true, true, true, "TASK_ASSIGNED", "تخصیص تسک جدید" },
                    { 3, true, (byte)8, (byte)2, null, (byte)1, null, null, null, "اعلان تکمیل تسک به سازنده", 3, true, 1, "[2,6]", (byte)0, true, false, true, "TASK_COMPLETED", "تکمیل تسک واگذار شده" },
                    { 4, true, (byte)6, (byte)3, null, (byte)2, null, null, null, "یادآوری تسک‌های نزدیک به سررسید", 4, true, 1, "[3]", (byte)0, true, true, true, "TASK_REMINDER", "یادآوری سررسید تسک" },
                    { 5, true, (byte)10, (byte)4, null, (byte)0, null, null, null, "اعلان ثبت کامنت، WorkLog یا تغییرات", 5, true, 1, "[4,5,8,10,11,14]", (byte)0, true, false, true, "TASK_UPDATED", "تغییرات در تسک" }
                });
        }
    }
}
