using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePatternDetails_Tbl_AspNetRoles_RoleID",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternID",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_AspNetUsers_CreatorUserId",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_AspNetUsers_UserId",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_ScheduleId",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropIndex(
                name: "IX_RolePatternDetails_Tbl_RoleID",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskScheduleAssignment",
                table: "TaskScheduleAssignment");

            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "RolePatternDescription",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "RolePatternName",
                table: "RolePattern_Tbl");

            migrationBuilder.RenameTable(
                name: "TaskScheduleAssignment",
                newName: "TaskScheduleAssignment_Tbl");

            migrationBuilder.RenameColumn(
                name: "RolePatternID",
                table: "RolePatternDetails_Tbl",
                newName: "RolePatternId");

            migrationBuilder.RenameColumn(
                name: "RolePatternDetailsID",
                table: "RolePatternDetails_Tbl",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_RolePatternDetails_Tbl_RolePatternID",
                table: "RolePatternDetails_Tbl",
                newName: "IX_RolePatternDetails_Tbl_RolePatternId");

            migrationBuilder.RenameColumn(
                name: "RolePatternID",
                table: "RolePattern_Tbl",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_UserId",
                table: "TaskScheduleAssignment_Tbl",
                newName: "IX_TaskScheduleAssignment_Tbl_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_TaskScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                newName: "IX_TaskScheduleAssignment_Tbl_TaskScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_ScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                newName: "IX_TaskScheduleAssignment_Tbl_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment_Tbl",
                newName: "IX_TaskScheduleAssignment_Tbl_PredefinedCopyDescriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_CreatorUserId",
                table: "TaskScheduleAssignment_Tbl",
                newName: "IX_TaskScheduleAssignment_Tbl_CreatorUserId");

            migrationBuilder.AddColumn<string>(
                name: "ActionName",
                table: "RolePatternDetails_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreate",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanRead",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ControllerName",
                table: "RolePatternDetails_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "DataAccessLevel",
                table: "RolePatternDetails_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RolePatternDetails_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "AccessLevel",
                table: "RolePattern_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "RolePattern_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "RolePattern_Tbl",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RolePattern_Tbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RolePattern_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemPattern",
                table: "RolePattern_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "RolePattern_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdaterUserId",
                table: "RolePattern_Tbl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatternName",
                table: "RolePattern_Tbl",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskScheduleAssignment_Tbl",
                table: "TaskScheduleAssignment_Tbl",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PermissionLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<byte>(type: "tinyint", nullable: false),
                    AccessGranted = table.Column<bool>(type: "bit", nullable: false),
                    DenialReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleViewer_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleViewer_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRolePattern_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RolePatternId = table.Column<int>(type: "int", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRolePattern_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_RolePattern_Tbl_RolePatternId",
                        column: x => x.RolePatternId,
                        principalTable: "RolePattern_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("ALTER TABLE [RolePattern_Tbl] NOCHECK CONSTRAINT [FK_RolePattern_Tbl_AspNetUsers_CreatorUserId]");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "RoleLevel" },
                values: new object[,]
                {
                    { "1", "8e446cc7-743a-4133-8241-0f374fcbbc0d", "مدیر سیستم", "Admin", "ADMIN", "1" },
                    { "2", "5b6877d1-6fe6-4f8c-92a4-33fdf65a391f", "مدیر", "Manager", "MANAGER", "2" },
                    { "3", "8f4cee96-4bf9-4019-b589-4de5c0230e2c", "سرپرست", "Supervisor", "SUPERVISOR", "3" },
                    { "4", "523c9ab5-4b4c-43e2-84be-12c4b6f74eed", "کارمند", "Employee", "EMPLOYEE", "4" },
                    { "5", "aa5d01a0-a905-44ef-9e53-9c694828dbff", "کاربر عادی", "User", "USER", "5" }
                });

            migrationBuilder.InsertData(
                table: "Branch_Tbl",
                columns: new[] { "Id", "Address", "BranchId", "CreateDate", "Description", "Email", "IsActive", "IsMainBranch", "LastUpdateDate", "ManagerName", "Name", "ParentId", "Phone" },
                values: new object[] { 1, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "شعبه اصلی سازمان", null, true, true, null, null, "دفتر مرکزی", null, null });

            migrationBuilder.InsertData(
                table: "PredefinedCopyDescription_Tbl",
                columns: new[] { "Id", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "جهت اطلاع و پیگیری", true, "جهت اطلاع" },
                    { 2, "جهت انجام اقدامات لازم", true, "جهت اقدام" },
                    { 3, "جهت بررسی و اعلام نظر", true, "جهت بررسی" },
                    { 4, "جهت تایید و ابلاغ", true, "جهت تایید" },
                    { 5, "جهت نظارت و کنترل", true, "جهت نظارت" },
                    { 6, "جهت هماهنگی‌های لازم", true, "جهت هماهنگی" },
                    { 7, "جهت پیگیری و گزارش", true, "جهت پیگیری" },
                    { 8, "جهت اجرای دستورات", true, "جهت اجرا" }
                });

            migrationBuilder.InsertData(
                table: "RolePattern_Tbl",
                columns: new[] { "Id", "AccessLevel", "CreateDate", "CreatorUserId", "Description", "IsActive", "IsSystemPattern", "LastUpdateDate", "LastUpdaterUserId", "PatternName" },
                values: new object[,]
                {
                    { 1, (byte)1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "دسترسی کامل به تمام بخش‌ها", true, true, null, null, "مدیریت کامل" },
                    { 2, (byte)2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "مدیریت عملیات و تسک‌ها", true, true, null, null, "مدیر عملیات" },
                    { 3, (byte)4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "دسترسی به ماژول فروش و CRM", true, true, null, null, "کارشناس فروش" },
                    { 4, (byte)5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "دسترسی محدود به تسک‌های شخصی", true, true, null, null, "کاربر عادی" }
                });

            migrationBuilder.Sql("ALTER TABLE [RolePattern_Tbl] CHECK CONSTRAINT [FK_RolePattern_Tbl_AspNetUsers_CreatorUserId]");

            migrationBuilder.InsertData(
                table: "TaskCategory_Tbl",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "ParentCategoryId", "Title" },
                values: new object[,]
                {
                    { 1, "دسته‌بندی عمومی برای تسک‌ها", (byte)1, true, null, "عمومی" },
                    { 2, "تسک‌های مربوط به امور اداری", (byte)2, true, null, "اداری" },
                    { 3, "تسک‌های فنی و تخصصی", (byte)3, true, null, "فنی" },
                    { 4, "تسک‌های مربوط به فروش", (byte)4, true, null, "فروش" },
                    { 5, "تسک‌های مربوط به خدمات مشتریان", (byte)5, true, null, "خدمات" },
                    { 6, "تسک‌های بازاریابی و تبلیغات", (byte)6, true, null, "بازاریابی" },
                    { 7, "تسک‌های مربوط به امور مالی", (byte)7, true, null, "مالی" },
                    { 8, "تسک‌های مربوط به HR", (byte)8, true, null, "منابع انسانی" },
                    { 9, "تسک‌های پروژه‌ای", (byte)9, true, null, "پروژه" },
                    { 10, "تسک‌های فوری و اضطراری", (byte)10, true, null, "فوری" }
                });

            migrationBuilder.InsertData(
                table: "RolePatternDetails_Tbl",
                columns: new[] { "Id", "ActionName", "CanApprove", "CanCreate", "CanDelete", "CanEdit", "CanRead", "ControllerName", "DataAccessLevel", "IsActive", "RolePatternId" },
                values: new object[,]
                {
                    { 1, "*", true, true, true, true, true, "Task", (byte)2, true, 1 },
                    { 2, "*", true, true, true, true, true, "CRM", (byte)2, true, 1 },
                    { 3, "*", true, true, true, true, true, "Stakeholder", (byte)2, true, 1 },
                    { 4, "*", true, true, true, true, true, "Contract", (byte)2, true, 1 },
                    { 5, "*", true, true, true, true, true, "User", (byte)2, true, 1 },
                    { 6, "*", true, true, true, true, true, "RolePattern", (byte)2, true, 1 },
                    { 7, "*", true, true, true, true, true, "Task", (byte)1, true, 2 },
                    { 8, "Index,Details,Create,Edit", false, true, false, true, true, "CRM", (byte)1, true, 2 },
                    { 9, "Index,Details", false, false, false, false, true, "Stakeholder", (byte)1, true, 2 },
                    { 10, "*", false, true, false, true, true, "CRM", (byte)0, true, 3 },
                    { 11, "Index,Details,Create,Edit", false, true, false, true, true, "Stakeholder", (byte)0, true, 3 },
                    { 12, "Index,Details,MyTasks", false, false, false, false, true, "Task", (byte)0, true, 3 },
                    { 13, "Index,Details,MyTasks", false, false, false, false, true, "Task", (byte)0, true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_PatternName",
                table: "RolePattern_Tbl",
                column: "PatternName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_Tbl_CreatorUserId",
                table: "RolePattern_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_Tbl_LastUpdaterUserId",
                table: "RolePattern_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionLog_Tbl_UserId",
                table: "PermissionLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_AddedByUserId",
                table: "TaskScheduleViewer_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_ScheduleId",
                table: "TaskScheduleViewer_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_UserId",
                table: "TaskScheduleViewer_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_Tbl_AssignedByUserId",
                table: "UserRolePattern_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_Tbl_RolePatternId",
                table: "UserRolePattern_Tbl",
                column: "RolePatternId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_User_Pattern",
                table: "UserRolePattern_Tbl",
                columns: new[] { "UserId", "RolePatternId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePattern_Tbl_AspNetUsers_CreatorUserId",
                table: "RolePattern_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePattern_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "RolePattern_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternId",
                table: "RolePatternDetails_Tbl",
                column: "RolePatternId",
                principalTable: "RolePattern_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_CreatorUserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_UserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment_Tbl",
                column: "PredefinedCopyDescriptionId",
                principalTable: "PredefinedCopyDescription_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_ScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                column: "ScheduleId",
                principalTable: "TaskSchedule_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                column: "TaskScheduleId",
                principalTable: "TaskSchedule_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePattern_Tbl_AspNetUsers_CreatorUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePattern_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternId",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_CreatorUserId",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_UserId",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_ScheduleId",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropTable(
                name: "PermissionLog_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleViewer_Tbl");

            migrationBuilder.DropTable(
                name: "UserRolePattern_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_RolePattern_PatternName",
                table: "RolePattern_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_RolePattern_Tbl_CreatorUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_RolePattern_Tbl_LastUpdaterUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskScheduleAssignment_Tbl",
                table: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5");

            migrationBuilder.DeleteData(
                table: "Branch_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PredefinedCopyDescription_Tbl",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RolePatternDetails_Tbl",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RolePattern_Tbl",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RolePattern_Tbl",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RolePattern_Tbl",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RolePattern_Tbl",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "ActionName",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CanCreate",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "CanRead",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "ControllerName",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "DataAccessLevel",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RolePatternDetails_Tbl");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "IsSystemPattern",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "LastUpdaterUserId",
                table: "RolePattern_Tbl");

            migrationBuilder.DropColumn(
                name: "PatternName",
                table: "RolePattern_Tbl");

            migrationBuilder.RenameTable(
                name: "TaskScheduleAssignment_Tbl",
                newName: "TaskScheduleAssignment");

            migrationBuilder.RenameColumn(
                name: "RolePatternId",
                table: "RolePatternDetails_Tbl",
                newName: "RolePatternID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RolePatternDetails_Tbl",
                newName: "RolePatternDetailsID");

            migrationBuilder.RenameIndex(
                name: "IX_RolePatternDetails_Tbl_RolePatternId",
                table: "RolePatternDetails_Tbl",
                newName: "IX_RolePatternDetails_Tbl_RolePatternID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RolePattern_Tbl",
                newName: "RolePatternID");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_Tbl_UserId",
                table: "TaskScheduleAssignment",
                newName: "IX_TaskScheduleAssignment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment",
                newName: "IX_TaskScheduleAssignment_TaskScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_Tbl_ScheduleId",
                table: "TaskScheduleAssignment",
                newName: "IX_TaskScheduleAssignment_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment",
                newName: "IX_TaskScheduleAssignment_PredefinedCopyDescriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskScheduleAssignment_Tbl_CreatorUserId",
                table: "TaskScheduleAssignment",
                newName: "IX_TaskScheduleAssignment_CreatorUserId");

            migrationBuilder.AddColumn<string>(
                name: "RoleID",
                table: "RolePatternDetails_Tbl",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolePatternDescription",
                table: "RolePattern_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolePatternName",
                table: "RolePattern_Tbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskScheduleAssignment",
                table: "TaskScheduleAssignment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePatternDetails_Tbl_RoleID",
                table: "RolePatternDetails_Tbl",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePatternDetails_Tbl_AspNetRoles_RoleID",
                table: "RolePatternDetails_Tbl",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternID",
                table: "RolePatternDetails_Tbl",
                column: "RolePatternID",
                principalTable: "RolePattern_Tbl",
                principalColumn: "RolePatternID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_AspNetUsers_CreatorUserId",
                table: "TaskScheduleAssignment",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_AspNetUsers_UserId",
                table: "TaskScheduleAssignment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment",
                column: "PredefinedCopyDescriptionId",
                principalTable: "PredefinedCopyDescription_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_ScheduleId",
                table: "TaskScheduleAssignment",
                column: "ScheduleId",
                principalTable: "TaskSchedule_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment",
                column: "TaskScheduleId",
                principalTable: "TaskSchedule_Tbl",
                principalColumn: "Id");
        }
    }
}
