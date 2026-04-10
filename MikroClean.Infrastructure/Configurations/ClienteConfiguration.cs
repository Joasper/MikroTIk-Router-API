using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Cedula)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Telefono)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Direccion)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.ReferenciaPago)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            // Foreign Key
            builder.HasOne(x => x.Organization)
                .WithMany(o => o.Clientes)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indices
            builder.HasIndex(x => new { x.OrganizationId, x.IsActive });
            builder.HasIndex(x => x.Cedula).IsUnique();
            builder.HasIndex(x => x.Email);

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
