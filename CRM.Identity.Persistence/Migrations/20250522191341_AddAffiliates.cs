using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Affiliate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affiliate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SecondTelephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Skype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    KycStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsProblematic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBonusAbuser = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BonusAbuserReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasInvestments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SecurityCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IDPassportNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CountrySpecificIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MarketingType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistrationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistrationIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RegistrationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationDevice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCommunication = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RefererType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FTDTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LTDTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowTransactions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AnonymousCall = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    ClaimedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.Id);
                });

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
                    AllowedRoles = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsTwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TwoFactorSecret = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsTwoFactorVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RecoveryCodes = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AffiliateSecret",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AffiliateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpRestriction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliateSecret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffiliateSecret_Affiliate_AffiliateId",
                        column: x => x.AffiliateId,
                        principalTable: "Affiliate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AffiliateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SecondTelephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Skype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    KycStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsProblematic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBonusAbuser = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BonusAbuserReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasInvestments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SecurityCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IDPassportNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CountrySpecificIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MarketingType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistrationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistrationIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RegistrationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationDevice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCommunication = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RefererType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FTDTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LTDTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowTransactions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AnonymousCall = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Client_Affiliate_AffiliateId",
                        column: x => x.AffiliateId,
                        principalTable: "Affiliate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "TradingAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountLogin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Leverage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Server = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Equity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsDemo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingAccount_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradingAccount_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_Email",
                table: "Affiliate",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_IsActive",
                table: "Affiliate",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Affiliate_Name",
                table: "Affiliate",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_AffiliateId",
                table: "AffiliateSecret",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_ApiKey",
                table: "AffiliateSecret",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_ExpirationDate",
                table: "AffiliateSecret",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_IsActive",
                table: "AffiliateSecret",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateSecret_SecretKey",
                table: "AffiliateSecret",
                column: "SecretKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_AffiliateId",
                table: "Client",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Country",
                table: "Client",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Email",
                table: "Client",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_FTDTime",
                table: "Client",
                column: "FTDTime");

            migrationBuilder.CreateIndex(
                name: "IX_Client_IsBonusAbuser",
                table: "Client",
                column: "IsBonusAbuser");

            migrationBuilder.CreateIndex(
                name: "IX_Client_IsProblematic",
                table: "Client",
                column: "IsProblematic");

            migrationBuilder.CreateIndex(
                name: "IX_Client_RegistrationDate",
                table: "Client",
                column: "RegistrationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Source",
                table: "Client",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Status",
                table: "Client",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Country",
                table: "Lead",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Email",
                table: "Lead",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lead_IsBonusAbuser",
                table: "Lead",
                column: "IsBonusAbuser");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_IsProblematic",
                table: "Lead",
                column: "IsProblematic");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_RegistrationDate",
                table: "Lead",
                column: "RegistrationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Source",
                table: "Lead",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Status",
                table: "Lead",
                column: "Status");

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
                name: "IX_TradingAccount_AccountLogin",
                table: "TradingAccount",
                column: "AccountLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_Balance",
                table: "TradingAccount",
                column: "Balance");

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_ClientId",
                table: "TradingAccount",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_ClientId_Currency",
                table: "TradingAccount",
                columns: new[] { "ClientId", "Currency" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_Currency",
                table: "TradingAccount",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_IsDemo",
                table: "TradingAccount",
                column: "IsDemo");

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccount_LeadId",
                table: "TradingAccount",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsTwoFactorEnabled",
                table: "User",
                column: "IsTwoFactorEnabled",
                filter: "\"IsTwoFactorEnabled\" = true");

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
                name: "AffiliateSecret");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "RoleDefaultPermission");

            migrationBuilder.DropTable(
                name: "TradingAccount");

            migrationBuilder.DropTable(
                name: "UserPermission");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Lead");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Affiliate");
        }
    }
}
