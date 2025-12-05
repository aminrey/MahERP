using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <summary>
    /// ⭐⭐⭐ Migration: اضافه کردن سیستم تنظیمات تسک
    /// </summary>
    public partial class AddTaskSettingsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⭐⭐⭐ جدول تنظیمات تسک (Task-Level)
            migrationBuilder.CreateTable(
                name: "TaskSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    
                    // تنظیمات (Comma-Separated: "a,b,c,d,e")
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b,c,d,e"),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    
                    // متادیتا
                    IsInherited = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InheritedFrom = table.Column<byte>(type: "tinyint", nullable: true, comment: "0=Global, 1=Branch, 2=Category"),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSettings_Tasks",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSettings_CreatedBy",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Index برای جستجوی سریع
            migrationBuilder.CreateIndex(
                name: "IX_TaskSettings_TaskId",
                table: "TaskSettings_Tbl",
                column: "TaskId",
                unique: true); // یک تسک فقط یک تنظیم دارد

            // ⭐⭐⭐ جدول تنظیمات پیش‌فرض دسته‌بندی (Category-Level)
            migrationBuilder.CreateTable(
                name: "TaskCategoryDefaultSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false),
                    
                    // تنظیمات
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b,c,d,e"),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    
                    // متادیتا
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategoryDefaultSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCategoryDefaultSettings_TaskCategory",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategoryDefaultSettings_TaskCategoryId",
                table: "TaskCategoryDefaultSettings_Tbl",
                column: "TaskCategoryId",
                unique: true);

            // ⭐⭐⭐ جدول تنظیمات پیش‌فرض شعبه (Branch-Level)
            migrationBuilder.CreateTable(
                name: "BranchDefaultTaskSettings_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    
                    // تنظیمات
                    CanCommentRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b,c,d,e"),
                    CanAddMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanRemoveMembersRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CanEditAfterCompletionRoles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "a,b"),
                    CreatorCanEditDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    
                    // متادیتا
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchDefaultTaskSettings_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchDefaultTaskSettings_Branch",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDefaultTaskSettings_BranchId",
                table: "BranchDefaultTaskSettings_Tbl",
                column: "BranchId",
                unique: true);

            // ⭐⭐⭐ جدول لاگ تغییرات تنظیمات
            migrationBuilder.CreateTable(
                name: "TaskSettingsChangeLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    SettingType = table.Column<byte>(type: "tinyint", nullable: false, comment: "1-5"),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSettingsChangeLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSettingsChangeLog_Tasks",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSettingsChangeLog_ChangedBy",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettingsChangeLog_TaskId",
                table: "TaskSettingsChangeLog_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSettingsChangeLog_ChangeDate",
                table: "TaskSettingsChangeLog_Tbl",
                column: "ChangeDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TaskSettingsChangeLog_Tbl");
            migrationBuilder.DropTable(name: "TaskSettings_Tbl");
            migrationBuilder.DropTable(name: "TaskCategoryDefaultSettings_Tbl");
            migrationBuilder.DropTable(name: "BranchDefaultTaskSettings_Tbl");
        }
    }
}
