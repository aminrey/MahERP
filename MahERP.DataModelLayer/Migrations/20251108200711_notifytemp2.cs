using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class notifytemp2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_UserId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ChatidUserTelegram",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "NotificationTemplateHistory_Tbl",
                newName: "MessageTemplate");

            migrationBuilder.RenameColumn(
                name: "ChannelType",
                table: "NotificationTemplate_Tbl",
                newName: "NotificationEventType");

            migrationBuilder.AddColumn<bool>(
                name: "IsTelegramEnabled",
                table: "Settings_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "Settings_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TelegramSystemLogGroupId",
                table: "Settings_Tbl",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RecipientType",
                table: "NotificationTemplateRecipient_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Channel",
                table: "NotificationTemplate_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByUserId",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageTemplate",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "TelegramChatId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_ContactId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_OrganizationId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_LastModifiedByUserId",
                table: "NotificationTemplate_Tbl",
                column: "LastModifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_LastModifiedByUserId",
                table: "NotificationTemplate_Tbl",
                column: "LastModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_UserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_Contact_Tbl_ContactId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_Organization_Tbl_OrganizationId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_LastModifiedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_UserId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_Contact_Tbl_ContactId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_Organization_Tbl_OrganizationId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_ContactId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_OrganizationId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_LastModifiedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "IsTelegramEnabled",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "TelegramSystemLogGroupId",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "RecipientType",
                table: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "MessageTemplate",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.RenameColumn(
                name: "MessageTemplate",
                table: "NotificationTemplateHistory_Tbl",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "NotificationEventType",
                table: "NotificationTemplate_Tbl",
                newName: "ChannelType");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "NotificationTemplateRecipient_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TelegramChatId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChatidUserTelegram",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                column: "NotificationTypeConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                column: "NotificationTypeConfigId",
                principalTable: "NotificationTypeConfig_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_UserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
