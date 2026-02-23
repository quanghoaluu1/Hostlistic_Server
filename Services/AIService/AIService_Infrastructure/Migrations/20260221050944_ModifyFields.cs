using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputParams",
                table: "AiRequests");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "AiGeneratedContents",
                newName: "PlainContent");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalContext",
                table: "AiRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AiRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "AiRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "AiRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Objectives",
                table: "AiRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetAudience",
                table: "AiRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tone",
                table: "AiRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "AiGeneratedContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HtmlContent",
                table: "AiGeneratedContents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "AiGeneratedContents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "LatencyMs",
                table: "AiGeneratedContents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AiGeneratedContents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "AiGeneratedContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContext",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "Objectives",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "Tone",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "HtmlContent",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "LatencyMs",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "AiGeneratedContents");

            migrationBuilder.RenameColumn(
                name: "PlainContent",
                table: "AiGeneratedContents",
                newName: "Content");

            migrationBuilder.AddColumn<string>(
                name: "InputParams",
                table: "AiRequests",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
