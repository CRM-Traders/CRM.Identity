using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleDefaultPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDefaultPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleDefaultPermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleDefaultPermission_PermissionId",
                table: "RoleDefaultPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDefaultPermission_Role_PermissionId",
                table: "RoleDefaultPermission",
                columns: new[] { "Role", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_PermissionId",
                table: "UserPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_UserId_PermissionId",
                table: "UserPermission",
                columns: new[] { "UserId", "PermissionId" },
                unique: true,
                filter: "\"IsGranted\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleDefaultPermission");

            migrationBuilder.DropTable(
                name: "UserPermission");
        }
    }
}
