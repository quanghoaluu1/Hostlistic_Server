using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dropPayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "TicketId1",
                table: "CheckIns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_TicketId1",
                table: "CheckIns",
                column: "TicketId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_Tickets_TicketId1",
                table: "CheckIns",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_Tickets_TicketId1",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_TicketId1",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "CheckIns");

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizerBankInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProofImageUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_WalletId",
                table: "PayoutRequests",
                column: "WalletId");
        }
    }
}
