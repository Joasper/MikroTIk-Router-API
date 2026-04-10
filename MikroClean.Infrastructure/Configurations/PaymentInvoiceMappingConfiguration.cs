using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class PaymentInvoiceMappingConfiguration : IEntityTypeConfiguration<PaymentInvoiceMapping>
    {
        public void Configure(EntityTypeBuilder<PaymentInvoiceMapping> builder)
        {
            builder.ToTable("PaymentInvoiceMappings");
            builder.HasKey(x => new { x.PaymentId, x.InvoiceId });

            builder.Property(x => x.MontoAplicado)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(x => x.FechaAplicacion)
                .HasColumnType("datetime2")
                .IsRequired();

            // Foreign Keys
            builder.HasOne(x => x.Payment)
                .WithMany(p => p.FacturasAplicadas)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Invoice)
                .WithMany(i => i.PagosAplicados)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => x.InvoiceId);
            builder.HasIndex(x => x.PaymentId);
        }
    }
}
