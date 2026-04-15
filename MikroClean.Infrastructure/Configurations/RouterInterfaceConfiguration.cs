using MikroClean.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MikroClean.Infrastructure.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad RouterInterface
/// </summary>
public class RouterInterfaceConfiguration : IEntityTypeConfiguration<RouterInterface>
{
    public void Configure(EntityTypeBuilder<RouterInterface> builder)
    {
        builder.ToTable("RouterInterfaces");

        builder.HasKey(x => x.Id);

        // Índice compuesto único: RouterId + MikroTikId
        builder.HasIndex(x => new { x.RouterId, x.MikroTikId })
            .IsUnique()
            .HasDatabaseName("IX_RouterInterfaces_RouterId_MikroTikId");

        // Índice por RouterId + Name
        builder.HasIndex(x => new { x.RouterId, x.Name })
            .HasDatabaseName("IX_RouterInterfaces_RouterId_Name");

        // Índice por RouterId + Type para filtrado rápido
        builder.HasIndex(x => new { x.RouterId, x.Type })
            .HasDatabaseName("IX_RouterInterfaces_RouterId_Type");

        // Índice por Running para consultas de interfaces activas
        builder.HasIndex(x => new { x.RouterId, x.Running })
            .HasDatabaseName("IX_RouterInterfaces_RouterId_Running");

        builder.Property(x => x.MikroTikId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DefaultName)
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MacAddress)
            .HasMaxLength(50);

        builder.Property(x => x.Comment)
            .HasMaxLength(500);

        // Relación con Router (FK restrict delete)
        builder.HasOne(x => x.Router)
            .WithMany()
            .HasForeignKey(x => x.RouterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filter
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
