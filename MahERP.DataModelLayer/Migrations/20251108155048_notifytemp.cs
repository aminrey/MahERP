using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class notifytemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTypeTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeConfigId = table.Column<int>(type: "int", nullable: false),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false),
                    ChannelType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UserGroupId = table.Column<int>(type: "int", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypeTemplate_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTypeTemplate_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationTypeTemplate_Tbl_NotificationTemplate_Tbl_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalTable: "NotificationTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationTypeTemplate_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                        column: x => x.NotificationTypeConfigId,
                        principalTable: "NotificationTypeConfig_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeTemplate_Tbl_CreatedByUserId",
                table: "NotificationTypeTemplate_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeTemplate_Tbl_NotificationTemplateId",
                table: "NotificationTypeTemplate_Tbl",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeTemplate_Type_Channel_Priority",
                table: "NotificationTypeTemplate_Tbl",
                columns: new[] { "NotificationTypeConfigId", "ChannelType", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeTemplate_Unique",
                table: "NotificationTypeTemplate_Tbl",
                columns: new[] { "NotificationTypeConfigId", "NotificationTemplateId", "ChannelType", "UserGroupId" },
                unique: true,
                filter: "[UserGroupId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTypeTemplate_Tbl");
        }
    }
}
