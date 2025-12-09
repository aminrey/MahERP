using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class crm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrmLeadStatus_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "#6c757d"),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "fa-circle"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPositive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLeadStatus_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLeadStatus_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLeadStatus_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmLead_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastContactDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLead_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_CrmLeadStatus_Tbl_StatusId",
                        column: x => x.StatusId,
                        principalTable: "CrmLeadStatus_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLead_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmLeadInteraction_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<byte>(type: "tinyint", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", maxLength: 500, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InteractionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    RelatedTaskId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLeadInteraction_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLeadInteraction_Tbl_CrmLead_Tbl_LeadId",
                        column: x => x.LeadId,
                        principalTable: "CrmLead_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrmLeadInteraction_Tbl_Tasks_Tbl_RelatedTaskId",
                        column: x => x.RelatedTaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CrmFollowUp_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    InteractionId = table.Column<int>(type: "int", nullable: true),
                    FollowUpType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionResult = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasReminder = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReminderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReminderMinutesBefore = table.Column<int>(type: "int", nullable: false),
                    SendEmailReminder = table.Column<bool>(type: "bit", nullable: false),
                    SendSmsReminder = table.Column<bool>(type: "bit", nullable: false),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmFollowUp_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_CrmLeadInteraction_Tbl_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "CrmLeadInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_CrmLead_Tbl_LeadId",
                        column: x => x.LeadId,
                        principalTable: "CrmLead_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CrmFollowUp_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_AssignedUserId",
                table: "CrmFollowUp_Tbl",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_DueDate",
                table: "CrmFollowUp_Tbl",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_InteractionId",
                table: "CrmFollowUp_Tbl",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_LeadId",
                table: "CrmFollowUp_Tbl",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_Reminder",
                table: "CrmFollowUp_Tbl",
                columns: new[] { "HasReminder", "ReminderSent", "ReminderDate" },
                filter: "[HasReminder] = 1 AND [ReminderSent] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_Status",
                table: "CrmFollowUp_Tbl",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_TaskId",
                table: "CrmFollowUp_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_Tbl_CreatorUserId",
                table: "CrmFollowUp_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_Tbl_LastUpdaterUserId",
                table: "CrmFollowUp_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_User_Status_Due",
                table: "CrmFollowUp_Tbl",
                columns: new[] { "AssignedUserId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_AssignedUserId",
                table: "CrmLead_Tbl",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_Branch_Status",
                table: "CrmLead_Tbl",
                columns: new[] { "BranchId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_BranchId",
                table: "CrmLead_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_ContactId",
                table: "CrmLead_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_NextFollowUpDate",
                table: "CrmLead_Tbl",
                column: "NextFollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_OrganizationId",
                table: "CrmLead_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_StatusId",
                table: "CrmLead_Tbl",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_Tbl_CreatorUserId",
                table: "CrmLead_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_Tbl_LastUpdaterUserId",
                table: "CrmLead_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Date",
                table: "CrmLeadInteraction_Tbl",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Lead_Date",
                table: "CrmLeadInteraction_Tbl",
                columns: new[] { "LeadId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_LeadId",
                table: "CrmLeadInteraction_Tbl",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_TaskId",
                table: "CrmLeadInteraction_Tbl",
                column: "RelatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Tbl_CreatorUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Tbl_LastUpdaterUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Type",
                table: "CrmLeadInteraction_Tbl",
                column: "InteractionType");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_DisplayOrder",
                table: "CrmLeadStatus_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_IsDefault",
                table: "CrmLeadStatus_Tbl",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_Tbl_CreatorUserId",
                table: "CrmLeadStatus_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_Tbl_LastUpdaterUserId",
                table: "CrmLeadStatus_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_Title",
                table: "CrmLeadStatus_Tbl",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrmFollowUp_Tbl");

            migrationBuilder.DropTable(
                name: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropTable(
                name: "CrmLead_Tbl");

            migrationBuilder.DropTable(
                name: "CrmLeadStatus_Tbl");
        }
    }
}
