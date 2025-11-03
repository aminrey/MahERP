using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class addtaskenhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "CompletionMode",
                table: "Tasks_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionMode",
                table: "Tasks_Tbl");
        }
    }
}
