using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Tickets");

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
    }
}
