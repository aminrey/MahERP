using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class moduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCrmModuleEnabled",
                table: "Settings_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaskingModuleEnabled",
                table: "Settings_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Settings_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByUserId",
                table: "Settings_Tbl",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchModulePermission_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamModulePermission_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModulePermission_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "UserModulePreference_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUsedModule = table.Column<byte>(type: "tinyint", nullable: false),
                    LastAccessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DefaultModule = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModulePreference_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePreference_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchModulePermission_Tbl_BranchId",
                table: "BranchModulePermission_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchModulePermission_Tbl_GrantedByUserId",
                table: "BranchModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamModulePermission_Tbl_GrantedByUserId",
                table: "TeamModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamModulePermission_Tbl_TeamId",
                table: "TeamModulePermission_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermission_Tbl_GrantedByUserId",
                table: "UserModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermission_Tbl_UserId",
                table: "UserModulePermission_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePreference_Tbl_UserId",
                table: "UserModulePreference_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "TeamModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserModulePreference_Tbl");

            migrationBuilder.DropColumn(
                name: "IsCrmModuleEnabled",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "IsTaskingModuleEnabled",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Settings_Tbl");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Settings_Tbl");
        }
    }
}
