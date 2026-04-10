using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Cedula = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    ReferenciaPago = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiscalVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    NumeroSecuencia = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RangoDesde = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RangoHasta = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Periodo = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalVouchers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    VelocidadMbps = table.Column<int>(type: "int", nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planes_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notas = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    DiaInicio = table.Column<int>(type: "int", nullable: false),
                    DiaCutoff = table.Column<int>(type: "int", nullable: false),
                    TipoCiclo = table.Column<int>(type: "int", nullable: false),
                    DiasGracia = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EsPrePago = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingTemplates_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    MetodoPago = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Notas = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    RegistradoPorUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    NumeroFactura = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TipoComprobante = table.Column<int>(type: "int", nullable: false, defaultValue: 31),
                    Periodo = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    EsAbono = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notas = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    FiscalVoucherId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_FiscalVouchers_FiscalVoucherId",
                        column: x => x.FiscalVoucherId,
                        principalTable: "FiscalVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Subscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    PppSecretId = table.Column<int>(type: "int", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscripciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscripciones_Planes_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscripciones_PppSecrets_PppSecretId",
                        column: x => x.PppSecretId,
                        principalTable: "PppSecrets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    PeriodoCubierto = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentInvoiceMappings",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentInvoiceMappings", x => new { x.PaymentId, x.InvoiceId });
                    table.ForeignKey(
                        name: "FK_PaymentInvoiceMappings_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentInvoiceMappings_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "Ver estad�sticas");

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Backup de configuraci�n");

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Solo visualizaci�n");

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "T�cnico de soporte");

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "Ver logs de auditor�a");

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Gestionar facturaci�n");

            migrationBuilder.InsertData(
                table: "Taxes",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeletedBy", "Descripcion", "IsActive", "Nombre", "Notas", "Porcentaje", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Impuesto sobre Transferencias de Bienes Industrializados y Servicios", true, "ITBIS", "Tasa estandar RD", 18.00m, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_BillingTemplates_ClienteId_IsActive",
                table: "BillingTemplates",
                columns: new[] { "ClienteId", "IsActive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cedula",
                table: "Clientes",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_OrganizationId_IsActive",
                table: "Clientes",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalVouchers_InvoiceId",
                table: "FiscalVouchers",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalVouchers_NumeroSecuencia",
                table: "FiscalVouchers",
                column: "NumeroSecuencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalVouchers_Periodo",
                table: "FiscalVouchers",
                column: "Periodo");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId",
                table: "InvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId_NumeroLinea",
                table: "InvoiceDetails",
                columns: new[] { "InvoiceId", "NumeroLinea" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClienteId_Estado",
                table: "Invoices",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClienteId_FechaVencimiento",
                table: "Invoices",
                columns: new[] { "ClienteId", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_FiscalVoucherId",
                table: "Invoices",
                column: "FiscalVoucherId",
                unique: true,
                filter: "[FiscalVoucherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_NumeroFactura",
                table: "Invoices",
                column: "NumeroFactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Periodo_Estado",
                table: "Invoices",
                columns: new[] { "Periodo", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceMappings_InvoiceId",
                table: "PaymentInvoiceMappings",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInvoiceMappings_PaymentId",
                table: "PaymentInvoiceMappings",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClienteId_FechaPago",
                table: "Payments",
                columns: new[] { "ClienteId", "FechaPago" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Referencia",
                table: "Payments",
                column: "Referencia");

            migrationBuilder.CreateIndex(
                name: "IX_Planes_RouterId_EsDefault",
                table: "Planes",
                columns: new[] { "RouterId", "EsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Planes_RouterId_IsActive",
                table: "Planes",
                columns: new[] { "RouterId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscripciones_ClienteId_Estado",
                table: "Subscripciones",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscripciones_PlanId",
                table: "Subscripciones",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscripciones_PppSecretId",
                table: "Subscripciones",
                column: "PppSecretId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IsActive",
                table: "Taxes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_Nombre",
                table: "Taxes",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingTemplates");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "PaymentInvoiceMappings");

            migrationBuilder.DropTable(
                name: "Subscripciones");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Planes");

            migrationBuilder.DropTable(
                name: "FiscalVouchers");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "Ver estadísticas");

            migrationBuilder.UpdateData(
                table: "RouterPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Backup de configuración");

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Solo visualización");

            migrationBuilder.UpdateData(
                table: "RouterRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Técnico de soporte");

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "Ver logs de auditoría");

            migrationBuilder.UpdateData(
                table: "SystemPermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Gestionar facturación");
        }
    }
}
