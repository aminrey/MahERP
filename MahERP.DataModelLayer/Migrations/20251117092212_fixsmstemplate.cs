using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixsmstemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "SmsTemplate_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "SmsTemplate_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "SmsTemplate_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "SmsTemplate_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "SmsTemplate_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTemplate_Tbl_AspNetUsers_CreatorUserId",
                table: "SmsTemplate_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
