using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AiRequests");

            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "AiGeneratedContents");

            migrationBuilder.RenameColumn(
                name: "FinalEditedText",
                table: "AiGeneratedContents",
                newName: "FinalEditedHtml");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChosenAt",
                table: "AiGeneratedContents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AiGeneratedContents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId1",
                table: "AiGeneratedContents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AiGeneratedContents_RequestId1",
                table: "AiGeneratedContents",
                column: "RequestId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AiGeneratedContents_AiRequests_RequestId1",
                table: "AiGeneratedContents",
                column: "RequestId1",
                principalTable: "AiRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiGeneratedContents_AiRequests_RequestId1",
                table: "AiGeneratedContents");

            migrationBuilder.DropIndex(
                name: "IX_AiGeneratedContents_RequestId1",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "ChosenAt",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "RequestId1",
                table: "AiGeneratedContents");

            migrationBuilder.RenameColumn(
                name: "FinalEditedHtml",
                table: "AiGeneratedContents",
                newName: "FinalEditedText");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AiRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "AiGeneratedContents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
