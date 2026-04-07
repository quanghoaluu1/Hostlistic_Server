using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketHolderInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HolderEmail",
                table: "Tickets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolderName",
                table: "Tickets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolderPhone",
                table: "Tickets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketTypeName",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TicketTypeName",
                table: "OrderDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolderEmail",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "HolderName",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "HolderPhone",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketTypeName",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketTypeName",
                table: "OrderDetails");
        }
    }
}
