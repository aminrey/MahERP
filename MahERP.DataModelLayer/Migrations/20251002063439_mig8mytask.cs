using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig8mytask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskMyDay_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsWorkedOn = table.Column<bool>(type: "bit", nullable: false),
                    WorkStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WorkDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskMyDay_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskMyDay_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskMyDay_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Tbl_TaskId",
                table: "TaskMyDay_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Tbl_UserId",
                table: "TaskMyDay_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskMyDay_Tbl");
        }
    }
}
