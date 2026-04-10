using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
            builder.ToTable("InvoiceDetails");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Descripcion)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.PeriodoCubierto)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Cantidad)
                .HasColumnType("decimal(18, 4)")
                .IsRequired();

            builder.Property(x => x.PrecioUnitario)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.NumeroLinea)
                .HasColumnType("int")
                .IsRequired();

            // Foreign Key
            builder.HasOne(x => x.Invoice)
                .WithMany(i => i.Detalles)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => x.InvoiceId);
            builder.HasIndex(x => new { x.InvoiceId, x.NumeroLinea });

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
