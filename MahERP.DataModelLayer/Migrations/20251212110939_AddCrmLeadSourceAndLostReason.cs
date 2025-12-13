using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmLeadSourceAndLostReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LostDate",
                table: "CrmLead_Tbl",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LostReasonId",
                table: "CrmLead_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LostReasonNote",
                table: "CrmLead_Tbl",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "CrmLead_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrmLeadSource_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "fa-globe"),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "#6c757d"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLeadSource_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLeadSource_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLeadSource_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmLostReason_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEnglish = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppliesTo = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    Category = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)5),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "fa-times-circle"),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "#dc3545"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresNote = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLostReason_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLostReason_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLostReason_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmPipelineStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#4285f4"),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WinProbability = table.Column<int>(type: "int", nullable: false),
                    IsWonStage = table.Column<bool>(type: "bit", nullable: false),
                    IsLostStage = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmPipelineStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmPipelineStages_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmPipelineStages_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    SourceLeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "IRR"),
                    Probability = table.Column<int>(type: "int", nullable: false),
                    WeightedValue = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ExpectedCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LostReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LostReasonId = table.Column<int>(type: "int", nullable: true),
                    LostReasonNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WinningCompetitor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NextActionType = table.Column<byte>(type: "tinyint", nullable: true),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextActionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextActionTaskId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_CrmLead_Tbl_SourceLeadId",
                        column: x => x.SourceLeadId,
                        principalTable: "CrmLead_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_CrmLostReason_Tbl_LostReasonId",
                        column: x => x.LostReasonId,
                        principalTable: "CrmLostReason_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_CrmPipelineStages_StageId",
                        column: x => x.StageId,
                        principalTable: "CrmPipelineStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunities_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmOpportunityActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PreviousStageId = table.Column<int>(type: "int", nullable: true),
                    NewStageId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmOpportunityActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmOpportunityActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOpportunityActivities_CrmOpportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "CrmOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrmOpportunityProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 1m),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmOpportunityProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmOpportunityProducts_CrmOpportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "CrmOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_LostReasonId",
                table: "CrmLead_Tbl",
                column: "LostReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_SourceId",
                table: "CrmLead_Tbl",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_Code",
                table: "CrmLeadSource_Tbl",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_DisplayOrder",
                table: "CrmLeadSource_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_IsDefault",
                table: "CrmLeadSource_Tbl",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_Name",
                table: "CrmLeadSource_Tbl",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_Tbl_CreatorUserId",
                table: "CrmLeadSource_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadSource_Tbl_LastUpdaterUserId",
                table: "CrmLeadSource_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_AppliesTo",
                table: "CrmLostReason_Tbl",
                column: "AppliesTo");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_Category",
                table: "CrmLostReason_Tbl",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_Code",
                table: "CrmLostReason_Tbl",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_DisplayOrder",
                table: "CrmLostReason_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_Tbl_CreatorUserId",
                table: "CrmLostReason_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_Tbl_LastUpdaterUserId",
                table: "CrmLostReason_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLostReason_Title",
                table: "CrmLostReason_Tbl",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunities_CreatorUserId",
                table: "CrmOpportunities",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_AssignedUserId",
                table: "CrmOpportunities",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_Branch_Stage_Active",
                table: "CrmOpportunities",
                columns: new[] { "BranchId", "StageId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_BranchId",
                table: "CrmOpportunities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_ContactId",
                table: "CrmOpportunities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_ExpectedCloseDate",
                table: "CrmOpportunities",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_LostReasonId",
                table: "CrmOpportunities",
                column: "LostReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_NextActionDate",
                table: "CrmOpportunities",
                column: "NextActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_OrganizationId",
                table: "CrmOpportunities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_SourceLeadId",
                table: "CrmOpportunities",
                column: "SourceLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_StageId",
                table: "CrmOpportunities",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_User_Active",
                table: "CrmOpportunities",
                columns: new[] { "AssignedUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunityActivities_UserId",
                table: "CrmOpportunityActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunityActivity_Opp_Date",
                table: "CrmOpportunityActivities",
                columns: new[] { "OpportunityId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunityActivity_OpportunityId",
                table: "CrmOpportunityActivities",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunityProduct_OpportunityId",
                table: "CrmOpportunityProducts",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStage_Branch_Default",
                table: "CrmPipelineStages",
                columns: new[] { "BranchId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStage_Branch_Order",
                table: "CrmPipelineStages",
                columns: new[] { "BranchId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStage_BranchId",
                table: "CrmPipelineStages",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStages_CreatorUserId",
                table: "CrmPipelineStages",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl",
                column: "SourceId",
                principalTable: "CrmLeadSource_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl",
                column: "LostReasonId",
                principalTable: "CrmLostReason_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropTable(
                name: "CrmLeadSource_Tbl");

            migrationBuilder.DropTable(
                name: "CrmOpportunityActivities");

            migrationBuilder.DropTable(
                name: "CrmOpportunityProducts");

            migrationBuilder.DropTable(
                name: "CrmOpportunities");

            migrationBuilder.DropTable(
                name: "CrmLostReason_Tbl");

            migrationBuilder.DropTable(
                name: "CrmPipelineStages");

            migrationBuilder.DropIndex(
                name: "IX_CrmLead_LostReasonId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLead_SourceId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "LostDate",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "LostReasonId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "LostReasonNote",
                table: "CrmLead_Tbl");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "CrmLead_Tbl");
        }
    }
}
