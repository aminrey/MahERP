using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig5assignedTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedTeamId",
                table: "TaskAssignment_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignedTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedTeamId",
                principalTable: "Team_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedTeamId",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignment_Tbl_AssignedTeamId",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "AssignedTeamId",
                table: "TaskAssignment_Tbl");
        }
    }
}
