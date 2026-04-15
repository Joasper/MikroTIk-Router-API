using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRouterInterfaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouterInterfaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    MikroTikId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DefaultName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Mtu = table.Column<int>(type: "int", nullable: false, defaultValue: 1500),
                    ActualMtu = table.Column<int>(type: "int", nullable: false),
                    MaxL2Mtu = table.Column<int>(type: "int", nullable: false),
                    MacAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    LinkDowns = table.Column<int>(type: "int", nullable: false),
                    RxByte = table.Column<long>(type: "bigint", nullable: false),
                    TxByte = table.Column<long>(type: "bigint", nullable: false),
                    RxPacket = table.Column<long>(type: "bigint", nullable: false),
                    TxPacket = table.Column<long>(type: "bigint", nullable: false),
                    RxDrop = table.Column<long>(type: "bigint", nullable: false),
                    TxDrop = table.Column<long>(type: "bigint", nullable: false),
                    TxQueueDrop = table.Column<long>(type: "bigint", nullable: false),
                    RxError = table.Column<long>(type: "bigint", nullable: false),
                    TxError = table.Column<long>(type: "bigint", nullable: false),
                    FpRxByte = table.Column<long>(type: "bigint", nullable: false),
                    FpTxByte = table.Column<long>(type: "bigint", nullable: false),
                    FpRxPacket = table.Column<long>(type: "bigint", nullable: false),
                    FpTxPacket = table.Column<long>(type: "bigint", nullable: false),
                    Running = table.Column<bool>(type: "bit", nullable: false),
                    Disabled = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    SyncState = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouterInterfaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouterInterfaces_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índices
            migrationBuilder.CreateIndex(
                name: "IX_RouterInterfaces_RouterId_MikroTikId",
                table: "RouterInterfaces",
                columns: new[] { "RouterId", "MikroTikId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouterInterfaces_RouterId_Name",
                table: "RouterInterfaces",
                columns: new[] { "RouterId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RouterInterfaces_RouterId_Type",
                table: "RouterInterfaces",
                columns: new[] { "RouterId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_RouterInterfaces_RouterId_Running",
                table: "RouterInterfaces",
                columns: new[] { "RouterId", "Running" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RouterInterfaces");
        }
    }
}
