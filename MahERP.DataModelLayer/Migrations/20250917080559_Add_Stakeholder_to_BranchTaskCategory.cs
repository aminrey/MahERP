using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class Add_Stakeholder_to_BranchTaskCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StakeholderId",
                table: "BranchTaskCategory_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategory_Tbl_StakeholderId",
                table: "BranchTaskCategory_Tbl",
                column: "StakeholderId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchTaskCategory_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "BranchTaskCategory_Tbl",
                column: "StakeholderId",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchTaskCategory_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "BranchTaskCategory_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_BranchTaskCategory_Tbl_StakeholderId",
                table: "BranchTaskCategory_Tbl");

            migrationBuilder.DropColumn(
                name: "StakeholderId",
                table: "BranchTaskCategory_Tbl");
        }
    }
}
