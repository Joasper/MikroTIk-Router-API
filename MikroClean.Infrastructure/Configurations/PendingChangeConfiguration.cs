using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class PendingChangeConfiguration : IEntityTypeConfiguration<PendingChange>
    {
        public void Configure(EntityTypeBuilder<PendingChange> builder)
        {
            builder.ToTable("PendingChanges");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Resource)
                .HasConversion<int>()
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Operation)
                .HasConversion<int>()
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.PayloadJson)
                .HasColumnType("varchar(max)")
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.EntityKey)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.LastError)
                .HasColumnType("varchar(1000)")
                .HasMaxLength(1000)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.RetryCount)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.NextRetryAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.ProcessedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.HasOne(x => x.Router)
                .WithMany(r => r.PendingChanges)
                .HasForeignKey(x => x.RouterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.RouterId, x.Status, x.NextRetryAt });
        }
    }
}
