using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class addcrminteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NextFollowUpNote",
                table: "CRMInteraction_Tbl",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "CRMInteraction_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "CRMInteraction_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_ContactId",
                table: "CRMInteraction_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Contact_Tbl_ContactId",
                table: "CRMInteraction_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Organization_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Contact_Tbl_ContactId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Organization_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CRMInteraction_Tbl_ContactId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CRMInteraction_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "NextFollowUpNote",
                table: "CRMInteraction_Tbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
