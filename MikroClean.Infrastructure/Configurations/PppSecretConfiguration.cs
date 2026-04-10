using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Infrastructure.Configurations
{
    public class PppSecretConfiguration : IEntityTypeConfiguration<PppSecret>
    {
        public void Configure(EntityTypeBuilder<PppSecret> builder)
        {
            builder.ToTable("PppSecrets");
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(); // Username

            builder.Property(x => x.Password)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Profile)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Service)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("pppoe")
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(x => x.Disabled)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

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
                   .WithMany(r => r.PppSecrets)
                   .HasForeignKey(x => x.RouterId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasIndex(x => new { x.RouterId, x.Name }).IsUnique();
        }
    }
}
