using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmIntegrationToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CrmContractId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrmCustomerId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrmFollowUpId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrmLeadId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrmOpportunityId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CrmSourceType",
                table: "Tasks_Tbl",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrmTicketId",
                table: "Tasks_Tbl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SourceModule",
                table: "Tasks_Tbl",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_CrmFollowUpId",
                table: "Tasks_Tbl",
                column: "CrmFollowUpId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_CrmLeadId",
                table: "Tasks_Tbl",
                column: "CrmLeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_CrmFollowUp_Tbl_CrmFollowUpId",
                table: "Tasks_Tbl",
                column: "CrmFollowUpId",
                principalTable: "CrmFollowUp_Tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tbl_CrmLead_Tbl_CrmLeadId",
                table: "Tasks_Tbl",
                column: "CrmLeadId",
                principalTable: "CrmLead_Tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_CrmFollowUp_Tbl_CrmFollowUpId",
                table: "Tasks_Tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tbl_CrmLead_Tbl_CrmLeadId",
                table: "Tasks_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Tbl_CrmFollowUpId",
                table: "Tasks_Tbl");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Tbl_CrmLeadId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmContractId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmCustomerId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmFollowUpId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmLeadId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmOpportunityId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmSourceType",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "CrmTicketId",
                table: "Tasks_Tbl");

            migrationBuilder.DropColumn(
                name: "SourceModule",
                table: "Tasks_Tbl");
        }
    }
}
