using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvenTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "EventSettlements",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventName",
                table: "EventSettlements");
        }
    }
}
