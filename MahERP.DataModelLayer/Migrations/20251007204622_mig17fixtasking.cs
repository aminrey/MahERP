using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig17fixtasking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "DisplayLevel",
                table: "Tasks_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Tasks_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AssignedInTeamId",
                table: "TaskAssignment_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignedInTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedInTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedInTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedInTeamId",
                principalTable: "Team_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedInTeamId",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignment_Tbl_AssignedInTeamId",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "DisplayLevel",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "AssignedInTeamId",
                table: "TaskAssignment_Tbl");
        }
    }
}
