using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoReminderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "EmailCampaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoReminder",
                table: "EmailCampaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "IsAutoReminder",
                table: "EmailCampaigns");
        }
    }
}
