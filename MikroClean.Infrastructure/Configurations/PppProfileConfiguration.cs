using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Infrastructure.Configurations
{
    public class PppProfileConfiguration : IEntityTypeConfiguration<PppProfile>
    {
        public void Configure(EntityTypeBuilder<PppProfile> builder)
        {
            builder.ToTable("PppProfiles");
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.LocalAddress)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.RemoteAddress)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.DnsServers)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.RateLimit)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.OnlyOne)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("default")
                .IsRequired();

            builder.Property(x => x.IsDefault)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.MikroTikId)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.SyncState)
                .HasConversion<int>()
                .HasColumnType("int")
                .HasDefaultValue(SyncState.Synced)
                .IsRequired();

            builder.HasOne(x => x.Router)
                   .WithMany(r => r.PppProfiles)
                   .HasForeignKey(x => x.RouterId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasIndex(x => new { x.RouterId, x.Name }).IsUnique();
        }
    }
}
