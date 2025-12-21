using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class intervaltask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalDayOfWeek",
                table: "ScheduledTaskCreation_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                table: "ScheduledTaskCreation_Tbl",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalDayOfWeek",
                table: "ScheduledTaskCreation_Tbl");

            migrationBuilder.DropColumn(
                name: "IntervalDays",
                table: "ScheduledTaskCreation_Tbl");
        }
    }
}
