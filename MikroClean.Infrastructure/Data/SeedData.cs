using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Seed SystemRoles
            modelBuilder.Entity<SystemRole>().HasData(
                new SystemRole { Id = 1, Name = "SuperAdmin", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemRole { Id = 2, Name = "ITSupport", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemRole { Id = 3, Name = "ClientAdmin", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemRole { Id = 4, Name = "ClientUser", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed Default SuperAdmin User
            // Username: superadmin
            // Password: Admin123!
            // IMPORTANTE: Cambiar esta contraseña después del primer acceso
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "superadmin",
                    Email = "admin@mikroclean.com",
                    PasswordHash = "$2a$11$MBPrDHis3eXf4sAYX181au9oPxaEmGWIoSTiXu4PBOax6tUPNE73K",
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    OrganizationId = null,
                    SystemRoleId = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed SystemPermissions
            modelBuilder.Entity<SystemPermission>().HasData(
                new SystemPermission { Id = 1, Name = "VIEW_ALL_ORGANIZATIONS", Description = "Ver todas las organizaciones", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 2, Name = "MANAGE_ORGANIZATIONS", Description = "Gestionar organizaciones", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 3, Name = "VIEW_LICENSES", Description = "Ver licencias", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 4, Name = "MANAGE_LICENSES", Description = "Gestionar licencias", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 5, Name = "VIEW_ROUTERS", Description = "Ver routers", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 6, Name = "MANAGE_ROUTERS", Description = "Gestionar routers", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 7, Name = "VIEW_USERS", Description = "Ver usuarios", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 8, Name = "MANAGE_USERS", Description = "Gestionar usuarios", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 9, Name = "VIEW_AUDIT_LOGS", Description = "Ver logs de auditoría", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new SystemPermission { Id = 10, Name = "MANAGE_BILLING", Description = "Gestionar facturación", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed RouterRoles
            modelBuilder.Entity<RouterRole>().HasData(
                new RouterRole { Id = 1, Name = "NetworkAdmin", Description = "Administrador completo del router", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterRole { Id = 2, Name = "UserManager", Description = "Gestiona usuarios del router", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterRole { Id = 3, Name = "Viewer", Description = "Solo visualización", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterRole { Id = 4, Name = "Technician", Description = "Técnico de soporte", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed RouterPermissions
            modelBuilder.Entity<RouterPermission>().HasData(
                new RouterPermission { Id = 1, Name = "REBOOT_ROUTER", Description = "Reiniciar el router", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 2, Name = "CREATE_NETWORK", Description = "Crear redes", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 3, Name = "DELETE_NETWORK", Description = "Eliminar redes", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 4, Name = "MANAGE_FIREWALL", Description = "Gestionar firewall", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 5, Name = "KICK_USER", Description = "Desconectar usuarios", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 6, Name = "VIEW_LOGS", Description = "Ver logs del router", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 7, Name = "MANAGE_DHCP", Description = "Gestionar DHCP", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 8, Name = "MANAGE_QOS", Description = "Gestionar QoS", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 9, Name = "VIEW_STATISTICS", Description = "Ver estadísticas", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new RouterPermission { Id = 10, Name = "BACKUP_CONFIG", Description = "Backup de configuración", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed SystemRolePermissions (SuperAdmin tiene todos los permisos)
            modelBuilder.Entity<SystemRolePermission>().HasData(
                new SystemRolePermission { RoleId = 1, PermissionId = 1 },
                new SystemRolePermission { RoleId = 1, PermissionId = 2 },
                new SystemRolePermission { RoleId = 1, PermissionId = 3 },
                new SystemRolePermission { RoleId = 1, PermissionId = 4 },
                new SystemRolePermission { RoleId = 1, PermissionId = 5 },
                new SystemRolePermission { RoleId = 1, PermissionId = 6 },
                new SystemRolePermission { RoleId = 1, PermissionId = 7 },
                new SystemRolePermission { RoleId = 1, PermissionId = 8 },
                new SystemRolePermission { RoleId = 1, PermissionId = 9 },
                new SystemRolePermission { RoleId = 1, PermissionId = 10 },
                // ITSupport
                new SystemRolePermission { RoleId = 2, PermissionId = 1 },
                new SystemRolePermission { RoleId = 2, PermissionId = 3 },
                new SystemRolePermission { RoleId = 2, PermissionId = 5 },
                new SystemRolePermission { RoleId = 2, PermissionId = 6 },
                new SystemRolePermission { RoleId = 2, PermissionId = 7 },
                new SystemRolePermission { RoleId = 2, PermissionId = 9 },
                // ClientAdmin
                new SystemRolePermission { RoleId = 3, PermissionId = 5 },
                new SystemRolePermission { RoleId = 3, PermissionId = 6 },
                new SystemRolePermission { RoleId = 3, PermissionId = 7 },
                new SystemRolePermission { RoleId = 3, PermissionId = 8 },
                // ClientUser
                new SystemRolePermission { RoleId = 4, PermissionId = 5 },
                new SystemRolePermission { RoleId = 4, PermissionId = 7 }
            );

            // Seed RouterRolePermissions (NetworkAdmin tiene todos los permisos de router)
            modelBuilder.Entity<RouterRolePermission>().HasData(
                new RouterRolePermission { RoleId = 1, PermissionId = 1 },
                new RouterRolePermission { RoleId = 1, PermissionId = 2 },
                new RouterRolePermission { RoleId = 1, PermissionId = 3 },
                new RouterRolePermission { RoleId = 1, PermissionId = 4 },
                new RouterRolePermission { RoleId = 1, PermissionId = 5 },
                new RouterRolePermission { RoleId = 1, PermissionId = 6 },
                new RouterRolePermission { RoleId = 1, PermissionId = 7 },
                new RouterRolePermission { RoleId = 1, PermissionId = 8 },
                new RouterRolePermission { RoleId = 1, PermissionId = 9 },
                new RouterRolePermission { RoleId = 1, PermissionId = 10 },
                // UserManager
                new RouterRolePermission { RoleId = 2, PermissionId = 5 },
                new RouterRolePermission { RoleId = 2, PermissionId = 6 },
                new RouterRolePermission { RoleId = 2, PermissionId = 7 },
                new RouterRolePermission { RoleId = 2, PermissionId = 9 },
                // Viewer
                new RouterRolePermission { RoleId = 3, PermissionId = 6 },
                new RouterRolePermission { RoleId = 3, PermissionId = 9 },
                // Technician
                new RouterRolePermission { RoleId = 4, PermissionId = 1 },
                new RouterRolePermission { RoleId = 4, PermissionId = 6 },
                new RouterRolePermission { RoleId = 4, PermissionId = 9 },
                new RouterRolePermission { RoleId = 4, PermissionId = 10 }
            );
        }
    }
}
