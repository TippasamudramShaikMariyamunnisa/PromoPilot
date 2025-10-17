using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoPilot.Migrations
{
    /// <inheritdoc />
    public partial class RenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Campaigns_CampaignID",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignReports_Campaigns_CampaignID",
                table: "CampaignReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Campaigns_CampaignID",
                table: "Engagements");

            migrationBuilder.DropForeignKey(
                name: "FK_ExecutionStatuses_Campaigns_CampaignID",
                table: "ExecutionStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExecutionStatuses",
                table: "ExecutionStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Engagements",
                table: "Engagements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignReports",
                table: "CampaignReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.RenameTable(
                name: "ExecutionStatuses",
                newName: "ExecutionStatus");

            migrationBuilder.RenameTable(
                name: "Engagements",
                newName: "Engagement");

            migrationBuilder.RenameTable(
                name: "Campaigns",
                newName: "Campaign");

            migrationBuilder.RenameTable(
                name: "CampaignReports",
                newName: "CampaignReport");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Budget");

            migrationBuilder.RenameIndex(
                name: "IX_ExecutionStatuses_CampaignID",
                table: "ExecutionStatus",
                newName: "IX_ExecutionStatus_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_Engagements_CampaignID",
                table: "Engagement",
                newName: "IX_Engagement_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignReports_CampaignID",
                table: "CampaignReport",
                newName: "IX_CampaignReport_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_CampaignID",
                table: "Budget",
                newName: "IX_Budget_CampaignID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExecutionStatus",
                table: "ExecutionStatus",
                column: "StatusID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Engagement",
                table: "Engagement",
                column: "EngagementID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign",
                column: "CampaignID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignReport",
                table: "CampaignReport",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budget",
                table: "Budget",
                column: "BudgetID");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Campaign_CampaignID",
                table: "Budget",
                column: "CampaignID",
                principalTable: "Campaign",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignReport_Campaign_CampaignID",
                table: "CampaignReport",
                column: "CampaignID",
                principalTable: "Campaign",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Engagement_Campaign_CampaignID",
                table: "Engagement",
                column: "CampaignID",
                principalTable: "Campaign",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExecutionStatus_Campaign_CampaignID",
                table: "ExecutionStatus",
                column: "CampaignID",
                principalTable: "Campaign",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Campaign_CampaignID",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignReport_Campaign_CampaignID",
                table: "CampaignReport");

            migrationBuilder.DropForeignKey(
                name: "FK_Engagement_Campaign_CampaignID",
                table: "Engagement");

            migrationBuilder.DropForeignKey(
                name: "FK_ExecutionStatus_Campaign_CampaignID",
                table: "ExecutionStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExecutionStatus",
                table: "ExecutionStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Engagement",
                table: "Engagement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignReport",
                table: "CampaignReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budget",
                table: "Budget");

            migrationBuilder.RenameTable(
                name: "ExecutionStatus",
                newName: "ExecutionStatuses");

            migrationBuilder.RenameTable(
                name: "Engagement",
                newName: "Engagements");

            migrationBuilder.RenameTable(
                name: "CampaignReport",
                newName: "CampaignReports");

            migrationBuilder.RenameTable(
                name: "Campaign",
                newName: "Campaigns");

            migrationBuilder.RenameTable(
                name: "Budget",
                newName: "Budgets");

            migrationBuilder.RenameIndex(
                name: "IX_ExecutionStatus_CampaignID",
                table: "ExecutionStatuses",
                newName: "IX_ExecutionStatuses_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_Engagement_CampaignID",
                table: "Engagements",
                newName: "IX_Engagements_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignReport_CampaignID",
                table: "CampaignReports",
                newName: "IX_CampaignReports_CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_CampaignID",
                table: "Budgets",
                newName: "IX_Budgets_CampaignID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExecutionStatuses",
                table: "ExecutionStatuses",
                column: "StatusID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Engagements",
                table: "Engagements",
                column: "EngagementID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignReports",
                table: "CampaignReports",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns",
                column: "CampaignID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "BudgetID");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Campaigns_CampaignID",
                table: "Budgets",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignReports_Campaigns_CampaignID",
                table: "CampaignReports",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Campaigns_CampaignID",
                table: "Engagements",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExecutionStatuses_Campaigns_CampaignID",
                table: "ExecutionStatuses",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "CampaignID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
