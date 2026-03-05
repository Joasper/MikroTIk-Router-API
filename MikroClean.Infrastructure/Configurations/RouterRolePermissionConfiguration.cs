using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class RouterRolePermissionConfiguration : IEntityTypeConfiguration<RouterRolePermission>
    {
        public void Configure(EntityTypeBuilder<RouterRolePermission> builder)
        {
            builder.ToTable("RouterRolePermissions");
            
            builder.HasKey(rrp => new { rrp.RoleId, rrp.PermissionId });
            
            builder.Property(rrp => rrp.RoleId)
                .IsRequired();
            
            builder.Property(rrp => rrp.PermissionId)
                .IsRequired();

            // Indexes
            builder.HasIndex(rrp => rrp.PermissionId);
        }
    }
}
