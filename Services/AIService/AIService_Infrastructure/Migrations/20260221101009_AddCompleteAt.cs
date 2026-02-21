using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AiRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AiRequests");
        }
    }
}
