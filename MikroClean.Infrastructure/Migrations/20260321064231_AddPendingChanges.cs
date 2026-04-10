using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IpPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Ranges = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    NextPool = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    MikroTikId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IpPools_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    Resource = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    EntityKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingChanges_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    LocalAddress = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RemoteAddress = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DnsServers = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    RateLimit = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    OnlyOne = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "default"),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    MikroTikId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppProfiles_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Service = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "pppoe"),
                    Profile = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Disabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Comment = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    MikroTikId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppSecrets_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Interface = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    DefaultProfile = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true, defaultValue: "default"),
                    MaxMtu = table.Column<int>(type: "int", nullable: true, defaultValue: 1480),
                    MaxMru = table.Column<int>(type: "int", nullable: true, defaultValue: 1480),
                    KeepaliveTimeout = table.Column<int>(type: "int", nullable: true, defaultValue: 10),
                    OneSessionPerHost = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Disabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Comment = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    MikroTikId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppServers_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$MBPrDHis3eXf4sAYX181au9oPxaEmGWIoSTiXu4PBOax6tUPNE73K");

            migrationBuilder.CreateIndex(
                name: "IX_IpPools_RouterId_Name",
                table: "IpPools",
                columns: new[] { "RouterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingChanges_RouterId_Status_NextRetryAt",
                table: "PendingChanges",
                columns: new[] { "RouterId", "Status", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PppProfiles_RouterId_Name",
                table: "PppProfiles",
                columns: new[] { "RouterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PppSecrets_RouterId_Name",
                table: "PppSecrets",
                columns: new[] { "RouterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PppServers_RouterId_Name",
                table: "PppServers",
                columns: new[] { "RouterId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IpPools");

            migrationBuilder.DropTable(
                name: "PendingChanges");

            migrationBuilder.DropTable(
                name: "PppProfiles");

            migrationBuilder.DropTable(
                name: "PppSecrets");

            migrationBuilder.DropTable(
                name: "PppServers");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$XdJN8p5BvL8kZ0qY0qY0qO8kZ0qY0qY0qO8kZ0qY0qY0qO8kZ0qY0q");
        }
    }
}
