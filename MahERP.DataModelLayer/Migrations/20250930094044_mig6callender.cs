using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig6callender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PersonalDatesUpdatedDate",
                table: "TaskAssignment_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PersonalEndDate",
                table: "TaskAssignment_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PersonalStartDate",
                table: "TaskAssignment_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalTimeNote",
                table: "TaskAssignment_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalDatesUpdatedDate",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "PersonalEndDate",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "PersonalStartDate",
                table: "TaskAssignment_Tbl");

            migrationBuilder.DropColumn(
                name: "PersonalTimeNote",
                table: "TaskAssignment_Tbl");
        }
    }
}
