using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscripciones");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FechaInicio)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.FechaFin)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(x => x.Estado)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Notas)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            // Foreign Keys
            builder.HasOne(x => x.Cliente)
                .WithMany(c => c.Subscripciones)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Plan)
                .WithMany(p => p.Subscripciones)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PppSecret)
                .WithMany()
                .HasForeignKey(x => x.PppSecretId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indices
            builder.HasIndex(x => new { x.ClienteId, x.Estado });
            builder.HasIndex(x => x.PppSecretId);

            // Auditoría
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            builder.Property(x => x.DeletedAt).HasColumnType("datetime2");
        }
    }
}
