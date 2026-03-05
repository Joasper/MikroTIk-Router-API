using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Infrastructure.Data;

namespace MikroClean.Infrastructure.Context;

public class MikroCleanContext : DbContext
{
    public MikroCleanContext(DbContextOptions<MikroCleanContext> opts) : base(opts)
    {
    }

    // DbSets
    public DbSet<Organizations> Organizations { get; set; }
    public DbSet<License> Licenses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Router> Routers { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }
    public DbSet<SystemRolePermission> SystemRolePermissions { get; set; }
    public DbSet<RouterRole> RouterRoles { get; set; }
    public DbSet<RouterPermission> RouterPermissions { get; set; }
    public DbSet<RouterRolePermission> RouterRolePermissions { get; set; }
    public DbSet<UserRouterAccess> UserRouterAccess { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RouterStatus> RouterStatus { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MikroCleanContext).Assembly);

        // Seed initial data
        SeedData.Seed(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
