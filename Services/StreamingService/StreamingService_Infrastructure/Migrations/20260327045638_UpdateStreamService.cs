using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamingService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStreamService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"StreamRooms\" ALTER COLUMN \"IsRecordEnabled\" TYPE boolean USING (\"IsRecordEnabled\" != 0);");

            migrationBuilder.AddColumn<bool>(
                name: "IsChatEnabled",
                table: "StreamRooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsQnAEnabled",
                table: "StreamRooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireHostToStart",
                table: "StreamRooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "StreamParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChatEnabled",
                table: "StreamRooms");

            migrationBuilder.DropColumn(
                name: "IsQnAEnabled",
                table: "StreamRooms");

            migrationBuilder.DropColumn(
                name: "RequireHostToStart",
                table: "StreamRooms");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "StreamParticipants");

            migrationBuilder.AlterColumn<int>(
                name: "IsRecordEnabled",
                table: "StreamRooms",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
