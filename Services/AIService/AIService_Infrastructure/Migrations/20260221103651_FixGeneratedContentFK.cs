using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixGeneratedContentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiGeneratedContents_AiRequests_RequestId1",
                table: "AiGeneratedContents");

            migrationBuilder.DropIndex(
                name: "IX_AiGeneratedContents_RequestId1",
                table: "AiGeneratedContents");

            migrationBuilder.DropColumn(
                name: "RequestId1",
                table: "AiGeneratedContents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
