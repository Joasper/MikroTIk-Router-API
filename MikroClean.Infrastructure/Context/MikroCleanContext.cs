using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Context;

public class MikroCleanContext : DbContext
{
    public MikroCleanContext(DbContextOptions<MikroCleanContext> opts) : base(opts)
    {
        
    }

    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnType("varchar");
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
        });

        base.OnModelCreating(modelBuilder);

    }
}
