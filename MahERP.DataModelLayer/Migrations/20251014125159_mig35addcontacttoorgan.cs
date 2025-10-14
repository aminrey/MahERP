using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig35addcontacttoorgan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                table: "DepartmentMember_Tbl");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "DepartmentMember_Tbl",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                table: "DepartmentMember_Tbl",
                column: "PositionId",
                principalTable: "DepartmentPosition_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                table: "DepartmentMember_Tbl");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "DepartmentMember_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                table: "DepartmentMember_Tbl",
                column: "PositionId",
                principalTable: "DepartmentPosition_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
