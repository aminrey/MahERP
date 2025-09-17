using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class Add_Stakeholder_to_BranchTaskCategory_renametable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchTaskCategory_Tbl");

            migrationBuilder.CreateTable(
                name: "Branch_TaskCategory_Stakeholder_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch_TaskCategory_Stakeholder_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_TaskCategory_Stakeholder_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_TaskCategory_Stakeholder_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_TaskCategory_Stakeholder_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_TaskCategory_Stakeholder_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TaskCategory_Stakeholder_Tbl_AssignedByUserId",
                table: "Branch_TaskCategory_Stakeholder_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TaskCategory_Stakeholder_Tbl_BranchId",
                table: "Branch_TaskCategory_Stakeholder_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TaskCategory_Stakeholder_Tbl_StakeholderId",
                table: "Branch_TaskCategory_Stakeholder_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TaskCategory_Stakeholder_Tbl_TaskCategoryId",
                table: "Branch_TaskCategory_Stakeholder_Tbl",
                column: "TaskCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branch_TaskCategory_Stakeholder_Tbl");

            migrationBuilder.CreateTable(
                name: "BranchTaskCategory_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchTaskCategory_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategory_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategory_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategory_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategory_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategory_Tbl_AssignedByUserId",
                table: "BranchTaskCategory_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategory_Tbl_BranchId",
                table: "BranchTaskCategory_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategory_Tbl_StakeholderId",
                table: "BranchTaskCategory_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategory_Tbl_TaskCategoryId",
                table: "BranchTaskCategory_Tbl",
                column: "TaskCategoryId");
        }
    }
}
