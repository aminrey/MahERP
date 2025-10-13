using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig27roleNewSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameFa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemPermission = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdaterId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_AspNetUsers_LastUpdaterId",
                        column: x => x.LastUpdaterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_Permission_Tbl_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Role_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameFa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdaterId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Tbl_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Role_Tbl_AspNetUsers_LastUpdaterId",
                        column: x => x.LastUpdaterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermissionChangeLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldSourceRoleId = table.Column<int>(type: "int", nullable: true),
                    NewSourceRoleId = table.Column<int>(type: "int", nullable: true),
                    OldIsActive = table.Column<bool>(type: "bit", nullable: false),
                    NewIsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionChangeLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_Role_Tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceRoleId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsManuallyModified = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_Role_Tbl_SourceRoleId",
                        column: x => x.SourceRoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_Role_Tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_CreatorId",
                table: "Permission_Tbl",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_LastUpdaterId",
                table: "Permission_Tbl",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_ParentId",
                table: "Permission_Tbl",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_ChangedByUserId",
                table: "PermissionChangeLog_Tbl",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_PermissionId",
                table: "PermissionChangeLog_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_UserId",
                table: "PermissionChangeLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Tbl_CreatorId",
                table: "Role_Tbl",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Tbl_LastUpdaterId",
                table: "Role_Tbl",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_AssignedByUserId",
                table: "RolePermission_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_PermissionId",
                table: "RolePermission_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_RoleId",
                table: "RolePermission_Tbl",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_AssignedByUserId",
                table: "UserPermission_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_ModifiedByUserId",
                table: "UserPermission_Tbl",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_PermissionId",
                table: "UserPermission_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_SourceRoleId",
                table: "UserPermission_Tbl",
                column: "SourceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_UserId",
                table: "UserPermission_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_AssignedByUserId",
                table: "UserRole_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_RoleId",
                table: "UserRole_Tbl",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_UserId",
                table: "UserRole_Tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionChangeLog_Tbl");

            migrationBuilder.DropTable(
                name: "RolePermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserPermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserRole_Tbl");

            migrationBuilder.DropTable(
                name: "Permission_Tbl");

            migrationBuilder.DropTable(
                name: "Role_Tbl");
        }
    }
}
