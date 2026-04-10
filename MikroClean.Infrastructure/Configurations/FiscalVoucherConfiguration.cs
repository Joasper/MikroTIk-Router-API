using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class FiscalVoucherConfiguration : IEntityTypeConfiguration<FiscalVoucher>
    {
        public void Configure(EntityTypeBuilder<FiscalVoucher> builder)
        {
            builder.ToTable("FiscalVouchers");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.NumeroSecuencia)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.FechaRegistro)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.RangoDesde)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.RangoHasta)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Periodo)
                .HasColumnType("varchar(7)")
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Notas)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            // Foreign Key
            builder.HasOne(x => x.Invoice)
                .WithOne(i => i.FiscalVoucher)
                .HasForeignKey<FiscalVoucher>(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => x.NumeroSecuencia).IsUnique();
            builder.HasIndex(x => x.Periodo);
            builder.HasIndex(x => x.InvoiceId);

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
