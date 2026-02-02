using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitAIDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    InputParams = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiGeneratedContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsChosen = table.Column<bool>(type: "boolean", nullable: false),
                    FinalEditedText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiGeneratedContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiGeneratedContents_AiRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AiRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiGeneratedContents_RequestId",
                table: "AiGeneratedContents",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiGeneratedContents");

            migrationBuilder.DropTable(
                name: "AiRequests");
        }
    }
}
