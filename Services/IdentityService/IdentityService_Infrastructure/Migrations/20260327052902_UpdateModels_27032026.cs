using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels_27032026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationInMonths",
                table: "SubscriptionPlans",
                newName: "DurationInDays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationInDays",
                table: "SubscriptionPlans",
                newName: "DurationInMonths");
        }
    }
}
