using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class RouterStatusConfiguration : IEntityTypeConfiguration<RouterStatus>
    {
        public void Configure(EntityTypeBuilder<RouterStatus> builder)
        {
            builder.ToTable("RouterStatus");
            
            builder.HasKey(rs => rs.RouterId);
            
            builder.Property(rs => rs.RouterId)
                .IsRequired();
            
            builder.Property(rs => rs.IsOnline)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(rs => rs.CpuUsage)
                .IsRequired(false);
            
            builder.Property(rs => rs.MemoryUsage)
                .IsRequired(false);
            
            builder.Property(rs => rs.Uptime)
                .IsRequired(false);
            
            builder.Property(rs => rs.ActiveUsers)
                .IsRequired(false);
            
            builder.Property(rs => rs.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(rs => new { rs.IsOnline, rs.LastUpdated });
        }
    }
}
