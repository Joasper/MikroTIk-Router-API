using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class BillingTemplateConfiguration : IEntityTypeConfiguration<BillingTemplate>
    {
        public void Configure(EntityTypeBuilder<BillingTemplate> builder)
        {
            builder.ToTable("BillingTemplates");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DiaInicio)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.DiaCutoff)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.TipoCiclo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.DiasGracia)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.EsPrePago)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            // Foreign Key
            builder.HasOne(x => x.Cliente)
                .WithMany(c => c.PlantillasBilling)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices
            builder.HasIndex(x => new { x.ClienteId, x.IsActive }).IsUnique();

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
