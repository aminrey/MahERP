using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig_04_NeedHoseinTesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "TaskReminderSchedule_Tbl",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "TaskOperation_Tbl",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsStarred",
                table: "TaskOperation_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStarred",
                table: "TaskOperation_Tbl");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "TaskReminderSchedule_Tbl",
                newName: "CreateDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "TaskOperation_Tbl",
                newName: "CreateDate");
        }
    }
}
