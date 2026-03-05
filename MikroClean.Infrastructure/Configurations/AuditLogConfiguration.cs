using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            
            builder.HasKey(al => al.Id);
            
            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");
            
            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(al => al.EntityId)
                .IsRequired(false);
            
            builder.Property(al => al.OldValues)
                .HasColumnType("nvarchar(max)");
            
            builder.Property(al => al.NewValues)
                .HasColumnType("nvarchar(max)");
            
            builder.Property(al => al.IpAddress)
                .HasMaxLength(45)
                .HasColumnType("varchar(45)");
            
            builder.Property(al => al.UserId)
                .IsRequired();
            
            builder.Property(al => al.RouterId)
                .IsRequired(false);

            // Relationships
            builder.HasOne(al => al.Router)
                .WithMany(r => r.AuditLogs)
                .HasForeignKey(al => al.RouterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(al => new { al.UserId, al.CreatedAt });
            builder.HasIndex(al => new { al.RouterId, al.CreatedAt });
            builder.HasIndex(al => new { al.EntityType, al.EntityId });
            builder.HasIndex(al => al.Action);
        }
    }
}
