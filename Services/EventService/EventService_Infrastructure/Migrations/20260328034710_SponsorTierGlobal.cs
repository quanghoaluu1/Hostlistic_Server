using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SponsorTierGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SponsorTiers_Events_EventId",
                table: "SponsorTiers");

            migrationBuilder.DropIndex(
                name: "IX_SponsorTiers_EventId",
                table: "SponsorTiers");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "SponsorTiers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "SponsorTiers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SponsorTiers_EventId",
                table: "SponsorTiers",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorTiers_Events_EventId",
                table: "SponsorTiers",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
