using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventName",
                table: "EventSettlements",
                newName: "EventTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventTitle",
                table: "EventSettlements",
                newName: "EventName");
        }
    }
}
