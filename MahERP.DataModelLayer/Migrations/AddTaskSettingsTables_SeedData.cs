using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskSettingsTables_SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⭐⭐⭐ ایجاد تنظیمات پیش‌فرض سراسری سیستم
            migrationBuilder.InsertData(
                table: "TaskSettings_Tbl",
                columns: new[] 
                { 
                    "TaskId", 
                    "BranchId", 
                    "CategoryId", 
                    "CanCommentRoles", 
                    "CanAddMembersRoles", 
                    "CanRemoveMembersRoles", 
                    "CanEditAfterCompletionRoles", 
                    "CreatorCanEditDelete", 
                    "IsInherited", 
                    "InheritedFrom", 
                    "CreatedDate", 
                    "UpdatedDate" 
                },
                values: new object[] 
                { 
                    null,           // TaskId - null = سراسری
                    null,           // BranchId - null = سراسری
                    null,           // CategoryId - null = سراسری
                    "a,b,c,d,e",    // CanCommentRoles - همه می‌توانند کامنت بگذارند
                    "a,b,c",        // CanAddMembersRoles - Admin, Manager, Supervisor
                    "a,b",          // CanRemoveMembersRoles - Admin, Manager
                    "a,b",          // CanEditAfterCompletionRoles - Admin, Manager
                    false,          // CreatorCanEditDelete - سازنده نمی‌تواند حذف/ویرایش کند
                    false,          // IsInherited
                    (byte)0,        // InheritedFrom - 0 = Global
                    DateTime.Now,   // CreatedDate
                    null            // UpdatedDate
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // حذف تنظیمات سراسری
            migrationBuilder.DeleteData(
                table: "TaskSettings_Tbl",
                keyColumns: new[] { "TaskId", "BranchId", "CategoryId" },
                keyValues: new object[] { null, null, null }
            );
        }
    }
}
