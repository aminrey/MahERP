using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig37OrganizationGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchOrganizationGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganizationGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystemGroup = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BranchOrganizationGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchGroupId = table.Column<int>(type: "int", nullable: false),
                    BranchOrganizationId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganizationGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_BranchOrganizationGroup_Tbl_BranchGroupId",
                        column: x => x.BranchGroupId,
                        principalTable: "BranchOrganizationGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_BranchOrganization_Tbl_BranchOrganizationId",
                        column: x => x.BranchOrganizationId,
                        principalTable: "BranchOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_OrganizationGroup_Tbl_GroupId",
                        column: x => x.GroupId,
                        principalTable: "OrganizationGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_BranchId",
                table: "BranchOrganizationGroup_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_CreatorUserId",
                table: "BranchOrganizationGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_LastUpdaterUserId",
                table: "BranchOrganizationGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Group_Organization",
                table: "BranchOrganizationGroupMember_Tbl",
                columns: new[] { "BranchGroupId", "BranchOrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Tbl_AddedByUserId",
                table: "BranchOrganizationGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Tbl_BranchOrganizationId",
                table: "BranchOrganizationGroupMember_Tbl",
                column: "BranchOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Code",
                table: "OrganizationGroup_Tbl",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Tbl_CreatorUserId",
                table: "OrganizationGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Tbl_LastUpdaterUserId",
                table: "OrganizationGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Group_Organization",
                table: "OrganizationGroupMember_Tbl",
                columns: new[] { "GroupId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Tbl_AddedByUserId",
                table: "OrganizationGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Tbl_OrganizationId",
                table: "OrganizationGroupMember_Tbl",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchOrganizationGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "BranchOrganizationGroup_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationGroup_Tbl");
        }
    }
}
