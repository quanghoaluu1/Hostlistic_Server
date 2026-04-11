using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrackId",
                table: "StreamRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamRooms_EventId_TrackId_Status",
                table: "StreamRooms",
                columns: new[] { "EventId", "TrackId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreamRooms_EventId_TrackId_Status",
                table: "StreamRooms");

            migrationBuilder.DropColumn(
                name: "TrackId",
                table: "StreamRooms");
        }
    }
}
