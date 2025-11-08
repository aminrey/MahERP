using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class notifyrec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "SendMode",
                table: "NotificationTypeConfig_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "NotificationRecipient_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeConfigId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipient_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRecipient_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationRecipient_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationRecipient_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                        column: x => x.NotificationTypeConfigId,
                        principalTable: "NotificationTypeConfig_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                column: "SendMode",
                value: (byte)0);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 2,
                column: "SendMode",
                value: (byte)0);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 3,
                column: "SendMode",
                value: (byte)0);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 4,
                column: "SendMode",
                value: (byte)0);

            migrationBuilder.UpdateData(
                table: "NotificationTypeConfig_Tbl",
                keyColumn: "Id",
                keyValue: 5,
                column: "SendMode",
                value: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipient_Tbl_CreatedByUserId",
                table: "NotificationRecipient_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipient_Tbl_UserId",
                table: "NotificationRecipient_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipient_TypeUser",
                table: "NotificationRecipient_Tbl",
                columns: new[] { "NotificationTypeConfigId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "SendMode",
                table: "NotificationTypeConfig_Tbl");
        }
    }
}
