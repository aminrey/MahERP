using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class addtaskenhancement2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIndependentCompletion",
                table: "Tasks_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIndependentCompletion",
                table: "Tasks_Tbl");
        }
    }
}
