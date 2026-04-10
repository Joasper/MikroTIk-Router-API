using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.NumeroFactura)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.TipoComprobante)
                .HasColumnType("int")
                .HasDefaultValue(31)
                .IsRequired();

            builder.Property(x => x.Periodo)
                .HasColumnType("varchar(7)")
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.FechaEmision)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.FechaVencimiento)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.MontoBase)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.MontoImpuesto)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.Total)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.MontoPagado)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(x => x.Estado)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.EsAbono)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.Notas)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            // Foreign Keys
            builder.HasOne(x => x.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.FiscalVoucher)
                .WithOne(fv => fv.Invoice)
                .HasForeignKey<Invoice>(x => x.FiscalVoucherId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indices
            builder.HasIndex(x => new { x.ClienteId, x.Estado });
            builder.HasIndex(x => new { x.Periodo, x.Estado });
            builder.HasIndex(x => x.NumeroFactura).IsUnique();
            builder.HasIndex(x => new { x.ClienteId, x.FechaVencimiento });

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
