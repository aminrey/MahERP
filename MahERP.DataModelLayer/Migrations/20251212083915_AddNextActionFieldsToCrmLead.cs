using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddNextActionFieldsToCrmLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextActionDate",
                table: "CrmLead_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextActionNote",
                table: "CrmLead_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextActionTaskId",
                table: "CrmLead_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "NextActionType",
                table: "CrmLead_Tbl",
                type: "tinyint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_Tbl_NextActionTaskId",
                table: "CrmLead_Tbl",
                column: "NextActionTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Tasks_Tbl_NextActionTaskId",
                table: "CrmLead_Tbl",
                column: "NextActionTaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Tasks_Tbl_NextActionTaskId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLead_Tbl_NextActionTaskId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "NextActionDate",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "NextActionNote",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "NextActionTaskId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "NextActionType",
                table: "CrmLead_Tbl");
        }
    }
}
