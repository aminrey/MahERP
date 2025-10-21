using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig36groupcontacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchContactGroup_Tbl",
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
                    table.PrimaryKey("PK_BranchContactGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactGroup_Tbl",
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
                    table.PrimaryKey("PK_ContactGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BranchContactGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchGroupId = table.Column<int>(type: "int", nullable: false),
                    BranchContactId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchContactGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_BranchContactGroup_Tbl_BranchGroupId",
                        column: x => x.BranchGroupId,
                        principalTable: "BranchContactGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_BranchContact_Tbl_BranchContactId",
                        column: x => x.BranchContactId,
                        principalTable: "BranchContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ContactGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_ContactGroup_Tbl_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ContactGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Branch_Code",
                table: "BranchContactGroup_Tbl",
                columns: new[] { "BranchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Tbl_CreatorUserId",
                table: "BranchContactGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Tbl_LastUpdaterUserId",
                table: "BranchContactGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Group_Contact",
                table: "BranchContactGroupMember_Tbl",
                columns: new[] { "BranchGroupId", "BranchContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Tbl_AddedByUserId",
                table: "BranchContactGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Tbl_BranchContactId",
                table: "BranchContactGroupMember_Tbl",
                column: "BranchContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Code",
                table: "ContactGroup_Tbl",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Tbl_CreatorUserId",
                table: "ContactGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Tbl_LastUpdaterUserId",
                table: "ContactGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Group_Contact",
                table: "ContactGroupMember_Tbl",
                columns: new[] { "GroupId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Tbl_AddedByUserId",
                table: "ContactGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Tbl_ContactId",
                table: "ContactGroupMember_Tbl",
                column: "ContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchContactGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "ContactGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "BranchContactGroup_Tbl");

            migrationBuilder.DropTable(
                name: "ContactGroup_Tbl");
        }
    }
}
