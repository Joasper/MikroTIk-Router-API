using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MikroClean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouterPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouterPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouterRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouterRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxRouters = table.Column<int>(type: "int", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licenses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false),
                    User = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MacAddress = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouterRolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouterRolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RouterRolePermissions_RouterPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "RouterPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouterRolePermissions_RouterRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RouterRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemRolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_SystemRolePermissions_SystemPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "SystemPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemRolePermissions_SystemRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    SystemRoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_SystemRoles_SystemRoleId",
                        column: x => x.SystemRoleId,
                        principalTable: "SystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouterStatus",
                columns: table => new
                {
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CpuUsage = table.Column<int>(type: "int", nullable: true),
                    MemoryUsage = table.Column<int>(type: "int", nullable: true),
                    Uptime = table.Column<long>(type: "bigint", nullable: true),
                    ActiveUsers = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouterStatus", x => x.RouterId);
                    table.ForeignKey(
                        name: "FK_RouterStatus_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Module = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ActionUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRouterAccess",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RouterId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRouterAccess", x => new { x.UserId, x.RouterId });
                    table.ForeignKey(
                        name: "FK_UserRouterAccess_RouterRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RouterRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRouterAccess_Routers_RouterId",
                        column: x => x.RouterId,
                        principalTable: "Routers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRouterAccess_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RouterPermissions",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeletedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1097), null, null, "Reiniciar el router", "REBOOT_ROUTER", null, null },
                    { 2, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1100), null, null, "Crear redes", "CREATE_NETWORK", null, null },
                    { 3, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1102), null, null, "Eliminar redes", "DELETE_NETWORK", null, null },
                    { 4, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1104), null, null, "Gestionar firewall", "MANAGE_FIREWALL", null, null },
                    { 5, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1107), null, null, "Desconectar usuarios", "KICK_USER", null, null },
                    { 6, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1109), null, null, "Ver logs del router", "VIEW_LOGS", null, null },
                    { 7, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1111), null, null, "Gestionar DHCP", "MANAGE_DHCP", null, null },
                    { 8, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1114), null, null, "Gestionar QoS", "MANAGE_QOS", null, null },
                    { 9, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1116), null, null, "Ver estadísticas", "VIEW_STATISTICS", null, null },
                    { 10, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1118), null, null, "Backup de configuración", "BACKUP_CONFIG", null, null }
                });

            migrationBuilder.InsertData(
                table: "RouterRoles",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeletedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1026), null, null, "Administrador completo del router", "NetworkAdmin", null, null },
                    { 2, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1030), null, null, "Gestiona usuarios del router", "UserManager", null, null },
                    { 3, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1032), null, null, "Solo visualización", "Viewer", null, null },
                    { 4, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(1035), null, null, "Técnico de soporte", "Technician", null, null }
                });

            migrationBuilder.InsertData(
                table: "SystemPermissions",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeletedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(929), null, null, "Ver todas las organizaciones", "VIEW_ALL_ORGANIZATIONS", null, null },
                    { 2, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(933), null, null, "Gestionar organizaciones", "MANAGE_ORGANIZATIONS", null, null },
                    { 3, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(935), null, null, "Ver licencias", "VIEW_LICENSES", null, null },
                    { 4, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(938), null, null, "Gestionar licencias", "MANAGE_LICENSES", null, null },
                    { 5, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(940), null, null, "Ver routers", "VIEW_ROUTERS", null, null },
                    { 6, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(942), null, null, "Gestionar routers", "MANAGE_ROUTERS", null, null },
                    { 7, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(945), null, null, "Ver usuarios", "VIEW_USERS", null, null },
                    { 8, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(947), null, null, "Gestionar usuarios", "MANAGE_USERS", null, null },
                    { 9, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(949), null, null, "Ver logs de auditoría", "VIEW_AUDIT_LOGS", null, null },
                    { 10, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(951), null, null, "Gestionar facturación", "MANAGE_BILLING", null, null }
                });

            migrationBuilder.InsertData(
                table: "SystemRoles",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeletedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(624), null, null, "SuperAdmin", null, null },
                    { 2, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(629), null, null, "ITSupport", null, null },
                    { 3, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(631), null, null, "ClientAdmin", null, null },
                    { 4, new DateTime(2026, 3, 6, 18, 34, 22, 878, DateTimeKind.Utc).AddTicks(634), null, null, "ClientUser", null, null }
                });

            migrationBuilder.InsertData(
                table: "RouterRolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 9, 2 },
                    { 6, 3 },
                    { 9, 3 },
                    { 1, 4 },
                    { 6, 4 },
                    { 9, 4 },
                    { 10, 4 }
                });

            migrationBuilder.InsertData(
                table: "SystemRolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 1, 2 },
                    { 3, 2 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 9, 2 },
                    { 5, 3 },
                    { 6, 3 },
                    { 7, 3 },
                    { 8, 3 },
                    { 5, 4 },
                    { 7, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RouterId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "RouterId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_IsActive_EndDate",
                table: "Licenses",
                columns: new[] { "IsActive", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Key",
                table: "Licenses",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_OrganizationId",
                table: "Licenses",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Module_Severity",
                table: "Notifications",
                columns: new[] { "Module", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RouterId",
                table: "Notifications",
                column: "RouterId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsViewed_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsViewed", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DeletedAt",
                table: "Organizations",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Email",
                table: "Organizations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_RouterPermissions_Name",
                table: "RouterPermissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouterRolePermissions_PermissionId",
                table: "RouterRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouterRoles_Name",
                table: "RouterRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routers_DeletedAt",
                table: "Routers",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Routers_LastSeen",
                table: "Routers",
                column: "LastSeen");

            migrationBuilder.CreateIndex(
                name: "IX_Routers_MacAddress",
                table: "Routers",
                column: "MacAddress",
                unique: true,
                filter: "[MacAddress] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Routers_OrganizationId_IsActive",
                table: "Routers",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RouterStatus_IsOnline_LastUpdated",
                table: "RouterStatus",
                columns: new[] { "IsOnline", "LastUpdated" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_Name",
                table: "SystemPermissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemRolePermissions_PermissionId",
                table: "SystemRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_Name",
                table: "SystemRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRouterAccess_RoleId",
                table: "UserRouterAccess",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRouterAccess_RouterId",
                table: "UserRouterAccess",
                column: "RouterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAt",
                table: "Users",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId_IsActive",
                table: "Users",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SystemRoleId",
                table: "Users",
                column: "SystemRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RouterRolePermissions");

            migrationBuilder.DropTable(
                name: "RouterStatus");

            migrationBuilder.DropTable(
                name: "SystemRolePermissions");

            migrationBuilder.DropTable(
                name: "UserRouterAccess");

            migrationBuilder.DropTable(
                name: "RouterPermissions");

            migrationBuilder.DropTable(
                name: "SystemPermissions");

            migrationBuilder.DropTable(
                name: "RouterRoles");

            migrationBuilder.DropTable(
                name: "Routers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "SystemRoles");

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });
        }
    }
}
