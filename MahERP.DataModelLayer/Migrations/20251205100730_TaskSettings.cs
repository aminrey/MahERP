using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class TaskSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchDefaultTaskSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchDefaultTaskSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchDefaultTaskSettings_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchDefaultTaskSettings_Tbl_AspNetUsers_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchDefaultTaskSettings_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCategoryDefaultSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false),
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategoryDefaultSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCategoryDefaultSettings_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCategoryDefaultSettings_Tbl_AspNetUsers_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskCategoryDefaultSettings_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsInherited = table.Column<bool>(type: "bit", nullable: false),
                    InheritedFrom = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSettings_Tbl_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSettings_Tbl_AspNetUsers_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskSettings_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskSettingsChangeLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    SettingType = table.Column<byte>(type: "tinyint", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSettingsChangeLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSettingsChangeLog_Tbl_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSettingsChangeLog_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDefaultTaskSettings_Tbl_BranchId",
                table: "BranchDefaultTaskSettings_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDefaultTaskSettings_Tbl_CreatedByUserId",
                table: "BranchDefaultTaskSettings_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDefaultTaskSettings_Tbl_LastModifiedByUserId",
                table: "BranchDefaultTaskSettings_Tbl",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategoryDefaultSettings_Tbl_CreatedByUserId",
                table: "TaskCategoryDefaultSettings_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategoryDefaultSettings_Tbl_LastModifiedByUserId",
                table: "TaskCategoryDefaultSettings_Tbl",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategoryDefaultSettings_Tbl_TaskCategoryId",
                table: "TaskCategoryDefaultSettings_Tbl",
                column: "TaskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettings_Tbl_CreatedByUserId",
                table: "TaskSettings_Tbl",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettings_Tbl_LastModifiedByUserId",
                table: "TaskSettings_Tbl",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettings_Tbl_TaskId",
                table: "TaskSettings_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettingsChangeLog_Tbl_ChangedByUserId",
                table: "TaskSettingsChangeLog_Tbl",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettingsChangeLog_Tbl_TaskId",
                table: "TaskSettingsChangeLog_Tbl",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchDefaultTaskSettings_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCategoryDefaultSettings_Tbl");

            migrationBuilder.DropTable(
                name: "TaskSettings_Tbl");

            migrationBuilder.DropTable(
                name: "TaskSettingsChangeLog_Tbl");
        }
    }
}
