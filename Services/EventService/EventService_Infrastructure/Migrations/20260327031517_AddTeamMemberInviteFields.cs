using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMemberInviteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedAt",
                table: "EventTeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InviteToken",
                table: "EventTeamMembers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InviteTokenExpiry",
                table: "EventTeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvitedByUserId",
                table: "EventTeamMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "EventTeamMembers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserFullName",
                table: "EventTeamMembers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTeamMembers_InviteToken",
                table: "EventTeamMembers",
                column: "InviteToken",
                unique: true,
                filter: "\"InviteToken\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventTeamMembers_InviteToken",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "DeclinedAt",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "InviteToken",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "InviteTokenExpiry",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "InvitedByUserId",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "EventTeamMembers");

            migrationBuilder.DropColumn(
                name: "UserFullName",
                table: "EventTeamMembers");
        }
    }
}
