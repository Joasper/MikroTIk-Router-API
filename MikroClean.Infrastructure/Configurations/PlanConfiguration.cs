using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.ToTable("Planes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Descripcion)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.VelocidadMbps)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.PrecioMensual)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.EsDefault)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            // Foreign Key
            builder.HasOne(x => x.Router)
                .WithMany(r => r.Planes)
                .HasForeignKey(x => x.RouterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => new { x.RouterId, x.EsDefault });
            builder.HasIndex(x => new { x.RouterId, x.IsActive });

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
