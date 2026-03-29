using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionSnapshotAndCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedInByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckInType = table.Column<int>(type: "integer", nullable: false),
                    AttendeeName = table.Column<string>(type: "text", nullable: false),
                    AttendeeEmail = table.Column<string>(type: "text", nullable: false),
                    TicketCode = table.Column<string>(type: "text", nullable: false),
                    TicketTypeName = table.Column<string>(type: "text", nullable: false),
                    SessionName = table.Column<string>(type: "text", nullable: true),
                    EventTitle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckIns_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    SessionOrder = table.Column<int>(type: "integer", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_EventId",
                table: "CheckIns",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_TicketId_SessionId",
                table: "CheckIns",
                columns: new[] { "TicketId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSnapshots_EventId",
                table: "SessionSnapshots",
                column: "EventId");

            // Filtered unique indexes for duplicate check-in prevention.
            // EF Core has limited PostgreSQL support for filtered indexes with value predicates,
            // so these are added via raw SQL.
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX "IX_CheckIn_Ticket_EventLevel"
                ON "CheckIns" ("TicketId")
                WHERE "CheckInType" = 0;
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX "IX_CheckIn_Ticket_Session"
                ON "CheckIns" ("TicketId", "SessionId")
                WHERE "SessionId" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_CheckIn_Ticket_EventLevel"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_CheckIn_Ticket_Session"";");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "SessionSnapshots");
        }
    }
}
