using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddLogEntity02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserActivityLog_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLog_Tbl_UserId",
                table: "UserActivityLog_Tbl",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLog_Tbl_AspNetUsers_UserId",
                table: "UserActivityLog_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLog_Tbl_AspNetUsers_UserId",
                table: "UserActivityLog_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLog_Tbl_UserId",
                table: "UserActivityLog_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserActivityLog_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
