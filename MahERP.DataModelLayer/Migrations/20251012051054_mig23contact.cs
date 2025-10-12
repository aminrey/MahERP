using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig23contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ContactPhone_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "ContactPhone_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_Tbl_CreatorUserId",
                table: "ContactPhone_Tbl",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhone_Tbl_AspNetUsers_CreatorUserId",
                table: "ContactPhone_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhone_Tbl_AspNetUsers_CreatorUserId",
                table: "ContactPhone_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_ContactPhone_Tbl_CreatorUserId",
                table: "ContactPhone_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ContactPhone_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "ContactPhone_Tbl");
        }
    }
}
