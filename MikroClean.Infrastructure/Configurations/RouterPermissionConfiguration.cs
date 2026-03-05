using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class RouterPermissionConfiguration : IEntityTypeConfiguration<RouterPermission>
    {
        public void Configure(EntityTypeBuilder<RouterPermission> builder)
        {
            builder.ToTable("RouterPermissions");
            
            builder.HasKey(rp => rp.Id);
            
            builder.Property(rp => rp.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(rp => rp.Description)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            // Relationships
            builder.HasMany(rp => rp.RolePermissions)
                .WithOne(rrp => rrp.Permission)
                .HasForeignKey(rrp => rrp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(rp => rp.Name)
                .IsUnique();
        }
    }
}
