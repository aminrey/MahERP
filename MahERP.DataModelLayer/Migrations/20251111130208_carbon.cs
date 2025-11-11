using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class carbon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskCarbonCopy_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCarbonCopy_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCarbonCopy_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCarbonCopy_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TaskCarbonCopy_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCarbonCopy_Tbl_AddedByUserId",
                table: "TaskCarbonCopy_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCarbonCopy_Tbl_TaskId",
                table: "TaskCarbonCopy_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCarbonCopy_Tbl_UserId",
                table: "TaskCarbonCopy_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskCarbonCopy_Tbl");
        }
    }
}
