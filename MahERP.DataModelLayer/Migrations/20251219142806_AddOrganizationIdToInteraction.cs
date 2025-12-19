using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToInteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Interaction_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_Tbl_OrganizationId",
                table: "Interaction_Tbl",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interaction_Tbl_Organization_Tbl_OrganizationId",
                table: "Interaction_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interaction_Tbl_Organization_Tbl_OrganizationId",
                table: "Interaction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Interaction_Tbl_OrganizationId",
                table: "Interaction_Tbl");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Interaction_Tbl");
        }
    }
}
