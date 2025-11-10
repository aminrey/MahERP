using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class somefields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "TypeNameFa" },
                values: new object[] { "ارسال پیام زمان بندی شده)", "اعلان زمانبدی شده" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "TypeNameFa" },
                values: new object[] { "ارسال لیست تسک‌های انجام نشده هر روز (زمان‌بندی شده)", "اعلان روزانه تسک‌های انجام نشده" });
        }
    }
}
