using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailBulk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedCount",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendCompletedAt",
                table: "EmailCampaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendStartedAt",
                table: "EmailCampaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SentCount",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRecipients",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    TicketTypeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BookingConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCheckedIn = table.Column<bool>(type: "boolean", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRecipients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRecipients_EventId_IsCheckedIn",
                table: "EventRecipients",
                columns: new[] { "EventId", "IsCheckedIn" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRecipients_EventId_UserId_TicketTypeId",
                table: "EventRecipients",
                columns: new[] { "EventId", "UserId", "TicketTypeId" },
                unique: true,
                filter: "\"TicketTypeId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRecipients");

            migrationBuilder.DropColumn(
                name: "FailedCount",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "SendCompletedAt",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "SendStartedAt",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "SentCount",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "TotalRecipients",
                table: "EmailCampaigns");
        }
    }
}
