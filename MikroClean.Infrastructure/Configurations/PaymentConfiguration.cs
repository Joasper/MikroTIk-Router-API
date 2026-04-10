using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Monto)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.FechaPago)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.Tipo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Referencia)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.MetodoPago)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Notas)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.RegistradoPorUserId)
                .HasColumnType("int")
                .IsRequired(false);

            // Foreign Key
            builder.HasOne(x => x.Cliente)
                .WithMany(c => c.Pagos)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => new { x.ClienteId, x.FechaPago });
            builder.HasIndex(x => x.Referencia);

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
