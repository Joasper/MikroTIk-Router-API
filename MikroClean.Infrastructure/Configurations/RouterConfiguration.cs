using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class RouterConfiguration : IEntityTypeConfiguration<Router>
    {
        public void Configure(EntityTypeBuilder<Router> builder)
        {
            builder.ToTable("Routers");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(r => r.Ip)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnType("varchar(45)");
            
            builder.Property(r => r.User)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(r => r.EncryptedPassword)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");
            
            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(r => r.Model)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(r => r.Version)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
            
            builder.Property(r => r.LastSeen)
                .IsRequired(false);
            
            builder.Property(r => r.MacAddress)
                .HasMaxLength(17)
                .HasColumnType("varchar(17)");
            
            builder.Property(r => r.Location)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");
            
            builder.Property(r => r.OrganizationId)
                .IsRequired();

            // Relationships
            builder.HasOne(r => r.RouterStatus)
                .WithOne(rs => rs.Router)
                .HasForeignKey<RouterStatus>(rs => rs.RouterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.UserAccesses)
                .WithOne(ura => ura.Router)
                .HasForeignKey(ura => ura.RouterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.AuditLogs)
                .WithOne(al => al.Router)
                .HasForeignKey(al => al.RouterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(r => r.MacAddress)
                .IsUnique()
                .HasFilter("[MacAddress] IS NOT NULL");
            
            builder.HasIndex(r => new { r.OrganizationId, r.IsActive });
            builder.HasIndex(r => r.LastSeen);
            builder.HasIndex(r => r.DeletedAt);
        }
    }
}
