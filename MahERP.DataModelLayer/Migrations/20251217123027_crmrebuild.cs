using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class crmrebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityCRM_Tbl_CRMInteraction_Tbl_CRMId",
                table: "ActivityCRM_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMAttachment_Tbl_AspNetUsers_UploaderUserId",
                table: "CRMAttachment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMAttachment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMAttachment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMComment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_Tbl_CRMComment_Tbl_ParentCommentId",
                table: "CRMComment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMComment_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_CrmLeadInteraction_Tbl_InteractionId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_Tasks_Tbl_TaskId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Branch_Tbl_BranchId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Contact_Tbl_ContactId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Contract_Tbl_ContractId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Organization_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Branch_Tbl_BranchId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Contact_Tbl_ContactId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadStatus_Tbl_StatusId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Organization_Tbl_OrganizationId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_Tasks_Tbl_RelatedTaskId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_AssignedUserId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_CreatorUserId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Branch_Tbl_BranchId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Contact_Tbl_ContactId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmLead_Tbl_SourceLeadId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmLostReason_Tbl_LostReasonId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmPipelineStages_StageId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Organization_Tbl_OrganizationId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunityActivities_AspNetUsers_UserId",
                table: "CrmOpportunityActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_Tbl_AspNetUsers_UserId",
                table: "CRMParticipant_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMParticipant_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMParticipant_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmPipelineStages_AspNetUsers_CreatorUserId",
                table: "CrmPipelineStages");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmPipelineStages_Branch_Tbl_BranchId",
                table: "CrmPipelineStages");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMTeam_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMTeam_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_Tbl_Team_Tbl_TeamId",
                table: "CRMTeam_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderCRM_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCRMDetails_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmPipelineStage_Branch_Default",
                table: "CrmPipelineStages");

            migrationBuilder.DropIndex(
                name: "IX_CrmPipelineStage_Branch_Order",
                table: "CrmPipelineStages");

            migrationBuilder.DropIndex(
                name: "IX_CrmOpportunityActivity_Opp_Date",
                table: "CrmOpportunityActivities");

            migrationBuilder.DropIndex(
                name: "IX_CrmOpportunity_Branch_Stage_Active",
                table: "CrmOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_CrmOpportunity_ExpectedCloseDate",
                table: "CrmOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_CrmOpportunity_NextActionDate",
                table: "CrmOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_CrmOpportunity_User_Active",
                table: "CrmOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_CrmLostReason_AppliesTo",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLostReason_Category",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLostReason_Code",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLostReason_DisplayOrder",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLostReason_Title",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadStatus_DisplayOrder",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadStatus_IsDefault",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadStatus_Title",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadSource_Code",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadSource_DisplayOrder",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadSource_IsDefault",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadSource_Name",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadInteraction_Date",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadInteraction_Lead_Date",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLeadInteraction_Type",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLead_Branch_Status",
                table: "CrmLead_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmLead_NextFollowUpDate",
                table: "CrmLead_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmFollowUp_DueDate",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmFollowUp_Reminder",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmFollowUp_Status",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_CrmFollowUp_User_Status_Due",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMTeam_Tbl",
                table: "CRMTeam_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMParticipant_Tbl",
                table: "CRMParticipant_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMInteraction_Tbl",
                table: "CRMInteraction_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMComment_Tbl",
                table: "CRMComment_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMAttachment_Tbl",
                table: "CRMAttachment_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMTeam_Tbl",
                newName: "CRMTeam");

            migrationBuilder.RenameTable(
                name: "CRMParticipant_Tbl",
                newName: "CRMParticipant");

            migrationBuilder.RenameTable(
                name: "CRMInteraction_Tbl",
                newName: "CRMInteraction");

            migrationBuilder.RenameTable(
                name: "CRMComment_Tbl",
                newName: "CRMComment");

            migrationBuilder.RenameTable(
                name: "CRMAttachment_Tbl",
                newName: "CRMAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_CrmPipelineStage_BranchId",
                table: "CrmPipelineStages",
                newName: "IX_CrmPipelineStages_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunityProduct_OpportunityId",
                table: "CrmOpportunityProducts",
                newName: "IX_CrmOpportunityProducts_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunityActivity_OpportunityId",
                table: "CrmOpportunityActivities",
                newName: "IX_CrmOpportunityActivities_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_StageId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_StageId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_SourceLeadId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_SourceLeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_OrganizationId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_LostReasonId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_LostReasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_ContactId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_BranchId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunity_AssignedUserId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunities_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLeadInteraction_TaskId",
                table: "CrmLeadInteraction_Tbl",
                newName: "IX_CrmLeadInteraction_Tbl_RelatedTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLeadInteraction_LeadId",
                table: "CrmLeadInteraction_Tbl",
                newName: "IX_CrmLeadInteraction_Tbl_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_StatusId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_SourceId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_OrganizationId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_LostReasonId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_LostReasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_ContactId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_BranchId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_AssignedUserId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_Tbl_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_TaskId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_Tbl_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_LeadId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_Tbl_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_InteractionId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_Tbl_InteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_AssignedUserId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_Tbl_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_Tbl_TeamId",
                table: "CRMTeam",
                newName: "IX_CRMTeam_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_Tbl_CRMInteractionId",
                table: "CRMTeam",
                newName: "IX_CRMTeam_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_Tbl_CreatorUserId",
                table: "CRMTeam",
                newName: "IX_CRMTeam_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_Tbl_UserId",
                table: "CRMParticipant",
                newName: "IX_CRMParticipant_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_Tbl_StakeholderContactId",
                table: "CRMParticipant",
                newName: "IX_CRMParticipant_StakeholderContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_Tbl_CRMInteractionId",
                table: "CRMParticipant",
                newName: "IX_CRMParticipant_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_StakeholderId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_StakeholderId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_StakeholderContactId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_StakeholderContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_OrganizationId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_LastUpdaterUserId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_LastUpdaterUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_CreatorUserId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_ContractId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_ContractId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_ContactId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_Tbl_BranchId",
                table: "CRMInteraction",
                newName: "IX_CRMInteraction_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_Tbl_ParentCommentId",
                table: "CRMComment",
                newName: "IX_CRMComment_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_Tbl_CRMInteractionId",
                table: "CRMComment",
                newName: "IX_CRMComment_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_Tbl_CreatorUserId",
                table: "CRMComment",
                newName: "IX_CRMComment_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMAttachment_Tbl_UploaderUserId",
                table: "CRMAttachment",
                newName: "IX_CRMAttachment_UploaderUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMAttachment_Tbl_CRMInteractionId",
                table: "CRMAttachment",
                newName: "IX_CRMAttachment_CRMInteractionId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmPipelineStages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmPipelineStages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmPipelineStages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "#4285f4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "CrmOpportunityProducts",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldDefaultValue: 1m);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmOpportunityProducts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunityProducts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunityActivities",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActivityDate",
                table: "CrmOpportunityActivities",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmOpportunities",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "CrmOpportunities",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "IRR");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunities",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresNote",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLostReason_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "fa-times-circle");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLostReason_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLostReason_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLostReason_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "#dc3545");

            migrationBuilder.AlterColumn<byte>(
                name: "Category",
                table: "CrmLostReason_Tbl",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)5);

            migrationBuilder.AlterColumn<byte>(
                name: "AppliesTo",
                table: "CrmLostReason_Tbl",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPositive",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinal",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLeadStatus_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "fa-circle");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLeadStatus_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadStatus_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLeadStatus_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "#6c757d");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLeadSource_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "fa-globe");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLeadSource_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadSource_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLeadSource_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "#6c757d");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InteractionDate",
                table: "CrmLeadInteraction_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadInteraction_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "CrmLead_Tbl",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLead_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLead_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "CrmFollowUp_Tbl",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)0);

            migrationBuilder.AlterColumn<bool>(
                name: "ReminderSent",
                table: "CrmFollowUp_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<byte>(
                name: "Priority",
                table: "CrmFollowUp_Tbl",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)1);

            migrationBuilder.AlterColumn<bool>(
                name: "HasReminder",
                table: "CrmFollowUp_Tbl",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmFollowUp_Tbl",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<byte>(
                name: "ContactType",
                table: "Contact_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMTeam",
                table: "CRMTeam",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMParticipant",
                table: "CRMParticipant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMInteraction",
                table: "CRMInteraction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMComment",
                table: "CRMComment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMAttachment",
                table: "CRMAttachment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LeadStageStatus_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#6c757d"),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStageStatus_Tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostPurchaseStage_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#6c757d"),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPurchaseStage_Tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Goal_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    CurrentLeadStageStatusId = table.Column<int>(type: "int", nullable: true),
                    IsConverted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConversionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ActualValue = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goal_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goal_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goal_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goal_Tbl_LeadStageStatus_Tbl_CurrentLeadStageStatusId",
                        column: x => x.CurrentLeadStageStatusId,
                        principalTable: "LeadStageStatus_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goal_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteractionType_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LeadStageStatusId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionType_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionType_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteractionType_Tbl_LeadStageStatus_Tbl_LeadStageStatusId",
                        column: x => x.LeadStageStatusId,
                        principalTable: "LeadStageStatus_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interaction_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    InteractionTypeId = table.Column<int>(type: "int", nullable: false),
                    PostPurchaseStageId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InteractionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NextAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasReferral = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsReferred = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interaction_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interaction_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interaction_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interaction_Tbl_InteractionType_Tbl_InteractionTypeId",
                        column: x => x.InteractionTypeId,
                        principalTable: "InteractionType_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interaction_Tbl_PostPurchaseStage_Tbl_PostPurchaseStageId",
                        column: x => x.PostPurchaseStageId,
                        principalTable: "PostPurchaseStage_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteractionGoal_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractionId = table.Column<int>(type: "int", nullable: false),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionGoal_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionGoal_Tbl_Goal_Tbl_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goal_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionGoal_Tbl_Interaction_Tbl_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interaction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referral_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferrerContactId = table.Column<int>(type: "int", nullable: false),
                    ReferredContactId = table.Column<int>(type: "int", nullable: false),
                    ReferrerInteractionId = table.Column<int>(type: "int", nullable: true),
                    ReferredInteractionId = table.Column<int>(type: "int", nullable: true),
                    ReferralDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    StatusChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferralType = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    MarketerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referral_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referral_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referral_Tbl_Contact_Tbl_ReferredContactId",
                        column: x => x.ReferredContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referral_Tbl_Contact_Tbl_ReferrerContactId",
                        column: x => x.ReferrerContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referral_Tbl_Interaction_Tbl_ReferredInteractionId",
                        column: x => x.ReferredInteractionId,
                        principalTable: "Interaction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referral_Tbl_Interaction_Tbl_ReferrerInteractionId",
                        column: x => x.ReferrerInteractionId,
                        principalTable: "Interaction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LeadStageStatus_Tbl",
                columns: new[] { "Id", "ColorCode", "Description", "DisplayOrder", "Icon", "IsActive", "StageType", "Title", "TitleEnglish" },
                values: new object[,]
                {
                    { 1, "#17a2b8", "اولین تماس/آشنایی با محصول یا خدمت", 1, "fa-eye", true, (byte)1, "آگاهی", "Awareness" },
                    { 2, "#6f42c1", "نشان دادن علاقه به محصول/خدمت", 2, "fa-heart", true, (byte)2, "علاقه‌مندی", "Interest" },
                    { 3, "#fd7e14", "بررسی و مقایسه با گزینه‌های دیگر", 3, "fa-balance-scale", true, (byte)3, "ارزیابی", "Evaluation" },
                    { 4, "#ffc107", "آماده تصمیم نهایی برای خرید", 4, "fa-gavel", true, (byte)4, "تصمیم‌گیری", "Decision" },
                    { 5, "#28a745", "انجام خرید - تبدیل به مشتری", 5, "fa-shopping-cart", true, (byte)5, "خرید", "Purchase" }
                });

            migrationBuilder.InsertData(
                table: "PostPurchaseStage_Tbl",
                columns: new[] { "Id", "ColorCode", "Description", "DisplayOrder", "Icon", "IsActive", "StageType", "Title", "TitleEnglish" },
                values: new object[,]
                {
                    { 1, "#20c997", "تعاملات برای نگهداشت و حفظ مشتری", 1, "fa-user-shield", true, (byte)1, "حفظ مشتری", "Retention" },
                    { 2, "#007bff", "مشتری کسی را به ما معرفی کرده است", 2, "fa-share-alt", true, (byte)2, "ارجاع/توصیه", "Referral" },
                    { 3, "#e83e8c", "تعاملات برای افزایش وفاداری مشتری", 3, "fa-medal", true, (byte)3, "وفادارسازی", "Loyalty" },
                    { 4, "#ffc107", "تعاملات ویژه با مشتریان خاص و VIP", 4, "fa-crown", true, (byte)4, "VIP", "VIP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goal_ContactId",
                table: "Goal_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Goal_IsConverted",
                table: "Goal_Tbl",
                column: "IsConverted");

            migrationBuilder.CreateIndex(
                name: "IX_Goal_OrganizationId",
                table: "Goal_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Goal_Tbl_CreatorUserId",
                table: "Goal_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Goal_Tbl_CurrentLeadStageStatusId",
                table: "Goal_Tbl",
                column: "CurrentLeadStageStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_ContactId",
                table: "Interaction_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_HasReferral",
                table: "Interaction_Tbl",
                column: "HasReferral");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_InteractionDate",
                table: "Interaction_Tbl",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_InteractionTypeId",
                table: "Interaction_Tbl",
                column: "InteractionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_Tbl_CreatorUserId",
                table: "Interaction_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_Tbl_PostPurchaseStageId",
                table: "Interaction_Tbl",
                column: "PostPurchaseStageId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionGoal_Tbl_GoalId",
                table: "InteractionGoal_Tbl",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionGoal_Unique",
                table: "InteractionGoal_Tbl",
                columns: new[] { "InteractionId", "GoalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractionType_LeadStageStatusId",
                table: "InteractionType_Tbl",
                column: "LeadStageStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionType_Tbl_CreatorUserId",
                table: "InteractionType_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionType_Title",
                table: "InteractionType_Tbl",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageStatus_DisplayOrder",
                table: "LeadStageStatus_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageStatus_StageType",
                table: "LeadStageStatus_Tbl",
                column: "StageType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostPurchaseStage_DisplayOrder",
                table: "PostPurchaseStage_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PostPurchaseStage_StageType",
                table: "PostPurchaseStage_Tbl",
                column: "StageType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referral_ReferralDate",
                table: "Referral_Tbl",
                column: "ReferralDate");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_ReferredContactId",
                table: "Referral_Tbl",
                column: "ReferredContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_ReferrerContactId",
                table: "Referral_Tbl",
                column: "ReferrerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Status",
                table: "Referral_Tbl",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Tbl_CreatorUserId",
                table: "Referral_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Tbl_ReferredInteractionId",
                table: "Referral_Tbl",
                column: "ReferredInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Tbl_ReferrerInteractionId",
                table: "Referral_Tbl",
                column: "ReferrerInteractionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityCRM_Tbl_CRMInteraction_CRMId",
                table: "ActivityCRM_Tbl",
                column: "CRMId",
                principalTable: "CRMInteraction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMAttachment_AspNetUsers_UploaderUserId",
                table: "CRMAttachment",
                column: "UploaderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMAttachment_CRMInteraction_CRMInteractionId",
                table: "CRMAttachment",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_AspNetUsers_CreatorUserId",
                table: "CRMComment",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_CRMComment_ParentCommentId",
                table: "CRMComment",
                column: "ParentCommentId",
                principalTable: "CRMComment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_CRMInteraction_CRMInteractionId",
                table: "CRMComment",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmFollowUp_Tbl",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmFollowUp_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmFollowUp_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_CrmLeadInteraction_Tbl_InteractionId",
                table: "CrmFollowUp_Tbl",
                column: "InteractionId",
                principalTable: "CrmLeadInteraction_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_Tasks_Tbl_TaskId",
                table: "CrmFollowUp_Tbl",
                column: "TaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_AspNetUsers_CreatorUserId",
                table: "CRMInteraction",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_AspNetUsers_LastUpdaterUserId",
                table: "CRMInteraction",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Branch_Tbl_BranchId",
                table: "CRMInteraction",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Contact_Tbl_ContactId",
                table: "CRMInteraction",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Contract_Tbl_ContractId",
                table: "CRMInteraction",
                column: "ContractId",
                principalTable: "Contract_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Organization_Tbl_OrganizationId",
                table: "CRMInteraction",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMInteraction",
                column: "StakeholderContactId",
                principalTable: "StakeholderContact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Stakeholder_Tbl_StakeholderId",
                table: "CRMInteraction",
                column: "StakeholderId",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmLead_Tbl",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLead_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLead_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Branch_Tbl_BranchId",
                table: "CrmLead_Tbl",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Contact_Tbl_ContactId",
                table: "CrmLead_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl",
                column: "SourceId",
                principalTable: "CrmLeadSource_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadStatus_Tbl_StatusId",
                table: "CrmLead_Tbl",
                column: "StatusId",
                principalTable: "CrmLeadStatus_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl",
                column: "LostReasonId",
                principalTable: "CrmLostReason_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Organization_Tbl_OrganizationId",
                table: "CrmLead_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_Tasks_Tbl_RelatedTaskId",
                table: "CrmLeadInteraction_Tbl",
                column: "RelatedTaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadSource_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadSource_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadStatus_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadStatus_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLostReason_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLostReason_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_AssignedUserId",
                table: "CrmOpportunities",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_CreatorUserId",
                table: "CrmOpportunities",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Branch_Tbl_BranchId",
                table: "CrmOpportunities",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Contact_Tbl_ContactId",
                table: "CrmOpportunities",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmLead_Tbl_SourceLeadId",
                table: "CrmOpportunities",
                column: "SourceLeadId",
                principalTable: "CrmLead_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmLostReason_Tbl_LostReasonId",
                table: "CrmOpportunities",
                column: "LostReasonId",
                principalTable: "CrmLostReason_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmPipelineStages_StageId",
                table: "CrmOpportunities",
                column: "StageId",
                principalTable: "CrmPipelineStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Organization_Tbl_OrganizationId",
                table: "CrmOpportunities",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunityActivities_AspNetUsers_UserId",
                table: "CrmOpportunityActivities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_AspNetUsers_UserId",
                table: "CRMParticipant",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_CRMInteraction_CRMInteractionId",
                table: "CRMParticipant",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMParticipant",
                column: "StakeholderContactId",
                principalTable: "StakeholderContact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmPipelineStages_AspNetUsers_CreatorUserId",
                table: "CrmPipelineStages",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmPipelineStages_Branch_Tbl_BranchId",
                table: "CrmPipelineStages",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_AspNetUsers_CreatorUserId",
                table: "CRMTeam",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_CRMInteraction_CRMInteractionId",
                table: "CRMTeam",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_Team_Tbl_TeamId",
                table: "CRMTeam",
                column: "TeamId",
                principalTable: "Team_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityCRM_Tbl_CRMInteraction_CRMId",
                table: "ActivityCRM_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMAttachment_AspNetUsers_UploaderUserId",
                table: "CRMAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMAttachment_CRMInteraction_CRMInteractionId",
                table: "CRMAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_AspNetUsers_CreatorUserId",
                table: "CRMComment");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_CRMComment_ParentCommentId",
                table: "CRMComment");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMComment_CRMInteraction_CRMInteractionId",
                table: "CRMComment");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_CrmLeadInteraction_Tbl_InteractionId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmFollowUp_Tbl_Tasks_Tbl_TaskId",
                table: "CrmFollowUp_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_AspNetUsers_CreatorUserId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_AspNetUsers_LastUpdaterUserId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Branch_Tbl_BranchId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Contact_Tbl_ContactId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Contract_Tbl_ContractId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Organization_Tbl_OrganizationId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMInteraction_Stakeholder_Tbl_StakeholderId",
                table: "CRMInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Branch_Tbl_BranchId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Contact_Tbl_ContactId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadStatus_Tbl_StatusId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLead_Tbl_Organization_Tbl_OrganizationId",
                table: "CrmLead_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_Tasks_Tbl_RelatedTaskId",
                table: "CrmLeadInteraction_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadSource_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadStatus_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLostReason_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_AssignedUserId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_CreatorUserId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Branch_Tbl_BranchId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Contact_Tbl_ContactId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmLead_Tbl_SourceLeadId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmLostReason_Tbl_LostReasonId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_CrmPipelineStages_StageId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunities_Organization_Tbl_OrganizationId",
                table: "CrmOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmOpportunityActivities_AspNetUsers_UserId",
                table: "CrmOpportunityActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_AspNetUsers_UserId",
                table: "CRMParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_CRMInteraction_CRMInteractionId",
                table: "CRMParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMParticipant_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmPipelineStages_AspNetUsers_CreatorUserId",
                table: "CrmPipelineStages");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmPipelineStages_Branch_Tbl_BranchId",
                table: "CrmPipelineStages");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_AspNetUsers_CreatorUserId",
                table: "CRMTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_CRMInteraction_CRMInteractionId",
                table: "CRMTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMTeam_Team_Tbl_TeamId",
                table: "CRMTeam");

            migrationBuilder.DropTable(
                name: "InteractionGoal_Tbl");

            migrationBuilder.DropTable(
                name: "Referral_Tbl");

            migrationBuilder.DropTable(
                name: "Goal_Tbl");

            migrationBuilder.DropTable(
                name: "Interaction_Tbl");

            migrationBuilder.DropTable(
                name: "InteractionType_Tbl");

            migrationBuilder.DropTable(
                name: "PostPurchaseStage_Tbl");

            migrationBuilder.DropTable(
                name: "LeadStageStatus_Tbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMTeam",
                table: "CRMTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMParticipant",
                table: "CRMParticipant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMInteraction",
                table: "CRMInteraction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMComment",
                table: "CRMComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CRMAttachment",
                table: "CRMAttachment");

            migrationBuilder.DropColumn(
                name: "ContactType",
                table: "Contact_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMTeam",
                newName: "CRMTeam_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMParticipant",
                newName: "CRMParticipant_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMInteraction",
                newName: "CRMInteraction_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMComment",
                newName: "CRMComment_Tbl");

            migrationBuilder.RenameTable(
                name: "CRMAttachment",
                newName: "CRMAttachment_Tbl");

            migrationBuilder.RenameIndex(
                name: "IX_CrmPipelineStages_BranchId",
                table: "CrmPipelineStages",
                newName: "IX_CrmPipelineStage_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunityProducts_OpportunityId",
                table: "CrmOpportunityProducts",
                newName: "IX_CrmOpportunityProduct_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunityActivities_OpportunityId",
                table: "CrmOpportunityActivities",
                newName: "IX_CrmOpportunityActivity_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_StageId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_StageId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_SourceLeadId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_SourceLeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_OrganizationId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_LostReasonId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_LostReasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_ContactId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_BranchId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmOpportunities_AssignedUserId",
                table: "CrmOpportunities",
                newName: "IX_CrmOpportunity_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLeadInteraction_Tbl_RelatedTaskId",
                table: "CrmLeadInteraction_Tbl",
                newName: "IX_CrmLeadInteraction_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLeadInteraction_Tbl_LeadId",
                table: "CrmLeadInteraction_Tbl",
                newName: "IX_CrmLeadInteraction_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_StatusId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_SourceId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_OrganizationId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_LostReasonId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_LostReasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_ContactId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_BranchId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmLead_Tbl_AssignedUserId",
                table: "CrmLead_Tbl",
                newName: "IX_CrmLead_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_Tbl_TaskId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_Tbl_LeadId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_Tbl_InteractionId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_InteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CrmFollowUp_Tbl_AssignedUserId",
                table: "CrmFollowUp_Tbl",
                newName: "IX_CrmFollowUp_AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_TeamId",
                table: "CRMTeam_Tbl",
                newName: "IX_CRMTeam_Tbl_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_CRMInteractionId",
                table: "CRMTeam_Tbl",
                newName: "IX_CRMTeam_Tbl_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMTeam_CreatorUserId",
                table: "CRMTeam_Tbl",
                newName: "IX_CRMTeam_Tbl_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_UserId",
                table: "CRMParticipant_Tbl",
                newName: "IX_CRMParticipant_Tbl_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_StakeholderContactId",
                table: "CRMParticipant_Tbl",
                newName: "IX_CRMParticipant_Tbl_StakeholderContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMParticipant_CRMInteractionId",
                table: "CRMParticipant_Tbl",
                newName: "IX_CRMParticipant_Tbl_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_StakeholderId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_StakeholderId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_StakeholderContactId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_StakeholderContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_OrganizationId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_LastUpdaterUserId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_LastUpdaterUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_CreatorUserId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_ContractId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_ContractId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_ContactId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMInteraction_BranchId",
                table: "CRMInteraction_Tbl",
                newName: "IX_CRMInteraction_Tbl_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_ParentCommentId",
                table: "CRMComment_Tbl",
                newName: "IX_CRMComment_Tbl_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_CRMInteractionId",
                table: "CRMComment_Tbl",
                newName: "IX_CRMComment_Tbl_CRMInteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMComment_CreatorUserId",
                table: "CRMComment_Tbl",
                newName: "IX_CRMComment_Tbl_CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMAttachment_UploaderUserId",
                table: "CRMAttachment_Tbl",
                newName: "IX_CRMAttachment_Tbl_UploaderUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMAttachment_CRMInteractionId",
                table: "CRMAttachment_Tbl",
                newName: "IX_CRMAttachment_Tbl_CRMInteractionId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmPipelineStages",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmPipelineStages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmPipelineStages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#4285f4",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "CrmOpportunityProducts",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 1m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmOpportunityProducts",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunityProducts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunityActivities",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActivityDate",
                table: "CrmOpportunityActivities",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmOpportunities",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "CrmOpportunities",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "IRR",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmOpportunities",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresNote",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLostReason_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLostReason_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "fa-times-circle",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLostReason_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLostReason_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLostReason_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "#dc3545",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Category",
                table: "CrmLostReason_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)5,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "AppliesTo",
                table: "CrmLostReason_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPositive",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinal",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLeadStatus_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLeadStatus_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "fa-circle",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLeadStatus_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadStatus_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLeadStatus_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "#6c757d",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLeadSource_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CrmLeadSource_Tbl",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "fa-globe",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CrmLeadSource_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadSource_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "CrmLeadSource_Tbl",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "#6c757d",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "InteractionDate",
                table: "CrmLeadInteraction_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLeadInteraction_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "CrmLead_Tbl",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CrmLead_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmLead_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "CrmFollowUp_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "ReminderSent",
                table: "CrmFollowUp_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "Priority",
                table: "CrmFollowUp_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "HasReminder",
                table: "CrmFollowUp_Tbl",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CrmFollowUp_Tbl",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMTeam_Tbl",
                table: "CRMTeam_Tbl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMParticipant_Tbl",
                table: "CRMParticipant_Tbl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMInteraction_Tbl",
                table: "CRMInteraction_Tbl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMComment_Tbl",
                table: "CRMComment_Tbl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CRMAttachment_Tbl",
                table: "CRMAttachment_Tbl",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StakeholderCRM_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesRepUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreditRating = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastContactDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadSource = table.Column<byte>(type: "tinyint", nullable: false),
                    PotentialValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Preferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesStage = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderCRM_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_AspNetUsers_SalesRepUserId",
                        column: x => x.SalesRepUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCRMDetails_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCRMDetails_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStage_Branch_Default",
                table: "CrmPipelineStages",
                columns: new[] { "BranchId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmPipelineStage_Branch_Order",
                table: "CrmPipelineStages",
                columns: new[] { "BranchId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunityActivity_Opp_Date",
                table: "CrmOpportunityActivities",
                columns: new[] { "OpportunityId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_Branch_Stage_Active",
                table: "CrmOpportunities",
                columns: new[] { "BranchId", "StageId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_ExpectedCloseDate",
                table: "CrmOpportunities",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_NextActionDate",
                table: "CrmOpportunities",
                column: "NextActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOpportunity_User_Active",
                table: "CrmOpportunities",
                columns: new[] { "AssignedUserId", "IsActive" });

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
                name: "IX_CrmLostReason_Title",
                table: "CrmLostReason_Tbl",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_DisplayOrder",
                table: "CrmLeadStatus_Tbl",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_IsDefault",
                table: "CrmLeadStatus_Tbl",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadStatus_Title",
                table: "CrmLeadStatus_Tbl",
                column: "Title");

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
                name: "IX_CrmLeadInteraction_Date",
                table: "CrmLeadInteraction_Tbl",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Lead_Date",
                table: "CrmLeadInteraction_Tbl",
                columns: new[] { "LeadId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeadInteraction_Type",
                table: "CrmLeadInteraction_Tbl",
                column: "InteractionType");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_Branch_Status",
                table: "CrmLead_Tbl",
                columns: new[] { "BranchId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLead_NextFollowUpDate",
                table: "CrmLead_Tbl",
                column: "NextFollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmFollowUp_DueDate",
                table: "CrmFollowUp_Tbl",
                column: "DueDate");

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
                name: "IX_CrmFollowUp_User_Status_Due",
                table: "CrmFollowUp_Tbl",
                columns: new[] { "AssignedUserId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_SalesRepUserId",
                table: "StakeholderCRM_Tbl",
                column: "SalesRepUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_StakeholderId",
                table: "StakeholderCRM_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_StakeholderContactId",
                table: "TaskCRMDetails_Tbl",
                column: "StakeholderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_TaskId",
                table: "TaskCRMDetails_Tbl",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityCRM_Tbl_CRMInteraction_Tbl_CRMId",
                table: "ActivityCRM_Tbl",
                column: "CRMId",
                principalTable: "CRMInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMAttachment_Tbl_AspNetUsers_UploaderUserId",
                table: "CRMAttachment_Tbl",
                column: "UploaderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMAttachment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMAttachment_Tbl",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMComment_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_Tbl_CRMComment_Tbl_ParentCommentId",
                table: "CRMComment_Tbl",
                column: "ParentCommentId",
                principalTable: "CRMComment_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMComment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMComment_Tbl",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmFollowUp_Tbl",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmFollowUp_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmFollowUp_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_CrmLeadInteraction_Tbl_InteractionId",
                table: "CrmFollowUp_Tbl",
                column: "InteractionId",
                principalTable: "CrmLeadInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmFollowUp_Tbl_Tasks_Tbl_TaskId",
                table: "CrmFollowUp_Tbl",
                column: "TaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMInteraction_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CRMInteraction_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Branch_Tbl_BranchId",
                table: "CRMInteraction_Tbl",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Contact_Tbl_ContactId",
                table: "CRMInteraction_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Contract_Tbl_ContractId",
                table: "CRMInteraction_Tbl",
                column: "ContractId",
                principalTable: "Contract_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Organization_Tbl_OrganizationId",
                table: "CRMInteraction_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMInteraction_Tbl",
                column: "StakeholderContactId",
                principalTable: "StakeholderContact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMInteraction_Tbl_Stakeholder_Tbl_StakeholderId",
                table: "CRMInteraction_Tbl",
                column: "StakeholderId",
                principalTable: "Stakeholder_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_AssignedUserId",
                table: "CrmLead_Tbl",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLead_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLead_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Branch_Tbl_BranchId",
                table: "CrmLead_Tbl",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Contact_Tbl_ContactId",
                table: "CrmLead_Tbl",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadSource_Tbl_SourceId",
                table: "CrmLead_Tbl",
                column: "SourceId",
                principalTable: "CrmLeadSource_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLeadStatus_Tbl_StatusId",
                table: "CrmLead_Tbl",
                column: "StatusId",
                principalTable: "CrmLeadStatus_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_CrmLostReason_Tbl_LostReasonId",
                table: "CrmLead_Tbl",
                column: "LostReasonId",
                principalTable: "CrmLostReason_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLead_Tbl_Organization_Tbl_OrganizationId",
                table: "CrmLead_Tbl",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadInteraction_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadInteraction_Tbl_Tasks_Tbl_RelatedTaskId",
                table: "CrmLeadInteraction_Tbl",
                column: "RelatedTaskId",
                principalTable: "Tasks_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadSource_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadSource_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadSource_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLeadStatus_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLeadStatus_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLeadStatus_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_CreatorUserId",
                table: "CrmLostReason_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmLostReason_Tbl_AspNetUsers_LastUpdaterUserId",
                table: "CrmLostReason_Tbl",
                column: "LastUpdaterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_AssignedUserId",
                table: "CrmOpportunities",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_AspNetUsers_CreatorUserId",
                table: "CrmOpportunities",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Branch_Tbl_BranchId",
                table: "CrmOpportunities",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Contact_Tbl_ContactId",
                table: "CrmOpportunities",
                column: "ContactId",
                principalTable: "Contact_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmLead_Tbl_SourceLeadId",
                table: "CrmOpportunities",
                column: "SourceLeadId",
                principalTable: "CrmLead_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmLostReason_Tbl_LostReasonId",
                table: "CrmOpportunities",
                column: "LostReasonId",
                principalTable: "CrmLostReason_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_CrmPipelineStages_StageId",
                table: "CrmOpportunities",
                column: "StageId",
                principalTable: "CrmPipelineStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunities_Organization_Tbl_OrganizationId",
                table: "CrmOpportunities",
                column: "OrganizationId",
                principalTable: "Organization_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmOpportunityActivities_AspNetUsers_UserId",
                table: "CrmOpportunityActivities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_Tbl_AspNetUsers_UserId",
                table: "CRMParticipant_Tbl",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMParticipant_Tbl",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMParticipant_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                table: "CRMParticipant_Tbl",
                column: "StakeholderContactId",
                principalTable: "StakeholderContact_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmPipelineStages_AspNetUsers_CreatorUserId",
                table: "CrmPipelineStages",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmPipelineStages_Branch_Tbl_BranchId",
                table: "CrmPipelineStages",
                column: "BranchId",
                principalTable: "Branch_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_Tbl_AspNetUsers_CreatorUserId",
                table: "CRMTeam_Tbl",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                table: "CRMTeam_Tbl",
                column: "CRMInteractionId",
                principalTable: "CRMInteraction_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMTeam_Tbl_Team_Tbl_TeamId",
                table: "CRMTeam_Tbl",
                column: "TeamId",
                principalTable: "Team_Tbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
