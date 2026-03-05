using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            
            builder.HasKey(n => n.Id);
            
            builder.Property(n => n.Module)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(n => n.Severity)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");
            
            builder.Property(n => n.IsViewed)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(n => n.ActionUrl)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");
            
            builder.Property(n => n.ExpiresAt)
                .IsRequired(false);
            
            builder.Property(n => n.UserId)
                .IsRequired();
            
            builder.Property(n => n.RouterId)
                .IsRequired(false);

            // Relationships
            builder.HasOne(n => n.Router)
                .WithMany()
                .HasForeignKey(n => n.RouterId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(n => new { n.UserId, n.IsViewed, n.CreatedAt });
            builder.HasIndex(n => n.RouterId);
            builder.HasIndex(n => new { n.Module, n.Severity });
        }
    }
}
