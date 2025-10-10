using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig21MydayTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskMyDay_Tbl_AspNetUsers_UserId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskMyDay_Tbl_Tasks_Tbl_TaskId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskMyDay_Tbl_TaskId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskMyDay_Tbl_UserId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskMyDay_Tbl",
                newName: "TaskAssignmentId");

            migrationBuilder.RenameColumn(
                name: "IsWorkedOn",
                table: "TaskMyDay_Tbl",
                newName: "IsRemoved");

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedDate",
                table: "TaskMyDay_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "TaskMyDay_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Assignment_Date",
                table: "TaskMyDay_Tbl",
                columns: new[] { "TaskAssignmentId", "PlannedDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskMyDay_Tbl_TaskAssignment_Tbl_TaskAssignmentId",
                table: "TaskMyDay_Tbl",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignment_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskMyDay_Tbl_TaskAssignment_Tbl_TaskAssignmentId",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TaskMyDay_Assignment_Date",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropColumn(
                name: "RemovedDate",
                table: "TaskMyDay_Tbl");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "TaskMyDay_Tbl");

            migrationBuilder.RenameColumn(
                name: "TaskAssignmentId",
                table: "TaskMyDay_Tbl",
                newName: "TaskId");

            migrationBuilder.RenameColumn(
                name: "IsRemoved",
                table: "TaskMyDay_Tbl",
                newName: "IsWorkedOn");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaskMyDay_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TaskMyDay_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Tbl_TaskId",
                table: "TaskMyDay_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Tbl_UserId",
                table: "TaskMyDay_Tbl",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskMyDay_Tbl_AspNetUsers_UserId",
                table: "TaskMyDay_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskMyDay_Tbl_Tasks_Tbl_TaskId",
                table: "TaskMyDay_Tbl",
                column: "TaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
