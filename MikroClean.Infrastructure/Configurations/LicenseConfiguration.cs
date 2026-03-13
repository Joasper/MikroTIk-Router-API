using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class LicenseConfiguration : IEntityTypeConfiguration<License>
    {
        public void Configure(EntityTypeBuilder<License> builder)
        {
            builder.ToTable("Licenses");
            
            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.Key)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(l => l.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(l => l.StartDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            builder.Property(l => l.EndDate)
                .IsRequired();
            
            builder.Property(l => l.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(l => l.MaxRouters)
                .IsRequired(false);
            
            builder.Property(l => l.MaxUsers)
                .IsRequired(false);
            
            builder.Property(l => l.OrganizationId)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(l => l.Key)
                .IsUnique();
            
            builder.HasIndex(l => l.OrganizationId);
            
            builder.HasIndex(l => new { l.IsActive, l.EndDate });
        }
    }
}
