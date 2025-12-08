using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class departmentposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BasePositionId",
                table: "DepartmentPosition_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    DefaultPowerLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    IsCommon = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresDegree = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MinimumDegree = table.Column<byte>(type: "tinyint", nullable: true),
                    MinimumExperienceYears = table.Column<int>(type: "int", nullable: true),
                    SuggestedMinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuggestedMaxSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CanHireSubordinates = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationPosition_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPosition_Tbl_BasePositionId",
                table: "DepartmentPosition_Tbl",
                column: "BasePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosition_Tbl_CreatorUserId",
                table: "OrganizationPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosition_Tbl_LastUpdaterUserId",
                table: "OrganizationPosition_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_Category",
                table: "OrganizationPosition_Tbl",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Position_DisplayOrder",
                table: "OrganizationPosition_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Position_IsCommon",
                table: "OrganizationPosition_Tbl",
                column: "IsCommon",
                filter: "[IsCommon] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Position_Level_PowerLevel",
                table: "OrganizationPosition_Tbl",
                columns: new[] { "Level", "DefaultPowerLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_Position_Title",
                table: "OrganizationPosition_Tbl",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentPosition_Tbl_OrganizationPosition_Tbl_BasePositionId",
                table: "DepartmentPosition_Tbl",
                column: "BasePositionId",
                principalTable: "OrganizationPosition_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentPosition_Tbl_OrganizationPosition_Tbl_BasePositionId",
                table: "DepartmentPosition_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationPosition_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentPosition_Tbl_BasePositionId",
                table: "DepartmentPosition_Tbl");

            migrationBuilder.DropColumn(
                name: "BasePositionId",
                table: "DepartmentPosition_Tbl");
        }
    }
}
