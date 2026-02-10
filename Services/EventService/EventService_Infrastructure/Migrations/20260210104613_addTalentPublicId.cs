using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addTalentPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPublicId",
                table: "Talents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPublicId",
                table: "Talents");
        }
    }
}
