using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organizations>
    {
        public void Configure(EntityTypeBuilder<Organizations> builder)
        {
            builder.ToTable("Organizations");
            
            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");
            
            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(o => o.Phone)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");
            
            builder.Property(o => o.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            builder.Property(o => o.UpdatedAt)
                .IsRequired(false);
            
            builder.Property(o => o.DeletedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(o => o.License)
                .WithOne(l => l.Organization)
                .HasForeignKey<License>(l => l.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Routers)
                .WithOne(r => r.Organization)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(o => o.Email);
            builder.HasIndex(o => o.DeletedAt);
        }
    }
}
