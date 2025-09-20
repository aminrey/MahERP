using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig4fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TaskViewer_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "TaskViewer_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaskViewer_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "TaskViewer_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdaterUserId",
                table: "TaskViewer_Tbl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SpecialPermissionType",
                table: "TaskViewer_Tbl",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "TaskViewer_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_LastUpdaterUserId",
                table: "TaskViewer_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_TeamId",
                table: "TaskViewer_Tbl",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskViewer_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "TaskViewer_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskViewer_Tbl_Team_Tbl_TeamId",
                table: "TaskViewer_Tbl",
                column: "TeamId",
                principalTable: "Team_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskViewer_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskViewer_Tbl_Team_Tbl_TeamId",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskViewer_Tbl_LastUpdaterUserId",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskViewer_Tbl_TeamId",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdaterUserId",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "SpecialPermissionType",
                table: "TaskViewer_Tbl");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TaskViewer_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TaskViewer_Tbl",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
