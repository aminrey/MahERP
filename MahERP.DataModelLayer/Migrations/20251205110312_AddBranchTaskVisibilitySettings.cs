using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchTaskVisibilitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchTaskVisibilitySettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ManagerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DefaultVisibleTeamIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShowAllSubTeamsByDefault = table.Column<bool>(type: "bit", nullable: false),
                    MaxTasksToShow = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchTaskVisibilitySettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchTaskVisibilitySettings_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchTaskVisibilitySettings_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchTaskVisibilitySettings_Tbl_AspNetUsers_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchTaskVisibilitySettings_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskVisibilitySettings_Tbl_BranchId",
                table: "BranchTaskVisibilitySettings_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskVisibilitySettings_Tbl_CreatorUserId",
                table: "BranchTaskVisibilitySettings_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskVisibilitySettings_Tbl_LastUpdaterUserId",
                table: "BranchTaskVisibilitySettings_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskVisibilitySettings_Tbl_ManagerUserId",
                table: "BranchTaskVisibilitySettings_Tbl",
                column: "ManagerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchTaskVisibilitySettings_Tbl");
        }
    }
}
