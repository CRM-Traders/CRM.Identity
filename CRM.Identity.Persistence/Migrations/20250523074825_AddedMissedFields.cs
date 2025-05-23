using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedMissedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AffiliateSecret_ApiKey",
                table: "AffiliateSecret");

            migrationBuilder.DropIndex(
                name: "IX_Affiliate_Email",
                table: "Affiliate");

            migrationBuilder.DropIndex(
                name: "IX_Affiliate_Name",
                table: "Affiliate");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "AffiliateSecret");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Affiliate");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Affiliate");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Lead",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Client",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Affiliate",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Lead_UserId",
                table: "Lead",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_UserId",
                table: "Client",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_UserId",
                table: "Affiliate",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Affiliate_User_UserId",
                table: "Affiliate",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Client_User_UserId",
                table: "Client",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_User_UserId",
                table: "Lead",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Affiliate_User_UserId",
                table: "Affiliate");

            migrationBuilder.DropForeignKey(
                name: "FK_Client_User_UserId",
                table: "Client");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_User_UserId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_UserId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Client_UserId",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Affiliate_UserId",
                table: "Affiliate");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Affiliate");

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "AffiliateSecret",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Affiliate",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Affiliate",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_ApiKey",
                table: "AffiliateSecret",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_Email",
                table: "Affiliate",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_Name",
                table: "Affiliate",
                column: "Name");
        }
    }
}
