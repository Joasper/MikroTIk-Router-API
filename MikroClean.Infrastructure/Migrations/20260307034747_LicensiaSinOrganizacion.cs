using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LicensiaSinOrganizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Licenses_OrganizationId",
                table: "Licenses");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Licenses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2701));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2703));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2704));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2706));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2708));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2709));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2711));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2713));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2714));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2716));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2657));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2660));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2662));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2664));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2452));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2454));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2456));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2458));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2460));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2461));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2463));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2465));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2466));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2256));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2260));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2262));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 7, 3, 47, 46, 112, DateTimeKind.Utc).AddTicks(2263));

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_OrganizationId",
                table: "Licenses",
                column: "OrganizationId",
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Licenses_OrganizationId",
                table: "Licenses");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Licenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1097));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1100));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1102));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1104));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1107));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1109));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1111));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1114));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1116));

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1118));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1026));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1030));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1032));

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1035));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(929));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(935));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(938));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(942));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(945));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(947));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(949));

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(951));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(624));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(629));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(631));

            migrationBuilder.UpdateData(
                table: "SystemRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(634));

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_OrganizationId",
                table: "Licenses",
                column: "OrganizationId",
                unique: true);
        }
    }
}
