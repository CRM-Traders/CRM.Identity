using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorEnabled",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorVerified",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecoveryCodes",
                table: "User",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsTwoFactorEnabled",
                table: "User",
                column: "IsTwoFactorEnabled",
                filter: "\"IsTwoFactorEnabled\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_IsTwoFactorEnabled",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsTwoFactorEnabled",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsTwoFactorVerified",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RecoveryCodes",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "User");
        }
    }
}
