using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig3Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "TeamMember_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "RoleDescription",
                table: "TeamMember_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "TeamMember_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    CanViewSubordinateTasks = table.Column<bool>(type: "bit", nullable: false),
                    CanViewPeerTasks = table.Column<bool>(type: "bit", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_PositionId",
                table: "TeamMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_CreatorUserId",
                table: "TeamPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_LastUpdaterUserId",
                table: "TeamPosition_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_TeamId",
                table: "TeamPosition_Tbl",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMember_Tbl_TeamPosition_Tbl_PositionId",
                table: "TeamMember_Tbl",
                column: "PositionId",
                principalTable: "TeamPosition_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMember_Tbl_TeamPosition_Tbl_PositionId",
                table: "TeamMember_Tbl");

            migrationBuilder.DropTable(
                name: "TeamPosition_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_TeamMember_Tbl_PositionId",
                table: "TeamMember_Tbl");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "TeamMember_Tbl");

            migrationBuilder.AlterColumn<string>(
                name: "RoleDescription",
                table: "TeamMember_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "TeamMember_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
