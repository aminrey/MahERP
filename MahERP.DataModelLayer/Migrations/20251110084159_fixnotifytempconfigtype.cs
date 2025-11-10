using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixnotifytempconfigtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelatedEventTypes",
                table: "NotificationTypeConfig_Tbl",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                column: "RelatedEventTypes",
                value: "[13]");

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 2,
                column: "RelatedEventTypes",
                value: "[1,12]");

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 3,
                column: "RelatedEventTypes",
                value: "[2,6]");

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 4,
                column: "RelatedEventTypes",
                value: "[3]");

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 5,
                column: "RelatedEventTypes",
                value: "[4,5,8,10,11,14]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedEventTypes",
                table: "NotificationTypeConfig_Tbl");
        }
    }
}
