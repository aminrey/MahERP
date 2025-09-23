using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig_uknow_From_amin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedUserInfo",
                table: "Tasks_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignerUserId",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedUserId",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DeletedAssignedUserInfo",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedAssignerUserInfo",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletelyDeletedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletelyDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedUserInfo",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "DeletedAssignedUserInfo",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "DeletedAssignerUserInfo",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompletelyDeletedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsCompletelyDeleted",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "AssignerUserId",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignedUserId",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
