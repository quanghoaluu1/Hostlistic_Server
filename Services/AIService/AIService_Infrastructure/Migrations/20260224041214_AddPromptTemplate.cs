using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "text", nullable: false),
                    DefaultTemperature = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    DefaultMaxTokens = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplate_Category",
                table: "PromptTemplate",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplate_TemplateKey",
                table: "PromptTemplate",
                column: "TemplateKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptTemplate");
        }
    }
}
