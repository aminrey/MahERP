using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskViewPermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GranteeUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionType = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TargetTeamId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskViewPermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_GranteeUserId",
                        column: x => x.GranteeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_Team_Tbl_TargetTeamId",
                        column: x => x.TargetTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_AddedByUserId",
                table: "TaskViewPermission_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_GranteeUserId",
                table: "TaskViewPermission_Tbl",
                column: "GranteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_LastUpdaterUserId",
                table: "TaskViewPermission_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TargetTeamId",
                table: "TaskViewPermission_Tbl",
                column: "TargetTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TargetUserId",
                table: "TaskViewPermission_Tbl",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TeamId",
                table: "TaskViewPermission_Tbl",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskViewPermission_Tbl");
        }
    }
}
