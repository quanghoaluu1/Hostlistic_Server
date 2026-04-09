using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankBin",
                table: "OrganizerBankInfos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankBin",
                table: "OrganizerBankInfos");
        }
    }
}
