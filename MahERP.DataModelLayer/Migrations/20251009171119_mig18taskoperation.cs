using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig18taskoperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "TaskOperation_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TaskOperation_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TaskOperationWorkLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskOperationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskOperationWorkLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskOperationWorkLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskOperationWorkLog_Tbl_TaskOperation_Tbl_TaskOperationId",
                        column: x => x.TaskOperationId,
                        principalTable: "TaskOperation_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperationWorkLog_Tbl_TaskOperationId",
                table: "TaskOperationWorkLog_Tbl",
                column: "TaskOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperationWorkLog_Tbl_UserId",
                table: "TaskOperationWorkLog_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskOperationWorkLog_Tbl");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "TaskOperation_Tbl");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TaskOperation_Tbl");
        }
    }
}
