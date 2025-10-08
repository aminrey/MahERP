using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig16stakeholderBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "StakeholderBranch_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignDate",
                table: "StakeholderBranch_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AssignedByUserId",
                table: "StakeholderBranch_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_AssignedByUserId",
                table: "StakeholderBranch_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StakeholderBranch_Tbl_AspNetUsers_AssignedByUserId",
                table: "StakeholderBranch_Tbl",
                column: "AssignedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StakeholderBranch_Tbl_AspNetUsers_AssignedByUserId",
                table: "StakeholderBranch_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_StakeholderBranch_Tbl_AssignedByUserId",
                table: "StakeholderBranch_Tbl");

            migrationBuilder.DropColumn(
                name: "AssignDate",
                table: "StakeholderBranch_Tbl");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "StakeholderBranch_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorUserId",
                table: "StakeholderBranch_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
