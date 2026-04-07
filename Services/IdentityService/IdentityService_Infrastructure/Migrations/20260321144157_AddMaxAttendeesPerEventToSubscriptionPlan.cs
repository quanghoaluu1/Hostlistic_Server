using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxAttendeesPerEventToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAttendeesPerEvent",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAttendeesPerEvent",
                table: "SubscriptionPlans");
        }
    }
}
