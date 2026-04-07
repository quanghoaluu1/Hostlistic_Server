using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendaMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgendaMode",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgendaMode",
                table: "Events");
        }
    }
}
