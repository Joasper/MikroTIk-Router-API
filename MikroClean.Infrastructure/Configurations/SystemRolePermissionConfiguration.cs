using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class SystemRolePermissionConfiguration : IEntityTypeConfiguration<SystemRolePermission>
    {
        public void Configure(EntityTypeBuilder<SystemRolePermission> builder)
        {
            builder.ToTable("SystemRolePermissions");
            
            builder.HasKey(srp => new { srp.RoleId, srp.PermissionId });
            
            builder.Property(srp => srp.RoleId)
                .IsRequired();
            
            builder.Property(srp => srp.PermissionId)
                .IsRequired();

            // Indexes
            builder.HasIndex(srp => srp.PermissionId);
        }
    }
}
