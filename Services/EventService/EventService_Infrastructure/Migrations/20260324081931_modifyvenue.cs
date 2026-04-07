using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modifyvenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_EventId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_EventId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrackId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_VenueId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_SessionId",
                table: "SessionBookings");

            migrationBuilder.DropIndex(
                name: "IX_Events_VenueId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Venues",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Venues",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Venues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "LayoutPublicId",
                table: "Venues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Tracks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "EventTeamMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "EventMode",
                table: "Events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerId",
                table: "Events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventId_Name",
                table: "Venues",
                columns: new[] { "EventId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_EventId_SortOrder",
                table: "Tracks",
                columns: new[] { "EventId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_EventId_Ordering",
                table: "Sessions",
                columns: new[] { "EventId", "StartTime", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrackId_TimeRange",
                table: "Sessions",
                columns: new[] { "TrackId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_VenueId_TimeRange",
                table: "Sessions",
                columns: new[] { "VenueId", "StartTime", "EndTime" },
                filter: "\"VenueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SessionId_Status",
                table: "SessionBookings",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_UserId_SessionId",
                table: "SessionBookings",
                columns: new[] { "UserId", "SessionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Events_EventId",
                table: "Venues",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Events_EventId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventId_Name",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_EventId_SortOrder",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_EventId_Ordering",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TrackId_TimeRange",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_VenueId_TimeRange",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_SessionId_Status",
                table: "SessionBookings");

            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_UserId_SessionId",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "LayoutPublicId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Venues",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "EventTeamMembers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "EventMode",
                table: "Events",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_EventId",
                table: "Tracks",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_EventId",
                table: "Sessions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TrackId",
                table: "Sessions",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_VenueId",
                table: "Sessions",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SessionId",
                table: "SessionBookings",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
