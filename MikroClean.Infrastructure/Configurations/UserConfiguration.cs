using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("varchar(255)");
            
            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(u => u.LastLogin)
                .IsRequired(false);
            
            builder.Property(u => u.FailedLoginAttempts)
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(u => u.LockedUntil)
                .IsRequired(false);
            
            builder.Property(u => u.OrganizationId)
                .IsRequired(false);
            
            builder.Property(u => u.SystemRoleId)
                .IsRequired();

            // Relationships
            builder.HasOne(u => u.SystemRole)
                .WithMany(sr => sr.Users)
                .HasForeignKey(u => u.SystemRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.RouterAccesses)
                .WithOne(ura => ura.User)
                .HasForeignKey(ura => ura.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.AuditLogs)
                .WithOne(al => al.User)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.HasIndex(u => u.Email)
                .IsUnique();
            
            builder.HasIndex(u => new { u.OrganizationId, u.IsActive });
            builder.HasIndex(u => u.DeletedAt);
        }
    }
}
