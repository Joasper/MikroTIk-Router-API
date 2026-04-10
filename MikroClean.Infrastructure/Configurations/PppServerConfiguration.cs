using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Infrastructure.Configurations
{
    public class PppServerConfiguration : IEntityTypeConfiguration<PppServer>
    {
        public void Configure(EntityTypeBuilder<PppServer> builder)
        {
            builder.ToTable("PppServers");
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(); // Service Name

            builder.Property(x => x.Interface)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.DefaultProfile)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("default")
                .IsRequired(false);

            builder.Property(x => x.MaxMtu)
                .HasColumnType("int")
                .HasDefaultValue(1480)
                .IsRequired(false);

            builder.Property(x => x.MaxMru)
                .HasColumnType("int")
                .HasDefaultValue(1480)
                .IsRequired(false);

            builder.Property(x => x.KeepaliveTimeout)
                .HasColumnType("int")
                .HasDefaultValue(10)
                .IsRequired(false);

            builder.Property(x => x.OneSessionPerHost).HasColumnType("bit").HasDefaultValue(true);

            builder.Property(x => x.Disabled)
                .HasColumnType("bit")
                .HasDefaultValue(false)
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
                   .WithMany(r => r.PppServers)
                   .HasForeignKey(x => x.RouterId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasIndex(x => new { x.RouterId, x.Name }).IsUnique();
        }
    }
}
