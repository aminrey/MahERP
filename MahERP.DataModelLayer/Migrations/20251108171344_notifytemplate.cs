using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class notifytemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_LastModifierUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "NotificationTypeTemplate_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TemplateVariable_Template_Variable",
                table: "NotificationTemplateVariable_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_CreatorUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_LastModifierUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Type_Code",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.RenameColumn(
                name: "TemplateType",
                table: "NotificationTemplate_Tbl",
                newName: "RecipientMode");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "NotificationTemplateVariable_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<byte>(
                name: "DataType",
                table: "NotificationTemplateVariable_Tbl",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)0);

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "UsageCount",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateName",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateCode",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystemTemplate",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte>(
                name: "ChannelType",
                table: "NotificationTemplate_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NotificationTemplateRecipient_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateRecipient_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateRecipient_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateRecipient_Tbl_NotificationTemplate_Tbl_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalTable: "NotificationTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateVariable_Tbl_TemplateId",
                table: "NotificationTemplateVariable_Tbl",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_CreatedByUserId",
                table: "NotificationTemplate_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                column: "NotificationTypeConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_CreatedByUserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_NotificationTemplateId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRecipient_Tbl_UserId",
                table: "NotificationTemplateRecipient_Tbl",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplate_Tbl",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl",
                column: "NotificationTypeConfigId",
                principalTable: "NotificationTypeConfig_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_CreatedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_Tbl_NotificationTypeConfig_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "NotificationTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplateVariable_Tbl_TemplateId",
                table: "NotificationTemplateVariable_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_CreatedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_Tbl_NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.DropColumn(
                name: "NotificationTypeConfigId",
                table: "NotificationTemplate_Tbl");

            migrationBuilder.RenameColumn(
                name: "RecipientMode",
                table: "NotificationTemplate_Tbl",
                newName: "TemplateType");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "NotificationTemplateVariable_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "DataType",
                table: "NotificationTemplateVariable_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "UsageCount",
                table: "NotificationTemplate_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateName",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateCode",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystemTemplate",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "NotificationTemplate_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "NotificationTemplate_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifierUserId",
                table: "NotificationTemplate_Tbl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationTypeTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false),
                    NotificationTypeConfigId = table.Column<int>(type: "int", nullable: false),
                    ChannelType = table.Column<byte>(type: "tinyint", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UserGroupId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_TemplateVariable_Template_Variable",
                table: "NotificationTemplateVariable_Tbl",
                columns: new[] { "TemplateId", "VariableName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_CreatorUserId",
                table: "NotificationTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Tbl_LastModifierUserId",
                table: "NotificationTemplate_Tbl",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Type_Code",
                table: "NotificationTemplate_Tbl",
                columns: new[] { "TemplateType", "TemplateCode" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "NotificationTemplate_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_Tbl_AspNetUsers_LastModifierUserId",
                table: "NotificationTemplate_Tbl",
                column: "LastModifierUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
