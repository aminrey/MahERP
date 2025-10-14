using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                table: "UserPermission_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                table: "UserPermission_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserPermission_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByUserId",
                table: "UserPermission_Tbl",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "UserPermission_Tbl",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                table: "UserPermission_Tbl",
                column: "AssignedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                table: "UserPermission_Tbl",
                column: "ModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                table: "UserPermission_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                table: "UserPermission_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserPermission_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByUserId",
                table: "UserPermission_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "UserPermission_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                table: "UserPermission_Tbl",
                column: "AssignedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                table: "UserPermission_Tbl",
                column: "ModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
