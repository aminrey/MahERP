using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class contactphone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplateRecipient_Tbl_ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl",
                column: "ContactPhoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTemplateRecipient_Tbl_ContactPhone_Tbl_ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl",
                column: "ContactPhoneId",
                principalTable: "ContactPhone_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTemplateRecipient_Tbl_ContactPhone_Tbl_ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_SmsTemplateRecipient_Tbl_ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl");

            migrationBuilder.DropColumn(
                name: "ContactPhoneId",
                table: "SmsTemplateRecipient_Tbl");
        }
    }
}
