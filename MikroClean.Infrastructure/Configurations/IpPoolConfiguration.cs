using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Infrastructure.Configurations
{
    public class IpPoolConfiguration : IEntityTypeConfiguration<IpPool>
    {
        public void Configure(EntityTypeBuilder<IpPool> builder)
        {
            builder.ToTable("IpPools");
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Ranges)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.NextPool)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Comment)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.MikroTikId)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(); // *ID from MikroTik

            builder.Property(x => x.SyncState)
                .HasConversion<int>()
                .HasColumnType("int")
                .HasDefaultValue(SyncState.Synced)
                .IsRequired();

            builder.HasOne(x => x.Router)
                   .WithMany(r => r.IpPools)
                   .HasForeignKey(x => x.RouterId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            // Unique constraint for Name per Router
            builder.HasIndex(x => new { x.RouterId, x.Name }).IsUnique();
        }
    }
}
