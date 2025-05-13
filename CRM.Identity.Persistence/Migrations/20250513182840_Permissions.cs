using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Section = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    AllowedRoles = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Order",
                table: "Permission",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Section_Title_ActionType",
                table: "Permission",
                columns: new[] { "Section", "Title", "ActionType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permission");
        }
    }
}
