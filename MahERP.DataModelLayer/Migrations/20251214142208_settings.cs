using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanEditSettingsRoles",
                table: "TaskSettings_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CanEditSettingsRoles",
                table: "TaskCategoryDefaultSettings_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CanEditSettingsRoles",
                table: "BranchDefaultTaskSettings_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanEditSettingsRoles",
                table: "TaskSettings_Tbl");

            migrationBuilder.DropColumn(
                name: "CanEditSettingsRoles",
                table: "TaskCategoryDefaultSettings_Tbl");

            migrationBuilder.DropColumn(
                name: "CanEditSettingsRoles",
                table: "BranchDefaultTaskSettings_Tbl");
        }
    }
}
