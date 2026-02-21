using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSaleEndTicketType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaleEndUnit",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "SaleEndWhen",
                table: "TicketTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaleEndUnit",
                table: "TicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaleEndWhen",
                table: "TicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
