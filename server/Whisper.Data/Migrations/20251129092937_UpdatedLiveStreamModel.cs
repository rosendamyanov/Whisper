using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Whisper.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLiveStreamModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatId",
                table: "Streams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAt",
                table: "Streams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streams_ChatId",
                table: "Streams",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Streams_Chats_ChatId",
                table: "Streams",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Streams_Chats_ChatId",
                table: "Streams");

            migrationBuilder.DropIndex(
                name: "IX_Streams_ChatId",
                table: "Streams");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Streams");

            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "Streams");
        }
    }
}
