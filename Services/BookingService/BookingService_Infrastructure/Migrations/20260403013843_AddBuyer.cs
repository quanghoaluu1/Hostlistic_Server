using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerAvatarUrl",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerAvatarUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "Orders");
        }
    }
}
