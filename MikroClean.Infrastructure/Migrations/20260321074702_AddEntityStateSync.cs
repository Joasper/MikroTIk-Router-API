using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityStateSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyncState",
                table: "PppServers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SyncState",
                table: "PppSecrets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SyncState",
                table: "PppProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SyncState",
                table: "IpPools",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncState",
                table: "PppServers");

            migrationBuilder.DropColumn(
                name: "SyncState",
                table: "PppSecrets");

            migrationBuilder.DropColumn(
                name: "SyncState",
                table: "PppProfiles");

            migrationBuilder.DropColumn(
                name: "SyncState",
                table: "IpPools");
        }
    }
}
