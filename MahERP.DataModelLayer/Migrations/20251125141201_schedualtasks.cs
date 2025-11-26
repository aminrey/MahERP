using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class schedualtasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ScheduleId",
                table: "Tasks_Tbl",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_ScheduledTaskCreation_Tbl_ScheduleId",
                table: "Tasks_Tbl",
                column: "ScheduleId",
                principalTable: "ScheduledTaskCreation_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_ScheduledTaskCreation_Tbl_ScheduleId",
                table: "Tasks_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Tbl_ScheduleId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Tasks_Tbl");
        }
    }
}
