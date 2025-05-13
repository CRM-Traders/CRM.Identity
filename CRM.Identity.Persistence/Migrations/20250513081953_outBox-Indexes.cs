using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class outBoxIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClaimedBy",
                table: "OutboxMessage",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AggregateType",
                table: "OutboxMessage",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_AggregateId",
                table: "OutboxMessage",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_CreatedAt",
                table: "OutboxMessage",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Priority",
                table: "OutboxMessage",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ProcessedAt_IsClaimed",
                table: "OutboxMessage",
                columns: new[] { "ProcessedAt", "IsClaimed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_AggregateId",
                table: "OutboxMessage");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_CreatedAt",
                table: "OutboxMessage");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_Priority",
                table: "OutboxMessage");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_ProcessedAt_IsClaimed",
                table: "OutboxMessage");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimedBy",
                table: "OutboxMessage",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AggregateType",
                table: "OutboxMessage",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);
        }
    }
}
